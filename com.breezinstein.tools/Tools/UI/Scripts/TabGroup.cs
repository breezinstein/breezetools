using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Breezinstein.Tools.UI
{
    [AddComponentMenu("Breeze's Tools/UI/Tab Button")]
    public class TabGroup : MonoBehaviour
    {
        public List<TabButton> tabButtons;
        public List<GameObject> pages;

        public Color tabIdleColor;
        public Color tabHoverColor;
        public Color tabSelectedColor;
        public TabButton selectedTab;

        public void Start()
        {
            if (selectedTab != null)
            {
                OnTabSelected(selectedTab);
            }
        }
        public void Subscribe(TabButton button)
        {
            tabButtons.Add(button);
            // Sort by order in hierarchy
            tabButtons.Sort((x, y) => x.transform.GetSiblingIndex().CompareTo(y.transform.GetSiblingIndex()));
            ResetTabs();
        }

        public void OnTabEnter(TabButton button)
        {
            ResetTabs();
            if (selectedTab == null || button != selectedTab)
            {
                button.background.color = tabHoverColor;
            }
        }

        public void OnTabExit(TabButton button)
        {
            ResetTabs();
        }

        public void OnTabSelected(TabButton button)
        {
            if (selectedTab != null)
            {
                selectedTab.Deselect();
            }
            selectedTab = button;
            selectedTab.Select();
            ResetTabs();
            button.background.color = tabSelectedColor;
            int index = button.transform.GetSiblingIndex();
            for (int i = 0; i < pages.Count; i++)
            {
                pages[i].SetActive(i == index);
            }
        }

        public void ResetTabs()
        {
            foreach (TabButton button in tabButtons)
            {
                if (selectedTab != null && button == selectedTab)
                {
                    continue;
                }
                button.background.color = tabIdleColor;
            }
        }

        public void NextTab()
        {
            int currentIndex = selectedTab.transform.GetSiblingIndex();
            int nextIndex = currentIndex < tabButtons.Count - 1 ? currentIndex + 1 : tabButtons.Count - 1;
            OnTabSelected(tabButtons[nextIndex]);
        }

        public void PreviousTab()
        {
            int currentIndex = selectedTab.transform.GetSiblingIndex();
            int previousIndex = currentIndex > 0 ? currentIndex - 1 : 0;
            OnTabSelected(tabButtons[previousIndex]);
        }

        public TabButton GetTabByName(string name)
        {
            return tabButtons.Find(tab => tab.name == name);
        }
    }
}