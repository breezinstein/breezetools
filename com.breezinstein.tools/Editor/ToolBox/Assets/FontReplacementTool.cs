using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using TMPro;
using Breezinstein.Tools.Core;

namespace Breezinstein.Tools.Assets
{
    [System.Serializable]
    public class FontReplacementTool : BaseBreezeTool
    {
        public override string ToolName => "Font Replacement";
        public override string ToolDescription => "Replace fonts in Text and TextMeshPro components throughout the project";
        public override string Category => "Asset Management";

        private Font oldFont;
        private Font newFont;
        private TMP_FontAsset oldTMPFont;
        private TMP_FontAsset newTMPFont;
        private bool includeInactive = true;
        private bool includeTextComponents = true;
        private bool includeTMPComponents = true;
        private bool searchInPrefabs = true;
        
        private List<Component> foundComponents = new List<Component>();
        private Vector2 scrollPosition;

        public override VisualElement CreateUI()
        {
            var root = new VisualElement();

            // Header
            var header = new Label("Font Replacement Tool");
            header.AddToClassList("header-label");
            root.Add(header);

            var description = new Label("Replace fonts in Text and TextMeshPro components across the project");
            description.AddToClassList("description-label");
            root.Add(description);

            // Font replacement section
            var fontSection = new Foldout { text = "Font Replacement", value = true };
              // Legacy Text fonts
            var textGroup = new VisualElement();
            var textLabel = new Label("Legacy Text Components");
            textLabel.AddToClassList("section-header");
            textGroup.Add(textLabel);
            
            var oldFontField = new ObjectField("Old Font") 
            { 
                objectType = typeof(Font),
                value = oldFont
            };
            oldFontField.RegisterValueChangedCallback(evt => oldFont = evt.newValue as Font);
            textGroup.Add(oldFontField);

            var newFontField = new ObjectField("New Font") 
            { 
                objectType = typeof(Font),
                value = newFont
            };
            newFontField.RegisterValueChangedCallback(evt => newFont = evt.newValue as Font);
            textGroup.Add(newFontField);

            fontSection.Add(textGroup);            // TextMeshPro fonts
            var tmpGroup = new VisualElement();
            var tmpLabel = new Label("TextMeshPro Components");
            tmpLabel.AddToClassList("section-header");
            tmpGroup.Add(tmpLabel);
            
            var oldTMPFontField = new ObjectField("Old TMP Font") 
            { 
                objectType = typeof(TMP_FontAsset),
                value = oldTMPFont
            };
            oldTMPFontField.RegisterValueChangedCallback(evt => oldTMPFont = evt.newValue as TMP_FontAsset);
            tmpGroup.Add(oldTMPFontField);

            var newTMPFontField = new ObjectField("New TMP Font") 
            { 
                objectType = typeof(TMP_FontAsset),
                value = newTMPFont
            };
            newTMPFontField.RegisterValueChangedCallback(evt => newTMPFont = evt.newValue as TMP_FontAsset);
            tmpGroup.Add(newTMPFontField);

            fontSection.Add(tmpGroup);
            root.Add(fontSection);

            // Options
            var optionsSection = new Foldout { text = "Options", value = true };
            
            var includeInactiveToggle = new Toggle("Include Inactive Objects") { value = includeInactive };
            includeInactiveToggle.RegisterValueChangedCallback(evt => includeInactive = evt.newValue);
            optionsSection.Add(includeInactiveToggle);

            var includeTextToggle = new Toggle("Include Text Components") { value = includeTextComponents };
            includeTextToggle.RegisterValueChangedCallback(evt => includeTextComponents = evt.newValue);
            optionsSection.Add(includeTextToggle);

            var includeTMPToggle = new Toggle("Include TextMeshPro Components") { value = includeTMPComponents };
            includeTMPToggle.RegisterValueChangedCallback(evt => includeTMPComponents = evt.newValue);
            optionsSection.Add(includeTMPToggle);

            var searchPrefabsToggle = new Toggle("Search in Prefabs") { value = searchInPrefabs };
            searchPrefabsToggle.RegisterValueChangedCallback(evt => searchInPrefabs = evt.newValue);
            optionsSection.Add(searchPrefabsToggle);

            root.Add(optionsSection);

            // Actions
            var actionsContainer = new VisualElement();
            actionsContainer.style.flexDirection = FlexDirection.Row;
            actionsContainer.style.marginTop = 10;

            var findButton = new Button(() => FindFonts()) { text = "Find Fonts" };
            findButton.AddToClassList("primary-button");
            actionsContainer.Add(findButton);

            var replaceButton = new Button(() => ReplaceFonts()) { text = "Replace Fonts" };
            replaceButton.AddToClassList("secondary-button");
            actionsContainer.Add(replaceButton);

            root.Add(actionsContainer);

            // Results
            var resultsSection = new Foldout { text = "Results", value = false };
            
            var resultsLabel = new Label("Found components will appear here");
            resultsLabel.name = "results-label";
            resultsSection.Add(resultsLabel);

            var resultsList = new ListView();
            resultsList.name = "results-list";
            resultsList.style.height = 200;
            resultsList.style.display = DisplayStyle.None;
            resultsSection.Add(resultsList);

            root.Add(resultsSection);

            return root;
        }

