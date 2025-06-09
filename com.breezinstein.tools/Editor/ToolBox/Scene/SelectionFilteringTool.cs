using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using Breezinstein.Tools.Core;

namespace Breezinstein.Tools.Scene
{
    [System.Serializable]
    public class SelectionFilteringTool : BaseBreezeTool
    {
        public override string ToolName => "Selection & Filtering";
        public override string Category => "Scene Organization";
        public override string ToolDescription => "Advanced selection and filtering tools for scene objects";
        public override int Priority => 8;

        private FilterMode filterMode = FilterMode.ByComponent;
        private string componentTypeName = "";
        private string namePattern = "";
        private string tagFilter = "";
        private int layerFilter = 0;
        private bool useRegex = false;
        private bool includeInactive = true;
        private bool includeChildren = false;
        private Vector3 minBounds = Vector3.negativeInfinity;
        private Vector3 maxBounds = Vector3.positiveInfinity;
        private bool useBoundsFilter = false;
        
        private List<GameObject> filteredObjects = new List<GameObject>();
        private List<SelectionSet> savedSelections = new List<SelectionSet>();

        public enum FilterMode
        {
            ByComponent,
            ByName,
            ByTag,
            ByLayer,
            ByBounds,
            Custom
        }

        [System.Serializable]
        public class SelectionSet
        {
            public string name;
            public List<int> objectInstanceIDs;
            public DateTime created;

            public SelectionSet(string name, GameObject[] objects)
            {
                this.name = name;
                this.objectInstanceIDs = objects.Select(obj => obj.GetInstanceID()).ToList();
                this.created = DateTime.Now;
            }
        }

        public override VisualElement CreateUI()
        {
            var root = new VisualElement();

            // Header
            var header = new Label("Selection & Filtering Tool");
            header.AddToClassList("header-label");
            root.Add(header);

            var description = new Label("Advanced tools for selecting and filtering scene objects");
            description.AddToClassList("description-label");
            root.Add(description);

            // Filter mode selection
            var modeSection = new Foldout { text = "Filter Mode", value = true };
            
            var modeField = new EnumField("Filter By", filterMode);
            modeField.RegisterValueChangedCallback(evt => 
            {
                filterMode = (FilterMode)evt.newValue;
                UpdateModeSpecificUI();
            });
            modeSection.Add(modeField);

            root.Add(modeSection);

            // Mode-specific options container
            var modeOptionsContainer = new VisualElement();
            modeOptionsContainer.name = "mode-options";
            root.Add(modeOptionsContainer);

            // General options
            var optionsSection = new Foldout { text = "Options", value = true };
            
            var includeInactiveToggle = new Toggle("Include Inactive Objects") { value = includeInactive };
            includeInactiveToggle.RegisterValueChangedCallback(evt => includeInactive = evt.newValue);
            optionsSection.Add(includeInactiveToggle);

            var includeChildrenToggle = new Toggle("Include Children") { value = includeChildren };
            includeChildrenToggle.RegisterValueChangedCallback(evt => includeChildren = evt.newValue);
            optionsSection.Add(includeChildrenToggle);

            root.Add(optionsSection);

            // Quick filters
            var quickFiltersSection = new Foldout { text = "Quick Filters", value = false };
            
            var quickButtonsContainer = new VisualElement();
            quickButtonsContainer.style.flexDirection = FlexDirection.Row;
            quickButtonsContainer.style.flexWrap = Wrap.Wrap;            var quickFilters = new[]
            {
                ("Cameras", (System.Action)(() => SelectByComponent<Camera>())),
                ("Lights", (System.Action)(() => SelectByComponent<Light>())),
                ("Renderers", (System.Action)(() => SelectByComponent<Renderer>())),
                ("Colliders", (System.Action)(() => SelectByComponent<Collider>())),
                ("Empty Objects", (System.Action)(() => SelectEmptyObjects())),
                ("Missing Scripts", (System.Action)(() => SelectMissingScripts()))
            };

            foreach (var quickFilter in quickFilters)
            {
                var button = new Button(quickFilter.Item2) { text = quickFilter.Item1 };
                button.AddToClassList("quick-filter-button");
                quickButtonsContainer.Add(button);
            }

            quickFiltersSection.Add(quickButtonsContainer);
            root.Add(quickFiltersSection);

            // Actions
            var actionsContainer = new VisualElement();
            actionsContainer.style.flexDirection = FlexDirection.Row;
            actionsContainer.style.marginTop = 10;

            var filterButton = new Button(() => ApplyFilter()) { text = "Apply Filter" };
            filterButton.AddToClassList("primary-button");
            actionsContainer.Add(filterButton);

            var selectButton = new Button(() => SelectFiltered()) { text = "Select Filtered" };
            selectButton.AddToClassList("secondary-button");
            actionsContainer.Add(selectButton);

            var clearButton = new Button(() => ClearSelection()) { text = "Clear Selection" };
            clearButton.AddToClassList("secondary-button");
            actionsContainer.Add(clearButton);

            root.Add(actionsContainer);

            // Selection management
            var selectionSection = new Foldout { text = "Selection Management", value = false };
            
            var saveContainer = new VisualElement();
            saveContainer.style.flexDirection = FlexDirection.Row;
            
            var selectionNameField = new TextField("Selection Name") { value = "New Selection" };
            saveContainer.Add(selectionNameField);
            
            var saveButton = new Button(() => SaveCurrentSelection(selectionNameField.value)) { text = "Save" };
            saveContainer.Add(saveButton);
            
            selectionSection.Add(saveContainer);

            var savedSelectionsList = new ListView();
            savedSelectionsList.name = "saved-selections";
            savedSelectionsList.style.height = 150;
            selectionSection.Add(savedSelectionsList);

            root.Add(selectionSection);

            // Results
            var resultsSection = new Foldout { text = "Results", value = true };
            
            var resultsLabel = new Label("No filter applied");
            resultsLabel.name = "results-label";
            resultsSection.Add(resultsLabel);

            var resultsList = new ListView();
            resultsList.name = "results-list";
            resultsList.style.height = 200;
            resultsList.style.display = DisplayStyle.None;
            resultsSection.Add(resultsList);

            root.Add(resultsSection);

            // Initialize mode-specific UI
            UpdateModeSpecificUI();
            UpdateSavedSelectionsList();

            return root;
        }

