A Unity Editor extension that provides essential utilities for streamlining common object manipulation tasks and scene management.

All tools support undo/redo
## Features

### Pivot Utilities

Quickly modify object pivot points without affecting their visual appearance:

- **Center Pivot**: Automatically centers the pivot point within the object's bounds
- **Pivot to Base**: Moves the pivot point to the bottom of the object, useful for props and characters

### Randomizer

Add controlled variation to selected objects:

- **Rotation**: Randomize rotation within specified angles
- **Position**: Add random offset to object positions
- **Scale**: Apply random scaling within defined limits

### Distribute

Create organized arrays of objects with precise control:

- **Preview Distribution**: See the final arrangement before applying using hologram shader
- **Array Creation**:
    - X Array: Linear distribution along X axis
    - Y Array: Linear distribution along Y axis
    - Z Array: Linear distribution along Z axis
    - Radial Array: Circular distribution around a center point

### Populate

Intelligently populate scenes with objects using advanced placement options:

- **Surface Detection**: Place objects on detected surfaces with proper alignment
- **Placement Rules**:
    - Density control
    - Minimum spacing between objects
    - Normal alignment
    - Surface angle limits
- **Distribution Options**:
    - Single prefab placement
    - Multi-prefab weighted distribution
    - Group placement maintaining relative positions
- **Avoidance Settings**:
    - Collision detection
    - Overlap prevention
    - Exclusion zones
- **Preview Mode**: Visualize placement before committing changes
### Replace Selected

Batch replace components and assets on selected objects:

- **Game Object**: Swap selected objects with a different prefab
- **Font**: Replace fonts on Text components
- **Material**: Update materials on selected objects

### Replace in Scene

Global replacement tools for entire scenes:

- **Font**: Replace all instances of a specific font
- **Material**: Replace all instances of a specific material

### Missing Reference Helper

Utility for identifying and fixing missing script references and broken prefab connections in your scene.

---

This tool is designed to enhance workflow efficiency in Unity by providing quick access to commonly used object manipulation and management features through a single interface.