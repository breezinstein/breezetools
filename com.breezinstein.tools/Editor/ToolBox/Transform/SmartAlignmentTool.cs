using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using Breezinstein.Tools.Core;

namespace Breezinstein.Tools.TransformTools
{
    public class SmartAlignmentTool : BaseBreezeTool
    {
        public override string ToolName => "Smart Alignment & Snapping";
        public override string ToolDescription => "Align objects to bounds, grid snap, surface snap, and pivot utilities";
        public override string Category => "Transform Tools";
        public override int Priority => 20;
        
        private float _gridSize = 1f;
        private bool _snapToGrid = false;
        private LayerMask _surfaceLayerMask = -1;
        
        public override VisualElement CreateUI()
        {
            var container = new VisualElement();
            container.AddToClassList("tool-section");
            
            CreateAlignmentSection(container);
            CreateGridSnappingSection(container);
            CreateSurfaceSnappingSection(container);
            CreatePivotSection(container);
            
            return container;
        }
        
        private void CreateAlignmentSection(VisualElement parent)
        {
            var section = new VisualElement();
            section.AddToClassList("tool-section");
            
            var header = new Label("Bounds Alignment");
            header.AddToClassList("tool-section-header");
            section.Add(header);
            
            // X-axis alignment
            var xRow = new VisualElement();
            xRow.AddToClassList("button-row");
            xRow.Add(new Button(() => AlignToBounds(AlignmentType.MinX)) { text = "Left" });
            xRow.Add(new Button(() => AlignToBounds(AlignmentType.CenterX)) { text = "Center X" });
            xRow.Add(new Button(() => AlignToBounds(AlignmentType.MaxX)) { text = "Right" });
            section.Add(xRow);
            
            // Y-axis alignment
            var yRow = new VisualElement();
            yRow.AddToClassList("button-row");
            yRow.Add(new Button(() => AlignToBounds(AlignmentType.MinY)) { text = "Bottom" });
            yRow.Add(new Button(() => AlignToBounds(AlignmentType.CenterY)) { text = "Center Y" });
            yRow.Add(new Button(() => AlignToBounds(AlignmentType.MaxY)) { text = "Top" });
            section.Add(yRow);
            
            // Z-axis alignment
            var zRow = new VisualElement();
            zRow.AddToClassList("button-row");
            zRow.Add(new Button(() => AlignToBounds(AlignmentType.MinZ)) { text = "Back" });
            zRow.Add(new Button(() => AlignToBounds(AlignmentType.CenterZ)) { text = "Center Z" });
            zRow.Add(new Button(() => AlignToBounds(AlignmentType.MaxZ)) { text = "Front" });
            section.Add(zRow);
            
            parent.Add(section);
        }
        
        private void CreateGridSnappingSection(VisualElement parent)
        {
            var section = new VisualElement();
            section.AddToClassList("tool-section");
            
            var header = new Label("Grid Snapping");
            header.AddToClassList("tool-section-header");
            section.Add(header);
            
            var gridSizeField = new FloatField("Grid Size");
            gridSizeField.value = _gridSize;
            gridSizeField.RegisterValueChangedCallback(evt => _gridSize = Mathf.Max(0.1f, evt.newValue));
            section.Add(gridSizeField);
            
            var snapToggle = new Toggle("Enable Grid Snapping");
            snapToggle.value = _snapToGrid;
            snapToggle.RegisterValueChangedCallback(evt => _snapToGrid = evt.newValue);
            section.Add(snapToggle);
            
            var buttonRow = new VisualElement();
            buttonRow.AddToClassList("button-row");
            buttonRow.Add(new Button(() => SnapToGrid()) { text = "Snap to Grid" });
            buttonRow.Add(new Button(() => SnapToOrigin()) { text = "Snap to Origin" });
            section.Add(buttonRow);
            
            parent.Add(section);
        }
        
        private void CreateSurfaceSnappingSection(VisualElement parent)
        {
            var section = new VisualElement();
            section.AddToClassList("tool-section");
            
            var header = new Label("Surface Snapping");
            header.AddToClassList("tool-section-header");
            section.Add(header);
            
            var layerField = new LayerMaskField("Surface Layers");
            layerField.value = _surfaceLayerMask;
            layerField.RegisterValueChangedCallback(evt => _surfaceLayerMask = evt.newValue);
            section.Add(layerField);
            
            var buttonRow = new VisualElement();
            buttonRow.AddToClassList("button-row");
            buttonRow.Add(new Button(() => SnapToSurface()) { text = "Snap to Surface" });
            buttonRow.Add(new Button(() => AlignToSurfaceNormal()) { text = "Align to Normal" });
            section.Add(buttonRow);
            
            parent.Add(section);
        }
        
