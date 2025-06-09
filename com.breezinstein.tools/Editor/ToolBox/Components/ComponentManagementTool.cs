using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Breezinstein.Tools.Core;

namespace Breezinstein.Tools.Components
{
    public class ComponentManagementTool : BaseBreezeTool
    {
        public override string ToolName => "Component Management";
        public override string ToolDescription => "Batch add, remove, edit, and copy component values";
        public override string Category => "Component Management";
        public override int Priority => 10;
        
        private Type _selectedComponentType;
        private Component _sourceComponent;
        private bool _includeChildren = false;
        private bool _overwriteValues = true;
        private List<string> _propertiesToCopy = new List<string>();
        
        public override VisualElement CreateUI()
        {
            var container = new VisualElement();
            container.AddToClassList("tool-section");
            
            CreateComponentSelectionSection(container);
            CreateAddRemoveSection(container);
            CreateCopyPasteSection(container);
            CreateFindSection(container);
            CreateOptionsSection(container);
            
            return container;
        }
        
        private void CreateComponentSelectionSection(VisualElement parent)
        {
            var section = new VisualElement();
            section.AddToClassList("tool-section");
            
            var header = new Label("Component Type Selection");
            header.AddToClassList("tool-section-header");
            section.Add(header);
            
            var componentField = new TextField("Component Type");
            componentField.value = _selectedComponentType?.Name ?? "";
            componentField.RegisterValueChangedCallback(evt => 
            {
                _selectedComponentType = FindComponentType(evt.newValue);
            });
            section.Add(componentField);
            
            var helpLabel = new Label("Enter component name (e.g., 'Rigidbody', 'BoxCollider', 'MeshRenderer')");
            helpLabel.AddToClassList("tool-description");
            section.Add(helpLabel);
            
            parent.Add(section);
        }
        
        private void CreateAddRemoveSection(VisualElement parent)
        {
            var section = new VisualElement();
            section.AddToClassList("tool-section");
            
            var header = new Label("Add/Remove Components");
            header.AddToClassList("tool-section-header");
            section.Add(header);
            
            var buttonRow = new VisualElement();
            buttonRow.AddToClassList("button-row");
            
            buttonRow.Add(new Button(() => AddComponentToSelection()) { text = "Add Component" });
            buttonRow.Add(new Button(() => RemoveComponentFromSelection()) { text = "Remove Component" });
            
            section.Add(buttonRow);
            
            var buttonRow2 = new VisualElement();
            buttonRow2.AddToClassList("button-row");
            
            buttonRow2.Add(new Button(() => ToggleComponentEnabled()) { text = "Toggle Enabled" });
            buttonRow2.Add(new Button(() => ResetComponent()) { text = "Reset Values" });
            
            section.Add(buttonRow2);
            parent.Add(section);
        }
        
        private void CreateCopyPasteSection(VisualElement parent)
        {
            var section = new VisualElement();
            section.AddToClassList("tool-section");
            
            var header = new Label("Copy/Paste Component Values");
            header.AddToClassList("tool-section-header");
            section.Add(header);
            
            var sourceField = new ObjectField("Source Component");
            sourceField.objectType = typeof(Component);
            sourceField.value = _sourceComponent;
            sourceField.RegisterValueChangedCallback(evt => 
            {
                _sourceComponent = evt.newValue as Component;
                UpdatePropertyList();
            });
            section.Add(sourceField);
            
            var overwriteToggle = new Toggle("Overwrite Existing Values");
            overwriteToggle.value = _overwriteValues;
            overwriteToggle.RegisterValueChangedCallback(evt => _overwriteValues = evt.newValue);
            section.Add(overwriteToggle);
            
            var buttonRow = new VisualElement();
            buttonRow.AddToClassList("button-row");
            
            buttonRow.Add(new Button(() => CopyComponentFromSelection()) { text = "Copy from Selection" });
            buttonRow.Add(new Button(() => PasteComponentToSelection()) { text = "Paste to Selection" });
            
            section.Add(buttonRow);
            parent.Add(section);
        }
        
