using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Breezinstein.Tools.Inventory.Editor
{
    [CustomEditor(typeof(ItemDatabase))]
    public class ItemDatabaseEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            var database = (ItemDatabase)target;

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Tools", EditorStyles.boldLabel);

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Rebuild From Project"))
                {
                    Rebuild(database);
                }

                if (GUILayout.Button("Validate IDs"))
                {
                    Validate(database);
                }
            }

            EditorGUILayout.HelpBox(
                "'Rebuild From Project' scans every InventoryItemData asset and adds any missing ones to this database.",
                MessageType.Info);
        }

        private static void Rebuild(ItemDatabase database)
        {
            string[] guids = AssetDatabase.FindAssets($"t:{nameof(InventoryItemData)}");
            int added = 0;
            Undo.RecordObject(database, "Rebuild Item Database");
            foreach (var guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var item = AssetDatabase.LoadAssetAtPath<InventoryItemData>(path);
                if (item == null) continue;
                if (database.Contains(item)) continue;
                database.Add(item);
                added++;
            }
            database.RefreshIndex();
            EditorUtility.SetDirty(database);
            AssetDatabase.SaveAssetIfDirty(database);
            Debug.Log($"ItemDatabase '{database.name}': added {added} item(s), total {database.Count}.");
        }

        private static void Validate(ItemDatabase database)
        {
            var byId = new Dictionary<string, InventoryItemData>();
            var duplicates = new List<InventoryItemData>();
            var missing = new List<InventoryItemData>();

            foreach (var item in database.Items)
            {
                if (item == null) continue;
                if (string.IsNullOrEmpty(item.Id))
                {
                    missing.Add(item);
                    continue;
                }
                if (byId.TryGetValue(item.Id, out _))
                {
                    duplicates.Add(item);
                }
                else
                {
                    byId[item.Id] = item;
                }
            }

            Debug.Log($"ItemDatabase '{database.name}': {database.Count} entries, "
                      + $"{missing.Count} missing IDs, {duplicates.Count} duplicate IDs.");

            foreach (var item in missing) Debug.LogWarning($"Missing ID: {item.name}", item);
            foreach (var item in duplicates) Debug.LogWarning($"Duplicate ID: {item.name} ({item.Id})", item);
        }
    }
}
