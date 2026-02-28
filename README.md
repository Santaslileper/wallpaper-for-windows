<div align="center">

# 🖥️ Wallpaper for Windows
### *Set any webpage as your live desktop wallpaper.*

[![Platform](https://img.shields.io/badge/platform-Windows-blue?logo=windows)](https://github.com/Santaslileper/wallpaper-for-windows)
[![Language](https://img.shields.io/badge/language-C%23-239120?logo=csharp)](https://github.com/Santaslileper/wallpaper-for-windows)
[![Renderer](https://img.shields.io/badge/renderer-Rust-orange?logo=rust)](https://github.com/Santaslileper/wallpaper-for-windows)
[![License](https://img.shields.io/badge/license-Personal--Use--Only-red)](LICENSE)
[![Status](https://img.shields.io/badge/status-active-brightgreen)](https://github.com/Santaslileper/wallpaper-for-windows)

**Wallpaper for Windows** is a lightweight live wallpaper engine that renders any HTML file directly on your desktop — animated clocks, particle systems, quote carousels, 3D scenes, anything a browser can run.

[**Download Latest Release**](https://github.com/Santaslileper/wallpaper-for-windows/releases/latest) • [**Source Code**](src/WallpaperManager.cs) • [**Report a Bug**](../../issues)

---

</div>

## 💡 Why This Exists

Windows has no native live wallpaper support. Third-party solutions are bloated, paid, or require constant internet access. This engine is:

- **Entirely local** — no internet required (except CDN-dependent wallpapers)
- **Zero install** — just run the standalone `.exe` file, everything runs locally.
- **Fully open** — HTML wallpapers are plain files you can read, edit, and create yourself
- **Single Executable** — Written entirely in C# to ship as one `WallpaperManager.exe`

### ✨ Key Features
- **🎨 HTML Wallpapers:** Any `.html` file becomes a live, animated desktop background
- **🖥️ Multi-Monitor:** Assign different wallpapers per monitor independently
- **⚡ Rust Renderer:** Lightning fast, embedded Chromium-based window
- **🔄 Auto-Restore:** Remembers your last wallpapers and restores them on startup
- **🕹️ Game Mode:** Automatically hides wallpapers when a fullscreen app is detected
- **📦 Portable:** No installer, no registry changes beyond optional autostart

---

## ⚡ Quick Start

Download and launch the engine directly from your terminal:

```powershell
[Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12
$d = "$env:USERPROFILE\Desktop\wallpaper-for-windows"
irm https://github.com/Santaslileper/wallpaper-for-windows/archive/refs/heads/main.zip -OutFile "$env:TEMP\wp.zip"
Expand-Archive "$env:TEMP\wp.zip" -DestinationPath $d -Force
Start-Process "$d\wallpaper-for-windows-main\WallpaperManager.exe"
```

Or manually:
1. Download the latest `wallpaper-for-windows` zip from the releases page.
2. Extract the folder to your `Desktop` or `Documents`.
3. Double-click `WallpaperManager.exe`.

> [!NOTE]
> **What does this do?** It launches the manager. No installer, no admin rights, nothing hidden.

> [!TIP]
> After launching, a small **control panel** appears. Pick a wallpaper from the dropdown, choose your monitor, and hit **LAUNCH**. Look for a running wallpaper window to appear behind your icons.

---

## 🎨 Bundled Wallpapers

| File | Description |
| :--- | :--- |
| `default.html` | Animated gradient + live clock + floating particles |
| `temporal-flux.html` | Cyberpunk HUD — clock, 3D cube, quote carousel |
| `memento-vivere.html` | Philosophical dark wallpaper — quote rotation |

---

## 🚀 Releases & Fast Info

| Item | Details |
| :--- | :--- |
| **Engine** | Single C# WinForms executable (`WallpaperManager.exe`) |
| **Renderer** | Rust (`wall-renderer.exe`) — embedded Chromium-based window |
| **Requirement** | Windows 10 / 11, .NET Framework 4.0+ |
| **Install Type** | Portable — no installer, no admin rights needed |
| **Autostart** | Optional — toggle in Settings panel inside the app |

---

## ⚙️ How It Works

1. **Manager** (`WallpaperManager.exe`) provides a dark-mode GUI to browse and launch wallpapers.
2. **Renderer** (`wall-renderer.exe`) opens a borderless Chromium window and places it behind desktop icons using Windows `SetParent` API.
3. **Wallpaper HTML** runs in the renderer — full JS, CSS animations, Canvas, WebGL.
4. **Game Mode** polls the foreground window every second; if a fullscreen app is detected on a monitor, the wallpaper on that monitor is hidden to save GPU.

---

## 📁 Project Structure

```
wallpaper-for-windows/
├── WallpaperManager.exe         ← Main app (double-click to start)
├── assets/
│   ├── icon.ico                 ← App icon
│   ├── data/                    ← settings.json (auto-created, gitignored)
│   └── wallpapers/
│       ├── default.html
│       ├── temporal-flux.html
│       └── memento-vivere.html
└── tools/
    └── wall-renderer/
        └── bin/
            └── wall-renderer.exe ← Pre-built Rust renderer
```

---

## ⚖️ License & Privacy

- **License:** Personal Use Only. Non-commercial use permitted. No redistribution without written consent.
- **Privacy:** 100% offline. No telemetry, no network calls (except CDN fonts/libs used by individual wallpapers). All settings and memory states stay strictly on your local machine.

---

<div align="center">
Created with ❤️ by <b>Santaslileper</b>
</div>
