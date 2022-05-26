using UnityEngine;

namespace Breezinstein.Tools.Inventory
{
    [CreateAssetMenu(fileName ="New Item",menuName ="Breeze Tools/Inventory/Create New Item")]
    public class InventoryItemData : ScriptableObject
    {
        public string id;
        public string itemName = "New Item";
        public float value;
        public string description;
        public Sprite icon = null;
    }
}