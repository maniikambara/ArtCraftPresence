using System.Diagnostics;

namespace ArtCraftPresence.Services;

/// <summary>A Craft app that is running right now.</summary>
public sealed record RunningCraftApp(CraftApp App, int ProcessId, DateTime StartTimeUtc, string WindowTitle, string Version);

/// <summary>Finds which ArtCraft app is open, preferring the one the user is looking at.</summary>
public class AppDetector
{
    private int _lastFocusedPid;
    private readonly Dictionary<string, string> _versionCache = new(StringComparer.OrdinalIgnoreCase);

    public RunningCraftApp? Detect()
    {
        var settings = SettingsManager.Settings;
        var running = new List<RunningCraftApp>();

        foreach (var app in SettingsManager.Apps.Where(a => a.Enabled))
        {
            Process[] processes;
            try
            {
                processes = Process.GetProcessesByName(app.ProcessName);
            }
            catch
            {
                continue;
            }

            foreach (var p in processes)
            {
                using (p)
                {
                    try
                    {
                        if (p.HasExited) continue;
                        var path = TryGetPath(p);

                        if (settings.OnlyFromInstallRoot && path is not null &&
                            !path.StartsWith(settings.InstallRoot, StringComparison.OrdinalIgnoreCase))
                            continue;

                        var titles = NativeWindow.GetWindowTitles(p.Id);
                        // CLI tools / background helpers have no window: skip them.
                        if (titles.Count == 0) continue;

                        running.Add(new RunningCraftApp(app, p.Id, SafeStartTime(p), PickTitle(titles, app),
                            GetVersion(path ?? app.ExePath(settings.InstallRoot))));
                    }
                    catch (Exception ex)
                    {
                        Logger.Log($"Error reading process {app.ProcessName}: {ex.Message}");
                    }
                }
            }
        }

        if (running.Count == 0) return null;

        var fg = NativeWindow.ForegroundProcessId();
        var focused = running.FirstOrDefault(r => r.ProcessId == fg);
        if (focused is not null)
        {
            _lastFocusedPid = focused.ProcessId;
            return focused;
        }

        // Keep showing the last app the user focused; otherwise the most recently started one.
        return running.FirstOrDefault(r => r.ProcessId == _lastFocusedPid)
               ?? running.OrderByDescending(r => r.StartTimeUtc).First();
    }

    /// <summary>Prefer the window that mentions the app name (the main window), else the longest title.</summary>
    private static string PickTitle(List<string> titles, CraftApp app) =>
        titles.FirstOrDefault(t => t.Contains(app.Name, StringComparison.OrdinalIgnoreCase))
        ?? titles.OrderByDescending(t => t.Length).First();

    private static string? TryGetPath(Process p)
    {
        try
        {
            return p.MainModule?.FileName;
        }
        catch
        {
            return null;
        }
    }

    private static DateTime SafeStartTime(Process p)
    {
        try
        {
            return p.StartTime.ToUniversalTime();
        }
        catch
        {
            return DateTime.UtcNow;
        }
    }

    private string GetVersion(string exePath)
    {
        if (_versionCache.TryGetValue(exePath, out var cached)) return cached;

        var version = string.Empty;
        try
        {
            if (File.Exists(exePath))
            {
                var info = FileVersionInfo.GetVersionInfo(exePath);
                version = (info.ProductVersion ?? info.FileVersion ?? string.Empty).Split('+', ' ')[0];
            }
        }
        catch
        {
            // ignore
        }

        _versionCache[exePath] = version;
        return version;
    }
}
