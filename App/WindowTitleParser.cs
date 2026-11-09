namespace ArtCraftPresence;

/// <summary>
///     Gets the open file / project name from a Craft app window title.
///     Handles "file - FilmCraft", "FilmCraft - file", "FilmCraft — file*", "FilmCraft | file" and "FilmCraft".
/// </summary>
public static class WindowTitleParser
{
    private static readonly string[] Separators = [" - ", " — ", " – ", " | ", " : "];

    public static string GetFileName(string? title, string appName)
    {
        if (string.IsNullOrWhiteSpace(title)) return string.Empty;

        var parts = SplitTitle(title.Trim());

        // Drop the parts that are just the app name (optionally followed by a version, e.g. "FilmCraft 0.21.0").
        var rest = parts.Where(p => !IsAppName(p, appName)).ToList();

        if (rest.Count == parts.Count && parts.Count == 1)
        {
            // Single part that is not the app name: maybe "FilmCraft file" without a separator.
            var only = parts[0];
            if (only.StartsWith(appName + " ", StringComparison.OrdinalIgnoreCase))
                only = only[(appName.Length + 1)..];
            rest = [only];
        }

        var name = string.Join(" - ", rest).Trim();
        name = name.Trim('*', '●', '•', ' ', '-', '\t', '[', ']', '(', ')').Trim();

        return IsPlaceholder(name) ? string.Empty : name;
    }

    private static List<string> SplitTitle(string title)
    {
        var parts = new List<string> { title };
        foreach (var sep in Separators)
            parts = parts.SelectMany(p => p.Split(sep, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)).ToList();
        return parts;
    }

    private static bool IsAppName(string part, string appName)
    {
        var p = part.Trim().TrimStart('*').Trim();
        if (p.Equals(appName, StringComparison.OrdinalIgnoreCase)) return true;
        if (!p.StartsWith(appName + " ", StringComparison.OrdinalIgnoreCase)) return false;

        // "FilmCraft 0.21.0" or "FilmCraft v0.21" or "FilmCraft (alpha)"
        var tail = p[(appName.Length + 1)..].Trim().TrimStart('v', 'V');
        return tail.Length > 0 && (char.IsDigit(tail[0]) || tail.StartsWith('(') ||
                                   tail.Equals("alpha", StringComparison.OrdinalIgnoreCase) ||
                                   tail.Equals("beta", StringComparison.OrdinalIgnoreCase));
    }

    private static bool IsPlaceholder(string name) =>
        name.Length == 0 ||
        name.Equals("Untitled", StringComparison.OrdinalIgnoreCase) ||
        name.Equals("Welcome", StringComparison.OrdinalIgnoreCase) ||
        name.Equals("Home", StringComparison.OrdinalIgnoreCase) ||
        name.Equals("Start", StringComparison.OrdinalIgnoreCase);
}
