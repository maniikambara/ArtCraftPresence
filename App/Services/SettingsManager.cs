using System.Diagnostics;
using YamlDotNet.Serialization;

namespace ArtCraftPresence.Services;

public static class SettingsManager
{
    public static readonly string SettingsDirectory = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        Constants.SETTINGS_DIRECTORY_NAME);

    public static readonly string SettingsFilePath = Path.Combine(SettingsDirectory, Constants.SETTINGS_FILENAME);
    public static readonly string AppsFilePath = Path.Combine(SettingsDirectory, Constants.APPS_FILENAME);

    public static AppSettings Settings { get; private set; } = new();
    public static List<CraftApp> Apps { get; private set; } = CraftAppCatalog.Defaults();

    private static readonly IDeserializer Deserializer =
        new DeserializerBuilder().IgnoreUnmatchedProperties().Build();

    private static readonly ISerializer Serializer = new SerializerBuilder().Build();

    public static void Load()
    {
        Directory.CreateDirectory(SettingsDirectory);
        Settings = LoadFile(SettingsFilePath, () => new AppSettings()) ?? new AppSettings();
        FillEmptyStrings(Settings, new AppSettings(), nameof(AppSettings.DefaultApplicationId),
            nameof(AppSettings.SmallImage), nameof(AppSettings.WorkingSuffixText), nameof(AppSettings.StateText));
        if (Settings.UpdateInterval < TimeSpan.FromSeconds(1)) Settings.UpdateInterval = TimeSpan.FromSeconds(1);
        SaveSettings();

        Apps = LoadApps();
        SaveApps();
    }

    private static List<CraftApp> LoadApps()
    {
        var loaded = LoadFile<List<CraftApp>>(AppsFilePath, CraftAppCatalog.Defaults) ?? [];
        loaded = loaded.Where(a => !string.IsNullOrWhiteSpace(a.ProcessName)).ToList();

        // Add apps that are new in the catalog but missing in the user's file.
        foreach (var def in CraftAppCatalog.Defaults())
        {
            var existing = loaded.FirstOrDefault(a =>
                a.ProcessName.Equals(def.ProcessName, StringComparison.OrdinalIgnoreCase));
            if (existing is null)
            {
                loaded.Add(def);
                continue;
            }

            FillEmptyStrings(existing, def, nameof(CraftApp.ApplicationId));
        }

        return loaded;
    }

    private static T? LoadFile<T>(string path, Func<T> fallback) where T : class
    {
        if (!File.Exists(path)) return fallback();

        try
        {
            var text = File.ReadAllText(path);
            if (string.IsNullOrWhiteSpace(text)) return fallback();
            return Deserializer.Deserialize<T>(text) ?? fallback();
        }
        catch (Exception ex)
        {
            BackupAsError(path);
            MessageBox.Show($"{Path.GetFileName(path)} is invalid and was reset to defaults.\n" +
                            $"The old file was saved with an .error extension\n\n{ex.Message}",
                Constants.APP_NAME, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return fallback();
        }
    }

    /// <summary>Replace null/empty string properties with the default value, except the listed ones.</summary>
    private static void FillEmptyStrings<T>(T target, T defaults, params string[] allowEmpty)
    {
        foreach (var prop in typeof(T).GetProperties().Where(p => p.CanWrite))
        {
            if (allowEmpty.Contains(prop.Name)) continue;
            var value = prop.GetValue(target);
            if (value is null || (value is string s && string.IsNullOrWhiteSpace(s)))
                prop.SetValue(target, prop.GetValue(defaults));
        }
    }

    private static void BackupAsError(string path)
    {
        try
        {
            var errorPath = $"{path}.{DateTime.Now:yyyyMMddHHmmss}.error";
            File.Move(path, errorPath);
        }
        catch
        {
            // ignore
        }
    }

    private const string SettingsHeader =
        "# ArtCraftPresence settings. Save this file, then right-click the tray icon > Restart.\n" +
        "# DefaultApplicationId: Application ID from https://discord.com/developers/applications\n" +
        "# ImageSource: Url (icons from getartcraft.com) or AssetKey (icons uploaded to the Discord Developer Portal)\n\n";

    private const string AppsHeader =
        "# ArtCraft apps to detect.\n" +
        "# ProcessName = .exe name without .exe | Folder = folder name inside InstallRoot\n" +
        "# ApplicationId (optional) = Application ID only for this app, so Discord shows \"Playing FilmCraft\".\n" +
        "#   Leave empty to use DefaultApplicationId from settings.yml.\n\n";

    public static void SaveSettings() =>
        File.WriteAllText(SettingsFilePath, SettingsHeader + Serializer.Serialize(Settings));

    public static void SaveApps() =>
        File.WriteAllText(AppsFilePath, AppsHeader + Serializer.Serialize(Apps));

    public static void SetStartup(bool enable)
    {
        var exePath = Process.GetCurrentProcess().MainModule?.FileName ?? Application.ExecutablePath;
        StartupManager.SetStartup(Constants.APP_NAME, enable ? exePath : null, enable);
        Settings.OpenOnStartup = enable;
        SaveSettings();
    }
}
