using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using Breezinstein.Tools.Core;

namespace Breezinstein.Tools.Assets
{
    public class MaterialReplacementTool : BaseBreezeTool
    {
        public override string ToolName => "Material Replacement";
        public override string ToolDescription => "Batch replace materials with advanced filtering options";
        public override string Category => "Asset Management";
        public override int Priority => 20;
        
        private Material _sourceMaterial;
        private Material _targetMaterial;
        private bool _replaceByShader = false;
        private Shader _targetShader;
        private bool _replaceByName = false;
        private string _materialNamePattern = "";
        private bool _includeInactive = true;
        private bool _searchInProject = false;
        
        public override VisualElement CreateUI()
        {
            var container = new VisualElement();
            container.AddToClassList("tool-section");
            
            CreateReplacementSection(container);
            CreateFilterSection(container);
            CreateScopeSection(container);
            CreateActionButtons(container);
            
            return container;
        }
        
        private void CreateReplacementSection(VisualElement parent)
        {
            var section = new VisualElement();
            section.AddToClassList("tool-section");
            
            var header = new Label("Material Replacement");
            header.AddToClassList("tool-section-header");
            section.Add(header);
            
            var sourceField = new ObjectField("Source Material");
            sourceField.objectType = typeof(Material);
            sourceField.value = _sourceMaterial;
            sourceField.RegisterValueChangedCallback(evt => _sourceMaterial = evt.newValue as Material);
            section.Add(sourceField);
            
            var targetField = new ObjectField("Target Material");
            targetField.objectType = typeof(Material);
            targetField.value = _targetMaterial;
            targetField.RegisterValueChangedCallback(evt => _targetMaterial = evt.newValue as Material);
            section.Add(targetField);
            
            parent.Add(section);
        }
        
        private void CreateFilterSection(VisualElement parent)
        {
            var section = new VisualElement();
            section.AddToClassList("tool-section");
            
            var header = new Label("Filter Options");
            header.AddToClassList("tool-section-header");
            section.Add(header);
            
            // Replace by shader
            var shaderToggle = new Toggle("Replace by Shader");
            shaderToggle.value = _replaceByShader;
            shaderToggle.RegisterValueChangedCallback(evt => _replaceByShader = evt.newValue);
            section.Add(shaderToggle);
            
            var shaderField = new ObjectField("Target Shader");
            shaderField.objectType = typeof(Shader);
            shaderField.value = _targetShader;
            shaderField.RegisterValueChangedCallback(evt => _targetShader = evt.newValue as Shader);
            section.Add(shaderField);
            
            // Replace by name pattern
            var nameToggle = new Toggle("Replace by Name Pattern");
            nameToggle.value = _replaceByName;
            nameToggle.RegisterValueChangedCallback(evt => _replaceByName = evt.newValue);
            section.Add(nameToggle);
            
            var nameField = new TextField("Name Pattern (regex)");
            nameField.value = _materialNamePattern;
            nameField.RegisterValueChangedCallback(evt => _materialNamePattern = evt.newValue);
            section.Add(nameField);
            
            parent.Add(section);
        }
        
        private void CreateScopeSection(VisualElement parent)
        {
            var section = new VisualElement();
            section.AddToClassList("tool-section");
            
            var header = new Label("Search Scope");
            header.AddToClassList("tool-section-header");
            section.Add(header);
            
            var inactiveToggle = new Toggle("Include Inactive Objects");
            inactiveToggle.value = _includeInactive;
            inactiveToggle.RegisterValueChangedCallback(evt => _includeInactive = evt.newValue);
            section.Add(inactiveToggle);
            
            var projectToggle = new Toggle("Search in Project Assets");
            projectToggle.value = _searchInProject;
            projectToggle.RegisterValueChangedCallback(evt => _searchInProject = evt.newValue);
            section.Add(projectToggle);
            
            parent.Add(section);
        }
        
