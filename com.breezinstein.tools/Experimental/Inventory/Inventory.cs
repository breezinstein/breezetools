using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Breezinstein.Tools.Inventory
{
    [CreateAssetMenu(menuName = "Breeze Tools/Inventory/Create New Inventory", fileName = "New Inventory")]
    public class Inventory : ScriptableObject
    {
        public string inventoryName = "Inventory";
        public Dictionary<InventoryItemData, InventoryItem> items = new Dictionary<InventoryItemData, InventoryItem>();
        public List<InventoryItem> inventory = new List<InventoryItem>();
        public delegate void OnInventoryChangedEvent();
        public OnInventoryChangedEvent onInventoryChanged;

        public void Add(InventoryItemData data, int amount)
        {
            if (items.TryGetValue(data, out InventoryItem value))
            {
                value.AddToStack(amount);
                onInventoryChanged?.Invoke();
            }
            else
            {
                InventoryItem newItem = new InventoryItem(data);
                inventory.Add(newItem);
                items.Add(data, newItem);
                onInventoryChanged?.Invoke();
            }
        }

        public void Remove(InventoryItemData data, int amount)
        {
            if (items.TryGetValue(data, out InventoryItem value))
            {
                value.RemoveFromStack(amount);

                if (value.stackSize < 1)
                {
                    inventory.Remove(value);
                    items.Remove(data);
                }
                onInventoryChanged?.Invoke();
            }
        }

        public void Load()
        {
            if (BreezeHelper.FileExists(inventoryName))
            {
                var loadedInventory = BreezeHelper.LoadFile(inventoryName).Deserialize<Inventory>();
                items = loadedInventory.items;
                inventory = loadedInventory.inventory;
            }
            else
            {
                Debug.Log("The file " + inventoryName + "does not exists");
                Reset();
            }
            Debug.Log($"{inventoryName} Loaded");
            onInventoryChanged?.Invoke();
        }

        public void Save()
        {
            BreezeHelper.SaveFile(inventoryName, BreezeHelper.Serialize(this));
        }

        public void Reset()
        {
            items = new Dictionary<InventoryItemData, InventoryItem>();
            inventory = new List<InventoryItem>();
            onInventoryChanged?.Invoke();

        }
    }
}