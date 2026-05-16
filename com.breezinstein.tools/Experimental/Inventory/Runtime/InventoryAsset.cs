using System;
using System.Collections.Generic;
using UnityEngine;

namespace Breezinstein.Tools.Inventory
{
    /// <summary>
    /// Designer-authored template for an inventory: capacity plus a list of starting items.
    /// Call <see cref="CreateInventory"/> at runtime to obtain a fresh runtime instance.
    /// </summary>
    [CreateAssetMenu(menuName = "Breeze Tools/Inventory/Inventory Template", fileName = "New Inventory Template", order = 2)]
    public class InventoryAsset : ScriptableObject
    {
        [Serializable]
        public struct StartingItem
        {
            public InventoryItemData item;
            [Min(1)] public int quantity;
        }

        [Min(1)]
        [SerializeField] private int capacity = Inventory.DefaultCapacity;

        [SerializeField] private List<StartingItem> startingItems = new List<StartingItem>();

        public int Capacity => Mathf.Max(1, capacity);
        public IReadOnlyList<StartingItem> StartingItems => startingItems;

        /// <summary>Builds a new runtime <see cref="Inventory"/> and fills it with the configured starting items.</summary>
        public Inventory CreateInventory()
        {
            var inventory = new Inventory(Capacity);
            for (int i = 0; i < startingItems.Count; i++)
            {
                var entry = startingItems[i];
                if (entry.item == null || entry.quantity <= 0) continue;
                var result = inventory.Add(entry.item, entry.quantity);
                if (result.Overflow > 0)
                {
                    Debug.LogWarning($"InventoryAsset '{name}': starting item '{entry.item.DisplayName}' "
                                     + $"overflowed by {result.Overflow}. Increase capacity or reduce quantity.");
                }
            }
            return inventory;
        }

        private void OnValidate()
        {
            if (capacity < 1) capacity = 1;
        }
    }
}
