using System.Diagnostics;
using ArtCraftPresence.Services;

namespace ArtCraftPresence;

internal static class Program
{
    [STAThread]
    private static void Main(string[] args)
    {
        if (args.Any(a => a.Equals("/uninstall", StringComparison.OrdinalIgnoreCase)))
        {
            StartupManager.SetStartup(Constants.APP_NAME, null, false);
            return;
        }

        using var mutex = new Mutex(true, @"Local\ArtCraftPresence-SingleInstance", out var isFirst);
        if (!isFirst && args.Any(a => a.Equals("/restart", StringComparison.OrdinalIgnoreCase)))
        {
            // Restarting from the tray: wait for the old instance to close.
            try
            {
                isFirst = mutex.WaitOne(TimeSpan.FromSeconds(15));
            }
            catch (AbandonedMutexException)
            {
                isFirst = true;
            }
        }

        if (!isFirst)
        {
            MessageBox.Show($"{Constants.APP_NAME} is already running. Look for its icon in the system tray.",
                Constants.APP_NAME, MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        ApplicationConfiguration.Initialize();
        // SynchronizationContext for the tray must exist before TrayManager is created.
        SynchronizationContext.SetSynchronizationContext(new WindowsFormsSynchronizationContext());

        SettingsManager.Load();
        Logger.Log($"{Constants.APP_NAME} started");

        // Keep the startup registry entry pointing at the current exe location.
        StartupManager.SetStartup(Constants.APP_NAME,
            SettingsManager.Settings.OpenOnStartup ? Environment.ProcessPath : null,
            SettingsManager.Settings.OpenOnStartup);

        using var tray = new TrayManager();
        using var discord = new DiscordManager();
        discord.StatusChanged += tray.SetStatus;

        if (string.IsNullOrWhiteSpace(SettingsManager.Settings.DefaultApplicationId) &&
            SettingsManager.Apps.All(a => string.IsNullOrWhiteSpace(a.ApplicationId)))
        {
            FirstRunSetup();
        }
        else if (!SettingsManager.Settings.DisablePopup)
        {
            TrayManager.Notify(Constants.APP_NAME,
                "Running in the background. Right-click the tray icon for settings or to exit.");
        }

        _ = discord.RunAsync();
        Application.Run();
    }

    /// <summary>Ask for the Discord Application ID on first run.</summary>
    private static void FirstRunSetup()
    {
        var answer = MessageBox.Show(
            "Welcome to ArtCraftPresence!\n\n" +
            "To show your status on Discord, a Discord Application ID is required:\n" +
            "1. Open the Discord Developer Portal > New Application (name it e.g. \"ArtCraft\")\n" +
            "2. Copy the \"Application ID\" from the General Information page\n" +
            "3. Paste it into DefaultApplicationId in settings.yml, save, then right-click the tray icon > Restart\n\n" +
            "Open the Developer Portal and settings.yml now?",
            Constants.APP_NAME, MessageBoxButtons.YesNo, MessageBoxIcon.Information);

        if (answer != DialogResult.Yes) return;

        try
        {
            Process.Start(new ProcessStartInfo(Constants.DISCORD_PORTAL_URL) { UseShellExecute = true });
            Process.Start(new ProcessStartInfo(SettingsManager.SettingsFilePath) { UseShellExecute = true });
        }
        catch (Exception ex)
        {
            Logger.Log($"First run open error: {ex.Message}");
        }
    }
}
