using System;

namespace Breezinstein.Tools.Inventory
{
    public sealed class SlotChangedEventArgs : EventArgs
    {
        public int SlotIndex { get; }
        public ItemStack Stack { get; }
        public SlotChangedEventArgs(int slotIndex, ItemStack stack)
        {
            SlotIndex = slotIndex;
            Stack = stack;
        }
    }

    public sealed class ItemAddedEventArgs : EventArgs
    {
        public InventoryItemData Item { get; }
        public int Amount { get; }
        public ItemAddedEventArgs(InventoryItemData item, int amount)
        {
            Item = item;
            Amount = amount;
        }
    }

    public sealed class ItemRemovedEventArgs : EventArgs
    {
        public InventoryItemData Item { get; }
        public int Amount { get; }
        public ItemRemovedEventArgs(InventoryItemData item, int amount)
        {
            Item = item;
            Amount = amount;
        }
    }
}
