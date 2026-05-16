using System;
using UnityEngine;

namespace Breezinstein.Tools.Inventory
{
    /// <summary>
    /// Designer-authored item definition. One asset per unique item.
    /// The runtime <see cref="ItemStack"/> only stores an <see cref="Id"/> and a quantity
    /// and resolves back to this asset through an <see cref="ItemDatabase"/> at load time.
    /// </summary>
    [CreateAssetMenu(menuName = "Breeze Tools/Inventory/Item", fileName = "New Item", order = 0)]
    public class InventoryItemData : ScriptableObject
    {
        [Header("Identity")]
        [Tooltip("Stable GUID. Auto-generated on first save; do not edit unless you know what you're doing.")]
        [SerializeField] private string id;

        [Tooltip("Display name shown in UI.")]
        [SerializeField] private string displayName = "New Item";

        [TextArea(2, 6)]
        [SerializeField] private string description = string.Empty;

        [Header("Presentation")]
        [SerializeField] private Sprite icon;
        [Tooltip("Optional. Assign an ItemCategory asset for grouping/sorting.")]
        [SerializeField] private ItemCategory category;
        [Tooltip("Optional. Assign an ItemRarity asset for UI tinting and sorting.")]
        [SerializeField] private ItemRarity rarity;

        [Header("Stacking & Economy")]
        [Min(1)]
        [Tooltip("Maximum quantity allowed per stack. Set to 1 to make the item unstackable.")]
        [SerializeField] private int maxStackSize = 99;

        [Min(0)]
        [Tooltip("Generic gold/value of one unit. Use however you like.")]
        [SerializeField] private int value = 0;

        public string Id => id;
        public string DisplayName => displayName;
        public string Description => description;
        public Sprite Icon => icon;
        public ItemCategory Category => category;
        public ItemRarity Rarity => rarity;
        public int MaxStackSize => Mathf.Max(1, maxStackSize);
        public int Value => value;
        public bool IsStackable => MaxStackSize > 1;

        private void OnValidate()
        {
            if (string.IsNullOrEmpty(id))
            {
                id = Guid.NewGuid().ToString("N");
            }

            if (maxStackSize < 1)
            {
                maxStackSize = 1;
            }
        }

        /// <summary>Force a new GUID. Editor-only convenience used by the custom inspector.</summary>
        public void RegenerateId()
        {
            id = Guid.NewGuid().ToString("N");
        }
    }
}
