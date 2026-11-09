# ArtCraftPresence

Discord Rich Presence untuk aplikasi [ArtCraft](https://getartcraft.com/). Program ini terinspirasi dari [DAWPresence](https://github.com/Myuuiii/DAWPresence).

Saat Anda membuka salah satu aplikasi ArtCraft (PhotoCraft, FilmCraft, PdfCraft, dan lainnya), status Discord Anda menampilkan:

- **icon aplikasi** yang sedang dipakai,
- **nama file / project** yang sedang dibuka, diambil dari judul jendela (atau "Idle"),
- **kategori** aplikasi, misalnya "Video and effects",
- **lama pemakaian**, dihitung sejak aplikasi dibuka,
- tombol **Get ArtCraft**.

Program berjalan di background dan icon-nya muncul di system tray.

## Aplikasi yang didukung

| Kategori | Aplikasi (nama proses) |
|---|---|
| Graphics and design | PhotoCraft (`photocraft`), VectorCraft (`vectorcraft`), LightCraft (`lightcraft`), DesignCraft (`designcraft`), FigCraft (`figcraft`), TextureCraft (`texturecraft`), MaterialCraft (`materialcraft`) |
| Video and effects | FilmCraft (`filmcraft`), EffectCraft (`effectcraft`) |
| Audio | SoundCraft (`soundcraft`) |
| CAD and drafting | CADCraft (`cadcraft`) |
| Office | WordCraft (`wordcraft`), GridCraft (`gridcraft`), DeckCraft (`deckcraft`), PdfCraft (`pdfcraft`) |
| Developer tools | Craft Anything (`craft-anything`) |

Lokasi instalasi default: `C:\Program Files\<NamaApp>\<namaapp>.exe`, contohnya `C:\Program Files\FilmCraft\filmcraft.exe`. File `*-cli.exe` dan proses tanpa jendela diabaikan.

Kalau beberapa aplikasi terbuka sekaligus, yang ditampilkan adalah aplikasi yang sedang Anda pakai (jendela aktif).

## Download

Unduh `ArtCraftPresence.exe` (atau file `.zip` yang sudah termasuk folder `icons/`) dari halaman **[Releases](../../releases/latest)**.

---

## 1. Buat Discord Application (wajib, sekali saja)

Discord hanya menampilkan Rich Presence dari "Application" yang terdaftar. Karena itu Anda perlu membuat satu application:

1. Buka <https://discord.com/developers/applications> lalu login.
2. Klik **New Application** dan beri nama **`ArtCraft`**. Nama ini yang muncul sebagai *"Playing ArtCraft"*.
3. Di halaman **General Information**, salin **Application ID** (berupa angka panjang).
4. (Opsional) Upload `icons/artcraft-app-icon-1024.png` sebagai **App Icon**.
5. **(Disarankan)** Buka **Rich Presence → Art Assets**, lalu upload `icons/artcraft.png` dengan nama key `artcraft`. File ini menjadi logo kecil di pojok icon aplikasi. Kalau belum di-upload, presence tetap jalan, hanya saja logo kecil tidak tampil.

### (Opsional) Tampilkan "Playing FilmCraft", bukan "Playing ArtCraft"

Ulangi langkah di atas untuk setiap aplikasi, dengan nama application `FilmCraft`, `PhotoCraft`, dan seterusnya. Lalu isi `ApplicationId` setiap aplikasi di `apps.yml` (lihat bagian 3). Aplikasi yang `ApplicationId`-nya kosong otomatis memakai `DefaultApplicationId`.

Ada cara lain tanpa membuat banyak application. Di `settings.yml`, isi `StatusDisplay: State` dan `StateText: '{app}'`. Dengan begitu, status di member list menampilkan nama aplikasi Craft.

## 2. Jalankan

1. Jalankan `publish\ArtCraftPresence.exe`. File ini self-contained, jadi tidak perlu install .NET.
   - Saran: pindahkan exe ini ke folder tetap, misalnya `C:\Program Files\ArtCraftPresence\`, sebelum dijalankan pertama kali. Fitur *start with Windows* mencatat lokasi exe.
2. Saat pertama kali dijalankan, akan muncul dialog. Klik **Yes** untuk membuka Developer Portal dan `settings.yml`.
3. Tempel Application ID ke baris berikut:
   ```yaml
   DefaultApplicationId: 123456789012345678
   ```
4. Simpan file, lalu **klik kanan icon tray, pilih Restart**.
5. Buka FilmCraft (atau aplikasi Craft lain). Status Discord Anda akan berubah dalam ±3 detik.

> Pastikan aplikasi Discord desktop sedang berjalan. Aktifkan juga **User Settings → Activity Privacy → Share my activity**.

## 3. Konfigurasi

Semua file konfigurasi ada di `%APPDATA%\ArtCraftPresence\`. Anda bisa membukanya lewat menu tray. Setelah mengubah file, pilih **Restart**.

### `settings.yml`

| Opsi | Default | Keterangan |
|---|---|---|
| `DefaultApplicationId` | *(kosong)* | Application ID Discord, **wajib** diisi |
| `InstallRoot` | `C:\Program Files` | Folder induk instalasi aplikasi Craft |
| `OnlyFromInstallRoot` | `false` | `true` = hanya deteksi exe yang ada di `InstallRoot` |
| `UpdateInterval` | `00:00:03` | Seberapa sering status dicek |
| `ImageSource` | `Url` | `Url` = icon diambil langsung dari getartcraft.com. `AssetKey` = icon yang Anda upload ke Developer Portal |
| `ShowSmallImage` / `SmallImage` | `true` / kosong | Logo kecil di pojok icon. Pada mode `AssetKey`, default key-nya `artcraft` |
| `IdleText` | `Idle` | Teks saat belum ada file yang dibuka |
| `WorkingPrefixText` / `WorkingSuffixText` | `Editing ` / kosong | Teks sebelum / sesudah nama file |
| `StateText` | `{category}` | Baris kedua. Placeholder yang bisa dipakai: `{app}`, `{category}`, `{version}` |
| `LargeImageText` | `{app} {version}` | Teks saat kursor diarahkan ke icon, contohnya "FilmCraft v0.21.0" |
| `StatusDisplay` | `Name` | Teks setelah "Playing" di member list: `Name`, `Details`, atau `State` |
| `ShowElapsedTime` | `true` | Tampilkan lama pemakaian |
| `KeepTimerAcrossApps` | `false` | `true` = timer tidak di-reset saat pindah aplikasi Craft |
| `SecretMode` / `SecretModeText` | `false` | Sembunyikan nama file. Bisa juga di-toggle dari tray |
| `ShowWebsiteButton` | `true` | Tombol "Get ArtCraft" |
| `DisablePopup` | `false` | Matikan notifikasi saat program start |
| `OpenOnStartup` | `true` | Jalan otomatis saat Windows start |
| `Debug` | `false` | Tulis log ke `log.txt` |

### `apps.yml`

Berisi daftar aplikasi yang dideteksi. Anda bisa menonaktifkan aplikasi tertentu (`Enabled: false`), mengganti nama yang tampil, atau menambah aplikasi Craft baru:

```yaml
- Name: FilmCraft
  ProcessName: filmcraft        # nama .exe tanpa .exe
  Folder: FilmCraft             # folder di InstallRoot
  Category: Video and effects
  ImageKey: filmcraft           # dipakai saat ImageSource: AssetKey
  IconUrl: https://getartcraft.com/images/apps/filmcraft/icon.webp
  ApplicationId: ''             # opsional, Application ID khusus FilmCraft
  Enabled: true
```

## Kalau icon tidak muncul di Discord

Mode `Url` memakai gambar langsung dari getartcraft.com. Kalau Discord tidak menampilkannya, pakai mode upload:

1. Di Developer Portal, buka application Anda, lalu **Rich Presence → Art Assets**.
2. Upload semua PNG dari folder `icons/`. Nama file menjadi key-nya, contohnya `filmcraft`, `photocraft`, `artcraft`.
   Kalau Anda memakai application terpisah per aplikasi, upload icon yang sesuai ke masing-masing application.
3. Di `settings.yml`, ubah ke `ImageSource: AssetKey`, lalu pilih Restart.

> Asset yang baru di-upload bisa butuh beberapa menit sebelum muncul.

## Troubleshooting

| Gejala | Solusi |
|---|---|
| Tray menulis "Application ID belum diisi" | Isi `DefaultApplicationId`, lalu Restart |
| Muncul notifikasi "Application ID tidak valid" | ID salah salin. Salin ulang dari halaman General Information |
| "Discord tidak terhubung" | Buka aplikasi Discord desktop. Versi browser tidak mendukung RPC |
| Status tidak muncul sama sekali | Cek Activity Privacy di Discord. Aktifkan `Debug: true`, lalu lihat `log.txt` |
| Nama file tidak terbaca | Aplikasi tersebut mungkin belum menaruh nama file di judul jendela. Yang tampil adalah `IdleText` |

## Build dari source

Butuh [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0).

```bat
tools\build.bat
```

Hasilnya `publish\ArtCraftPresence.exe`, berupa satu file self-contained.

Test:

```bat
dotnet test
```

## Struktur project

```
ArtCraftPresence/
├─ App/
│  ├─ Program.cs               entry point, single-instance, first-run setup
│  ├─ AppSettings.cs           isi settings.yml
│  ├─ CraftApp.cs              model + daftar 16 aplikasi ArtCraft
│  ├─ WindowTitleParser.cs     ambil nama file dari judul jendela
│  ├─ NativeWindow.cs          Win32: judul jendela & jendela aktif
│  └─ Services/
│     ├─ AppDetector.cs        cari aplikasi Craft yang sedang berjalan
│     ├─ DiscordManager.cs     kirim Rich Presence ke Discord
│     ├─ SettingsManager.cs    baca/tulis settings.yml & apps.yml
│     ├─ TrayManager.cs        icon & menu system tray
│     ├─ StartupManager.cs     start with Windows (registry HKCU\...\Run)
│     └─ Logger.cs
├─ App.Tests/                  unit test (xUnit)
├─ icons/                      icon PNG 512×512 untuk upload ke Discord
└─ tools/build.bat
```

Uninstall dari startup: `ArtCraftPresence.exe /uninstall`.

## Rilis versi baru

Rilis dibuat otomatis oleh GitHub Actions setiap kali tag `v*` di-push:

```bash
git tag v1.0.1
git push origin v1.0.1
```

## Lisensi

[GPL-3.0](LICENSE.md), sama seperti DAWPresence yang menjadi dasar proyek ini.

## Kredit

- Konsep dan struktur dari [DAWPresence](https://github.com/Myuuiii/DAWPresence) oleh Myuuiii.
- Icon aplikasi © ArtCraft ([getartcraft.com](https://getartcraft.com/)).
- [DiscordRichPresence](https://github.com/Lachee/discord-rpc-csharp) dan [YamlDotNet](https://github.com/aaubry/YamlDotNet).