        private void CreateFindSection(VisualElement parent)
        {
            var section = new VisualElement();
            section.AddToClassList("tool-section");
            
            var header = new Label("Find Components");
            header.AddToClassList("tool-section-header");
            section.Add(header);
            
            var buttonRow = new VisualElement();
            buttonRow.AddToClassList("button-row");
            
            buttonRow.Add(new Button(() => FindComponentsInScene()) { text = "Find in Scene" });
            buttonRow.Add(new Button(() => FindComponentsInProject()) { text = "Find in Project" });
            
            section.Add(buttonRow);
            
            var buttonRow2 = new VisualElement();
            buttonRow2.AddToClassList("button-row");
            
            buttonRow2.Add(new Button(() => SelectObjectsWithComponent()) { text = "Select Objects" });
            buttonRow2.Add(new Button(() => ShowComponentReport()) { text = "Component Report" });
            
            section.Add(buttonRow2);
            parent.Add(section);
        }
        
        private void CreateOptionsSection(VisualElement parent)
        {
            var section = new VisualElement();
            section.AddToClassList("tool-section");
            
            var header = new Label("Options");
            header.AddToClassList("tool-section-header");
            section.Add(header);
            
            var childrenToggle = new Toggle("Include Children");
            childrenToggle.value = _includeChildren;
            childrenToggle.RegisterValueChangedCallback(evt => _includeChildren = evt.newValue);
            section.Add(childrenToggle);
            
            parent.Add(section);
        }
        
        public override bool CanExecute()
        {
            return Selection.gameObjects.Length > 0;
        }
        
        private Type FindComponentType(string typeName)
        {
            if (string.IsNullOrEmpty(typeName)) return null;
            
            // Try common Unity component types first
            var unityType = Type.GetType($"UnityEngine.{typeName}, UnityEngine");
            if (unityType != null && typeof(Component).IsAssignableFrom(unityType))
                return unityType;
            
            // Search all loaded assemblies
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                var type = assembly.GetTypes()
                    .FirstOrDefault(t => t.Name.Equals(typeName, StringComparison.OrdinalIgnoreCase) 
                                     && typeof(Component).IsAssignableFrom(t));
                if (type != null) return type;
            }
            
            return null;
        }
        
        private void AddComponentToSelection()
        {
            if (_selectedComponentType == null)
            {
                Debug.LogError("No component type selected");
                return;
            }
            
            var targetObjects = GetTargetObjects();
            if (targetObjects.Length == 0) return;
            
            using (var undoScope = new UndoScope($"Add {_selectedComponentType.Name}"))
            {
                var addedComponents = new List<Component>();
                
                foreach (var obj in targetObjects)
                {
                    if (obj.GetComponent(_selectedComponentType) == null)
                    {
                        var component = obj.AddComponent(_selectedComponentType);
                        addedComponents.Add(component);                        undoScope.RegisterCreatedObject(component);
                    }
                }
            }
            
            Debug.Log($"Added {_selectedComponentType.Name} to {targetObjects.Length} objects");
        }
        
        private void RemoveComponentFromSelection()
        {
            if (_selectedComponentType == null)
            {
                Debug.LogError("No component type selected");
                return;
            }
            
            var targetObjects = GetTargetObjects();
            if (targetObjects.Length == 0) return;
            
            using (var undoScope = new UndoScope($"Remove {_selectedComponentType.Name}"))
            {
                var removedComponents = new List<Component>();
                
                foreach (var obj in targetObjects)
                {
                    var component = obj.GetComponent(_selectedComponentType);
                    if (component != null)
                    {
                        removedComponents.Add(component);
                        undoScope.DestroyObject(component);                    }
                }
            }
            
            Debug.Log($"Removed {_selectedComponentType.Name} from {targetObjects.Length} objects");
        }
        
        private void ToggleComponentEnabled()
        {
            if (_selectedComponentType == null)
            {
                Debug.LogError("No component type selected");
                return;
            }
            
            var targetObjects = GetTargetObjects();
            if (targetObjects.Length == 0) return;
            
            // Check if component type has an 'enabled' property
            var enabledProperty = _selectedComponentType.GetProperty("enabled", typeof(bool));
            if (enabledProperty == null)
            {
                Debug.LogError($"{_selectedComponentType.Name} does not have an 'enabled' property");
                return;
            }
            
            using (var undoScope = new UndoScope($"Toggle {_selectedComponentType.Name} Enabled"))
            {
                var modifiedComponents = new List<Component>();
                
                foreach (var obj in targetObjects)
                {
                    var component = obj.GetComponent(_selectedComponentType);
                    if (component != null)
                    {
                        var currentValue = (bool)enabledProperty.GetValue(component);
                        enabledProperty.SetValue(component, !currentValue);                        modifiedComponents.Add(component);
                    }
                }
            }
            
            Debug.Log($"Toggled enabled state for {_selectedComponentType.Name}");
        }
        
