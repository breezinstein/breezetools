using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Breezinstein.Tools.Inventory
{
    /// <summary>
    /// Central registry mapping <see cref="InventoryItemData.Id"/> to its asset.
    /// Required at load time to rebind serialized stacks back to their item definitions.
    /// </summary>
    [CreateAssetMenu(menuName = "Breeze Tools/Inventory/Item Database", fileName = "ItemDatabase", order = 1)]
    public class ItemDatabase : ScriptableObject, IEnumerable<InventoryItemData>
    {
        [Tooltip("All items known to the game. Use the Rebuild button in the inspector to auto-populate.")]
        [SerializeField] private List<InventoryItemData> items = new List<InventoryItemData>();

        private readonly Dictionary<string, InventoryItemData> lookup = new Dictionary<string, InventoryItemData>();
        private bool indexDirty = true;

        public int Count => items.Count;

        public IReadOnlyList<InventoryItemData> Items
        {
            get
            {
                EnsureIndex();
                return items;
            }
        }

        public bool TryGetItem(string id, out InventoryItemData item)
        {
            EnsureIndex();
            if (string.IsNullOrEmpty(id))
            {
                item = null;
                return false;
            }
            return lookup.TryGetValue(id, out item);
        }

        public InventoryItemData GetItem(string id)
        {
            TryGetItem(id, out var item);
            return item;
        }

        public bool Contains(InventoryItemData item)
        {
            return item != null && items.Contains(item);
        }

        public void Add(InventoryItemData item)
        {
            if (item == null || items.Contains(item)) return;
            items.Add(item);
            indexDirty = true;
        }

        public void Remove(InventoryItemData item)
        {
            if (items.Remove(item))
            {
                indexDirty = true;
            }
        }

        public void Clear()
        {
            items.Clear();
            indexDirty = true;
        }

        /// <summary>Forces the id lookup to be rebuilt. Call after mutating <see cref="items"/> externally.</summary>
        public void RefreshIndex()
        {
            indexDirty = true;
            EnsureIndex();
        }

        private void EnsureIndex()
        {
            if (!indexDirty) return;
            lookup.Clear();
            for (int i = 0; i < items.Count; i++)
            {
                var entry = items[i];
                if (entry == null || string.IsNullOrEmpty(entry.Id)) continue;
                lookup[entry.Id] = entry;
            }
            indexDirty = false;
        }

        private void OnEnable() => indexDirty = true;
        private void OnValidate() => indexDirty = true;

        public IEnumerator<InventoryItemData> GetEnumerator()
        {
            EnsureIndex();
            return items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
