using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;

namespace Breezinstein.Tools.UI
{
    [RequireComponent(typeof(Image))]
    [AddComponentMenu("Breeze's Tools/UI/Tab Button")]
    public class TabButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        public TabGroup tabGroup;

        public Image background;

        public UnityEvent onTabSelected;
        public UnityEvent onTabDeselected;

        public void OnPointerClick(PointerEventData eventData)
        {
            tabGroup.OnTabSelected(this);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            tabGroup.OnTabEnter(this);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            tabGroup.OnTabExit(this);
        }

        void Start()
        {
            background = GetComponent<Image>();
            if (tabGroup != null)
            {
                tabGroup.Subscribe(this);
            }
        }

        public void Select()
        {
            if (onTabSelected != null)
            {
                onTabSelected.Invoke();
            }
        }

        public void Deselect()
        {
            if (onTabDeselected != null)
            {
                onTabDeselected.Invoke();
            }
        }
    }
}
