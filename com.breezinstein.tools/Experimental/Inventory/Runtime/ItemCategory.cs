using UnityEngine;

namespace Breezinstein.Tools.Inventory
{
    /// <summary>
    /// Designer-authored item category (e.g. Weapon, Consumable, Quest). Game-specific,
    /// so it's a ScriptableObject rather than a hard-coded enum. Useful for filtering,
    /// sorting and UI grouping. Create via
    /// <c>Assets &gt; Create &gt; Breeze Tools &gt; Inventory &gt; Item Category</c>.
    /// </summary>
    [CreateAssetMenu(menuName = "Breeze Tools/Inventory/Item Category", fileName = "New Category", order = 11)]
    public class ItemCategory : ScriptableObject
    {
        [SerializeField] private string displayName = "Misc";
        [SerializeField] private Sprite icon;
        [Tooltip("Optional short identifier. Falls back to the asset name.")]
        [SerializeField] private string id;
        [Tooltip("Sort order used by InventoryView when sorting by category.")]
        [SerializeField] private int sortOrder = 0;

        public string Id => string.IsNullOrEmpty(id) ? name : id;
        public string DisplayName => displayName;
        public Sprite Icon => icon;
        public int SortOrder => sortOrder;

        public override string ToString() => DisplayName;
    }
}