        private void CreatePivotSection(VisualElement parent)
        {
            var section = new VisualElement();
            section.AddToClassList("tool-section");
            
            var header = new Label("Pivot Utilities");
            header.AddToClassList("tool-section-header");
            section.Add(header);
            
            var buttonRow = new VisualElement();
            buttonRow.AddToClassList("button-row");
            buttonRow.Add(new Button(() => CenterPivot()) { text = "Center Pivot" });
            buttonRow.Add(new Button(() => MovePivotToBase()) { text = "Pivot to Base" });
            section.Add(buttonRow);
            
            parent.Add(section);
        }
        
        public override bool CanExecute()
        {
            return Selection.transforms.Length > 0;
        }
        
        private enum AlignmentType
        {
            MinX, CenterX, MaxX,
            MinY, CenterY, MaxY,
            MinZ, CenterZ, MaxZ
        }
        
        private void AlignToBounds(AlignmentType alignmentType)
        {
            var selectedTransforms = Selection.transforms;
            if (selectedTransforms.Length < 2) return;
            
            using (var undoScope = new UndoScope("Align Objects", selectedTransforms.Cast<Object>().ToArray()))
            {
                var bounds = CalculateBounds(selectedTransforms);
                
                foreach (var transform in selectedTransforms)
                {
                    var renderer = transform.GetComponent<Renderer>();
                    var objBounds = renderer ? renderer.bounds : new Bounds(transform.position, Vector3.zero);
                    
                    var newPosition = transform.position;
                    
                    switch (alignmentType)
                    {
                        case AlignmentType.MinX:
                            newPosition.x = bounds.min.x + (transform.position.x - objBounds.min.x);
                            break;
                        case AlignmentType.CenterX:
                            newPosition.x = bounds.center.x + (transform.position.x - objBounds.center.x);
                            break;
                        case AlignmentType.MaxX:
                            newPosition.x = bounds.max.x + (transform.position.x - objBounds.max.x);
                            break;
                        case AlignmentType.MinY:
                            newPosition.y = bounds.min.y + (transform.position.y - objBounds.min.y);
                            break;
                        case AlignmentType.CenterY:
                            newPosition.y = bounds.center.y + (transform.position.y - objBounds.center.y);
                            break;
                        case AlignmentType.MaxY:
                            newPosition.y = bounds.max.y + (transform.position.y - objBounds.max.y);
                            break;
                        case AlignmentType.MinZ:
                            newPosition.z = bounds.min.z + (transform.position.z - objBounds.min.z);
                            break;
                        case AlignmentType.CenterZ:
                            newPosition.z = bounds.center.z + (transform.position.z - objBounds.center.z);
                            break;
                        case AlignmentType.MaxZ:
                            newPosition.z = bounds.max.z + (transform.position.z - objBounds.max.z);
                            break;
                    }
                    
                    transform.position = newPosition;
                }
            }
            
            BreezeEvents.EmitObjectsTransformed(selectedTransforms);
            LogOperation($"Aligned {selectedTransforms.Length} objects using {alignmentType}");
        }
        
        private void SnapToGrid()
        {
            var selectedTransforms = Selection.transforms;
            if (selectedTransforms.Length == 0) return;
            
            using (var undoScope = new UndoScope("Snap to Grid", selectedTransforms.Cast<Object>().ToArray()))
            {
                foreach (var transform in selectedTransforms)
                {
                    var pos = transform.position;
                    pos.x = Mathf.Round(pos.x / _gridSize) * _gridSize;
                    pos.y = Mathf.Round(pos.y / _gridSize) * _gridSize;
                    pos.z = Mathf.Round(pos.z / _gridSize) * _gridSize;
                    transform.position = pos;
                }
            }
            
            BreezeEvents.EmitObjectsTransformed(selectedTransforms);
            LogOperation($"Snapped {selectedTransforms.Length} objects to grid (size: {_gridSize})");
        }
        
        private void SnapToOrigin()
        {
            var selectedTransforms = Selection.transforms;
            if (selectedTransforms.Length == 0) return;
            
            using (var undoScope = new UndoScope("Snap to Origin", selectedTransforms.Cast<Object>().ToArray()))
            {
                foreach (var transform in selectedTransforms)
                {
                    transform.position = Vector3.zero;
                }
            }
            
            BreezeEvents.EmitObjectsTransformed(selectedTransforms);
            LogOperation($"Snapped {selectedTransforms.Length} objects to origin");
        }
        