        private void ResetComponent()
        {
            if (_selectedComponentType == null)
            {
                Debug.LogError("No component type selected");
                return;
            }
            
            var targetObjects = GetTargetObjects();
            if (targetObjects.Length == 0) return;
            
            using (var undoScope = new UndoScope($"Reset {_selectedComponentType.Name}"))
            {
                var modifiedComponents = new List<Component>();
                
                foreach (var obj in targetObjects)
                {
                    var component = obj.GetComponent(_selectedComponentType);
                    if (component != null)
                    {                    // Create a temporary component to get default values
                    var tempObj = new GameObject();
                    var defaultComponent = tempObj.AddComponent(_selectedComponentType);
                    EditorUtility.CopySerialized(defaultComponent, component);
                    UnityEngine.Object.DestroyImmediate(tempObj);
                        
                        modifiedComponents.Add(component);
                    }                }
            }
            
            Debug.Log($"Reset {_selectedComponentType.Name} to default values");
        }
        
        private void CopyComponentFromSelection()
        {
            var selectedObjects = Selection.gameObjects;
            if (selectedObjects.Length == 0) return;
            
            var firstComponent = selectedObjects[0].GetComponent(_selectedComponentType);
            if (firstComponent != null)
            {
                _sourceComponent = firstComponent;
                UpdatePropertyList();
                Debug.Log($"Copied {_selectedComponentType.Name} from {selectedObjects[0].name}");
            }
            else
            {
                Debug.LogError($"Selected object does not have {_selectedComponentType.Name} component");
            }
        }
        
        private void PasteComponentToSelection()
        {
            if (_sourceComponent == null)
            {
                Debug.LogError("No source component to copy from");
                return;
            }
            
            var targetObjects = GetTargetObjects();
            if (targetObjects.Length == 0) return;
            
            using (var undoScope = new UndoScope($"Paste {_sourceComponent.GetType().Name}"))
            {
                var modifiedComponents = new List<Component>();
                
                foreach (var obj in targetObjects)
                {
                    var targetComponent = obj.GetComponent(_sourceComponent.GetType());
                    if (targetComponent != null)
                    {
                        if (_overwriteValues)
                        {
                            EditorUtility.CopySerialized(_sourceComponent, targetComponent);
                        }
                        else
                        {
                            CopySelectedProperties(_sourceComponent, targetComponent);
                        }
                        
                        modifiedComponents.Add(targetComponent);
                    }                }
            }
            
            Debug.Log($"Pasted {_sourceComponent.GetType().Name} to {targetObjects.Length} objects");
        }
        
        private void CopySelectedProperties(Component source, Component target)
        {
            var sourceType = source.GetType();
            var serializedSource = new SerializedObject(source);
            var serializedTarget = new SerializedObject(target);
            
            foreach (var propertyName in _propertiesToCopy)
            {
                var sourceProperty = serializedSource.FindProperty(propertyName);
                var targetProperty = serializedTarget.FindProperty(propertyName);
                
                if (sourceProperty != null && targetProperty != null)
                {
                    targetProperty.SetGenericValue(sourceProperty.GetGenericValue());
                }
            }
            
            serializedTarget.ApplyModifiedProperties();
        }
        
        private void UpdatePropertyList()
        {
            _propertiesToCopy.Clear();
            
            if (_sourceComponent == null) return;
            
            var serializedObject = new SerializedObject(_sourceComponent);
            var property = serializedObject.GetIterator();
            
            if (property.NextVisible(true))
            {
                do
                {
                    if (property.name != "m_Script")
                    {
                        _propertiesToCopy.Add(property.name);
                    }
                }
                while (property.NextVisible(false));
            }
        }
        
        private void FindComponentsInScene()
        {            if (_selectedComponentType == null)
            {
                Debug.LogError("No component type selected");
                return;
            }
            
            var components = UnityEngine.Object.FindObjectsByType(_selectedComponentType, FindObjectsSortMode.None) as Component[];
            var gameObjects = components.Select(c => c.gameObject).Distinct().ToArray();
            
            Selection.objects = gameObjects;
            Debug.Log($"Found {gameObjects.Length} objects with {_selectedComponentType.Name} in scene");
        }
        
