namespace ArtCraftPresence.Services;

public static class Logger
{
    private static readonly object Lock = new();

    public static void Log(string message)
    {
        var line = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}";
        System.Diagnostics.Debug.WriteLine(line);
        if (!SettingsManager.Settings.Debug) return;

        try
        {
            lock (Lock)
            {
                var path = Path.Combine(SettingsManager.SettingsDirectory, Constants.LOG_FILENAME);
                if (File.Exists(path) && new FileInfo(path).Length > 2_000_000) File.Delete(path);
                File.AppendAllText(path, line + Environment.NewLine);
            }
        }
        catch
        {
            // Logging must never crash the app.
        }
    }
}
