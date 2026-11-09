# ArtCraftPresence

Discord Rich Presence for [ArtCraft](https://getartcraft.com/) apps. Inspired by [DAWPresence](https://github.com/Myuuiii/DAWPresence).

When you open an ArtCraft app (PhotoCraft, FilmCraft, PdfCraft and more), your Discord status shows:

- the **icon** of the app you're using,
- the **file / project** you have open, read from the window title (or "Idle"),
- the app's **category**, e.g. "Video and effects",
- **elapsed time** since the app was opened,
- a **Get ArtCraft** button.

It runs in the background with an icon in the system tray.

## Supported apps

| Category | App (process name) |
|---|---|
| Graphics and design | PhotoCraft (`photocraft`), VectorCraft (`vectorcraft`), LightCraft (`lightcraft`), DesignCraft (`designcraft`), FigCraft (`figcraft`), TextureCraft (`texturecraft`), MaterialCraft (`materialcraft`) |
| Video and effects | FilmCraft (`filmcraft`), EffectCraft (`effectcraft`) |
| Audio | SoundCraft (`soundcraft`) |
| CAD and drafting | CADCraft (`cadcraft`) |
| Office | WordCraft (`wordcraft`), GridCraft (`gridcraft`), DeckCraft (`deckcraft`), PdfCraft (`pdfcraft`) |
| Developer tools | Craft Anything (`craft-anything`) |

Default install location: `C:\Program Files\<AppName>\<appname>.exe`, e.g. `C:\Program Files\FilmCraft\filmcraft.exe`. `*-cli.exe` tools and processes without a window are ignored.

If several apps are open at once, the one you're currently using (the active window) is shown.

## Download

Download `ArtCraftPresence.exe` (or the `.zip`, which also includes the `icons/` folder) from the **[Releases](../../releases/latest)** page.

---

## 1. Create a Discord Application (required, one time)

Discord only shows Rich Presence for a registered "Application", so you need to create one:

1. Go to <https://discord.com/developers/applications> and log in.
2. Click **New Application** and name it **`ArtCraft`**. This name appears as *"Playing ArtCraft"*.
3. On the **General Information** page, copy the **Application ID** (a long number).
4. (Optional) Upload `icons/artcraft-app-icon-1024.png` as the **App Icon**.
5. **(Recommended)** Open **Rich Presence → Art Assets** and upload `icons/artcraft.png` with the key `artcraft`. It becomes the small logo in the corner of the app icon. Without it, the presence still works, but the small logo is not shown.

### (Optional) Show "Playing FilmCraft" instead of "Playing ArtCraft"

Repeat the steps above once per app, naming the applications `FilmCraft`, `PhotoCraft`, and so on. Then fill in each app's `ApplicationId` in `apps.yml` (see section 3). Apps with an empty `ApplicationId` use `DefaultApplicationId`.

There is also a way without creating many applications: in `settings.yml`, set `StatusDisplay: State` and `StateText: '{app}'`. The member list then shows the Craft app's name.

## 2. Run it

1. Run `ArtCraftPresence.exe`. It is self-contained, so you don't need to install .NET.
   - Tip: move the exe to a permanent folder, e.g. `C:\Program Files\ArtCraftPresence\`, before the first run. The *start with Windows* option remembers where the exe is.
2. On the first run, a dialog appears. Click **Yes** to open the Developer Portal and `settings.yml`.
3. Paste your Application ID into this line:
   ```yaml
   DefaultApplicationId: 123456789012345678
   ```
4. Save the file, then **right-click the tray icon and choose Restart**.
5. Open FilmCraft (or another Craft app). Your Discord status updates within about 3 seconds.

> Make sure the Discord desktop app is running, and that **User Settings → Activity Privacy → Share my activity** is turned on.

## 3. Configuration

All config files are in `%APPDATA%\ArtCraftPresence\`. You can open them from the tray menu. After editing a file, choose **Restart**.

### `settings.yml`

| Option | Default | Description |
|---|---|---|
| `DefaultApplicationId` | *(empty)* | Discord Application ID, **required** |
| `InstallRoot` | `C:\Program Files` | Parent folder where the Craft apps are installed |
| `OnlyFromInstallRoot` | `false` | `true` = only detect exes located in `InstallRoot` |
| `UpdateInterval` | `00:00:03` | How often to check for running apps |
| `ImageSource` | `Url` | `Url` = icons are loaded from getartcraft.com. `AssetKey` = icons you uploaded to the Developer Portal |
| `ShowSmallImage` / `SmallImage` | `true` / empty | Small logo in the corner of the icon. Asset key or https URL; empty = asset key `artcraft` |
| `IdleText` | `Idle` | Text shown when no file is open |
| `WorkingPrefixText` / `WorkingSuffixText` | `Editing ` / empty | Text before / after the file name |
| `StateText` | `{category}` | Second line. Placeholders: `{app}`, `{category}`, `{version}` |
| `LargeImageText` | `{app} {version}` | Text shown when hovering the icon, e.g. "FilmCraft v0.21.0" |
| `StatusDisplay` | `Name` | What appears after "Playing" in the member list: `Name`, `Details` or `State` |
| `ShowElapsedTime` | `true` | Show elapsed time |
| `KeepTimerAcrossApps` | `false` | `true` = don't reset the timer when switching between Craft apps |
| `SecretMode` / `SecretModeText` | `false` | Hide file names. Can also be toggled from the tray |
| `ShowWebsiteButton` | `true` | "Get ArtCraft" button |
| `DisablePopup` | `false` | Disable the notification on startup |
| `OpenOnStartup` | `true` | Start automatically with Windows |
| `Debug` | `false` | Write a log to `log.txt` |

### `apps.yml`

The list of apps to detect. You can disable an app (`Enabled: false`), change the displayed name, or add new Craft apps:

```yaml
- Name: FilmCraft
  ProcessName: filmcraft        # .exe name without .exe
  Folder: FilmCraft             # folder inside InstallRoot
  Category: Video and effects
  ImageKey: filmcraft           # used when ImageSource: AssetKey
  IconUrl: https://getartcraft.com/images/apps/filmcraft/icon.webp
  ApplicationId: ''             # optional, Application ID only for FilmCraft
  Enabled: true
```

## Icons not showing in Discord?

`Url` mode loads images directly from getartcraft.com. If Discord doesn't show them, use uploaded assets instead:

1. In the Developer Portal, open your application and go to **Rich Presence → Art Assets**.
2. Upload all PNGs from the `icons/` folder. The file name becomes the key, e.g. `filmcraft`, `photocraft`, `artcraft`.
   If you use a separate application per app, upload the matching icon to each application.
3. In `settings.yml`, set `ImageSource: AssetKey`, then choose Restart.

> Newly uploaded assets can take a few minutes to appear.

## Troubleshooting

| Symptom | Fix |
|---|---|
| Tray says "Application ID is not set" | Set `DefaultApplicationId`, then Restart |
| "Invalid Application ID" notification | The ID was copied incorrectly. Copy it again from the General Information page |
| "Not connected to Discord" | Open the Discord desktop app. The browser version doesn't support RPC |
| Status doesn't show at all | Check Activity Privacy in Discord. Set `Debug: true` and look at `log.txt` |
| File name isn't detected | The app may not put the file name in its window title yet. `IdleText` is shown instead |

## Building from source

Requires the [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0).

```bat
tools\build.bat
```

The output is `publish\ArtCraftPresence.exe`, a single self-contained file.

Tests:

```bat
dotnet test
```

## Project structure

```
ArtCraftPresence/
├─ App/
│  ├─ Program.cs               entry point, single instance, first-run setup
│  ├─ AppSettings.cs           settings.yml options
│  ├─ CraftApp.cs              app model + list of the 16 ArtCraft apps
│  ├─ WindowTitleParser.cs     gets the file name from the window title
│  ├─ NativeWindow.cs          Win32: window titles & active window
│  └─ Services/
│     ├─ AppDetector.cs        finds running Craft apps
│     ├─ DiscordManager.cs     sends Rich Presence to Discord
│     ├─ SettingsManager.cs    reads/writes settings.yml & apps.yml
│     ├─ TrayManager.cs        system tray icon & menu
│     ├─ StartupManager.cs     start with Windows (registry HKCU\...\Run)
│     └─ Logger.cs
├─ App.Tests/                  unit tests (xUnit)
├─ icons/                      512×512 PNG icons for uploading to Discord
└─ tools/build.bat
```

Remove from Windows startup: `ArtCraftPresence.exe /uninstall`.

## Releasing a new version

GitHub Actions creates a release automatically whenever a `v*` tag is pushed:

```bash
git tag v1.0.1
git push origin v1.0.1
```

## License

[GPL-3.0](LICENSE.md), the same as DAWPresence, which this project is based on.

## Credits

- Concept and structure from [DAWPresence](https://github.com/Myuuiii/DAWPresence) by Myuuiii.
- App icons © ArtCraft ([getartcraft.com](https://getartcraft.com/)).
- [DiscordRichPresence](https://github.com/Lachee/discord-rpc-csharp) and [YamlDotNet](https://github.com/aaubry/YamlDotNet).
