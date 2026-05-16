using UnityEngine;

namespace Breezinstein.Tools.Inventory
{
    /// <summary>
    /// Designer-authored rarity tier (e.g. Common, Rare, Legendary). Names, colors and
    /// sort order are game-specific, so this is intentionally a ScriptableObject rather
    /// than a hard-coded enum. Create assets via
    /// <c>Assets &gt; Create &gt; Breeze Tools &gt; Inventory &gt; Item Rarity</c>.
    /// </summary>
    [CreateAssetMenu(menuName = "Breeze Tools/Inventory/Item Rarity", fileName = "New Rarity", order = 10)]
    public class ItemRarity : ScriptableObject
    {
        [Tooltip("Display name shown in UI (e.g. \"Common\", \"Rare\").")]
        [SerializeField] private string displayName = "Common";

        [Tooltip("Sort weight. Higher = rarer. Used for sorting and comparisons.")]
        [SerializeField] private int tier = 0;

        [Tooltip("Tint used by UI to color item names, frames, etc.")]
        [SerializeField] private Color color = Color.white;

        [Tooltip("Optional short identifier (e.g. \"rare\"). Falls back to the asset name.")]
        [SerializeField] private string id;

        public string Id => string.IsNullOrEmpty(id) ? name : id;
        public string DisplayName => displayName;
        public int Tier => tier;
        public Color Color => color;

        public override string ToString() => $"{DisplayName} (tier {Tier})";
    }
}
