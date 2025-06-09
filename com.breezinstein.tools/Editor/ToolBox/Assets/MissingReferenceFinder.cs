using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using Breezinstein.Tools.Core;
using Object = UnityEngine.Object;

namespace Breezinstein.Tools.Assets
{    [System.Serializable]
    public class MissingReferenceFinder : BaseBreezeTool
    {
        public override string ToolName => "Missing Reference Finder";
        public override string Category => "Asset Management";
        public override string ToolDescription => "Find and fix missing script references and component dependencies";
        public override int Priority => 30;

        private bool searchInScene = true;
        private bool searchInPrefabs = true;
        private bool searchInScriptableObjects = true;
        private bool showMissingScripts = true;
        private bool showMissingReferences = true;
        
        private List<MissingReferenceInfo> missingReferences = new List<MissingReferenceInfo>();
        private Vector2 scrollPosition;

        public struct MissingReferenceInfo
        {
            public Object target;
            public string propertyPath;
            public string componentType;
            public string description;
            public MissingReferenceType type;
        }

        public enum MissingReferenceType
        {
            MissingScript,
            MissingReference,
            NullReference
        }

        public override VisualElement CreateUI()
        {
            var root = new VisualElement();

            // Header
            var header = new Label("Missing Reference Finder");
            header.AddToClassList("header-label");
            root.Add(header);

            var description = new Label("Find and fix missing script references and component dependencies");
            description.AddToClassList("description-label");
            root.Add(description);

            // Search options
            var optionsSection = new Foldout { text = "Search Options", value = true };
            
            var searchSceneToggle = new Toggle("Search in Scene") { value = searchInScene };
            searchSceneToggle.RegisterValueChangedCallback(evt => searchInScene = evt.newValue);
            optionsSection.Add(searchSceneToggle);

            var searchPrefabsToggle = new Toggle("Search in Prefabs") { value = searchInPrefabs };
            searchPrefabsToggle.RegisterValueChangedCallback(evt => searchInPrefabs = evt.newValue);
            optionsSection.Add(searchPrefabsToggle);

            var searchSOToggle = new Toggle("Search in ScriptableObjects") { value = searchInScriptableObjects };
            searchSOToggle.RegisterValueChangedCallback(evt => searchInScriptableObjects = evt.newValue);
            optionsSection.Add(searchSOToggle);

            root.Add(optionsSection);

            // Filter options
            var filterSection = new Foldout { text = "Filter Options", value = true };
            
            var showMissingScriptsToggle = new Toggle("Show Missing Scripts") { value = showMissingScripts };
            showMissingScriptsToggle.RegisterValueChangedCallback(evt => showMissingScripts = evt.newValue);
            filterSection.Add(showMissingScriptsToggle);

            var showMissingRefsToggle = new Toggle("Show Missing References") { value = showMissingReferences };
            showMissingRefsToggle.RegisterValueChangedCallback(evt => showMissingReferences = evt.newValue);
            filterSection.Add(showMissingRefsToggle);

            root.Add(filterSection);

            // Actions
            var actionsContainer = new VisualElement();
            actionsContainer.style.flexDirection = FlexDirection.Row;
            actionsContainer.style.marginTop = 10;

            var scanButton = new Button(() => ScanForMissingReferences()) { text = "Scan for Missing References" };
            scanButton.AddToClassList("primary-button");
            actionsContainer.Add(scanButton);

            var cleanupButton = new Button(() => CleanupMissingScripts()) { text = "Remove Missing Scripts" };
            cleanupButton.AddToClassList("warning-button");
            actionsContainer.Add(cleanupButton);

            root.Add(actionsContainer);

            // Results
            var resultsSection = new Foldout { text = "Results", value = true };
            
            var resultsLabel = new Label("No scan performed yet");
            resultsLabel.name = "results-label";
            resultsSection.Add(resultsLabel);

            var resultsList = new ListView();
            resultsList.name = "results-list";
            resultsList.style.height = 300;
            resultsList.style.display = DisplayStyle.None;
            resultsSection.Add(resultsList);

            root.Add(resultsSection);

            // Help section
            var helpSection = new Foldout { text = "Help", value = false };
            var helpText = new Label("• Missing Scripts: Components with null script references\n" +
                                   "• Missing References: Object fields pointing to deleted assets\n" +
                                   "• Use 'Remove Missing Scripts' to clean up null script components\n" +
                                   "• Double-click results to select objects in hierarchy/project");
            helpText.style.whiteSpace = WhiteSpace.Normal;
            helpSection.Add(helpText);
            root.Add(helpSection);

            return root;
        }

