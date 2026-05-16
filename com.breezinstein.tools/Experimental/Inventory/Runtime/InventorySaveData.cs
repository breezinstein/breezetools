using System;
using System.Collections.Generic;
using UnityEngine;

namespace Breezinstein.Tools.Inventory
{
    /// <summary>
    /// JSON-safe snapshot of an <see cref="Inventory"/>. Stores only item GUIDs and
    /// quantities; <see cref="InventoryItemData"/> references are re-resolved at load
    /// time through an <see cref="ItemDatabase"/>.
    /// </summary>
    [Serializable]
    public class InventorySaveData
    {
        [Serializable]
        public struct SlotEntry
        {
            public int slotIndex;
            public string itemId;
            public int quantity;
        }

        public int capacity;
        public List<SlotEntry> slots = new List<SlotEntry>();

        public static InventorySaveData FromInventory(Inventory inventory)
        {
            if (inventory == null) throw new ArgumentNullException(nameof(inventory));

            var data = new InventorySaveData { capacity = inventory.Capacity };
            for (int i = 0; i < inventory.Slots.Count; i++)
            {
                var stack = inventory.Slots[i];
                if (stack == null || stack.IsEmpty) continue;
                data.slots.Add(new SlotEntry
                {
                    slotIndex = i,
                    itemId = stack.ItemId,
                    quantity = stack.Quantity,
                });
            }
            return data;
        }

        public void ApplyTo(Inventory inventory, ItemDatabase database)
        {
            if (inventory == null) throw new ArgumentNullException(nameof(inventory));
            if (database == null) throw new ArgumentNullException(nameof(database));

            if (inventory.Capacity != capacity)
            {
                inventory.Resize(capacity);
            }

            var rebuilt = new ItemStack[capacity];
            for (int i = 0; i < slots.Count; i++)
            {
                var entry = slots[i];
                if (entry.slotIndex < 0 || entry.slotIndex >= capacity) continue;
                if (entry.quantity <= 0) continue;
                if (!database.TryGetItem(entry.itemId, out var item) || item == null)
                {
                    Debug.LogWarning($"InventorySaveData: item id '{entry.itemId}' not found in database. Slot {entry.slotIndex} skipped.");
                    continue;
                }
                rebuilt[entry.slotIndex] = new ItemStack(item, entry.quantity);
            }
            inventory.ReplaceSlotsForLoad(rebuilt);
        }
    }
}