        private void SnapToSurface()
        {
            var selectedTransforms = Selection.transforms;
            if (selectedTransforms.Length == 0) return;
            
            using (var undoScope = new UndoScope("Snap to Surface", selectedTransforms.Cast<Object>().ToArray()))
            {
                foreach (var transform in selectedTransforms)
                {
                    if (Physics.Raycast(transform.position + Vector3.up * 100f, Vector3.down, out RaycastHit hit, 200f, _surfaceLayerMask))
                    {
                        transform.position = hit.point;
                    }
                }
            }
            
            BreezeEvents.EmitObjectsTransformed(selectedTransforms);
            LogOperation($"Snapped {selectedTransforms.Length} objects to surface");
        }
        
        private void AlignToSurfaceNormal()
        {
            var selectedTransforms = Selection.transforms;
            if (selectedTransforms.Length == 0) return;
            
            using (var undoScope = new UndoScope("Align to Surface Normal", selectedTransforms.Cast<Object>().ToArray()))
            {
                foreach (var transform in selectedTransforms)
                {
                    if (Physics.Raycast(transform.position + Vector3.up * 100f, Vector3.down, out RaycastHit hit, 200f, _surfaceLayerMask))
                    {
                        transform.position = hit.point;
                        transform.rotation = Quaternion.LookRotation(Vector3.Cross(hit.normal, transform.right), hit.normal);
                    }
                }
            }
            
            BreezeEvents.EmitObjectsTransformed(selectedTransforms);
            LogOperation($"Aligned {selectedTransforms.Length} objects to surface normals");
        }
        
        private void CenterPivot()
        {
            var selectedTransforms = Selection.transforms;
            if (selectedTransforms.Length == 0) return;
            
            using (var undoScope = new UndoScope("Center Pivot", selectedTransforms.Cast<Object>().ToArray()))
            {
                foreach (var transform in selectedTransforms)
                {
                    var renderer = transform.GetComponent<Renderer>();
                    if (renderer != null)
                    {
                        var center = renderer.bounds.center;
                        var offset = center - transform.position;
                        
                        // Move children to maintain their world positions
                        for (int i = 0; i < transform.childCount; i++)
                        {
                            var child = transform.GetChild(i);
                            child.position -= offset;
                        }
                        
                        transform.position = center;
                    }
                }
            }
            
            BreezeEvents.EmitObjectsTransformed(selectedTransforms);
            LogOperation($"Centered pivot for {selectedTransforms.Length} objects");
        }
        
        private void MovePivotToBase()
        {
            var selectedTransforms = Selection.transforms;
            if (selectedTransforms.Length == 0) return;
            
            using (var undoScope = new UndoScope("Move Pivot to Base", selectedTransforms.Cast<Object>().ToArray()))
            {
                foreach (var transform in selectedTransforms)
                {
                    var renderer = transform.GetComponent<Renderer>();
                    if (renderer != null)
                    {
                        var bounds = renderer.bounds;
                        var basePoint = new Vector3(bounds.center.x, bounds.min.y, bounds.center.z);
                        var offset = basePoint - transform.position;
                        
                        // Move children to maintain their world positions
                        for (int i = 0; i < transform.childCount; i++)
                        {
                            var child = transform.GetChild(i);
                            child.position -= offset;
                        }
                        
                        transform.position = basePoint;
                    }
                }
            }
            
            BreezeEvents.EmitObjectsTransformed(selectedTransforms);
            LogOperation($"Moved pivot to base for {selectedTransforms.Length} objects");
        }
        
        private Bounds CalculateBounds(UnityEngine.Transform[] transforms)
        {
            if (transforms.Length == 0) return new Bounds();
            
            var firstRenderer = transforms[0].GetComponent<Renderer>();
            var bounds = firstRenderer ? firstRenderer.bounds : new Bounds(transforms[0].position, Vector3.zero);
            
            foreach (var transform in transforms.Skip(1))
            {
                var renderer = transform.GetComponent<Renderer>();
                if (renderer != null)
                {
                    bounds.Encapsulate(renderer.bounds);
                }
                else
                {
                    bounds.Encapsulate(transform.position);
                }
            }
            
            return bounds;
        }
    }
}
