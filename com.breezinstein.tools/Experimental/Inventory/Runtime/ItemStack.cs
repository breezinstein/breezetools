using System;
using UnityEngine;

namespace Breezinstein.Tools.Inventory
{
    /// <summary>
    /// A single slot's worth of stacked items at runtime.
    /// Serializable by both Unity (<see cref="SerializeField"/>) and Newtonsoft.Json
    /// so it can be persisted through <see cref="InventorySerializer"/>.
    /// </summary>
    [Serializable]
    public sealed class ItemStack
    {
        [SerializeField] private string itemId;
        [SerializeField] private int quantity;

        [NonSerialized] private InventoryItemData cachedData;

        public ItemStack() { }

        public ItemStack(InventoryItemData data, int quantity)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            if (quantity < 1) throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity must be >= 1.");
            cachedData = data;
            itemId = data.Id;
            this.quantity = Mathf.Min(quantity, data.MaxStackSize);
        }

        public string ItemId => itemId;
        public int Quantity => quantity;

        /// <summary>Item definition. May be <c>null</c> until <see cref="Resolve"/> is called after loading.</summary>
        public InventoryItemData Data => cachedData;

        public bool IsEmpty => quantity <= 0 || string.IsNullOrEmpty(itemId);
        public bool IsFull => cachedData != null && quantity >= cachedData.MaxStackSize;
        public int FreeSpace => cachedData == null ? 0 : Mathf.Max(0, cachedData.MaxStackSize - quantity);

        /// <summary>Resolves the cached <see cref="InventoryItemData"/> reference from <paramref name="database"/>.</summary>
        public void Resolve(ItemDatabase database)
        {
            if (database == null) throw new ArgumentNullException(nameof(database));
            database.TryGetItem(itemId, out cachedData);
        }

        /// <summary>Increases the stack quantity. Returns the amount that didn't fit.</summary>
        internal int AddInternal(int amount)
        {
            if (amount <= 0) return 0;
            if (cachedData == null) throw new InvalidOperationException("ItemStack has no resolved item data.");
            int space = FreeSpace;
            int added = Mathf.Min(space, amount);
            quantity += added;
            return amount - added;
        }

        /// <summary>Decreases the stack quantity. Returns the amount actually removed.</summary>
        internal int RemoveInternal(int amount)
        {
            if (amount <= 0) return 0;
            int removed = Mathf.Min(quantity, amount);
            quantity -= removed;
            return removed;
        }

        /// <summary>Replaces contents of this stack. Used by Inventory.SwapSlots / SplitStack.</summary>
        internal void Set(InventoryItemData data, int newQuantity)
        {
            cachedData = data;
            itemId = data == null ? string.Empty : data.Id;
            quantity = data == null ? 0 : Mathf.Clamp(newQuantity, 0, data.MaxStackSize);
        }

        internal void Clear()
        {
            cachedData = null;
            itemId = string.Empty;
            quantity = 0;
        }

        public ItemStack Clone()
        {
            var clone = new ItemStack
            {
                itemId = itemId,
                quantity = quantity,
                cachedData = cachedData,
            };
            return clone;
        }
    }
}
