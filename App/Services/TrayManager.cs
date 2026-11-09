using System.Diagnostics;

namespace ArtCraftPresence.Services;

public class TrayManager : IDisposable
{
    private static TrayManager? _instance;

    private readonly Icon _icon;
    private readonly NotifyIcon _trayIcon;
    private readonly ToolStripMenuItem _statusItem;
    private readonly SynchronizationContext? _ui;

    public TrayManager()
    {
        _instance = this;
        _ui = SynchronizationContext.Current;

        using var stream = typeof(TrayManager).Assembly.GetManifestResourceStream("appicon.ico");
        _icon = stream is not null ? new Icon(stream) : SystemIcons.Application;

        _statusItem = new ToolStripMenuItem("Waiting for an ArtCraft app...") { Enabled = false };

        var secretItem = new ToolStripMenuItem("Secret mode (hide file names)")
        {
            CheckOnClick = true,
            Checked = SettingsManager.Settings.SecretMode
        };
        secretItem.Click += (_, _) =>
        {
            SettingsManager.Settings.SecretMode = secretItem.Checked;
            SettingsManager.SaveSettings();
        };

        var startupItem = new ToolStripMenuItem("Open on Windows startup")
        {
            CheckOnClick = true,
            Checked = SettingsManager.Settings.OpenOnStartup
        };
        startupItem.Click += (_, _) => SettingsManager.SetStartup(startupItem.Checked);

        var menu = new ContextMenuStrip();
        menu.Items.Add(_statusItem);
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add(secretItem);
        menu.Items.Add(startupItem);
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add("Settings (settings.yml)", null, (_, _) => OpenPath(SettingsManager.SettingsFilePath));
        menu.Items.Add("App list (apps.yml)", null, (_, _) => OpenPath(SettingsManager.AppsFilePath));
        menu.Items.Add("Open settings folder", null, (_, _) => OpenPath(SettingsManager.SettingsDirectory));
        menu.Items.Add("Discord Developer Portal", null, (_, _) => OpenPath(Constants.DISCORD_PORTAL_URL));
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add("Restart (reload settings)", null, (_, _) => Restart());
        menu.Items.Add("Exit", null, (_, _) => Application.Exit());

        _trayIcon = new NotifyIcon
        {
            Icon = _icon,
            Text = Constants.APP_NAME,
            ContextMenuStrip = menu,
            Visible = true
        };
    }

    /// <summary>Update the status line in the tray menu and tooltip (thread-safe).</summary>
    public void SetStatus(string status) => RunOnUi(() =>
    {
        _statusItem.Text = status.Replace("&", "&&");
        var tip = $"{Constants.APP_NAME}\n{status}";
        _trayIcon.Text = tip.Length > 127 ? tip[..127] : tip;
    });

    /// <summary>Show a Windows notification balloon (thread-safe).</summary>
    public static void Notify(string title, string message)
    {
        if (_instance is null) return;
        _instance.RunOnUi(() => _instance._trayIcon.ShowBalloonTip(5000, title, message, ToolTipIcon.Info));
    }

    private void RunOnUi(Action action)
    {
        if (_ui is null) action();
        else _ui.Post(_ => action(), null);
    }

    private static void Restart()
    {
        try
        {
            Process.Start(new ProcessStartInfo(Environment.ProcessPath ?? Application.ExecutablePath, "/restart")
            {
                UseShellExecute = false
            });
        }
        catch (Exception ex)
        {
            Logger.Log($"Restart error: {ex.Message}");
        }

        Application.Exit();
    }

    private static void OpenPath(string path)
    {
        try
        {
            Process.Start(new ProcessStartInfo(path) { UseShellExecute = true });
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Could not open {path}\n{ex.Message}", Constants.APP_NAME,
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    public void Dispose()
    {
        _trayIcon.Visible = false;
        _trayIcon.Dispose();
        _icon.Dispose();
        _instance = null;
    }
}
