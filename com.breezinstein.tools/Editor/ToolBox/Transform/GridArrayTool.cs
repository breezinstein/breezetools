using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using Breezinstein.Tools.Core;

namespace Breezinstein.Tools.TransformTools
{
    public class GridArrayTool : BaseBreezeTool
    {
        public override string ToolName => "Grid & Radial Arrays";
        public override string ToolDescription => "Arrange selected objects in customizable grid or radial patterns";
        public override string Category => "Transform Tools";
        public override int Priority => 10;
        
        // Grid settings
        private Vector3Int _gridSize = new Vector3Int(3, 1, 3);
        private Vector3 _spacing = Vector3.one;
        private bool _useLocalSpace = true;
        
        // Radial settings
        private int _radialCount = 8;
        private float _radius = 5f;
        private float _startAngle = 0f;
        private Vector3 _radialAxis = Vector3.up;
        
        private bool _previewMode = false;
        private List<UnityEngine.Transform> _previewObjects = new List<UnityEngine.Transform>();
        
        public override VisualElement CreateUI()
        {
            var container = new VisualElement();
            container.AddToClassList("tool-section");
            
            // Grid Array Section
            CreateGridSection(container);
            
            // Radial Array Section
            CreateRadialSection(container);
            
            // Preview controls
            CreatePreviewSection(container);
            
            return container;
        }
        
        private void CreateGridSection(VisualElement parent)
        {
            var section = new VisualElement();
            section.AddToClassList("tool-section");
            
            var header = new Label("Grid Array");
            header.AddToClassList("tool-section-header");
            section.Add(header);
            
            // Grid size
            var gridSizeField = new Vector3IntField("Grid Size (X,Y,Z)");
            gridSizeField.value = _gridSize;
            gridSizeField.RegisterValueChangedCallback(evt => _gridSize = evt.newValue);
            section.Add(gridSizeField);
            
            // Spacing
            var spacingField = new Vector3Field("Spacing");
            spacingField.value = _spacing;
            spacingField.RegisterValueChangedCallback(evt => _spacing = evt.newValue);
            section.Add(spacingField);
            
            // Local space toggle
            var localSpaceToggle = new Toggle("Use Local Space");
            localSpaceToggle.value = _useLocalSpace;
            localSpaceToggle.RegisterValueChangedCallback(evt => _useLocalSpace = evt.newValue);
            section.Add(localSpaceToggle);
            
            // Grid buttons
            var buttonRow = new VisualElement();
            buttonRow.AddToClassList("button-row");
            
            var previewGridButton = new Button(() => PreviewGridArray()) { text = "Preview Grid" };
            var applyGridButton = new Button(() => ApplyGridArray()) { text = "Apply Grid" };
            
            buttonRow.Add(previewGridButton);
            buttonRow.Add(applyGridButton);
            section.Add(buttonRow);
            
            parent.Add(section);
        }
        
        private void CreateRadialSection(VisualElement parent)
        {
            var section = new VisualElement();
            section.AddToClassList("tool-section");
            
            var header = new Label("Radial Array");
            header.AddToClassList("tool-section-header");
            section.Add(header);
            
            // Count
            var countField = new IntegerField("Count");
            countField.value = _radialCount;
            countField.RegisterValueChangedCallback(evt => _radialCount = Mathf.Max(1, evt.newValue));
            section.Add(countField);
            
            // Radius
            var radiusField = new FloatField("Radius");
            radiusField.value = _radius;
            radiusField.RegisterValueChangedCallback(evt => _radius = Mathf.Max(0.1f, evt.newValue));
            section.Add(radiusField);
            
            // Start angle
            var angleField = new FloatField("Start Angle");
            angleField.value = _startAngle;
            angleField.RegisterValueChangedCallback(evt => _startAngle = evt.newValue);
            section.Add(angleField);
            
            // Axis
            var axisField = new Vector3Field("Rotation Axis");
            axisField.value = _radialAxis;
            axisField.RegisterValueChangedCallback(evt => _radialAxis = evt.newValue.normalized);
            section.Add(axisField);
            
            // Radial buttons
            var buttonRow = new VisualElement();
            buttonRow.AddToClassList("button-row");
            
            var previewRadialButton = new Button(() => PreviewRadialArray()) { text = "Preview Radial" };
            var applyRadialButton = new Button(() => ApplyRadialArray()) { text = "Apply Radial" };
            
            buttonRow.Add(previewRadialButton);
            buttonRow.Add(applyRadialButton);
            section.Add(buttonRow);
            
            parent.Add(section);
        }
        