        private void UpdateModeSpecificUI()
        {
            var container = GetUIElement<VisualElement>("mode-options");
            if (container == null) return;

            container.Clear();

            switch (filterMode)
            {
                case FilterMode.ByComponent:
                    var componentField = new TextField("Component Type") { value = componentTypeName };
                    componentField.RegisterValueChangedCallback(evt => componentTypeName = evt.newValue);
                    container.Add(componentField);
                    
                    var componentHelp = new Label("Enter component type name (e.g., 'MeshRenderer', 'BoxCollider')");
                    componentHelp.style.fontSize = 11;
                    componentHelp.style.color = Color.gray;
                    container.Add(componentHelp);
                    break;

                case FilterMode.ByName:
                    var nameField = new TextField("Name Pattern") { value = namePattern };
                    nameField.RegisterValueChangedCallback(evt => namePattern = evt.newValue);
                    container.Add(nameField);

                    var regexToggle = new Toggle("Use Regex") { value = useRegex };
                    regexToggle.RegisterValueChangedCallback(evt => useRegex = evt.newValue);
                    container.Add(regexToggle);
                    break;

                case FilterMode.ByTag:
                    var tagField = new TextField("Tag") { value = tagFilter };
                    tagField.RegisterValueChangedCallback(evt => tagFilter = evt.newValue);
                    container.Add(tagField);
                    break;                case FilterMode.ByLayer:
                    var layerField = new IntegerField("Layer") { value = layerFilter };
                    layerField.RegisterValueChangedCallback(evt => layerFilter = evt.newValue);
                    container.Add(layerField);
                    break;

                case FilterMode.ByBounds:
                    var boundsToggle = new Toggle("Use Bounds Filter") { value = useBoundsFilter };
                    boundsToggle.RegisterValueChangedCallback(evt => useBoundsFilter = evt.newValue);
                    container.Add(boundsToggle);

                    var minBoundsField = new Vector3Field("Min Bounds") { value = minBounds };
                    minBoundsField.RegisterValueChangedCallback(evt => minBounds = evt.newValue);
                    container.Add(minBoundsField);

                    var maxBoundsField = new Vector3Field("Max Bounds") { value = maxBounds };
                    maxBoundsField.RegisterValueChangedCallback(evt => maxBounds = evt.newValue);
                    container.Add(maxBoundsField);
                    break;
            }
        }        private void ApplyFilter()
        {
            try
            {
                filteredObjects.Clear();                var allObjects = includeInactive ? 
                    Resources.FindObjectsOfTypeAll<GameObject>().Where(go => go.scene.isLoaded) :
                    UnityEngine.Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None);

                foreach (var obj in allObjects)
                {
                    if (PassesFilter(obj))
                    {
                        filteredObjects.Add(obj);
                        
                        if (includeChildren)
                        {
                            var children = obj.GetComponentsInChildren<Transform>(includeInactive)
                                .Select(t => t.gameObject)
                                .Where(child => child != obj && PassesFilter(child));
                            filteredObjects.AddRange(children);
                        }
                    }
                }

                // Remove duplicates
                filteredObjects = filteredObjects.Distinct().ToList();

                UpdateResultsUI();
                Debug.Log($"Filter applied: {filteredObjects.Count} objects found");
            }
            catch (Exception e)
            {
                Debug.LogError($"Error applying filter: {e.Message}");
            }
        }

