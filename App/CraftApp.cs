namespace ArtCraftPresence;

/// <summary>One ArtCraft app (e.g. FilmCraft). Loaded from apps.yml so users can edit it.</summary>
public class CraftApp
{
    /// <summary>Name shown in Discord, e.g. "FilmCraft".</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Process name without .exe, e.g. "filmcraft".</summary>
    public string ProcessName { get; set; } = string.Empty;

    /// <summary>Install folder name under InstallRoot, e.g. "FilmCraft".</summary>
    public string Folder { get; set; } = string.Empty;

    /// <summary>Category from getartcraft.com, e.g. "Video and effects".</summary>
    public string Category { get; set; } = string.Empty;

    /// <summary>Asset key in the Discord Developer Portal (AssetKey mode).</summary>
    public string ImageKey { get; set; } = string.Empty;

    /// <summary>Icon URL (Url mode).</summary>
    public string IconUrl { get; set; } = string.Empty;

    /// <summary>
    ///     Optional Discord Application ID only for this app. When set, Discord shows
    ///     "Playing &lt;name of that Discord app&gt;". Empty = use DefaultApplicationId.
    /// </summary>
    public string ApplicationId { get; set; } = string.Empty;

    /// <summary>Set to false to never show presence for this app.</summary>
    public bool Enabled { get; set; } = true;

    public string ExePath(string installRoot) => Path.Combine(installRoot, Folder, ProcessName + ".exe");
}

public static class CraftAppCatalog
{
    private static CraftApp Make(string name, string slug, string category) => new()
    {
        Name = name,
        ProcessName = slug,
        Folder = name,
        Category = category,
        ImageKey = slug,
        IconUrl = $"{Constants.ICON_BASE_URL}/{slug}/icon.webp"
    };

    /// <summary>All apps listed on getartcraft.com.</summary>
    public static List<CraftApp> Defaults() =>
    [
        Make("PhotoCraft", "photocraft", "Graphics and design"),
        Make("VectorCraft", "vectorcraft", "Graphics and design"),
        Make("LightCraft", "lightcraft", "Graphics and design"),
        Make("DesignCraft", "designcraft", "Graphics and design"),
        Make("FigCraft", "figcraft", "Graphics and design"),
        Make("TextureCraft", "texturecraft", "Graphics and design"),
        Make("MaterialCraft", "materialcraft", "Graphics and design"),
        Make("FilmCraft", "filmcraft", "Video and effects"),
        Make("EffectCraft", "effectcraft", "Video and effects"),
        Make("SoundCraft", "soundcraft", "Audio"),
        Make("CADCraft", "cadcraft", "CAD and drafting"),
        Make("WordCraft", "wordcraft", "Office"),
        Make("GridCraft", "gridcraft", "Office"),
        Make("DeckCraft", "deckcraft", "Office"),
        Make("PdfCraft", "pdfcraft", "Office"),
        Make("Craft Anything", "craft-anything", "Developer tools")
    ];
}