        private void FindFonts()
        {
            foundComponents.Clear();

            try
            {
                if (searchInPrefabs)
                {
                    FindFontsInPrefabs();
                }

                FindFontsInScene();

                UpdateResultsUI();
            }
            catch (Exception e)
            {
                Debug.LogError($"Error finding fonts: {e.Message}");
            }
        }

        private void FindFontsInScene()
        {            var allObjects = includeInactive ? 
                Resources.FindObjectsOfTypeAll<GameObject>().Where(go => go.scene.isLoaded) :
                UnityEngine.Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None);

            foreach (var obj in allObjects)
            {
                if (includeTextComponents)
                {
                    var textComponents = obj.GetComponentsInChildren<UnityEngine.UI.Text>(includeInactive);
                    foreach (var text in textComponents)
                    {
                        if ((oldFont != null && text.font == oldFont) || oldFont == null)
                        {
                            foundComponents.Add(text);
                        }
                    }
                }

                if (includeTMPComponents)
                {
                    var tmpComponents = obj.GetComponentsInChildren<TMP_Text>(includeInactive);
                    foreach (var tmp in tmpComponents)
                    {
                        if ((oldTMPFont != null && tmp.font == oldTMPFont) || oldTMPFont == null)
                        {
                            foundComponents.Add(tmp);
                        }
                    }
                }
            }
        }

        private void FindFontsInPrefabs()
        {
            var prefabGuids = AssetDatabase.FindAssets("t:Prefab");
            
            foreach (var guid in prefabGuids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                
                if (prefab == null) continue;

                if (includeTextComponents)
                {
                    var textComponents = prefab.GetComponentsInChildren<UnityEngine.UI.Text>(true);
                    foreach (var text in textComponents)
                    {
                        if ((oldFont != null && text.font == oldFont) || oldFont == null)
                        {
                            foundComponents.Add(text);
                        }
                    }
                }

                if (includeTMPComponents)
                {
                    var tmpComponents = prefab.GetComponentsInChildren<TMP_Text>(true);
                    foreach (var tmp in tmpComponents)
                    {
                        if ((oldTMPFont != null && tmp.font == oldTMPFont) || oldTMPFont == null)
                        {
                            foundComponents.Add(tmp);
                        }
                    }
                }
            }
        }

        private void ReplaceFonts()
        {
            if (foundComponents.Count == 0)
            {
                Debug.LogWarning("No components found. Please run 'Find Fonts' first.");
                return;
            }

            using (var scope = new UndoScope("Replace Fonts"))
            {
                int replacedCount = 0;

                foreach (var component in foundComponents)
                {
                    if (component is UnityEngine.UI.Text textComponent && newFont != null)
                    {
                        Undo.RecordObject(textComponent, "Replace Font");
                        textComponent.font = newFont;
                        EditorUtility.SetDirty(textComponent);
                        replacedCount++;
                    }
                    else if (component is TMP_Text tmpComponent && newTMPFont != null)
                    {
                        Undo.RecordObject(tmpComponent, "Replace TMP Font");
                        tmpComponent.font = newTMPFont;
                        EditorUtility.SetDirty(tmpComponent);
                        replacedCount++;
                    }
                }                Debug.Log($"Replaced fonts in {replacedCount} components");
                // BreezeEvents.TriggerToolCompleted(ToolName, $"Replaced fonts in {replacedCount} components");
            }

            // Refresh the search results
            FindFonts();
        }

        private void UpdateResultsUI()
        {
            // This would be called from the UI update cycle
            var resultsLabel = GetUIElement<Label>("results-label");
            var resultsList = GetUIElement<ListView>("results-list");

            if (resultsLabel != null)
            {
                resultsLabel.text = $"Found {foundComponents.Count} components";
            }

            if (resultsList != null)
            {
                if (foundComponents.Count > 0)
                {
                    resultsList.style.display = DisplayStyle.Flex;
                    resultsList.itemsSource = foundComponents;
                    resultsList.makeItem = () => new Label();
                    resultsList.bindItem = (element, index) =>
                    {
                        var label = element as Label;
                        var component = foundComponents[index];
                        var componentType = component is UnityEngine.UI.Text ? "Text" : "TextMeshPro";
                        label.text = $"{componentType}: {component.gameObject.name} ({GetAssetPath(component)})";
                    };
                }
                else
                {
                    resultsList.style.display = DisplayStyle.None;
                }
            }
        }

        private string GetAssetPath(Component component)
        {
            var prefab = PrefabUtility.GetCorrespondingObjectFromOriginalSource(component);
            if (prefab != null)
            {
                return AssetDatabase.GetAssetPath(prefab);
            }
            return component.gameObject.scene.name;
        }

        private T GetUIElement<T>(string name) where T : VisualElement
        {
            // This would need to be implemented based on how the UI is structured
            // For now, returning null as placeholder
            return null;
        }
    }
}