        private bool PassesFilter(GameObject obj)
        {
            switch (filterMode)
            {
                case FilterMode.ByComponent:
                    if (string.IsNullOrEmpty(componentTypeName)) return false;
                    var componentType = System.Type.GetType($"UnityEngine.{componentTypeName}") ?? 
                                      System.Type.GetType(componentTypeName);
                    return componentType != null && obj.GetComponent(componentType) != null;

                case FilterMode.ByName:
                    if (string.IsNullOrEmpty(namePattern)) return true;
                    return useRegex ? 
                        System.Text.RegularExpressions.Regex.IsMatch(obj.name, namePattern) :
                        obj.name.Contains(namePattern);

                case FilterMode.ByTag:
                    return string.IsNullOrEmpty(tagFilter) || obj.CompareTag(tagFilter);

                case FilterMode.ByLayer:
                    return obj.layer == layerFilter;

                case FilterMode.ByBounds:
                    if (!useBoundsFilter) return true;
                    var pos = obj.transform.position;
                    return pos.x >= minBounds.x && pos.x <= maxBounds.x &&
                           pos.y >= minBounds.y && pos.y <= maxBounds.y &&
                           pos.z >= minBounds.z && pos.z <= maxBounds.z;

                default:
                    return true;
            }
        }

        private void SelectByComponent<T>() where T : Component        {
            var objects = includeInactive ? 
                Resources.FindObjectsOfTypeAll<T>().Where(c => c.gameObject.scene.isLoaded).Select(c => c.gameObject) :
                UnityEngine.Object.FindObjectsByType<T>(FindObjectsSortMode.None).Select(c => c.gameObject);
            
            filteredObjects = objects.Distinct().ToList();
            UpdateResultsUI();
            SelectFiltered();
        }        private void SelectEmptyObjects()
        {
            var allObjects = includeInactive ? 
                Resources.FindObjectsOfTypeAll<GameObject>().Where(go => go.scene.isLoaded) :
                UnityEngine.Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None);

            filteredObjects = allObjects.Where(obj => 
            {
                var components = obj.GetComponents<Component>();
                return components.Length == 1 && components[0] is Transform && obj.transform.childCount == 0;
            }).ToList();

