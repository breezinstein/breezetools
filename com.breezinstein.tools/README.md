# Breeze's Tools v3.0 - Unity Editor Toolkit

A comprehensive Unity Editor extension built with UI Toolkit, providing essential utilities for streamlining object manipulation, scene management, and workflow optimization in Unity 6.2+.

![Version](https://img.shields.io/badge/version-3.0.0-blue.svg)
![Unity](https://img.shields.io/badge/unity-2022.3%2B%20%7C%206.2%2B-blue.svg)

## ✨ New in v3.0 - UI Toolkit Edition

This major release rebuilds the entire BreezeToolBox interface using Unity's modern UI Toolkit, providing:
- **Modern Interface**: Responsive, theme-aware UI built with UXML/USS
- **Enhanced Preview System**: Real-time visual feedback for all operations
- **Improved Performance**: Better integration with Unity 6.2+ systems
- **Extended Functionality**: New features for advanced scene management

## 🎯 Core Features

### Pivot Utilities
Transform object pivot points without affecting visual appearance:
- **Center Pivot**: Automatically centers pivot point within object bounds
- **Pivot to Base**: Moves pivot point to object bottom (perfect for props and characters)

### 🎲 Randomizer
Add controlled variation to selected objects with granular control:
- **Random Rotation**: Per-axis rotation randomization with custom ranges
- **Random Position**: Offset positions with configurable constraints
- **Random Scale**: Uniform or per-axis scaling with defined limits

### 📐 Distribute
Create organized object arrays with precision:
- **Linear Arrays**: X, Y, Z axis distribution
- **Radial Arrays**: Circular arrangements with custom radius and angles
- **Preview Mode**: Visualize arrangements before applying with hologram preview
- **Face Center**: Option for radial arrays to orient objects toward center

### 🌱 Populate (Advanced)
Intelligent scene population system:
- **Surface Detection**: Smart placement on detected surfaces
- **Placement Rules**: Density control, spacing constraints, normal alignment
- **Multi-Prefab Support**: Weighted distribution across multiple prefabs
- **Collision Avoidance**: Prevent overlapping with existing objects
- **Layer-Based Filtering**: Target specific surface types

### 🔄 Replace Selected
Batch replacement tools for selected objects:
- **Game Objects**: Swap with different prefabs while maintaining transforms
- **Fonts**: Update Text component fonts across selections
- **Materials**: Replace materials on Renderer components

### 🌍 Replace in Scene
Global replacement for entire scenes:
- **Font Replacement**: Find and replace all instances of specific fonts
- **Material Replacement**: Global material swapping with match detection

### 🔍 Missing Reference Helper
Diagnostic tools for scene health:
- **Missing Script Detection**: Identify broken component references
- **Prefab Connection Repair**: Fix broken prefab links
- **Detailed Reporting**: Clear visualization of issues found

## 🚀 Quick Start

### Installation
1. Add the package to your Unity project via Package Manager
2. Access via `Window > Breeze Tools > Toolbox v3`

### First Use
1. Open the toolbox window
2. Select objects in your scene
3. Choose a tool section (Randomizer, Distribute, etc.)
4. Configure settings and preview results
5. Apply changes with full undo support

## 🛠️ Technical Requirements

- **Unity Version**: 2022.3+ (Optimized for Unity 6.2+)
- **Dependencies**: 
  - UI Toolkit (UnityEngine.UIElementsModule)
  - Editor UI Elements (UnityEditor.UIElementsModule)
  - Newtonsoft JSON 2.0.0+

## 📖 Usage Examples

### Quick Object Randomization
```csharp
// Access via Window > Breeze Tools > Toolbox v3
// 1. Select objects in scene
// 2. Open Randomizer section
// 3. Configure rotation/position/scale ranges
// 4. Apply transformations with undo support
```

### Creating Radial Arrays
```csharp
// 1. Select objects to arrange
// 2. Open Distribute > Radial Array
// 3. Set radius, start/end angles
// 4. Enable preview mode to visualize
// 5. Apply when satisfied with arrangement
```

### Scene-Wide Material Updates
```csharp
// 1. Open Replace in Scene > Material
// 2. Assign source and target materials
// 3. Execute replacement across all scene objects
```

## 🎨 Customization

The tool uses USS styling for complete visual customization. Edit `BreezeToolBox.uss` to modify:
- Color schemes and theming
- Layout spacing and sizing
- Typography and font styles
- Interactive states and animations

## 🔧 Development & Extension

### Architecture
- **UXML**: Declarative UI layout (`BreezeToolBox.uxml`)
- **USS**: Styling and theming (`BreezeToolBox.uss`)
- **C#**: Logic and Unity integration (`BreezeToolBox.cs`)

### Extension Points
The modular design allows for easy feature additions:
- Add new sections to UXML
- Extend the main class with new functionality
- Customize styling via USS
- Integrate with existing editor workflows

## 📝 Complete Feature Set

In addition to the new BreezeToolBox v3.0, this package includes:

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

## 📝 Changelog

### v3.0.0 (Current) - BreezeToolBox UI Toolkit Edition
- **BREAKING**: Complete UI Toolkit rebuild for Unity 6.2+ compatibility
- **NEW**: Enhanced preview system with visual feedback
- **NEW**: Advanced populate system with surface detection
- **NEW**: Modern, responsive interface design
- **NEW**: Comprehensive missing reference detection
- **IMPROVED**: Better undo/redo integration
- **IMPROVED**: Performance optimizations
- **IMPROVED**: Accessibility and usability enhancements

### Previous Versions
- v2.0.0: Basic IMGUI implementation with core utilities
- v1.x: Initial release with fundamental features

## 🤝 Contributing

This tool is part of the Breeze Tools ecosystem. For feature requests or bug reports:
- Open issues on the repository
- Submit pull requests for improvements
- Share usage examples and workflows

## 📜 License

See [LICENSE.md](LICENSE.md) for licensing information.

## 🔗 Links

- **Documentation**: [Comprehensive Guide](Documentation~/BreezeToolBox_v3.md)
- **Repository**: GitHub repository link
- **Support**: Issue tracker and community support
- **Related Projects**: Unity UI Extensions, DOTween, TextMeshPro

---

**Developed by breezinstein | Twin Crown Studios**  
*Enhancing Unity workflows since 2023*