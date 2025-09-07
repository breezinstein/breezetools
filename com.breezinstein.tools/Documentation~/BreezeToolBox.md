# BreezeToolBox - Unity Editor Tool

## Overview

BreezeToolBox is a comprehensive Unity Editor window that provides a collection of useful utilities for game development. It offers quick access to common transformation operations, object replacement tools, and font management features that can significantly speed up your development workflow.

## How to Access

The tool can be accessed through the Unity Editor menu:
- **Window** → **Breeze Tools** → **Toolbox**

## Features

### 1. Random Rotation

Apply random rotations to selected GameObjects with fine-grained control over each axis.

**Features:**
- Individual axis control (X, Y, Z toggles)
- Minimum and maximum rotation ranges for each axis
- Preserves original rotation values for disabled axes
- Works with multiple selected objects simultaneously

**Usage:**
1. Select one or more GameObjects in the scene
2. Expand the "Random Rotation" section
3. Enable the axes you want to randomize (X, Y, Z checkboxes)
4. Set the minimum and maximum rotation values for each axis
5. Click "Apply Rotation" to randomize the selected objects

### 2. Random Position

Randomly position selected GameObjects within specified bounds.

**Features:**
- Individual axis control (X, Y, Z toggles)
- Minimum and maximum position ranges for each axis
- Uses local position coordinates
- Preserves original position values for disabled axes

**Usage:**
1. Select one or more GameObjects in the scene
2. Expand the "Random Position" section
3. Enable the axes you want to randomize (X, Y, Z checkboxes)
4. Set the minimum and maximum position values for each axis
5. Click "Apply Position" to randomize the positions

### 3. Distribution Tools

Evenly distribute selected GameObjects along a specified axis between the outermost objects.

**Features:**
- X Array: Distributes objects along the X-axis
- Y Array: Distributes objects along the Y-axis
- Z Array: Distributes objects along the Z-axis
- Automatically calculates spacing based on the positions of the outermost objects
- Uses world position coordinates

**Usage:**
1. Select multiple GameObjects in the scene (minimum 2 objects)
2. Expand the "Distribute" section
3. Click the desired distribution button (X Array, Y Array, or Z Array)
4. Objects will be evenly spaced between the outermost positions on the selected axis

### 4. Object Replacement

Replace selected GameObjects with a specified prefab or GameObject while preserving position, rotation, and hierarchy.

**Features:**
- Maintains original position and rotation
- Preserves parent-child relationships
- Supports undo operations
- Works with prefabs and scene objects

**Usage:**
1. Select one or more GameObjects to replace
2. Expand the "Replace Objects" section
3. Drag a GameObject or prefab into the object field
4. Click "Replace" to swap the selected objects

### 5. Font Replacement

Batch replace fonts on Text components across multiple GameObjects.

**Features:**
- Works with Unity's legacy Text components
- Processes multiple objects simultaneously
- Only affects GameObjects that have Text components

**Usage:**
1. Select GameObjects containing Text components
2. Expand the "Replace Font" section
3. Drag a Font asset into the font field
4. Click "Replace Font" to update all selected Text components

## Technical Details

### Requirements
- Unity Editor (any version supporting EditorWindow)
- Works with Unity's built-in UI system

### Implementation Notes
- Uses `EditorWindow` for the interface
- Leverages Unity's `Selection` API to work with selected objects
- Implements proper undo support for destructive operations
- Uses foldout groups for organized UI presentation

### Code Structure
- **Namespace:** `Breezinstein.Tools`
- **Main Class:** `BreezeToolBox : EditorWindow`
- **Menu Path:** "Breeze Tools/Toolbox"

## Best Practices

1. **Always have objects selected** before using the tools - they operate on the current selection
2. **Use undo** (Ctrl+Z) if you need to revert changes, especially for the replace functions
3. **Test with a few objects first** before applying operations to large selections
4. **Save your scene** before performing major operations like object replacement

## Tips and Tricks

- **Random Rotation/Position**: Start with small ranges and gradually increase to find the right amount of randomization
- **Distribution**: Works best when you have a clear start and end point in your object arrangement
- **Object Replacement**: Great for quickly swapping placeholder objects with final assets
- **Font Replacement**: Useful for applying consistent typography across UI elements

## Troubleshooting

**Objects not changing:**
- Ensure objects are selected in the scene hierarchy
- Check that the appropriate axis toggles are enabled for random operations

**Unexpected results with distribution:**
- Verify you have at least 2 objects selected
- Distribution uses world coordinates, so nested objects may behave differently than expected

**Replace function not working:**
- Make sure you've assigned a GameObject to the replacement field
- Check console for any error messages
