using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Breezinstein.Tools.Inventory
{
    public class InventoryUISlot : MonoBehaviour
    {

        public Image icon;
        InventoryItem item;

        public void AddItem(InventoryItem newItem)
        {
            item = newItem;

            icon.sprite = item.data.icon;
            icon.enabled = true;
        }

        public void ClearSlot()
        {
            item = null;

            icon.sprite = null;
            icon.enabled = false;
        }
    }
}