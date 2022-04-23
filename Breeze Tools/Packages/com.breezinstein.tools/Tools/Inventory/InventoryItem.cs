using System;
using UnityEngine;

namespace Breezinstein.Tools.Inventory
{
    [Serializable]
    public class InventoryItem
    {
        public InventoryItemData data { get; private set; }
        public int stackSize { get; private set; }
        public InventoryItem(InventoryItemData source)
        {
            data = source;
            AddToStack();
        }

        public void AddToStack()
        {
            stackSize++;
        }

        public void AddToStack(int amount)
        {
            stackSize+= amount;
        }

        public void RemoveFromStack()
        {
            stackSize--;
        }
        public void RemoveFromStack(int amount)
        {
            stackSize-=amount;
        }
    }

}