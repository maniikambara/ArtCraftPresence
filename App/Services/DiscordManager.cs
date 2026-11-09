using DiscordRPC;
using DiscordRPC.Logging;

namespace ArtCraftPresence.Services;

public class DiscordManager : IDisposable
{
    private readonly AppDetector _detector = new();
    private readonly CancellationTokenSource _cts = new();

    private DiscordRpcClient? _client;
    private int _currentPid;
    private DateTime? _sessionStartUtc;
    private string _lastPresenceSignature = string.Empty;
    private bool _warnedNoAppId;
    private readonly HashSet<string> _invalidAppIds = [];

    /// <summary>Raised with a short status text for the tray (e.g. "FilmCraft: Editing intro.fcproj").</summary>
    public event Action<string>? StatusChanged;

    public string Status { get; private set; } = "Menunggu aplikasi ArtCraft...";

    public Task RunAsync() => Task.Run(LoopAsync);

    private async Task LoopAsync()
    {
        while (!_cts.IsCancellationRequested)
        {
            try
            {
                Tick();
            }
            catch (Exception ex)
            {
                Logger.Log($"Update error: {ex}");
            }

            try
            {
                await Task.Delay(SettingsManager.Settings.UpdateInterval, _cts.Token);
            }
            catch (TaskCanceledException)
            {
                break;
            }
        }
    }

    private void Tick()
    {
        var settings = SettingsManager.Settings;
        var running = _detector.Detect();

        if (running is null)
        {
            ClearClient();
            _sessionStartUtc = null;
            SetStatus("Menunggu aplikasi ArtCraft...");
            return;
        }

        var app = running.App;
        var appId = string.IsNullOrWhiteSpace(app.ApplicationId) ? settings.DefaultApplicationId : app.ApplicationId;
        appId = appId.Trim();

        if (string.IsNullOrEmpty(appId) || !appId.All(char.IsDigit))
        {
            SetStatus($"{app.Name} terdeteksi, tapi Application ID Discord belum diisi");
            if (!_warnedNoAppId)
            {
                _warnedNoAppId = true;
                TrayManager.Notify("Application ID belum diisi",
                    "Isi DefaultApplicationId di settings.yml (klik kanan icon tray > Settings), lalu Restart.");
            }

            return;
        }

        if (_invalidAppIds.Contains(appId))
        {
            SetStatus($"Application ID {appId} ditolak Discord - cek settings.yml / apps.yml");
            return;
        }

        if (_client is null || _client.ApplicationID != appId || _client.IsDisposed)
        {
            ClearClient();
            _client = new DiscordRpcClient(appId)
            {
                Logger = new ConsoleLogger { Level = LogLevel.Warning }
            };
            _client.OnError += (_, e) => Logger.Log($"Discord error: {e.Message}");
            _client.OnConnectionFailed += (_, _) => SetStatus("Discord tidak terhubung (apakah Discord sedang jalan?)");
            var idForClose = appId;
            _client.OnClose += (_, e) =>
            {
                Logger.Log($"Discord closed connection: {e.Code} {e.Reason}");
                if (e.Code != 4000) return; // 4000 = Invalid Client ID
                if (!_invalidAppIds.Add(idForClose)) return;
                TrayManager.Notify("Application ID tidak valid",
                    $"Discord menolak Application ID {idForClose}. Salin ulang dari Discord Developer Portal, lalu Restart.");
            };
            _client.Initialize();
            _lastPresenceSignature = string.Empty;
        }

        _sessionStartUtc ??= DateTime.UtcNow;
        if (_currentPid != running.ProcessId)
        {
            _currentPid = running.ProcessId;
            _lastPresenceSignature = string.Empty;
        }

        var presence = BuildPresence(running, settings);
        var signature = $"{appId}|{presence.Details}|{presence.State}|{presence.Assets?.LargeImageKey}|{presence.Timestamps?.Start}";
        if (signature == _lastPresenceSignature) return;

        _client.SetPresence(presence);
        _lastPresenceSignature = signature;
        SetStatus($"{app.Name}: {presence.Details}");
        Logger.Log($"Presence -> {app.Name} | {presence.Details} | {presence.State}");
    }

    internal RichPresence BuildPresence(RunningCraftApp running, AppSettings settings)
    {
        var app = running.App;
        var fileName = WindowTitleParser.GetFileName(running.WindowTitle, app.Name);

        string details;
        if (settings.SecretMode)
            details = settings.SecretModeText;
        else if (!string.IsNullOrEmpty(fileName))
            details = settings.WorkingPrefixText + fileName + settings.WorkingSuffixText;
        else
            details = settings.IdleText;

        var state = Fill(settings.StateText, running);
        var useUrl = settings.ImageSource == ImageSource.Url;

        var assets = new Assets
        {
            LargeImageKey = useUrl ? app.IconUrl : app.ImageKey,
            LargeImageText = Clip(Fill(settings.LargeImageText, running).Trim(), 120)
        };

        if (settings.ShowSmallImage)
        {
            // Always send a small image: DiscordRPC 1.6.1 throws while merging Discord's reply when it is null.
            // "artcraft" = asset uploaded in Developer Portal > Rich Presence > Art Assets.
            assets.SmallImageKey = string.IsNullOrWhiteSpace(settings.SmallImage) ? "artcraft" : settings.SmallImage.Trim();
            assets.SmallImageText = "ArtCraft";
        }

        var presence = new RichPresence
        {
            Details = Clip(details, 120),
            State = string.IsNullOrWhiteSpace(state) ? null : Clip(state, 120),
            Assets = assets,
            StatusDisplay = settings.StatusDisplay
        };

        if (settings.ShowElapsedTime)
        {
            var start = settings.KeepTimerAcrossApps && _sessionStartUtc.HasValue
                ? _sessionStartUtc.Value
                : running.StartTimeUtc;
            presence.Timestamps = new Timestamps { Start = start };
        }

        if (settings.ShowWebsiteButton)
            presence.Buttons = [new DiscordRPC.Button { Label = "Get ArtCraft", Url = Constants.WEBSITE_URL }];

        return presence;
    }

    private static string Fill(string template, RunningCraftApp r) =>
        (template ?? string.Empty)
        .Replace("{app}", r.App.Name)
        .Replace("{category}", r.App.Category)
        .Replace("{version}", string.IsNullOrEmpty(r.Version) ? string.Empty : "v" + r.Version)
        .Trim();

    /// <summary>Discord limits text fields to 128 bytes.</summary>
    internal static string Clip(string text, int maxBytes)
    {
        if (System.Text.Encoding.UTF8.GetByteCount(text) <= maxBytes) return text;
        while (text.Length > 0 && System.Text.Encoding.UTF8.GetByteCount(text + "...") > maxBytes)
            text = text[..^1];
        return text + "...";
    }

    private void SetStatus(string status)
    {
        if (status == Status) return;
        Status = status;
        StatusChanged?.Invoke(status);
    }

    private void ClearClient()
    {
        if (_client is null) return;
        try
        {
            _client.ClearPresence();
            _client.Dispose();
        }
        catch
        {
            // ignore
        }

        _client = null;
        _currentPid = 0;
        _lastPresenceSignature = string.Empty;
    }

    public void Dispose()
    {
        _cts.Cancel();
        ClearClient();
        _cts.Dispose();
    }
}
