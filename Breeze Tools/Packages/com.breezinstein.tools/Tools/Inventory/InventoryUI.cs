using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Breezinstein.Tools.Inventory
{
    public class InventoryUI : MonoBehaviour
    {
        public Transform itemsHolder;

        [SerializeField]
        Inventory inventory;
        InventoryUISlot[] slots; 
        void Start()
        {
            inventory.onInventoryChanged += UpdateUI;
            slots = itemsHolder.GetComponentsInChildren<InventoryUISlot>();
        }

        private void UpdateUI()
        {
            Debug.Log("Updating UI");
            for (int i = 0; i < slots.Length; i++)
            {

            }
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}