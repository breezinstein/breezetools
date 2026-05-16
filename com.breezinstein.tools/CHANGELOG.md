# Changelog
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added
- `AudioKeyAttribute` + `AudioKeyDrawer`: decorating a string field with `[AudioKey]`
  renders an Inspector dropdown populated from every `AudioLibrary` asset in the project.
- `SceneAttribute` + `SceneAttributeDrawer`: decorating a string field with `[Scene]`
  renders a dropdown of every scene currently in Build Settings.
- New `com.breezinstein.tools.spline` sub-assembly with `SplineIsland` (solid extruded
  mesh with taper, bevel and auto-collider) and `SplineHole` (editor-time hole visuals
  built from a closed `SplineContainer`). Both compile conditionally via the
  `BREEZINSTEIN_HAS_SPLINES` define, which is auto-set when `com.unity.splines >= 2.0.0`
  is installed in the project — so this module adds zero cost for projects that don't
  use Splines.
- EditMode tests for the three extracted features:
  `AudioKeyAttributeTests`, `SceneAttributeTests`, `SplineIslandTests`,
  `SplineHoleTests`. The Spline test files are guarded by
  `BREEZINSTEIN_HAS_SPLINES`, so they only compile when the Splines package is present.

### Changed
- **Inventory system (Experimental) — complete rework. Breaking change.**
  - `Inventory` is now a plain C# class (not a `ScriptableObject`), making it suitable
    for save profiles, characters, chests, etc. Authoring uses the new `InventoryAsset`
    template ScriptableObject that produces fresh inventories via `CreateInventory()`.
  - `InventoryItem` is replaced by `ItemStack`: a `[Serializable]` POCO that stores a
    GUID-based item id + quantity plus a non-serialized cached `InventoryItemData`
    reference resolved through a database at load time.
  - `InventoryItemData` now auto-generates a stable GUID `Id` in `OnValidate` and
    exposes `DisplayName`, `Description`, `Icon`, `Category`, `Rarity`, `MaxStackSize`,
    `Value` and `IsStackable`.
  - New `ItemCategory` and `ItemRarity` ScriptableObjects let each game define its own
    categories and rarity tiers (name, color, sort weight) without hard-coded enums.
  - New `ItemDatabase` ScriptableObject — registry of all items with GUID-based
    lookup; the custom inspector has *Rebuild From Project* and *Validate IDs* buttons.
  - Add/remove now return rich results: `Add(item, amount)` returns `AddResult { Added,
    Overflow }`; `Remove` returns the actual amount removed; `TryRemove` is atomic.
  - New slot operations: `SwapSlots`, `MoveSlot` (with same-item merge), `SplitStack`,
    `Compact`, `Resize`, and `SortByName / SortByCategory / SortByRarity / SortByQuantity`.
  - Strong events: `SlotChanged(index, stack)`, `ItemAdded(item, amount)`,
    `ItemRemoved(item, amount)`, `Cleared`.
  - Save/Load go through `InventorySerializer` + `InventorySaveData` DTO using
    `BreezeHelper` JSON IO. `InventorySerializer.LoadOrCreate` is provided for the
    common "load existing or start fresh" flow.
  - `InventoryUI` is replaced by `InventoryView`, which pools `InventorySlotView`s,
    subscribes to inventory events and repaints only changed slots.
  - `InventorySlotView` renders icon, rarity-tinted frame, quantity label and exposes
    `Clicked / PointerEntered / PointerExited` C# events for higher-level interactions.
- Added `BreezeTools.Tests` EditMode test suite with coverage for add/remove, overflow,
  slot operations, events and serialization.
- Added EditMode tests for `BreezeHelper`, `SerializableDictionary`, `EventManager<T>`,
  `AudioSettings`, `AudioLibrary`, `VibrationManager` and `NameGen`.
- Added `BreezeTools.PlayTests` PlayMode test suite covering `Singleton<T>`,
  `AudioManager`, `SafeArea`, `FlexibleGridLayout`, `RadialLayoutGroup` and
  `WindowManager` — i.e. the systems whose contract requires a live scene, Animator,
  AudioMixer or layout-rebuild cycle and therefore cannot be exercised meaningfully in
  EditMode.

### Fixed
- `SerializableDictionary` indexer setter silently dropped updates to existing keys
  because `SerializableKeyValuePair` is a struct, so `list[index].SetValue(value)`
  mutated a copy. The setter now replaces the entry in place.

