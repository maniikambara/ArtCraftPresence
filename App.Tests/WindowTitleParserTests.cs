using ArtCraftPresence.Services;
using Xunit;

namespace ArtCraftPresence.Tests;

public class WindowTitleParserTests
{
    [Theory]
    [InlineData("PhotoCraft", "PhotoCraft", "")]
    [InlineData("PhotoCraft 0.21.0", "PhotoCraft", "")]
    [InlineData("poster.png - PhotoCraft", "PhotoCraft", "poster.png")]
    [InlineData("*poster.png - PhotoCraft", "PhotoCraft", "poster.png")]
    [InlineData("PhotoCraft - poster.png", "PhotoCraft", "poster.png")]
    [InlineData("FilmCraft \u2014 Intro Video*", "FilmCraft", "Intro Video")]
    [InlineData("FilmCraft | My Edit.fcproj", "FilmCraft", "My Edit.fcproj")]
    [InlineData("Untitled - FigCraft", "FigCraft", "")]
    [InlineData("Craft Anything - my-app", "Craft Anything", "my-app")]
    [InlineData("report.pdf \u25CF - PdfCraft 0.3.1", "PdfCraft", "report.pdf")]
    [InlineData("", "PhotoCraft", "")]
    public void ParsesFileName(string title, string app, string expected) =>
        Assert.Equal(expected, WindowTitleParser.GetFileName(title, app));

    [Fact]
    public void CatalogHasAllSixteenAppsWithUniqueProcessNames()
    {
        var apps = CraftAppCatalog.Defaults();
        Assert.Equal(16, apps.Count);
        Assert.Equal(16, apps.Select(a => a.ProcessName).Distinct().Count());
        Assert.Contains(apps, a => a.Name == "FilmCraft" && a.ProcessName == "filmcraft" && a.Folder == "FilmCraft");
        Assert.All(apps, a => Assert.StartsWith("https://getartcraft.com/images/apps/", a.IconUrl));
    }

    [Fact]
    public void ClipKeepsTextUnder128Bytes()
    {
        var clipped = DiscordManager.Clip(new string('\u00e9', 200), 120);
        Assert.True(System.Text.Encoding.UTF8.GetByteCount(clipped) <= 120);
        Assert.EndsWith("...", clipped);
    }
}