        private void ScanForMissingReferences()
        {
            missingReferences.Clear();

            try
            {
                EditorUtility.DisplayProgressBar("Scanning", "Searching for missing references...", 0f);

                if (searchInScene)
                {
                    ScanScene();
                }

                if (searchInPrefabs)
                {
                    ScanPrefabs();
                }

                if (searchInScriptableObjects)
                {
                    ScanScriptableObjects();
                }

                UpdateResultsUI();
                
                var totalMissing = missingReferences.Count;
                var missingScripts = missingReferences.Count(r => r.type == MissingReferenceType.MissingScript);
                var missingRefs = missingReferences.Count(r => r.type == MissingReferenceType.MissingReference);
                
                Debug.Log($"Scan complete: {totalMissing} issues found ({missingScripts} missing scripts, {missingRefs} missing references)");
                LogOperation($"Scan complete: {totalMissing} issues found ({missingScripts} missing scripts, {missingRefs} missing references)");
            }
            catch (Exception e)
            {
                Debug.LogError($"Error scanning for missing references: {e.Message}");
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }

        private void ScanScene()
        {
            var allObjects = Resources.FindObjectsOfTypeAll<GameObject>()
                .Where(go => go.scene.isLoaded && go.hideFlags == HideFlags.None);

            foreach (var obj in allObjects)
            {
                ScanGameObject(obj);
            }
        }

        private void ScanPrefabs()
        {
            var prefabGuids = AssetDatabase.FindAssets("t:Prefab");
            
            for (int i = 0; i < prefabGuids.Length; i++)
            {
                EditorUtility.DisplayProgressBar("Scanning Prefabs", $"Scanning prefab {i + 1}/{prefabGuids.Length}", (float)i / prefabGuids.Length);
                
                var path = AssetDatabase.GUIDToAssetPath(prefabGuids[i]);
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                
                if (prefab != null)
                {
                    ScanGameObject(prefab, true);
                }
            }
        }

        private void ScanScriptableObjects()
        {
            var soGuids = AssetDatabase.FindAssets("t:ScriptableObject");
            
            for (int i = 0; i < soGuids.Length; i++)
            {
                EditorUtility.DisplayProgressBar("Scanning ScriptableObjects", $"Scanning SO {i + 1}/{soGuids.Length}", (float)i / soGuids.Length);
                
                var path = AssetDatabase.GUIDToAssetPath(soGuids[i]);
                var so = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);
                
                if (so != null)
                {
                    ScanObject(so);
                }
            }
        }

        private void ScanGameObject(GameObject obj, bool isPrefab = false)
        {
            var components = obj.GetComponentsInChildren<Component>(true);
            
            foreach (var component in components)
            {
                if (component == null)
                {
                    // Missing script
                    if (showMissingScripts)
                    {
                        missingReferences.Add(new MissingReferenceInfo
                        {
                            target = obj,
                            propertyPath = "Missing Script",
                            componentType = "Unknown",
                            description = $"Missing script on {obj.name}",
                            type = MissingReferenceType.MissingScript
                        });
                    }
                    continue;
                }

                ScanObject(component);
            }
        }

        private void ScanObject(Object obj)
        {
            if (obj == null) return;

            var serializedObject = new SerializedObject(obj);
            var iterator = serializedObject.GetIterator();
            
            while (iterator.NextVisible(true))
            {                if (iterator.propertyType == SerializedPropertyType.ObjectReference)
                {
                    if (iterator.objectReferenceValue == null)
                    {
                        // Missing reference
                        if (showMissingReferences)
                        {
                            missingReferences.Add(new MissingReferenceInfo
                            {
                                target = obj,
                                propertyPath = iterator.propertyPath,
                                componentType = obj.GetType().Name,
                                description = $"Missing reference: {iterator.displayName} in {obj.name}",
                                type = MissingReferenceType.MissingReference
                            });
                        }
                    }
                }
            }
        }

