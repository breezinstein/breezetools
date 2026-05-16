using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Breezinstein.Tools.Inventory
{
    /// <summary>
    /// Single inventory slot UI. Paints the item icon, optional rarity tint and a stack
    /// count. Pointer events surface as a UnityEvent-style C# event for higher-level
    /// systems (drag-drop, use-on-click, tooltip) to consume without coupling to UGUI.
    /// </summary>
    public class InventorySlotView : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {
        public delegate void SlotPointerHandler(InventorySlotView slot, PointerEventData eventData);

        public event SlotPointerHandler Clicked;
        public event SlotPointerHandler PointerEntered;
        public event SlotPointerHandler PointerExited;

        [Header("Wiring")]
        [SerializeField] private Image iconImage;
        [SerializeField] private Image rarityFrame;
        [SerializeField] private TMP_Text quantityLabel;
        [SerializeField] private GameObject emptyOverlay;

        [Header("Behaviour")]
        [Tooltip("If true the quantity label is hidden when the stack has only one item.")]
        [SerializeField] private bool hideCountWhenOne = true;

        public int SlotIndex { get; private set; } = -1;
        public ItemStack Stack { get; private set; }
        public bool IsEmpty => Stack == null || Stack.IsEmpty;

        public void SetSlot(int index, ItemStack stack)
        {
            SlotIndex = index;
            Stack = stack;

            bool empty = stack == null || stack.IsEmpty || stack.Data == null;

            if (emptyOverlay != null) emptyOverlay.SetActive(empty);

            if (iconImage != null)
            {
                iconImage.enabled = !empty;
                iconImage.sprite = empty ? null : stack.Data.Icon;
            }

            if (rarityFrame != null)
            {
                if (!empty && stack.Data.Rarity != null)
                {
                    rarityFrame.enabled = true;
                    rarityFrame.color = stack.Data.Rarity.Color;
                }
                else
                {
                    rarityFrame.enabled = false;
                }
            }

            if (quantityLabel != null)
            {
                bool show = !empty && (!hideCountWhenOne || stack.Quantity > 1);
                quantityLabel.enabled = show;
                if (show) quantityLabel.text = stack.Quantity.ToString();
            }
        }

        public void Clear() => SetSlot(SlotIndex, null);

        void IPointerClickHandler.OnPointerClick(PointerEventData eventData) => Clicked?.Invoke(this, eventData);
        void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData) => PointerEntered?.Invoke(this, eventData);
        void IPointerExitHandler.OnPointerExit(PointerEventData eventData) => PointerExited?.Invoke(this, eventData);
    }
}