            UpdateResultsUI();
            SelectFiltered();
        }

        private void SelectMissingScripts()
        {
            var allObjects = Resources.FindObjectsOfTypeAll<GameObject>()
                .Where(go => go.scene.isLoaded);

            filteredObjects = allObjects.Where(obj =>
            {
                var components = obj.GetComponents<Component>();
                return components.Any(c => c == null);
            }).ToList();

            UpdateResultsUI();
            SelectFiltered();
        }

        private void SelectFiltered()
        {
            if (filteredObjects.Count == 0)
            {
                Debug.LogWarning("No objects to select. Apply a filter first.");
                return;
            }

            Selection.objects = filteredObjects.ToArray();
            Debug.Log($"Selected {filteredObjects.Count} objects");
        }

        private void ClearSelection()
        {
            Selection.objects = new UnityEngine.Object[0];
        }

        private void SaveCurrentSelection(string name)
        {
            if (Selection.gameObjects.Length == 0)
            {
                Debug.LogWarning("No objects selected to save.");
                return;
            }

            if (string.IsNullOrEmpty(name))
            {
                name = $"Selection {savedSelections.Count + 1}";
            }

            var selectionSet = new SelectionSet(name, Selection.gameObjects);
            savedSelections.Add(selectionSet);
            
            UpdateSavedSelectionsList();
            Debug.Log($"Saved selection '{name}' with {Selection.gameObjects.Length} objects");
        }

        private void LoadSelection(SelectionSet selectionSet)
        {
            var objects = selectionSet.objectInstanceIDs
                .Select(id => EditorUtility.InstanceIDToObject(id) as GameObject)
                .Where(obj => obj != null)
                .ToArray();

            Selection.objects = objects;
            Debug.Log($"Loaded selection '{selectionSet.name}' with {objects.Length} objects");
        }

        private void DeleteSelection(SelectionSet selectionSet)
        {
            savedSelections.Remove(selectionSet);
            UpdateSavedSelectionsList();
        }

        private void UpdateResultsUI()
        {
            var resultsLabel = GetUIElement<Label>("results-label");
            var resultsList = GetUIElement<ListView>("results-list");

            if (resultsLabel != null)
            {
                resultsLabel.text = $"Found {filteredObjects.Count} objects";
            }

            if (resultsList != null)
            {
                if (filteredObjects.Count > 0)
                {
                    resultsList.style.display = DisplayStyle.Flex;
                    resultsList.itemsSource = filteredObjects;
                    resultsList.makeItem = () => new Label();
                    resultsList.bindItem = (element, index) =>
                    {
                        var label = element as Label;
                        var obj = filteredObjects[index];
                        label.text = $"{obj.name} ({obj.GetType().Name})";
                        
                        // Add click handler to ping object
                        label.RegisterCallback<MouseDownEvent>(evt =>
                        {
                            if (evt.clickCount == 2)
                            {
                                Selection.activeGameObject = obj;
                                EditorGUIUtility.PingObject(obj);
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

        private void UpdateSavedSelectionsList()
        {
            var savedSelectionsList = GetUIElement<ListView>("saved-selections");
            if (savedSelectionsList == null) return;

            savedSelectionsList.itemsSource = savedSelections;
            savedSelectionsList.makeItem = () =>
            {
                var container = new VisualElement();
                container.style.flexDirection = FlexDirection.Row;
                container.style.paddingLeft = 5;
                container.style.paddingRight = 5;
                
                var label = new Label();
                label.style.flexGrow = 1;
                container.Add(label);
                
                var loadButton = new Button() { text = "Load" };
                loadButton.style.width = 50;
                container.Add(loadButton);
                
                var deleteButton = new Button() { text = "×" };
                deleteButton.style.width = 25;
                container.Add(deleteButton);
                
                return container;
            };
            
            savedSelectionsList.bindItem = (element, index) =>
            {
                var container = element;
                var label = container.Q<Label>();
                var loadButton = container.Q<Button>("Load");
                var deleteButton = container.ElementAt(2) as Button;
                var selection = savedSelections[index];
                
                label.text = $"{selection.name} ({selection.objectInstanceIDs.Count} objects)";
                
                loadButton.clicked += () => LoadSelection(selection);
                deleteButton.clicked += () => DeleteSelection(selection);
            };
        }

        private T GetUIElement<T>(string name) where T : VisualElement
        {
            // This would need to be implemented based on how the UI is structured
            // For now, returning null as placeholder
            return null;
        }
    }
}
