namespace ArtCraftPresence;

public enum ImageSource
{
    /// <summary>Use the icon URL from getartcraft.com (no upload needed).</summary>
    Url,

    /// <summary>Use an asset key uploaded in the Discord Developer Portal (Rich Presence > Art Assets).</summary>
    AssetKey
}

public class AppSettings
{
    /// <summary>
    ///     Discord Application ID used for every app that has no ApplicationId of its own in apps.yml.
    ///     Create one at https://discord.com/developers/applications (name it e.g. "ArtCraft").
    /// </summary>
    public string DefaultApplicationId { get; set; } = string.Empty;

    /// <summary>Folder where the ArtCraft apps are installed (e.g. C:\Program Files\FilmCraft).</summary>
    public string InstallRoot { get; set; } = @"C:\Program Files";

    /// <summary>Only show presence for processes started from InstallRoot.</summary>
    public bool OnlyFromInstallRoot { get; set; } = false;

    /// <summary>How often running apps are checked.</summary>
    public TimeSpan UpdateInterval { get; set; } = new(0, 0, 3);

    /// <summary>Where the large image comes from: Url or AssetKey.</summary>
    public ImageSource ImageSource { get; set; } = ImageSource.Url;

    /// <summary>Show the small ArtCraft logo in the corner of the app icon.</summary>
    public bool ShowSmallImage { get; set; } = true;

    /// <summary>Small image: asset key or https URL. Empty = asset key "artcraft" (upload icons/artcraft.png to Art Assets).</summary>
    public string SmallImage { get; set; } = string.Empty;

    /// <summary>Text shown when the app is open but no file is open.</summary>
    public string IdleText { get; set; } = "Idle";

    /// <summary>Text before the file name.</summary>
    public string WorkingPrefixText { get; set; } = "Editing ";

    /// <summary>Text after the file name.</summary>
    public string WorkingSuffixText { get; set; } = string.Empty;

    /// <summary>Second line under the details. Placeholders: {app}, {category}, {version}. Empty = hidden.</summary>
    public string StateText { get; set; } = "{category}";

    /// <summary>Hover text of the large image. Placeholders: {app}, {category}, {version}.</summary>
    public string LargeImageText { get; set; } = "{app} {version}";

    /// <summary>
    ///     What Discord shows after "Playing" in the member list: Name (name of the Discord application),
    ///     Details (the details line) or State (the state line).
    /// </summary>
    public DiscordRPC.StatusDisplayType StatusDisplay { get; set; } = DiscordRPC.StatusDisplayType.Name;

    /// <summary>Show the elapsed time.</summary>
    public bool ShowElapsedTime { get; set; } = true;

    /// <summary>Keep the timer running when switching between Craft apps (true) or restart it per app (false).</summary>
    public bool KeepTimerAcrossApps { get; set; } = false;

    /// <summary>Hide file names.</summary>
    public bool SecretMode { get; set; } = false;

    /// <summary>Text shown instead of the file name in secret mode.</summary>
    public string SecretModeText { get; set; } = "Working on a secret project";

    /// <summary>Add a "Get ArtCraft" button to the presence.</summary>
    public bool ShowWebsiteButton { get; set; } = true;

    /// <summary>Show a notification when the app starts.</summary>
    public bool DisablePopup { get; set; } = false;

    /// <summary>Start with Windows.</summary>
    public bool OpenOnStartup { get; set; } = true;

    /// <summary>Write a log file to the settings folder.</summary>
    public bool Debug { get; set; } = false;
}
