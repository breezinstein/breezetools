# com.breezinstein.tools Documentation

## Table of Contents

1. [Package Overview](#1-package-overview)
2. [Class Hierarchy and Inheritance Relationships](#2-class-hierarchy-and-inheritance-relationships)
3. [Audio System](#3-audio-system)
4. [UI System](#4-ui-system)
5. [NameGen System](#5-namegen-system)
6. [Utility Components](#6-utility-components)
7. [Integration Examples](#7-integration-examples)
8. [Extending the Package](#8-extending-the-package)
9. [Error Handling and Troubleshooting](#9-error-handling-and-troubleshooting)
10. [Version History and Future Development](#10-version-history-and-future-development)
11. [Additional Resources](#11-additional-resources)

---

## 1. Package Overview

### 1.1 Introduction

**Breeze's Tools** is a comprehensive collection of helper tools designed to accelerate game development in Unity. The package provides a set of modular, reusable components that address common development needs across audio management, UI systems, procedural name generation, and general utilities.

**Version**: 0.0.5  
**Unity Compatibility**: 2022.1 or higher  
**License**: See LICENSE.md file for details

### 1.2 Package Architecture

The package is organized into several core modules:

- **Audio**: A complete audio management system with mixer integration
- **UI**: Window management and UI component utilities
- **NameGen**: Procedural name generation tools
- **Utilities**: General purpose helper classes and extensions

All components are contained within the `Breezinstein.Tools` namespace, with sub-namespaces for specific modules (e.g., `Breezinstein.Tools.Audio`).

The package follows several design patterns:
- **Singleton Pattern**: Used for manager classes that need global access
- **ScriptableObject-based Architecture**: Used for configuration and data management
- **Event-driven Communication**: Used for decoupled component interaction

### 1.3 Installation and Setup

#### Installation via Unity Package Manager

1. Open the Unity Package Manager (Window > Package Manager)
2. Click the "+" button and select "Add package from git URL..."
3. Enter the repository URL: `https://github.com/yourusername/com.breezinstein.tools.git`
4. Click "Add"

#### Manual Installation

1. Download the latest release from the repository
2. Extract the contents into your project's `Packages` directory
3. The package should be automatically detected by Unity

#### Dependencies

- Newtonsoft.Json (2.0.0 or higher) - Automatically installed via the Package Manager

### 1.4 Quick Start Guide

Here's how to quickly get started with the main components of the package:

#### Audio System Setup

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

#### UI Window System Setup

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

#### Name Generation

```csharp
// Generate random names
string singleName = NameGen.GenerateRandomSingleName;
string doubleName = NameGen.GenerateRandomDoubleName;
string username = NameGen.GenerateRandomUsername;
```

---

## 2. Class Hierarchy and Inheritance Relationships

### 2.1 Complete Class Diagram

The package includes the following key classes and their relationships:

- **AudioManager**: Inherits from Singleton<AudioManager>, manages audio playback and settings
- **AudioLibrary**: ScriptableObject for storing audio clips
- **AudioSettings**: Handles audio configuration and persistence
- **WindowManager**: Inherits from Singleton<WindowManager>, manages UI windows
- **Window**: Base class for UI windows
- **NameGen**: Static utility for name generation
- **Singleton<T>**: Generic singleton implementation
- **EventManager<T>**: Generic event system
- **SerializableDictionary<TKey, TValue>**: Implements ISerializationCallbackReceiver and IDictionary

### 2.2 Core Components

#### Audio System
- `AudioManager`: Central manager for all audio functionality
- `AudioLibrary`: ScriptableObject for storing audio clips
- `AudioSettings`: Handles audio configuration and persistence
- `AudioSlider`: UI component for controlling audio volume
- `AudioUIToggle`: UI component for toggling audio sources

#### UI System
- `WindowManager`: Manages UI windows and navigation
- `Window`: Base class for UI windows
- `FlexibleGridLayout`: Advanced grid layout component
- `RadialLayoutGroup`: Radial layout component
- `TabGroup` and `TabButton`: Tab-based UI navigation
- `Notification`: UI notification system
- `MessaageBox`: Dialog box system

#### NameGen System
- `NameGen`: Static utility for generating random names

#### Utility Components
- `BreezeHelper`: General utility functions
- `Singleton<T>`: Generic singleton implementation
- `EventManager<T>`: Generic event system
- `SerializableDictionary<TKey, TValue>`: Serializable dictionary implementation
- `SafeArea`: Handles safe area for different device screens
- `ChangeImageColor`: Utility for dynamic image coloring

---

## 3. Audio System

### 3.1 Architecture Overview

The audio system provides a complete solution for managing audio in Unity games. It integrates with Unity's Audio Mixer system and provides tools for volume control, audio playback, and settings persistence.

### 3.2 AudioManager

The `AudioManager` is the central component of the audio system. It manages audio playback, volume control, and settings persistence.

#### Key Properties and Methods

```csharp
// Properties
public static AudioSettings Settings { get; set; }
public AudioLibrary AudioLibrary;

// Methods
public static void PlaySoundEffect(string clipName)
public static void PlayMusic(string clipName, bool restart = true)
public static void PlayRandomMusic()
public static void StopMusic()
public void UpdateVolumes()
public void SetVolume(AudioSourceType sourceType, float volume)
public void ToggleSource(AudioSourceType sourceType)
```

#### Implementation Example

```csharp
// Setup
GameObject audioManagerObj = new GameObject("Audio Manager");
AudioManager audioManager = audioManagerObj.AddComponent<AudioManager>();

// Assign an AudioLibrary asset
audioManager.AudioLibrary = Resources.Load<AudioLibrary>("MyAudioLibrary");

// Play a sound effect
AudioManager.PlaySoundEffect("explosion");

// Play background music
AudioManager.PlayMusic("background_theme");

// Control volume
AudioManager.Instance.SetVolume(AudioManager.AudioSourceType.MUSIC, 0.5f);
```

### 3.3 AudioLibrary

The `AudioLibrary` is a ScriptableObject that stores audio clips and their metadata.

#### Key Properties

```csharp
public SerializableDictionary<string, AudioItem> clips;
```

#### AudioItem Class

```csharp
[System.Serializable]
public class AudioItem
{
    public AudioCategory category;
    public AudioClip clip;
    [Range(0.0f, 1.0f)]
    public float volume = 1f;
}
```

### 3.4 AudioSettings

The `AudioSettings` class manages audio configuration and persistence.

#### Key Properties and Methods

```csharp
// Properties
public bool MusicEnabled { get; set; }
public bool EffectsEnabled { get; set; }
public bool MainEnabled { get; set; }
public float MusicVolume { get; set; }
public float EffectsVolume { get; set; }
public float MainVolume { get; set; }

// Methods
public static AudioSettings Load()
public static void Save(AudioSettings settings)
public void Reset()
```

---

## 4. UI System

### 4.1 Architecture Overview

The UI system provides a comprehensive solution for managing UI windows, navigation, and animations in Unity games.

### 4.2 WindowManager

The `WindowManager` is the central component of the UI system. It manages window navigation, animations, and UI state.

#### Key Properties and Methods

```csharp
// Properties
public static WindowManager Instance;
public int defaultWindow;
public List<Window> windows;
public Stack<int> navList;

// Methods
public void OpenWindow(int windowID)
public void OpenWindow(string windowName)
public void CloseWindow(int windowID)
public void OpenPreviousWindow()
public void ShowNotification(string message, float duration)
public void OpenMessageBox(MessageTemplate messageInfo)
```

### 4.3 Window Class

The `Window` class is the base class for all UI windows. It provides functionality for animations, navigation, and window management.

#### Key Properties and Methods

```csharp
// Properties
public int ID;
public string windowName;

// Methods
public void PlayAnimation(string clipName)
public void Close()
public virtual void HandleBackButton()
```

### 4.4 Layout Components

- **FlexibleGridLayout**: Advanced grid layout with flexible sizing options
- **RadialLayoutGroup**: Arranges UI elements in a circular pattern

### 4.5 UI Utilities

- **Notification System**: Displays temporary messages to the user
- **Message Box System**: Displays dialog boxes with customizable content and buttons
- **Tab System**: Provides tab-based navigation

---

## 5. NameGen System

### 5.1 Architecture Overview

The NameGen system provides tools for generating random names from predefined lists.

### 5.2 NameGen Class

The `NameGen` class is a static utility for generating random names.

#### Key Properties and Methods

```csharp
// Properties
public static string GenerateRandomSingleName { get; }
public static string GenerateRandomDoubleName { get; }
public static string GenerateRandomUsername { get; }
public static string GenerateRandomDoubleUsername { get; }

// Private Methods
private static void LoadTextAssets()
private static string LatinToAscii(string inString)
```

### 5.3 Customization

Name lists are stored as text files in the `Resources/namelists` folder. Each line in the file represents a name.

---

## 6. Utility Components

### 6.1 BreezeHelper

The `BreezeHelper` class provides general utility functions for serialization, file operations, and string manipulation.

#### Key Methods

```csharp
// Serialization
public static string Serialize<T>(this T toSerialize)
public static T Deserialize<T>(this string toDeSerialize)

// File Operations
public static bool FileExists(string fileName)
public static string LoadFile(string fileName)
public static void SaveFile(string fileName, string fileContent)

// Utilities
public static int Fib(int n)
public static string RemoveSpecialCharacters(string str)
public static string Md5Sum(string strToEncrypt)
```

### 6.2 Singleton Pattern

The `Singleton<T>` class provides a generic implementation of the singleton pattern.

#### Key Properties and Methods

```csharp
// Properties
public static T Instance { get; }

// Methods
protected virtual void Awake()
protected virtual void OnApplicationQuit()
```

### 6.3 EventManager

The `EventManager<T>` class provides a generic event system for decoupled communication between components.

#### Key Methods

```csharp
public static void Register(string eventName, Action<T> method)
public static void Unregister(string eventName, Action<T> method)
public static void Trigger(string eventName, T payload)
```

### 6.4 SerializableDictionary

The `SerializableDictionary<TKey, TValue>` class provides a Unity-serializable dictionary implementation.

#### Key Properties and Methods

```csharp
// Properties
public TValue this[TKey key] { get; set; }
public ICollection<TKey> Keys { get; }
public ICollection<TValue> Values { get; }

// Methods
public void Add(TKey key, TValue value)
public bool Remove(TKey key)
public bool ContainsKey(TKey key)
public bool TryGetValue(TKey key, out TValue value)
```

---

## 7. Integration Examples

### 7.1 Unity Built-in Systems

#### Integration with Unity's UI System

```csharp
// Create a UI canvas
GameObject canvasObj = new GameObject("Canvas");
Canvas canvas = canvasObj.AddComponent<Canvas>();
canvas.renderMode = RenderMode.ScreenSpaceOverlay;
canvasObj.AddComponent<CanvasScaler>();
canvasObj.AddComponent<GraphicRaycaster>();

// Create a window manager
GameObject windowManagerObj = new GameObject("Window Manager");
windowManagerObj.transform.SetParent(canvasObj.transform);
WindowManager windowManager = windowManagerObj.AddComponent<WindowManager>();
```

#### Integration with Unity's Audio System

```csharp
// Create an audio mixer
// (Window > Audio > Audio Mixer)

// Create an audio manager
GameObject audioManagerObj = new GameObject("Audio Manager");
AudioManager audioManager = audioManagerObj.AddComponent<AudioManager>();

// Assign the audio mixer
audioManager.m_AudioMixer = Resources.Load<AudioMixer>("MainMixer");
```

### 7.2 Third-Party Assets

#### Integration with TextMeshPro

```csharp
// Add TextMeshPro to your project
// (Window > Package Manager > TextMeshPro)

// Create a UI window with TextMeshPro elements
GameObject windowObj = new GameObject("Window");
Window window = windowObj.AddComponent<Window>();
window.ID = 1;
window.windowName = "TextWindow";

// Add TextMeshPro text
GameObject textObj = new GameObject("Text");
textObj.transform.SetParent(windowObj.transform);
TMPro.TextMeshProUGUI text = textObj.AddComponent<TMPro.TextMeshProUGUI>();
text.text = "Hello, World!";
text.fontSize = 36;
text.alignment = TMPro.TextAlignmentOptions.Center;
```

---

## 8. Extending the Package

### 8.1 Extension Patterns

- Create custom window types by inheriting from `Window`
- Extend the audio system with custom behaviors
- Create custom event types for the `EventManager`
- Extend `SerializableDictionary` for custom data types

### 8.2 Advanced Customization

- Create custom UI components that integrate with the window system
- Implement specialized audio behaviors like fading or positional audio
- Create custom event systems for specific game events

---

## 9. Error Handling and Troubleshooting

### 9.1 Common Issues

- Missing references to audio clips or UI elements
- Incorrect window IDs or names
- Audio mixer group configuration issues
- Serialization errors with complex data types

### 9.2 Best Practices

- Use descriptive names for audio clips and UI elements
- Initialize managers in a consistent order
- Implement proper error handling for file operations
- Test on multiple devices for UI layout issues

---

## 10. Version History and Future Development

### 10.1 Version History

- **0.0.5**: Current version with basic functionality
- Future versions will include additional features and improvements

### 10.2 Roadmap

- Enhanced audio visualization tools
- Additional UI components and layouts
- Improved documentation and examples
- Performance optimizations

---

## 11. Additional Resources

### 11.1 Support Channels

- GitHub repository: [https://github.com/yourusername/com.breezinstein.tools](https://github.com/yourusername/com.breezinstein.tools)
- Documentation: [https://example.com/](https://example.com/)
- Issue tracker: [https://github.com/yourusername/com.breezinstein.tools/issues](https://github.com/yourusername/com.breezinstein.tools/issues)

### 11.2 Related Projects

- Unity UI Extensions: [https://github.com/Unity-UI-Extensions/com.unity.uiextensions](https://github.com/Unity-UI-Extensions/com.unity.uiextensions)
- DOTween: [http://dotween.demigiant.com/](http://dotween.demigiant.com/)
- TextMeshPro: [https://docs.unity3d.com/Manual/com.unity.textmeshpro.html](https://docs.unity3d.com/Manual/com.unity.textmeshpro.html)
