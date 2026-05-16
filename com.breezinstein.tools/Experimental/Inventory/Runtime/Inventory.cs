using System;
using System.Collections.Generic;
using UnityEngine;

namespace Breezinstein.Tools.Inventory
{
    /// <summary>
    /// Slot-based inventory. Pure C# runtime object (not a <c>ScriptableObject</c>) so it
    /// can be owned by a save profile, a character, a chest, etc. Use
    /// <see cref="InventoryAsset"/> to author a template that produces fresh instances.
    /// </summary>
    [Serializable]
    public class Inventory
    {
        public const int DefaultCapacity = 20;

        private readonly List<ItemStack> slots;
        private int capacity;

        public event EventHandler<SlotChangedEventArgs> SlotChanged;
        public event EventHandler<ItemAddedEventArgs> ItemAdded;
        public event EventHandler<ItemRemovedEventArgs> ItemRemoved;
        public event EventHandler Cleared;

        public Inventory() : this(DefaultCapacity) { }

        public Inventory(int capacity)
        {
            if (capacity < 1) throw new ArgumentOutOfRangeException(nameof(capacity), "Capacity must be >= 1.");
            this.capacity = capacity;
            slots = new List<ItemStack>(capacity);
            for (int i = 0; i < capacity; i++) slots.Add(null);
        }

        public int Capacity => capacity;

        /// <summary>Number of non-empty slots.</summary>
        public int UsedSlots
        {
            get
            {
                int count = 0;
                for (int i = 0; i < slots.Count; i++)
                {
                    if (slots[i] != null && !slots[i].IsEmpty) count++;
                }
                return count;
            }
        }

        public int FreeSlots => capacity - UsedSlots;
        public bool IsFull => FreeSlots == 0;
        public bool IsEmpty => UsedSlots == 0;

        public IReadOnlyList<ItemStack> Slots => slots;

        public ItemStack GetSlot(int index)
        {
            ValidateIndex(index);
            return slots[index];
        }

