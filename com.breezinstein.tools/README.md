# Breeze's Tools

A comprehensive collection of helper tools designed to accelerate game development in Unity. This package provides modular, reusable components that address common development needs across audio management, UI systems, procedural name generation, and general utilities.

![Version](https://img.shields.io/badge/version-0.0.5-blue.svg)
![Unity](https://img.shields.io/badge/unity-2022.1%2B-blue.svg)

## Features

### Audio System
- Complete audio management with Unity Audio Mixer integration
- Volume control and audio settings persistence
- Easy sound effect and music playback
- UI components for audio controls

### UI System
- Window management with navigation history
- Animation integration for smooth transitions
- Flexible layout components (grid, radial)
- Notification and message box systems
- Tab-based navigation

### Name Generation
- Procedural name generation from customizable lists
- Multiple name formats (single, double, username)
- Support for different cultural name lists

### Utility Components
- Serialization and file operation helpers
- Generic singleton implementation
- Event-driven communication system
- Serializable dictionary for Unity
- Safe area handling for different devices
- UI and image utilities

## Installation

### Via Unity Package Manager (Recommended)

1. Open the Unity Package Manager (Window > Package Manager)
2. Click the "+" button and select "Add package from git URL..."
3. Enter the repository URL: `https://github.com/yourusername/com.breezinstein.tools.git`
4. Click "Add"

### Manual Installation

1. Download the latest release from the repository
2. Extract the contents into your project's `Packages` directory
3. The package should be automatically detected by Unity

## Dependencies

- Newtonsoft.Json (2.0.0 or higher) - Automatically installed via the Package Manager

## Quick Start Guide

### Audio System

```csharp
// Create an AudioManager in your scene
GameObject audioManagerObj = new GameObject("Audio Manager");
AudioManager audioManager = audioManagerObj.AddComponent<AudioManager>();

// Create an AudioLibrary asset
// (Assets > Create > Breeze Tools > Audio > Create Audio Library)
// Assign it to the AudioManager

// Play sounds
AudioManager.PlaySoundEffect("explosion");
AudioManager.PlayMusic("background_theme");
```

### UI Window System

```csharp
// Create a WindowManager in your scene
GameObject windowManagerObj = new GameObject("Window Manager");
WindowManager windowManager = windowManagerObj.AddComponent<WindowManager>();

// Create windows as children of the WindowManager
// Each window should have the Window component attached

// Open windows by ID or name
WindowManager.Instance.OpenWindow(1);
WindowManager.Instance.OpenWindow("MainMenu");
```

### Name Generation

```csharp
// Generate random names
string singleName = NameGen.GenerateRandomSingleName;
string doubleName = NameGen.GenerateRandomDoubleName;
string username = NameGen.GenerateRandomUsername;
```

### Utility Examples

```csharp
// Serialization
MyData data = new MyData();
string json = data.Serialize();
MyData loadedData = json.Deserialize<MyData>();

// File operations
BreezeHelper.SaveFile("save_data", json);
string loadedJson = BreezeHelper.LoadFile("save_data");

// Event system
EventManager<GameEvent>.Register("PlayerDied", OnPlayerDied);
EventManager<GameEvent>.Trigger("PlayerDied", new GameEvent("PlayerDied", playerPosition));
```


## Documentation

For detailed documentation, please refer to the [comprehensive documentation](com.breezinstein.tools-documentation.md) included with this package.

## Extending the Package

The package is designed to be extended in various ways:

- Create custom window types by inheriting from `Window`
- Extend the audio system with custom behaviors
- Create custom event types for the `EventManager`
- Extend `SerializableDictionary` for custom data types

## Roadmap

- Enhanced audio visualization tools
- Additional UI components and layouts
- Improved documentation and examples
- Performance optimizations

## License

See the [LICENSE.md](LICENSE.md) file for details.

## Support

- GitHub repository: [https://github.com/yourusername/com.breezinstein.tools](https://github.com/yourusername/com.breezinstein.tools)
- Documentation: [https://example.com/](https://example.com/)
- Issue tracker: [https://github.com/yourusername/com.breezinstein.tools/issues](https://github.com/yourusername/com.breezinstein.tools/issues)

## Related Projects

- Unity UI Extensions: [https://github.com/Unity-UI-Extensions/com.unity.uiextensions](https://github.com/Unity-UI-Extensions/com.unity.uiextensions)
- DOTween: [http://dotween.demigiant.com/](http://dotween.demigiant.com/)
- TextMeshPro: [https://docs.unity3d.com/Manual/com.unity.textmeshpro.html](https://docs.unity3d.com/Manual/com.unity.textmeshpro.html)