        private void FindComponentsInProject()
        {
            if (_selectedComponentType == null)
            {
                Debug.LogError("No component type selected");
                return;
            }
            
            var prefabGUIDs = AssetDatabase.FindAssets("t:GameObject");
            var results = new List<GameObject>();
            
            foreach (var guid in prefabGUIDs)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                
                if (prefab != null && prefab.GetComponentInChildren(_selectedComponentType) != null)
                {
                    results.Add(prefab);
                }
            }
            
            Selection.objects = results.ToArray();
            Debug.Log($"Found {results.Count} prefabs with {_selectedComponentType.Name} in project");
        }
        
        private void SelectObjectsWithComponent()
        {
            FindComponentsInScene();
        }
        
        private void ShowComponentReport()
        {            if (_selectedComponentType == null)
            {
                Debug.LogError("No component type selected");
                return;
            }
            
            var components = UnityEngine.Object.FindObjectsByType(_selectedComponentType, FindObjectsSortMode.None) as Component[];
            
            Debug.Log($"=== Component Report: {_selectedComponentType.Name} ===");
            Debug.Log($"Total instances: {components.Length}");
            
            var groupedByScene = components.GroupBy(c => c.gameObject.scene);
            foreach (var group in groupedByScene)
            {
                Debug.Log($"Scene '{group.Key.name}': {group.Count()} instances");
            }
            
            Debug.Log($"Generated component report for {_selectedComponentType.Name}");
        }
        
        private GameObject[] GetTargetObjects()
        {
            var selectedObjects = Selection.gameObjects;
            if (!_includeChildren) return selectedObjects;
            
            var allTargets = new List<GameObject>();
            foreach (var obj in selectedObjects)
            {
                allTargets.Add(obj);
                allTargets.AddRange(obj.GetComponentsInChildren<Transform>().Select(t => t.gameObject).Skip(1));
            }
            
            return allTargets.ToArray();
        }
    }
    
    // Extension method for SerializedProperty
    public static class SerializedPropertyExtensions
    {
        public static object GetGenericValue(this SerializedProperty property)
        {
            switch (property.propertyType)
            {
                case SerializedPropertyType.Integer:
                    return property.intValue;
                case SerializedPropertyType.Boolean:
                    return property.boolValue;
                case SerializedPropertyType.Float:
                    return property.floatValue;
                case SerializedPropertyType.String:
                    return property.stringValue;
                case SerializedPropertyType.Color:
                    return property.colorValue;
                case SerializedPropertyType.Vector2:
                    return property.vector2Value;
                case SerializedPropertyType.Vector3:
                    return property.vector3Value;
                case SerializedPropertyType.Vector4:
                    return property.vector4Value;
                case SerializedPropertyType.Rect:
                    return property.rectValue;
                case SerializedPropertyType.ObjectReference:
                    return property.objectReferenceValue;
                default:
                    return null;
            }
        }
        
        public static void SetGenericValue(this SerializedProperty property, object value)
        {
            if (value == null) return;
            
            switch (property.propertyType)
            {
                case SerializedPropertyType.Integer:
                    property.intValue = (int)value;
                    break;
                case SerializedPropertyType.Boolean:
                    property.boolValue = (bool)value;
                    break;
                case SerializedPropertyType.Float:
                    property.floatValue = (float)value;
                    break;
                case SerializedPropertyType.String:
                    property.stringValue = (string)value;
                    break;
                case SerializedPropertyType.Color:
                    property.colorValue = (Color)value;
                    break;
                case SerializedPropertyType.Vector2:
                    property.vector2Value = (Vector2)value;
                    break;
                case SerializedPropertyType.Vector3:
                    property.vector3Value = (Vector3)value;
                    break;
                case SerializedPropertyType.Vector4:
                    property.vector4Value = (Vector4)value;
                    break;
                case SerializedPropertyType.Rect:
                    property.rectValue = (Rect)value;
                    break;
                case SerializedPropertyType.ObjectReference:
                    property.objectReferenceValue = (UnityEngine.Object)value;
                    break;
            }
        }
    }
}
