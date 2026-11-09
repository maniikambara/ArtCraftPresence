using ArtCraftPresence.Services;
using Xunit;
using Xunit.Abstractions;

namespace ArtCraftPresence.Tests;

/// <summary>Only does something when a Craft app is open (manual check).</summary>
public class LiveDetectionTests(ITestOutputHelper output)
{
    [Fact]
    public void DetectsOpenCraftApp()
    {
        var running = new AppDetector().Detect();
        if (running is null)
        {
            output.WriteLine("No Craft app open - nothing to check.");
            return;
        }

        output.WriteLine($"Detected {running.App.Name} pid={running.ProcessId} title='{running.WindowTitle}' version={running.Version}");
        var presence = new DiscordManager().BuildPresence(running, new AppSettings());
        output.WriteLine($"Details='{presence.Details}' State='{presence.State}' Image='{presence.Assets.LargeImageKey}' Hover='{presence.Assets.LargeImageText}'");
        Assert.False(string.IsNullOrEmpty(presence.Details));
    }
}
