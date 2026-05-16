using System.Collections.Generic;
using UnityEngine;

namespace Breezinstein.Tools.Inventory
{
    /// <summary>
    /// Renders an <see cref="Inventory"/> as a grid of <see cref="InventorySlotView"/>s.
    /// Subscribes to inventory events so the UI stays in sync without polling.
    /// </summary>
    public class InventoryView : MonoBehaviour
    {
        [Header("Wiring")]
        [Tooltip("Parent transform that holds the slot views. New slots are spawned here.")]
        [SerializeField] private Transform slotContainer;

        [Tooltip("Prefab spawned for each inventory slot. Must contain an InventorySlotView.")]
        [SerializeField] private InventorySlotView slotPrefab;

        [Tooltip("Optional database used to resolve serialized stacks. Required if you load inventories from disk.")]
        [SerializeField] private ItemDatabase database;

        private Inventory inventory;
        private readonly List<InventorySlotView> spawned = new List<InventorySlotView>();

        public Inventory Inventory => inventory;
        public ItemDatabase Database => database;

        private void OnDestroy()
        {
            Detach();
        }

        /// <summary>Binds this view to an inventory and (re)builds the slot views.</summary>
        public void Bind(Inventory target)
        {
            Detach();

            inventory = target;
            if (inventory == null) return;

            EnsureSlotViews(inventory.Capacity);
            RefreshAll();

            inventory.SlotChanged += OnSlotChanged;
            inventory.Cleared += OnCleared;
        }

        /// <summary>Disconnects from the current inventory without destroying the spawned slot views.</summary>
        public void Detach()
        {
            if (inventory != null)
            {
                inventory.SlotChanged -= OnSlotChanged;
                inventory.Cleared -= OnCleared;
            }
            inventory = null;
        }

        /// <summary>Forces a full repaint of all slots. Use after manually mutating the inventory bypassing events.</summary>
        public void RefreshAll()
        {
            if (inventory == null) return;
            EnsureSlotViews(inventory.Capacity);
            for (int i = 0; i < spawned.Count; i++)
            {
                var stack = i < inventory.Slots.Count ? inventory.Slots[i] : null;
                ResolveStack(stack);
                spawned[i].SetSlot(i, stack);
            }
        }

        private void OnSlotChanged(object sender, SlotChangedEventArgs e)
        {
            if (e.SlotIndex < 0 || e.SlotIndex >= spawned.Count) return;
            ResolveStack(e.Stack);
            spawned[e.SlotIndex].SetSlot(e.SlotIndex, e.Stack);
        }

        private void OnCleared(object sender, System.EventArgs e) => RefreshAll();

        private void ResolveStack(ItemStack stack)
        {
            if (stack == null || stack.IsEmpty) return;
            if (stack.Data != null) return;
            if (database != null) stack.Resolve(database);
        }

        private void EnsureSlotViews(int desired)
        {
            if (slotPrefab == null || slotContainer == null)
            {
                Debug.LogError($"InventoryView '{name}': slotPrefab and slotContainer must be assigned.", this);
                return;
            }

            while (spawned.Count < desired)
            {
                var view = Instantiate(slotPrefab, slotContainer);
                view.gameObject.name = $"Slot_{spawned.Count}";
                spawned.Add(view);
            }

            for (int i = 0; i < spawned.Count; i++)
            {
                spawned[i].gameObject.SetActive(i < desired);
            }
        }
    }
}
