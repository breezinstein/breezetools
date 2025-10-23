# Icon Generator Tool

A powerful Unity Editor tool for generating item icon sprites from prefabs with extensive customization options.

## Features

### Core Functionality
- **Batch Icon Generation**: Generate icons for multiple prefabs at once
- **Drag and Drop Support**: Easily add prefabs by dragging them into the tool window
- **Object Picker**: Add prefabs through Unity's standard object picker interface
- **Progress Feedback**: Real-time progress bar during batch generation
- **Error Handling**: Robust error handling with console warnings for invalid prefabs

### Camera Settings
- **Orthographic Rendering**: Uses orthographic camera for consistent icon rendering
- **Automatic Bounds Calculation**: Automatically frames the prefab based on renderer bounds
- **Configurable Rotation**: Set custom camera rotation angles (X, Y, Z)
- **Adjustable Distance**: Control camera distance from the object
- **Zoom Control**: Fine-tune the icon framing with zoom settings
- **Background Options**: Choose between transparent or solid color backgrounds

### Output Options
- **Multiple Resolution Presets**: 64x64, 128x128, 256x256, 512x512
- **Custom Resolution**: Define your own width and height
- **Flexible Path Modes**:
  - **Mirror Prefab Path**: Save icons in the same folder as the prefab
  - **Custom Folder**: Save all icons to a specified folder
  - **Prefab Folder with Subfolder**: Create an "Icons" subfolder next to prefabs
- **Subfolder Organization**: Optionally create subfolders to organize icons by category
- **Customizable Suffix**: Add a custom suffix to icon filenames (default: "_Icon")

### Post-Processing
- **Auto Asset Refresh**: Automatically refresh the AssetDatabase after generation
- **UI Sprite Configuration**: Automatically configure generated sprites for UI usage
- **Import Settings**: Configure filter mode, compression, and max texture size
- **Transparency Support**: Full alpha channel support for transparent backgrounds

## How to Use

### Opening the Tool
1. In Unity, go to **Tools → BreezeTools → Icon Generator**
2. The Icon Generator window will open

### Adding Prefabs
There are two ways to add prefabs:

**Method 1: Drag and Drop**
1. Drag one or more prefab assets from the Project window
2. Drop them into the "Drag and drop prefabs here" area

**Method 2: Add Button**
1. Click the "Add Prefab" button
2. Select a prefab from the object picker dialog
3. Click to add it to the list

### Configuring Settings

#### Icon Resolution
- Choose from preset resolutions (64x64, 128x128, 256x256, 512x512)
- Select "Custom" to define your own resolution

#### Camera Settings
- **Transparent Background**: Enable for PNG with transparency
- **Background Color**: Choose a solid color (when transparency is disabled)
- **Camera Distance**: Adjust how far the camera is from the object (1-20)
- **Camera Rotation**: Set the camera angles (X, Y, Z in degrees)
- **Camera Zoom**: Fine-tune the framing (0.1-3.0)

#### Output Settings
- **Output Path Mode**: Choose how to organize output files
  - *Mirror Prefab Path*: Icons saved in the same folder as prefabs
  - *Custom Folder*: All icons saved to a specific folder
  - *Prefab Folder with Subfolder*: Icons saved in an "Icons" subfolder
- **Custom Output Folder**: Specify the folder path (when using Custom Folder mode)
- **Icon Suffix**: Text to append to icon filenames (e.g., "_Icon")
- **Create Icons Subfolder**: Auto-create an "Icons" subfolder

#### Post-Processing
- **Auto Refresh AssetDatabase**: Refresh Unity's asset database after generation
- **Configure as UI Sprite**: Automatically set sprite import settings for UI usage

### Generating Icons
1. After adding prefabs and configuring settings, click **Generate Icons**
2. A progress bar will show the generation status
3. Generated icons will be saved to the configured output location
4. A success dialog will appear when complete