        /// <summary>
        /// Adds <paramref name="amount"/> of <paramref name="item"/>, filling existing
        /// stacks first then occupying empty slots. Returns how many units fit and
        /// how many overflowed.
        /// </summary>
        public AddResult Add(InventoryItemData item, int amount = 1)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));
            if (amount <= 0) return new AddResult(0, 0);

            int remaining = amount;

            if (item.IsStackable)
            {
                for (int i = 0; i < slots.Count && remaining > 0; i++)
                {
                    var slot = slots[i];
                    if (slot == null || slot.IsEmpty) continue;
                    if (slot.ItemId != item.Id) continue;
                    if (slot.IsFull) continue;

                    int leftover = slot.AddInternal(remaining);
                    int placed = remaining - leftover;
                    remaining = leftover;
                    if (placed > 0) RaiseSlotChanged(i, slot);
                }
            }

            for (int i = 0; i < slots.Count && remaining > 0; i++)
            {
                if (slots[i] != null && !slots[i].IsEmpty) continue;

                int chunk = Math.Min(remaining, item.MaxStackSize);
                var newStack = new ItemStack(item, chunk);
                slots[i] = newStack;
                remaining -= chunk;
                RaiseSlotChanged(i, newStack);
            }

            int added = amount - remaining;
            if (added > 0)
            {
                ItemAdded?.Invoke(this, new ItemAddedEventArgs(item, added));
            }
            return new AddResult(added, remaining);
        }

        /// <summary>Attempts to remove <paramref name="amount"/> only if the full amount is available.</summary>
        public bool TryRemove(InventoryItemData item, int amount = 1)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));
            if (amount <= 0) return true;
            if (GetCount(item) < amount) return false;
            RemoveInternal(item, amount);
            return true;
        }

        /// <summary>Removes up to <paramref name="amount"/>. Returns the amount actually removed.</summary>
        public int Remove(InventoryItemData item, int amount = 1)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));
            if (amount <= 0) return 0;
            return RemoveInternal(item, amount);
        }

        private int RemoveInternal(InventoryItemData item, int amount)
        {
            int remaining = amount;
            for (int i = slots.Count - 1; i >= 0 && remaining > 0; i--)
            {
                var slot = slots[i];
                if (slot == null || slot.IsEmpty) continue;
                if (slot.ItemId != item.Id) continue;

                int removed = slot.RemoveInternal(remaining);
                remaining -= removed;
                if (slot.IsEmpty)
                {
                    slots[i] = null;
                }
                RaiseSlotChanged(i, slots[i]);
            }

            int actuallyRemoved = amount - remaining;
            if (actuallyRemoved > 0)
            {
                ItemRemoved?.Invoke(this, new ItemRemovedEventArgs(item, actuallyRemoved));
            }
            return actuallyRemoved;
        }

        public int GetCount(InventoryItemData item)
        {
            if (item == null) return 0;
            int total = 0;
            for (int i = 0; i < slots.Count; i++)
            {
                var slot = slots[i];
                if (slot != null && !slot.IsEmpty && slot.ItemId == item.Id) total += slot.Quantity;
            }
            return total;
        }

        public bool Has(InventoryItemData item, int atLeast = 1) => GetCount(item) >= atLeast;

        /// <summary>Returns the first slot index containing <paramref name="item"/>, or -1.</summary>
        public int IndexOf(InventoryItemData item)
        {
            if (item == null) return -1;
            for (int i = 0; i < slots.Count; i++)
            {
                var slot = slots[i];
                if (slot != null && !slot.IsEmpty && slot.ItemId == item.Id) return i;
            }
            return -1;
        }

        public bool SwapSlots(int a, int b)
        {
            ValidateIndex(a);
            ValidateIndex(b);
            if (a == b) return false;
            (slots[a], slots[b]) = (slots[b], slots[a]);
            RaiseSlotChanged(a, slots[a]);
            RaiseSlotChanged(b, slots[b]);
            return true;
        }

        /// <summary>
        /// Moves the contents of slot <paramref name="from"/> onto slot <paramref name="to"/>.
        /// If <paramref name="to"/> is empty, the stack moves wholesale. If it holds the same item,
        /// the source stack merges into the target (respecting MaxStackSize). Otherwise the two
        /// stacks swap.
        /// </summary>
        public void MoveSlot(int from, int to)
        {
            ValidateIndex(from);
            ValidateIndex(to);
            if (from == to) return;

            var src = slots[from];
            if (src == null || src.IsEmpty) return;

            var dst = slots[to];
            if (dst == null || dst.IsEmpty)
            {
                slots[to] = src;
                slots[from] = null;
                RaiseSlotChanged(from, null);
                RaiseSlotChanged(to, slots[to]);
                return;
            }

            if (dst.ItemId == src.ItemId && src.Data != null && src.Data.IsStackable)
            {
                int overflow = dst.AddInternal(src.Quantity);
                if (overflow == 0)
                {
                    slots[from] = null;
                }
                else
                {
                    src.Set(src.Data, overflow);
                }
                RaiseSlotChanged(from, slots[from]);
                RaiseSlotChanged(to, dst);
                return;
            }

            SwapSlots(from, to);
        }

        /// <summary>
        /// Splits the stack in slot <paramref name="from"/> into an empty target slot.
        /// </summary>
        public bool SplitStack(int from, int to, int amount)
        {
            ValidateIndex(from);
            ValidateIndex(to);
            if (from == to || amount <= 0) return false;

            var src = slots[from];
            if (src == null || src.IsEmpty || src.Data == null) return false;
            if (amount >= src.Quantity) return false;

            var dst = slots[to];
            if (dst != null && !dst.IsEmpty) return false;

            int taken = src.RemoveInternal(amount);
            if (taken == 0) return false;

            slots[to] = new ItemStack(src.Data, taken);
            RaiseSlotChanged(from, src);
            RaiseSlotChanged(to, slots[to]);
            return true;
        }

        /// <summary>Sorts non-empty slots toward the start, preserving stack ordering by item id then quantity.</summary>
        public void Compact()
        {
            var nonEmpty = new List<ItemStack>(slots.Count);
            for (int i = 0; i < slots.Count; i++)
            {
                if (slots[i] != null && !slots[i].IsEmpty) nonEmpty.Add(slots[i]);
            }
            for (int i = 0; i < slots.Count; i++)
            {
                slots[i] = i < nonEmpty.Count ? nonEmpty[i] : null;
                RaiseSlotChanged(i, slots[i]);
            }
        }

        public void SortByName() => SortInternal((a, b) =>
            string.Compare(a.Data?.DisplayName, b.Data?.DisplayName, StringComparison.OrdinalIgnoreCase));

        public void SortByCategory() => SortInternal((a, b) =>
        {
            int orderA = a.Data != null && a.Data.Category != null ? a.Data.Category.SortOrder : int.MaxValue;
            int orderB = b.Data != null && b.Data.Category != null ? b.Data.Category.SortOrder : int.MaxValue;
            int c = orderA.CompareTo(orderB);
            return c != 0 ? c : string.Compare(a.Data?.DisplayName, b.Data?.DisplayName, StringComparison.OrdinalIgnoreCase);
        });

        public void SortByRarity() => SortInternal((a, b) =>
        {
            int tierA = a.Data != null && a.Data.Rarity != null ? a.Data.Rarity.Tier : int.MinValue;
            int tierB = b.Data != null && b.Data.Rarity != null ? b.Data.Rarity.Tier : int.MinValue;
            int c = tierB.CompareTo(tierA);
            return c != 0 ? c : string.Compare(a.Data?.DisplayName, b.Data?.DisplayName, StringComparison.OrdinalIgnoreCase);
        });

        public void SortByQuantity() => SortInternal((a, b) => b.Quantity.CompareTo(a.Quantity));

        private void SortInternal(Comparison<ItemStack> comparison)
        {
            var nonEmpty = new List<ItemStack>(slots.Count);
            for (int i = 0; i < slots.Count; i++)
            {
                if (slots[i] != null && !slots[i].IsEmpty) nonEmpty.Add(slots[i]);
            }
            nonEmpty.Sort(comparison);
            for (int i = 0; i < slots.Count; i++)
            {
                slots[i] = i < nonEmpty.Count ? nonEmpty[i] : null;
                RaiseSlotChanged(i, slots[i]);
            }
        }

        public void Clear()
        {
            for (int i = 0; i < slots.Count; i++)
            {
                if (slots[i] != null)
                {
                    slots[i] = null;
                    RaiseSlotChanged(i, null);
                }
            }
            Cleared?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>Resizes the inventory in-place. Items that no longer fit are dropped and returned.</summary>
        public List<ItemStack> Resize(int newCapacity)
        {
            if (newCapacity < 1) throw new ArgumentOutOfRangeException(nameof(newCapacity));
            var dropped = new List<ItemStack>();
            if (newCapacity == capacity) return dropped;

            if (newCapacity > capacity)
            {
                int diff = newCapacity - capacity;
                for (int i = 0; i < diff; i++) slots.Add(null);
            }
            else
            {
                for (int i = slots.Count - 1; i >= newCapacity; i--)
                {
                    if (slots[i] != null && !slots[i].IsEmpty) dropped.Add(slots[i]);
                    slots.RemoveAt(i);
                }
            }
            capacity = newCapacity;
            return dropped;
        }

        internal void ReplaceSlotsForLoad(IList<ItemStack> loaded)
        {
            for (int i = 0; i < slots.Count; i++)
            {
                slots[i] = i < loaded.Count ? loaded[i] : null;
                RaiseSlotChanged(i, slots[i]);
            }
        }

        private void RaiseSlotChanged(int index, ItemStack stack)
        {
            SlotChanged?.Invoke(this, new SlotChangedEventArgs(index, stack));
        }

        private void ValidateIndex(int index)
        {
            if (index < 0 || index >= slots.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(index), $"Slot index {index} is out of range [0, {slots.Count - 1}].");
            }
        }
    }
}
