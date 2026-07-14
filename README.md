# QuickAudioSwitcher 🎧🔊

Quickly switch between audio output devices from the system tray.

![Windows](https://img.shields.io/badge/Windows%2011-✓-blue)
![.NET](https://img.shields.io/badge/.NET-9.0-purple)
![Release](https://img.shields.io/github/v/release/Gitromanus/QuickAudioSwitcher)

## Features

- 🔄 **Left-click** tray icon — cycle through active audio output devices
- 📋 **Right-click** — menu with all devices for quick selection
- 🎯 **System icons** — each device shows its native Windows icon
- ⌨️ **Hotkey** — `Ctrl+F12` (configurable in Settings)
- 🌍 **Multi-language** — English, Русский, Deutsch, Français, Español, Português, Italiano, 中文, 日本語, 한국어
- ⚙️ **Settings** — configure hotkey and language via the tray menu
- 🚀 **Auto-start** — toggle on/off from the tray menu

## Installation

1. [**Download QuickAudioSwitcher.exe**](https://github.com/Gitromanus/QuickAudioSwitcher/releases/latest/download/QuickAudioSwitcher.exe) (latest release)
2. Run it — the app appears in the system tray

> **Note:** SmartScreen may show a warning on first launch. Click "More info" → "Run anyway".

## Usage

| Action | Result |
|--------|--------|
| Left-click | Switch to next device |
| Right-click | Open device menu |
| `Ctrl+F12` | Switch to next device |
| Menu → Settings | Change hotkey or language |
| Menu → Auto-start | Toggle auto-start with Windows |
| Menu → Exit | Close the app |

## How It Works

The app uses [SoundVolumeView](https://www.nirsoft.net/utils/sound_volume_view.html) by NirSoft to switch the default audio device. Device icons are extracted from the Windows registry (`mmres.dll`).

## Build from Source

```bash
git clone https://github.com/Gitromanus/QuickAudioSwitcher.git
cd QuickAudioSwitcher
dotnet publish -c Release -r win-x64 --self-contained false -p:PublishSingleFile=true -o publish
```

## Requirements

- Windows 11 / Windows 10
- [.NET 9.0 Runtime](https://dotnet.microsoft.com/en-us/download/dotnet/9.0)

## License

MIT