        private void CreatePreviewSection(VisualElement parent)
        {
            var section = new VisualElement();
            section.AddToClassList("preview-area");
            
            var header = new Label("Preview Mode");
            header.AddToClassList("tool-section-header");
            section.Add(header);
            
            var buttonRow = new VisualElement();
            buttonRow.AddToClassList("button-row");
            
            var cancelButton = new Button(() => CancelPreview()) { text = "Cancel Preview" };
            var applyButton = new Button(() => ApplyPreview()) { text = "Apply Preview" };
            
            buttonRow.Add(cancelButton);
            buttonRow.Add(applyButton);
            section.Add(buttonRow);
            
            parent.Add(section);
        }
        
        public override bool CanExecute()
        {
            return Selection.transforms.Length > 0;
        }
        
        private void PreviewGridArray()
        {
            if (!CanExecute()) return;
            
            CancelPreview();
            _previewMode = true;
            
            var selectedObjects = Selection.transforms;
            if (selectedObjects.Length == 0) return;
            
            var originalObject = selectedObjects[0];
            var totalCount = _gridSize.x * _gridSize.y * _gridSize.z;
            
            using (var undoScope = new UndoScope("Preview Grid Array"))
            {
                for (int x = 0; x < _gridSize.x; x++)
                {
                    for (int y = 0; y < _gridSize.y; y++)
                    {
                        for (int z = 0; z < _gridSize.z; z++)
                        {
                            if (x == 0 && y == 0 && z == 0) continue; // Skip original
                            
                            var position = new Vector3(x * _spacing.x, y * _spacing.y, z * _spacing.z);
                            if (!_useLocalSpace && originalObject.parent != null)
                            {
                                position = originalObject.parent.TransformPoint(position);
                            }
                            else if (_useLocalSpace)
                            {
                                position = originalObject.position + originalObject.TransformDirection(position);
                            }
                            else
                            {
                                position += originalObject.position;
                            }
                            
                            var clone = Object.Instantiate(originalObject.gameObject, position, originalObject.rotation, originalObject.parent);
                            clone.name = $"{originalObject.name}_Grid_{x}_{y}_{z}";
                            _previewObjects.Add(clone.transform);
                        }
                    }
                }
            }
            
            LogOperation($"Previewing grid array: {totalCount} objects");
        }
        
        private void PreviewRadialArray()
        {
            if (!CanExecute()) return;
            
            CancelPreview();
            _previewMode = true;
            
            var selectedObjects = Selection.transforms;
            if (selectedObjects.Length == 0) return;
            
            var originalObject = selectedObjects[0];
            var center = originalObject.position;
            
            using (var undoScope = new UndoScope("Preview Radial Array"))
            {
                for (int i = 1; i < _radialCount; i++) // Skip first (original)
                {
                    var angle = _startAngle + (360f / _radialCount) * i;
                    var rotation = Quaternion.AngleAxis(angle, _radialAxis);
                    var direction = rotation * Vector3.forward;
                    var position = center + direction * _radius;
                    
                    var clone = Object.Instantiate(originalObject.gameObject, position, originalObject.rotation * rotation, originalObject.parent);
                    clone.name = $"{originalObject.name}_Radial_{i}";
                    _previewObjects.Add(clone.transform);
                }
            }
            
            LogOperation($"Previewing radial array: {_radialCount} objects");
        }
        
        private void ApplyGridArray()
        {
            PreviewGridArray();
            ApplyPreview();
        }
        
        private void ApplyRadialArray()
        {
            PreviewRadialArray();
            ApplyPreview();
        }
        
        private void ApplyPreview()
        {
            if (!_previewMode) return;
            
            var allTransforms = new List<UnityEngine.Transform>(Selection.transforms);
            allTransforms.AddRange(_previewObjects);
            
            BreezeEvents.EmitObjectsTransformed(allTransforms.ToArray());
            
            _previewMode = false;
            _previewObjects.Clear();
            
            LogOperation("Applied array transformation");
        }
        
        private void CancelPreview()
        {
            if (!_previewMode) return;
            
            using (var undoScope = new UndoScope("Cancel Preview"))
            {
                foreach (var obj in _previewObjects)
                {
                    if (obj != null)
                    {
                        undoScope.DestroyObject(obj.gameObject);
                    }
                }
            }
            
            _previewObjects.Clear();
            _previewMode = false;
            
            LogOperation("Cancelled preview");
        }
        
        public override void OnToolDeselected()
        {
            CancelPreview();
        }
    }
}