        private void CreateActionButtons(VisualElement parent)
        {
            var section = new VisualElement();
            section.AddToClassList("tool-section");
            
            var buttonRow = new VisualElement();
            buttonRow.AddToClassList("button-row");
            
            buttonRow.Add(new Button(() => FindMaterials()) { text = "Find Materials" });
            buttonRow.Add(new Button(() => PreviewReplacement()) { text = "Preview" });
            
            section.Add(buttonRow);
            
            var buttonRow2 = new VisualElement();
            buttonRow2.AddToClassList("button-row");
            
            buttonRow2.Add(new Button(() => ReplaceInSelection()) { text = "Replace in Selection" });
            buttonRow2.Add(new Button(() => ReplaceInScene()) { text = "Replace in Scene" });
            
            section.Add(buttonRow2);
            
            if (_searchInProject)
            {
                var projectButton = new Button(() => ReplaceInProject()) { text = "Replace in Project" };
                section.Add(projectButton);
            }
            
            parent.Add(section);
        }
        
        public override bool CanExecute()
        {
            return _targetMaterial != null || _replaceByShader || _replaceByName;
        }
        
        private void FindMaterials()
        {
            var foundObjects = new List<GameObject>();
            
            if (_searchInProject)
            {
                foundObjects.AddRange(FindMaterialsInProject());
            }
            else
            {
                foundObjects.AddRange(FindMaterialsInScene());
            }
            
            Selection.objects = foundObjects.ToArray();
            LogOperation($"Found {foundObjects.Count} objects with matching materials");        }
        
        private List<GameObject> FindMaterialsInScene()
        {
            var results = new List<GameObject>();
            var allRenderers = _includeInactive ? 
                Resources.FindObjectsOfTypeAll<Renderer>() : 
                Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None);

            foreach (var renderer in allRenderers)
            {
                if (renderer.gameObject.scene.IsValid() && HasMatchingMaterial(renderer))
                {
                    results.Add(renderer.gameObject);
                }
            }
            
            return results;
        }
        
        private List<GameObject> FindMaterialsInProject()
        {
            var results = new List<GameObject>();
            var prefabGUIDs = AssetDatabase.FindAssets("t:GameObject");
            
            foreach (var guid in prefabGUIDs)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                
                if (prefab != null)
                {
                    var renderers = prefab.GetComponentsInChildren<Renderer>();
                    if (renderers.Any(HasMatchingMaterial))
                    {
                        results.Add(prefab);
                    }
                }
            }
            
