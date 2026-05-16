using System;
using UnityEngine;

namespace Breezinstein.Tools.Inventory
{
    /// <summary>
    /// Persists inventories as JSON via <see cref="BreezeHelper"/>. The runtime inventory
    /// only stores item GUIDs, so an <see cref="ItemDatabase"/> must be supplied on load
    /// to rebind the references.
    /// </summary>
    public static class InventorySerializer
    {
        public static void Save(Inventory inventory, string saveKey)
        {
            if (inventory == null) throw new ArgumentNullException(nameof(inventory));
            if (string.IsNullOrEmpty(saveKey)) throw new ArgumentException("saveKey is required.", nameof(saveKey));

            var dto = InventorySaveData.FromInventory(inventory);
            BreezeHelper.SaveFile(saveKey, BreezeHelper.Serialize(dto));
        }

        public static bool Load(Inventory inventory, string saveKey, ItemDatabase database)
        {
            if (inventory == null) throw new ArgumentNullException(nameof(inventory));
            if (database == null) throw new ArgumentNullException(nameof(database));
            if (string.IsNullOrEmpty(saveKey)) throw new ArgumentException("saveKey is required.", nameof(saveKey));

            if (!BreezeHelper.FileExists(saveKey))
            {
                Debug.Log($"InventorySerializer: no save file '{saveKey}' found. Inventory left unchanged.");
                return false;
            }

            var raw = BreezeHelper.LoadFile(saveKey);
            if (string.IsNullOrEmpty(raw))
            {
                Debug.LogWarning($"InventorySerializer: save file '{saveKey}' is empty.");
                return false;
            }

            var dto = raw.Deserialize<InventorySaveData>();
            if (dto == null)
            {
                Debug.LogWarning($"InventorySerializer: failed to parse save file '{saveKey}'.");
                return false;
            }

            dto.ApplyTo(inventory, database);
            return true;
        }

        /// <summary>Convenience: creates a fresh inventory with the saved capacity, then fills it.</summary>
        public static Inventory LoadOrCreate(string saveKey, ItemDatabase database, int fallbackCapacity = Inventory.DefaultCapacity)
        {
            if (database == null) throw new ArgumentNullException(nameof(database));

            if (!BreezeHelper.FileExists(saveKey))
            {
                return new Inventory(fallbackCapacity);
            }

            var raw = BreezeHelper.LoadFile(saveKey);
            var dto = string.IsNullOrEmpty(raw) ? null : raw.Deserialize<InventorySaveData>();
            int capacity = dto != null && dto.capacity > 0 ? dto.capacity : fallbackCapacity;
            var inventory = new Inventory(capacity);
            if (dto != null) dto.ApplyTo(inventory, database);
            return inventory;
        }

        public static bool DeleteSave(string saveKey)
        {
            if (string.IsNullOrEmpty(saveKey)) return false;
            string path = System.IO.Path.Combine(Application.persistentDataPath, $"{saveKey}.json");
            if (!System.IO.File.Exists(path)) return false;
            System.IO.File.Delete(path);
            return true;
        }
    }
}
