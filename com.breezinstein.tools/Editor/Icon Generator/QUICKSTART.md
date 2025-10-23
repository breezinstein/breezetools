# Icon Generator - Quick Start Guide

## Getting Started in 3 Steps

### Step 1: Open the Tool
- Go to **Tools → BreezeTools → Icon Generator** in the Unity menu bar
- The Icon Generator window will open (fully scrollable with collapsible sections)

### Step 2: Add Your Prefabs
Choose one method:
- **Drag & Drop**: Drag prefabs from your Project window into the drop area
- **Add Button**: Click "Add Prefab" and select from the object picker

### Step 3: Generate!
- Click the **Generate Icons** button
- Icons will be saved next to your prefabs in an "Icons" folder

## Default Settings
The tool comes preconfigured with sensible defaults:
- ✅ 256x256 resolution (perfect for UI)
- ✅ Transparent background
- ✅ Isometric camera angle (15°, -30°, 0°)
- ✅ Auto-configured as UI sprites
- ✅ Icons saved in "Icons" subfolder next to prefabs

## Common Adjustments

### Icon Too Zoomed In/Out?
→ Adjust the **Camera Zoom** slider (0.1 - 3.0)

### Want Different Angle?
→ Change **Camera Rotation** values
- Front view: (0, 0, 0)
- Side view: (0, 90, 0)
- Top view: (90, 0, 0)
- Isometric: (15, -30, 0) [default]

### Need Higher Resolution?
→ Change **Preset** dropdown to:
- 64x64 (small icons)
- 128x128 (medium icons)
- 256x256 (standard) ⭐
- 512x512 (high quality)
- Custom (your own size)

### Want All Icons in One Folder?
→ Change **Output Path Mode** to "Custom Folder"
→ Set **Custom Output Folder** to your desired path

## Tips for Best Results

1. **Collapsible Sections**: Click any section header (📦 Prefabs, ⚙️ Settings) to collapse/expand it
2. **Scrollable Interface**: The entire window is scrollable for easy access to all settings
3. **Test First**: Generate one icon to verify settings before batch processing
4. **Prefab Requirements**: Prefabs must have visible renderers (meshes, sprites)
5. **Transparent Backgrounds**: Leave "Transparent Background" ON for UI icons
6. **Batch Processing**: Add multiple similar items for consistent results

## Example Workflows

### Generate Icons for All Weapons
1. Open Icon Generator
2. Navigate to your Weapons folder in Project window
3. Select all weapon prefabs
4. Drag them into the Icon Generator drop area
5. Click "Generate Icons"
6. Find icons in: `Assets/YourFolder/Weapons/Icons/`

### Custom High-Quality Icons
1. Set Preset to "512x512"
2. Set Camera Zoom to "1.2"
3. Enable "Transparent Background"
4. Add your premium item prefabs
5. Generate!

### Multiple Camera Angles
Use the Examples menu:
- **Tools → BreezeTools → Icon Generator → Examples → Generate Multi-Angle Icons**
- Select a prefab first
- This creates 5 different angle views automatically!

## Troubleshooting

| Problem | Solution |
|---------|----------|
| Can't see generated icons | Enable "Auto Refresh AssetDatabase" or press Ctrl+R |
| Icon has wrong background | Check "Transparent Background" setting |
| Object off-center | Adjust "Camera Distance" and "Camera Rotation" |
| "No renderers" warning | Prefab needs a MeshRenderer, SpriteRenderer, or similar |
| Icon too small/large | Adjust "Camera Zoom" slider |

## Advanced Features

### Programmatic Usage
See `IconGeneratorExamples.cs` for code examples:
- Generate icons from scripts
- Batch process entire folders
- Custom camera configurations
- Multi-angle generation

### Output Path Modes
1. **Mirror Prefab Path**: Icons saved in same folder as prefab
2. **Custom Folder**: All icons go to one specified folder
3. **Prefab Folder with Subfolder**: Always creates "Icons" subfolder

### Post-Processing Options
- **Auto Refresh AssetDatabase**: Immediately show new icons
- **Configure as UI Sprite**: Sets proper import settings for UI
- Customize filter mode, compression, and max texture size

## Need Help?
- Read the full `README.md` for detailed documentation
- Check `IconGeneratorExamples.cs` for code samples
- Review console warnings for specific error messages

---

**Happy Icon Generating!** 🎨
