using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using Breezinstein.Tools.Core;

namespace Breezinstein.Tools.Scene
{    [System.Serializable]
    public class SmartGroupingTool : BaseBreezeTool
    {
        public override string ToolName => "Smart Grouping";
        public override string Category => "Scene Organization";
        public override string ToolDescription => "Intelligently group objects by type, tag, layer, or custom criteria";
        public override int Priority => 9;

        private GroupingMode groupingMode = GroupingMode.ByTag;
        private string customGroupName = "Group";
        private bool createEmptyParents = true;
        private bool preserveWorldPosition = true;
        private bool groupOnlySelected = true;
        private string layerFilter = "";
        private string tagFilter = "";
        private string nameFilter = "";
        private bool useRegexFilter = false;
        
        private List<GameObject> objectsToGroup = new List<GameObject>();

        public enum GroupingMode
        {
            ByTag,
            ByLayer,
            ByComponent,
            ByName,
            ByParent,
            Custom
        }

        public override VisualElement CreateUI()
        {
            var root = new VisualElement();

            // Header
            var header = new Label("Smart Grouping Tool");
            header.AddToClassList("header-label");
            root.Add(header);

            var description = new Label("Intelligently organize scene objects into groups based on various criteria");
            description.AddToClassList("description-label");
            root.Add(description);

            // Grouping mode selection
            var modeSection = new Foldout { text = "Grouping Mode", value = true };
            
            var modeField = new EnumField("Group By", groupingMode);
            modeField.RegisterValueChangedCallback(evt => 
            {
                groupingMode = (GroupingMode)evt.newValue;
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
            
            var groupSelectedToggle = new Toggle("Group Only Selected Objects") { value = groupOnlySelected };
            groupSelectedToggle.RegisterValueChangedCallback(evt => groupOnlySelected = evt.newValue);
            optionsSection.Add(groupSelectedToggle);

            var createEmptyToggle = new Toggle("Create Empty Parent Objects") { value = createEmptyParents };
            createEmptyToggle.RegisterValueChangedCallback(evt => createEmptyParents = evt.newValue);
            optionsSection.Add(createEmptyToggle);

            var preservePositionToggle = new Toggle("Preserve World Position") { value = preserveWorldPosition };
            preservePositionToggle.RegisterValueChangedCallback(evt => preserveWorldPosition = evt.newValue);
            optionsSection.Add(preservePositionToggle);

            root.Add(optionsSection);

            // Filters
            var filtersSection = new Foldout { text = "Filters", value = false };
            
            var nameFilterField = new TextField("Name Filter") { value = nameFilter };
            nameFilterField.RegisterValueChangedCallback(evt => nameFilter = evt.newValue);
            filtersSection.Add(nameFilterField);

            var regexToggle = new Toggle("Use Regex") { value = useRegexFilter };
            regexToggle.RegisterValueChangedCallback(evt => useRegexFilter = evt.newValue);
            filtersSection.Add(regexToggle);

            var tagFilterField = new TextField("Tag Filter") { value = tagFilter };
            tagFilterField.RegisterValueChangedCallback(evt => tagFilter = evt.newValue);
            filtersSection.Add(tagFilterField);

            var layerFilterField = new TextField("Layer Filter") { value = layerFilter };
            layerFilterField.RegisterValueChangedCallback(evt => layerFilter = evt.newValue);
            filtersSection.Add(layerFilterField);

            root.Add(filtersSection);

            // Actions
            var actionsContainer = new VisualElement();
            actionsContainer.style.flexDirection = FlexDirection.Row;
            actionsContainer.style.marginTop = 10;

            var previewButton = new Button(() => PreviewGrouping()) { text = "Preview Grouping" };
            previewButton.AddToClassList("secondary-button");
            actionsContainer.Add(previewButton);

            var groupButton = new Button(() => ApplyGrouping()) { text = "Apply Grouping" };
            groupButton.AddToClassList("primary-button");
            actionsContainer.Add(groupButton);

            var ungroupButton = new Button(() => UngroupSelected()) { text = "Ungroup Selected" };
            ungroupButton.AddToClassList("warning-button");
            actionsContainer.Add(ungroupButton);

            root.Add(actionsContainer);

            // Preview results
            var previewSection = new Foldout { text = "Preview", value = false };
            
            var previewLabel = new Label("Click 'Preview Grouping' to see results");
            previewLabel.name = "preview-label";
            previewSection.Add(previewLabel);

            var previewList = new ListView();
            previewList.name = "preview-list";
            previewList.style.height = 200;
            previewList.style.display = DisplayStyle.None;
            previewSection.Add(previewList);

            root.Add(previewSection);

            // Initialize mode-specific UI
            UpdateModeSpecificUI();

            return root;
        }

        private void UpdateModeSpecificUI()
        {
            var container = GetUIElement<VisualElement>("mode-options");
            if (container == null) return;

            container.Clear();

            switch (groupingMode)
            {
                case GroupingMode.Custom:
                    var customNameField = new TextField("Group Name") { value = customGroupName };
                    customNameField.RegisterValueChangedCallback(evt => customGroupName = evt.newValue);
                    container.Add(customNameField);
                    break;                case GroupingMode.ByComponent:
                    var componentInfo = new Label("Objects will be grouped by their component types");
                    componentInfo.AddToClassList("italic-text");
                    container.Add(componentInfo);
                    break;

                case GroupingMode.ByParent:
                    var parentInfo = new Label("Objects will be grouped under their direct parents");
                    parentInfo.AddToClassList("italic-text");
                    container.Add(parentInfo);
                    break;
            }
        }

        private void PreviewGrouping()
        {
            try
            {
                var groups = AnalyzeGrouping();
                UpdatePreviewUI(groups);
                
                Debug.Log($"Preview: Would create {groups.Count} groups");
            }
            catch (Exception e)
            {
                Debug.LogError($"Error previewing grouping: {e.Message}");
            }
        }

        private Dictionary<string, List<GameObject>> AnalyzeGrouping()
        {
            var objects = GetObjectsToGroup();
            var groups = new Dictionary<string, List<GameObject>>();

            foreach (var obj in objects)
            {
                var groupKey = GetGroupKey(obj);
                if (!groups.ContainsKey(groupKey))
                {
                    groups[groupKey] = new List<GameObject>();
                }
                groups[groupKey].Add(obj);
            }

            return groups;
        }

        private List<GameObject> GetObjectsToGroup()
        {            var objects = groupOnlySelected && Selection.gameObjects.Length > 0 
                ? Selection.gameObjects.ToList()
                : UnityEngine.Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None).ToList();

            // Apply filters
            if (!string.IsNullOrEmpty(nameFilter))
            {
                objects = objects.Where(obj => 
                    useRegexFilter 
                        ? System.Text.RegularExpressions.Regex.IsMatch(obj.name, nameFilter)
                        : obj.name.Contains(nameFilter)
                ).ToList();
            }

            if (!string.IsNullOrEmpty(tagFilter))
            {
                objects = objects.Where(obj => obj.CompareTag(tagFilter)).ToList();
            }

            if (!string.IsNullOrEmpty(layerFilter))
            {
                var layerInt = LayerMask.NameToLayer(layerFilter);
                if (layerInt >= 0)
                {
                    objects = objects.Where(obj => obj.layer == layerInt).ToList();
                }
            }

            return objects;
        }

        private string GetGroupKey(GameObject obj)
        {
            switch (groupingMode)
            {
                case GroupingMode.ByTag:
                    return $"Tag_{obj.tag}";
                
                case GroupingMode.ByLayer:
                    return $"Layer_{LayerMask.LayerToName(obj.layer)}";
                
                case GroupingMode.ByComponent:
                    var components = obj.GetComponents<Component>();
                    var componentType = components.Length > 1 ? components[1].GetType().Name : "Transform";
                    return $"Component_{componentType}";
                
                case GroupingMode.ByName:
                    var prefix = ExtractNamePrefix(obj.name);
                    return $"Name_{prefix}";
                
                case GroupingMode.ByParent:
                    var parentName = obj.transform.parent ? obj.transform.parent.name : "Root";
                    return $"Parent_{parentName}";
                
                case GroupingMode.Custom:
                    return customGroupName;
                
                default:
                    return "Ungrouped";
            }
        }

        private string ExtractNamePrefix(string name)
        {
            // Extract common prefix or first word
            var words = name.Split('_', ' ', '-');
            return words.Length > 0 ? words[0] : name;
        }

        private void ApplyGrouping()
        {
            try
            {
                var groups = AnalyzeGrouping();
                
                if (groups.Count == 0)
                {
                    Debug.LogWarning("No objects to group found.");
                    return;
                }

                using (var scope = new UndoScope("Smart Grouping"))
                {
                    int groupedCount = 0;

                    foreach (var group in groups)
                    {
                        if (group.Value.Count <= 1) continue; // Skip single object groups

                        GameObject parentGroup = null;

                        if (createEmptyParents)
                        {
                            parentGroup = new GameObject(group.Key);
                            Undo.RegisterCreatedObjectUndo(parentGroup, "Create Group");
                            
                            // Position the group at the average position
                            if (group.Value.Count > 0)
                            {
                                var avgPosition = group.Value.Aggregate(Vector3.zero, (sum, obj) => sum + obj.transform.position) / group.Value.Count;
                                parentGroup.transform.position = avgPosition;
                            }
                        }
                        else
                        {
                            // Use the first object as the parent
                            parentGroup = group.Value[0];
                            group.Value.RemoveAt(0);
                        }

                        // Parent remaining objects to the group
                        foreach (var obj in group.Value)
                        {
                            Undo.SetTransformParent(obj.transform, parentGroup.transform, "Group Object");
                            
                            if (!preserveWorldPosition)
                            {
                                obj.transform.localPosition = Vector3.zero;
                                obj.transform.localRotation = Quaternion.identity;
                            }
                            
                            groupedCount++;
                        }
                    }                    Debug.Log($"Grouped {groupedCount} objects into {groups.Count(g => g.Value.Count > 1)} groups");
                    LogOperation($"Grouped {groupedCount} objects");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Error applying grouping: {e.Message}");
            }
        }

        private void UngroupSelected()
        {
            var selectedObjects = Selection.gameObjects;
            if (selectedObjects.Length == 0)
            {
                Debug.LogWarning("No objects selected to ungroup.");
                return;
            }

            using (var scope = new UndoScope("Ungroup Objects"))
            {
                int ungroupedCount = 0;

                foreach (var obj in selectedObjects)
                {
                    var children = new List<Transform>();
                    for (int i = 0; i < obj.transform.childCount; i++)
                    {
                        children.Add(obj.transform.GetChild(i));
                    }

                    foreach (var child in children)
                    {
                        Undo.SetTransformParent(child, obj.transform.parent, "Ungroup Object");
                        ungroupedCount++;
                    }

                    // If the object has no components other than Transform, delete it
                    var components = obj.GetComponents<Component>();
                    if (components.Length == 1 && components[0] is Transform)
                    {
                        Undo.DestroyObjectImmediate(obj);
                    }
                }                Debug.Log($"Ungrouped {ungroupedCount} objects");
                LogOperation($"Ungrouped {ungroupedCount} objects");
            }
        }

        private void UpdatePreviewUI(Dictionary<string, List<GameObject>> groups)
        {
            var previewLabel = GetUIElement<Label>("preview-label");
            var previewList = GetUIElement<ListView>("preview-list");

            if (previewLabel != null)
            {
                var totalObjects = groups.Sum(g => g.Value.Count);
                var validGroups = groups.Count(g => g.Value.Count > 1);
                previewLabel.text = $"Would create {validGroups} groups containing {totalObjects} objects";
            }

            if (previewList != null)
            {
                var groupList = groups.Where(g => g.Value.Count > 1).ToList();
                
                if (groupList.Count > 0)
                {
                    previewList.style.display = DisplayStyle.Flex;
                    previewList.itemsSource = groupList;
                    previewList.makeItem = () => new Label();
                    previewList.bindItem = (element, index) =>
                    {
                        var label = element as Label;
                        var group = groupList[index];
                        label.text = $"{group.Key} ({group.Value.Count} objects)";
                    };
                }
                else
                {
                    previewList.style.display = DisplayStyle.None;
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