            return results;
        }
        
        private bool HasMatchingMaterial(Renderer renderer)
        {
            foreach (var material in renderer.sharedMaterials)
            {
                if (material == null) continue;
                
                if (_sourceMaterial != null && material == _sourceMaterial)
                    return true;
                
                if (_replaceByShader && _targetShader != null && material.shader == _targetShader)
                    return true;
                
                if (_replaceByName && !string.IsNullOrEmpty(_materialNamePattern))
                {
                    if (System.Text.RegularExpressions.Regex.IsMatch(material.name, _materialNamePattern))
                        return true;
                }
            }
            
            return false;
        }
          private void PreviewReplacement()
        {
            var materialCount = 0;
            var objectCount = 0;
            
            var allRenderers = _includeInactive ? 
                Resources.FindObjectsOfTypeAll<Renderer>() : 
                Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None);
            
            foreach (var renderer in allRenderers)
            {
                if (!renderer.gameObject.scene.IsValid() && !_searchInProject) continue;
                
                var hasMatch = false;
                foreach (var material in renderer.sharedMaterials)
                {
                    if (material == null) continue;
                    
                    if (ShouldReplaceMaterial(material))
                    {
                        materialCount++;
                        hasMatch = true;
                    }
                }
                
                if (hasMatch) objectCount++;
            }
            
            LogOperation($"Preview: {materialCount} materials on {objectCount} objects would be replaced");
        }
        
        private void ReplaceInSelection()
        {
            var selectedObjects = Selection.gameObjects;
            if (selectedObjects.Length == 0) return;
            
            var replacedCount = 0;
            
            using (var undoScope = new UndoScope("Replace Materials in Selection"))
            {
                foreach (var obj in selectedObjects)
                {
                    var renderers = obj.GetComponentsInChildren<Renderer>();
                    foreach (var renderer in renderers)
                    {
                        replacedCount += ReplaceMaterialsOnRenderer(renderer);
                    }
                }
            }
            
            BreezeEvents.EmitMaterialsReplaced(selectedObjects.Cast<Object>().ToArray());
            LogOperation($"Replaced {replacedCount} materials in selection");
        }
          private void ReplaceInScene()
        {
            var allRenderers = _includeInactive ? 
                Resources.FindObjectsOfTypeAll<Renderer>() : 
                Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None);
            
            var replacedCount = 0;
            var affectedObjects = new List<Object>();
            
            using (var undoScope = new UndoScope("Replace Materials in Scene"))
            {
                foreach (var renderer in allRenderers)
                {
                    if (!renderer.gameObject.scene.IsValid()) continue;
                    
                    var replaced = ReplaceMaterialsOnRenderer(renderer);
                    if (replaced > 0)
                    {
                        replacedCount += replaced;
                        affectedObjects.Add(renderer.gameObject);
                    }
                }
            }
            
            BreezeEvents.EmitMaterialsReplaced(affectedObjects.ToArray());
            LogOperation($"Replaced {replacedCount} materials in scene");
        }
        
        private void ReplaceInProject()
        {
            if (!_searchInProject) return;
            
            var prefabGUIDs = AssetDatabase.FindAssets("t:GameObject");
            var replacedCount = 0;
            var affectedAssets = new List<Object>();
            
            foreach (var guid in prefabGUIDs)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                
                if (prefab != null)
                {
                    var renderers = prefab.GetComponentsInChildren<Renderer>();
                    var hasChanges = false;
                    
                    foreach (var renderer in renderers)
                    {
                        var replaced = ReplaceMaterialsOnRenderer(renderer);
                        if (replaced > 0)
                        {
                            replacedCount += replaced;
                            hasChanges = true;
                        }
                    }
                    
                    if (hasChanges)
                    {
                        EditorUtility.SetDirty(prefab);
                        affectedAssets.Add(prefab);
                    }
                }
            }
            
            AssetDatabase.SaveAssets();
            BreezeEvents.EmitMaterialsReplaced(affectedAssets.ToArray());
            LogOperation($"Replaced {replacedCount} materials in project");
        }
        
        private int ReplaceMaterialsOnRenderer(Renderer renderer)
        {
            var materials = renderer.sharedMaterials;
            var replacedCount = 0;
            var hasChanges = false;
            
            for (int i = 0; i < materials.Length; i++)
            {
                if (materials[i] == null) continue;
                
                if (ShouldReplaceMaterial(materials[i]))
                {
                    materials[i] = _targetMaterial;
                    replacedCount++;
                    hasChanges = true;
                }
            }
            
            if (hasChanges)
            {
                renderer.sharedMaterials = materials;
            }
            
            return replacedCount;
        }
        
        private bool ShouldReplaceMaterial(Material material)
        {
            if (material == null) return false;
            
            // Direct material match
            if (_sourceMaterial != null && material == _sourceMaterial)
                return true;
            
            // Shader match
            if (_replaceByShader && _targetShader != null && material.shader == _targetShader)
                return true;
            
            // Name pattern match
            if (_replaceByName && !string.IsNullOrEmpty(_materialNamePattern))
            {
                try
                {
                    if (System.Text.RegularExpressions.Regex.IsMatch(material.name, _materialNamePattern))
                        return true;
                }
                catch (System.ArgumentException)
                {
                    LogWarning($"Invalid regex pattern: {_materialNamePattern}");
                }
            }
            
            return false;
        }
    }
}
