<div align="center">

<img src="Assets/Mercury.png" alt="Mercury Logo" width="150" height="150">

# Mercury Music Player

**A modern, Windows-native music player that streams music from YouTube**

[![.NET](https://img.shields.io/badge/.NET-6.0+-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![WPF UI](https://img.shields.io/badge/WPF_UI-Fluent_Design-0078D4?style=for-the-badge&logo=windows&logoColor=white)](https://github.com/lepoco/wpfui)
[![License](https://img.shields.io/badge/License-MIT-green?style=for-the-badge)](LICENSE)

[Features](#-features) • [Installation](#-installation) • [Usage](#-usage) • [Architecture](#%EF%B8%8F-architecture) • [Contributing](#-contributing)

</div>

---

## ✨ Features

- 🎵 **YouTube Music Streaming** — Search and stream music directly from YouTube
- 🎨 **Fluent Design** — Beautiful Windows 11 Mica backdrop with WPF UI library
- 🎛️ **System Integration** — Full Windows System Media Transport Controls (SMTC) support
- 📋 **Queue Management** — Build and manage your playback queue
- 🔁 **Repeat Modes** — No repeat, repeat single, or repeat all
- 🔊 **Volume Control** — Intuitive volume slider with mute toggle
- 🖼️ **Album Art** — Automatic thumbnail downloading and display
- 📌 **System Tray** — Minimize to tray and control playback from anywhere
- ⌨️ **Keyboard Shortcuts** — Quick controls without leaving your keyboard
- 🔍 **Advanced Search** — Filter by Songs, Videos, Albums, Artists, or Playlists

## 📸 Screenshots

*Coming soon...*

## 🚀 Installation

### Prerequisites

- Windows 10/11
- [.NET 6.0 Runtime](https://dotnet.microsoft.com/download/dotnet/6.0) or higher
- [VLC Media Player](https://www.videolan.org/vlc/) (required for LibVLC)

### Building from Source

1. **Clone the repository:**

```bash
git clone https://github.com/Aengstlicher1/Mercury.git
cd Mercury
```

2. **Restore NuGet packages:**

```bash
dotnet restore
```

3. **Build the project:**

```bash
dotnet build --configuration Release
```

4. **Run the application:**

```bash
dotnet run
```

## 🎮 Usage

### Searching for Music

1. Type your search query in the search bar at the top
2. Select a filter (Songs, Videos, Albums, Artists, Playlists) from the dropdown
3. Press Enter or wait for results to appear
4. Click on a song to start playing

### Playback Controls

| Control | Action |
|---------|--------|
| **Play/Pause** | Toggle playback |
| **Previous** | Go to previous song in queue |
| **Next** | Skip to next song in queue |
| **Repeat** | Cycle through repeat modes |
| **Volume** | Adjust volume (0-100%) |

### Keyboard Shortcuts

| Key | Action |
|-----|--------|
| `Space` | Play/Pause (when not in text input) |

### System Tray

- **Double-click** the tray icon to restore the window
- **Right-click** for context menu (Show, Exit, Settings)

## 🏗️ Architecture

Mercury follows the **MVVM (Model-View-ViewModel)** pattern with a service-based architecture.

### Project Structure

```
Mercury/
├── App.xaml / App.xaml.cs       # Application entry point, DI setup
├── MainWindow.xaml              # Main application window
├── Models/
│   ├── Song.cs                  # Song data model
│   ├── Converters/              # Value converters for XAML bindings
│   └── Messages/                # Messenger message classes
├── ViewModels/
│   ├── MainWindowModel.cs       # Main window view model
│   └── Pages/                   # Page-specific view models
├── Views/
│   └── Pages/                   # User control pages
├── Services/
│   ├── IMediaPlayerService.cs   # Media playback interface
│   ├── MediaPlayerService.cs    # LibVLC implementation
│   ├── ISearchService.cs        # Search functionality interface
│   ├── SearchService.cs         # YouTube search implementation
│   ├── IAppService.cs           # App lifecycle interface
│   └── AppService.cs            # Window management
└── Assets/                      # Icons, images, resources
```

### Technology Stack

| Component | Technology |
|-----------|------------|
| **Framework** | .NET 6.0+ |
| **UI Framework** | WPF (Windows Presentation Foundation) |
| **UI Library** | [WPF UI](https://github.com/lepoco/wpfui) (Fluent Design) |
| **Media Playback** | [LibVLCSharp](https://github.com/videolan/libvlcsharp) |
| **MVVM Toolkit** | [CommunityToolkit.Mvvm](https://github.com/CommunityToolkit/dotnet) |
| **YouTube Integration** | Custom YouTubeApi library |
| **Dependency Injection** | Microsoft.Extensions.DependencyInjection |

### Core Services

- **MediaPlayerService** — Manages LibVLC playback, queue, SMTC integration, and thumbnail caching
- **SearchService** — Handles YouTube Music search queries and filter state
- **AppService** — Controls application lifecycle and window state
- **NavigationService** — Manages page navigation within the app

## 🤝 Contributing

Contributions are welcome! Here's how you can help:

1. **Fork** the repository
2. **Create** a feature branch (`git checkout -b feature/amazing-feature`)
3. **Commit** your changes (`git commit -m 'Add amazing feature'`)
4. **Push** to the branch (`git push origin feature/amazing-feature`)
5. **Open** a Pull Request

### Development Guidelines

- Follow MVVM pattern — keep business logic out of code-behind
- Use `[ObservableProperty]` and `[RelayCommand]` from CommunityToolkit.Mvvm
- Use `DynamicResource` for colors/brushes to support theming
- Always use async/await for I/O operations
- Register new services in `App.xaml.cs`

## 📄 License

This project is licensed under the MIT License — see the [LICENSE](LICENSE) file for details.

## 🙏 Acknowledgments

- [WPF UI](https://github.com/lepoco/wpfui) — Beautiful Fluent Design components
- [LibVLCSharp](https://github.com/videolan/libvlcsharp) — Powerful media playback engine
- [CommunityToolkit.Mvvm](https://github.com/CommunityToolkit/dotnet) — Excellent MVVM helpers
- [VLC Media Player](https://www.videolan.org/) — The backbone of audio streaming

---

<div align="center">

**Made with ❤️ for music lovers**

⭐ Star this repo if you find it useful!

</div>