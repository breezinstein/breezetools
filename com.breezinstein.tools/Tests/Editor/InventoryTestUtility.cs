using NUnit.Framework;
using UnityEngine;

namespace Breezinstein.Tools.Inventory.Tests
{
    /// <summary>
    /// Shared helpers for inventory tests: creates throwaway <see cref="InventoryItemData"/>
    /// assets in memory so tests don't depend on assets on disk.
    /// </summary>
    public static class InventoryTestUtility
    {
        public static InventoryItemData CreateItem(string displayName = "Test Item", int maxStack = 99, string id = null)
        {
            var item = ScriptableObject.CreateInstance<InventoryItemData>();
            item.name = displayName;

            var so = new UnityEditor.SerializedObject(item);
            so.FindProperty("id").stringValue = string.IsNullOrEmpty(id) ? System.Guid.NewGuid().ToString("N") : id;
            so.FindProperty("displayName").stringValue = displayName;
            so.FindProperty("maxStackSize").intValue = Mathf.Max(1, maxStack);
            so.ApplyModifiedPropertiesWithoutUndo();
            return item;
        }

        public static ItemDatabase CreateDatabase(params InventoryItemData[] items)
        {
            var db = ScriptableObject.CreateInstance<ItemDatabase>();
            foreach (var item in items) db.Add(item);
            db.RefreshIndex();
            return db;
        }

        public static void Destroy(Object obj)
        {
            if (obj == null) return;
            if (Application.isPlaying) Object.Destroy(obj);
            else Object.DestroyImmediate(obj);
        }
    }
}
