using Microsoft.Win32;

namespace ArtCraftPresence.Services;

public static class StartupManager
{
    /// <summary>Adds or removes this app from Windows startup for the current user.</summary>
    public static void SetStartup(string appName, string? exePath, bool add)
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(Constants.STARTUP_REGISTRY_PATH, true);
            if (key is null) return;

            if (add && !string.IsNullOrWhiteSpace(exePath))
                key.SetValue(appName, $"\"{exePath}\"");
            else
                key.DeleteValue(appName, false);
        }
        catch (Exception ex)
        {
            Logger.Log($"Startup registry error: {ex.Message}");
        }
    }
}