        private void CleanupMissingScripts()
        {
            var missingScripts = missingReferences.Where(r => r.type == MissingReferenceType.MissingScript).ToList();
            
            if (missingScripts.Count == 0)
            {
                Debug.LogWarning("No missing scripts found to clean up.");
                return;
            }

            if (!EditorUtility.DisplayDialog("Remove Missing Scripts", 
                $"This will remove {missingScripts.Count} missing script components. This action cannot be undone. Continue?", 
                "Remove", "Cancel"))
            {
                return;
            }

            using (var scope = new UndoScope("Remove Missing Scripts"))
            {
                int removedCount = 0;

                foreach (var missing in missingScripts)
                {
                    if (missing.target is GameObject gameObject)
                    {
                        var components = gameObject.GetComponents<Component>();
                        for (int i = components.Length - 1; i >= 0; i--)
                        {
                            if (components[i] == null)
                            {
                                Undo.DestroyObjectImmediate(components[i]);
                                removedCount++;
                            }
                        }
                        
                        EditorUtility.SetDirty(gameObject);
                    }
                }

                Debug.Log($"Removed {removedCount} missing script components");
                LogOperation($"Removed {removedCount} missing script components");
            }

            // Refresh the scan
            ScanForMissingReferences();
        }

        private void UpdateResultsUI()
        {
            // Filter results based on current settings
            var filteredResults = missingReferences.Where(r =>
                (r.type == MissingReferenceType.MissingScript && showMissingScripts) ||
                (r.type == MissingReferenceType.MissingReference && showMissingReferences))
                .ToList();

            var resultsLabel = GetUIElement<Label>("results-label");
            var resultsList = GetUIElement<ListView>("results-list");

            if (resultsLabel != null)
            {
                resultsLabel.text = $"Found {filteredResults.Count} issues ({missingReferences.Count(r => r.type == MissingReferenceType.MissingScript)} missing scripts, {missingReferences.Count(r => r.type == MissingReferenceType.MissingReference)} missing references)";
            }

            if (resultsList != null)
            {
                if (filteredResults.Count > 0)
                {
                    resultsList.style.display = DisplayStyle.Flex;
                    resultsList.itemsSource = filteredResults;
                    resultsList.makeItem = () => 
                    {
                        var container = new VisualElement();
                        container.style.flexDirection = FlexDirection.Row;
                        container.style.paddingLeft = 5;
                        container.style.paddingRight = 5;
                        container.style.paddingTop = 2;
                        container.style.paddingBottom = 2;
                        
                        var icon = new Label();
                        icon.style.width = 20;
                        container.Add(icon);
                        
                        var label = new Label();
                        label.style.flexGrow = 1;
                        container.Add(label);
                        
                        return container;
                    };
                    
                    resultsList.bindItem = (element, index) =>
                    {
                        var container = element;
                        var icon = container.Q<Label>();
                        var label = container.ElementAt(1) as Label;
                        var missing = filteredResults[index];
                        
                        icon.text = missing.type == MissingReferenceType.MissingScript ? "⚠" : "❌";
                        label.text = missing.description;
                        
                        // Add click handler to select object
                        container.RegisterCallback<MouseDownEvent>(evt =>
                        {
                            if (evt.clickCount == 2 && missing.target != null)
                            {
                                Selection.activeObject = missing.target;
                                EditorGUIUtility.PingObject(missing.target);
                            }
                        });
                    };
                }
                else
                {
                    resultsList.style.display = DisplayStyle.None;
                }
            }
        }

        private T GetUIElement<T>(string name) where T : VisualElement
        {
            // This would need to be implemented based on how the UI is structured
            // For now, returning null as placeholder
            return null;
        }
    }
}