### Managing the Prefab List
- **Remove Individual Prefabs**: Click the "X" button next to a prefab name
- **Clear All**: Click "Clear All" to remove all prefabs from the list

## Technical Details

### How It Works
1. **Instantiation**: Each prefab is instantiated in an isolated position (far from scene origin)
2. **Bounds Calculation**: All renderers are analyzed to calculate combined bounds
3. **Camera Positioning**: An orthographic camera is positioned and rotated to capture the prefab
4. **Rendering**: The prefab is rendered to a RenderTexture with anti-aliasing
5. **Conversion**: RenderTexture is converted to Texture2D
6. **Encoding**: Texture2D is encoded as PNG with transparency support
7. **Saving**: PNG is saved to the configured output path
8. **Import Configuration**: Sprite import settings are automatically configured
9. **Cleanup**: Instantiated objects are destroyed

### Output Path Examples

**Mirror Prefab Path Mode**
- Prefab: `Assets/Items/Weapons/Sword.prefab`
- Icon: `Assets/Items/Weapons/Icons/Sword_Icon.png` (with subfolder)
- Icon: `Assets/Items/Weapons/Sword_Icon.png` (without subfolder)

**Custom Folder Mode**
- Custom Folder: `Assets/GeneratedIcons`
- Prefab: `Assets/Items/Weapons/Sword.prefab`
- Icon: `Assets/GeneratedIcons/Weapons/Sword_Icon.png` (with subfolder)
- Icon: `Assets/GeneratedIcons/Sword_Icon.png` (without subfolder)

**Prefab Folder with Subfolder Mode**
- Prefab: `Assets/Items/Weapons/Sword.prefab`
- Icon: `Assets/Items/Weapons/Icons/Sword_Icon.png`

### Requirements
- Unity 2021.1 or newer (for UI Toolkit support)
- Prefabs must have at least one Renderer component

### Limitations
- Prefabs without renderers cannot be processed
- UI elements (Canvas, UI components) may not render correctly
- Particle systems will render in their default state

## Tips and Best Practices

1. **Camera Rotation**: A rotation of (15, -30, 0) provides a nice isometric-style view
2. **Resolution**: Use 256x256 for most UI icons, 512x512 for high-DPI displays
3. **Transparent Backgrounds**: Essential for UI icons to blend with any background
4. **Batch Processing**: Add all similar items at once for consistent settings
5. **Test Settings**: Generate a single icon first to verify camera and zoom settings
6. **Organization**: Use subfolder mode to keep icons organized

## Troubleshooting

**Problem**: Icon is too zoomed in/out
- **Solution**: Adjust the "Camera Zoom" slider

**Problem**: Object is off-center in the icon
- **Solution**: Adjust "Camera Distance" and "Camera Rotation"

**Problem**: "No renderers" warning
- **Solution**: Ensure the prefab has visible mesh renderers or sprite renderers

**Problem**: Background isn't transparent
- **Solution**: Enable "Transparent Background" and ensure "Configure as UI Sprite" is checked

**Problem**: Icons aren't appearing in the Project window
- **Solution**: Ensure "Auto Refresh AssetDatabase" is enabled, or manually refresh

## Architecture

### Files
- **IconGeneratorWindow.cs**: Main editor window with UI Toolkit interface
- **IconGenerator.cs**: Core icon generation logic
- **IconGeneratorSettings.cs**: Serializable settings data class

### Key Classes
- `IconGeneratorWindow`: EditorWindow that provides the UI
- `IconGenerator`: Handles camera setup, rendering, and file I/O
- `IconGeneratorSettings`: Contains all configuration options

## Future Enhancements
Potential features for future versions:
- Lighting presets for better object visualization
- Multiple camera angle generation (front, side, top views)
- Animation frame capture for animated prefabs
- Batch rename utilities
- Template saving/loading for settings
- Preview window before generation

## Support
For issues or feature requests, please contact the BreezeTools development team.
