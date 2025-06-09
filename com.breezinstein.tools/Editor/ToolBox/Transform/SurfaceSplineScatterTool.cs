using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using Breezinstein.Tools.Core;

namespace Breezinstein.Tools.TransformTools
{
    [System.Serializable]
    public class SurfaceSplineScatterTool : BaseBreezeTool
    {        public override string ToolName => "Surface & Spline Scatter";
        public override string ToolDescription => "Scatter objects along surfaces, splines, or custom paths with advanced distribution controls";
        public override string Category => "Transform Tools";
        public override int Priority => 40;

        private ScatterMode scatterMode = ScatterMode.Surface;
        private GameObject prefabToScatter;
        private int scatterCount = 10;
        private float scatterRadius = 5f;
        private LayerMask surfaceLayerMask = -1;
        private bool alignToSurface = true;
        private bool randomRotation = false;
        private bool randomScale = false;
        private Vector2 scaleRange = new Vector2(0.8f, 1.2f);
        private Vector2 rotationRange = new Vector2(0f, 360f);
        private float splineLength = 10f;
        private AnimationCurve distributionCurve = AnimationCurve.Linear(0, 1, 1, 1);
        private bool useNoiseOffset = false;
        private float noiseStrength = 1f;
        private float noiseScale = 1f;
        private int seed = 0;
        
        private List<Vector3> splinePoints = new List<Vector3>();
        private List<GameObject> previewObjects = new List<GameObject>();
        private bool showPreview = true;

        public enum ScatterMode
        {
            Surface,
            Spline,
            Circle,
            Grid,
            Custom
        }

        public override VisualElement CreateUI()
        {
            var root = new VisualElement();

            // Header
            var header = new Label("Surface & Spline Scatter Tool");
            header.AddToClassList("header-label");
            root.Add(header);

            var description = new Label("Scatter objects along surfaces, splines, or custom paths with advanced distribution");
            description.AddToClassList("description-label");
            root.Add(description);

            // Scatter mode selection
            var modeSection = new Foldout { text = "Scatter Mode", value = true };
            
            var modeField = new EnumField("Mode", scatterMode);
            modeField.RegisterValueChangedCallback(evt => 
            {
                scatterMode = (ScatterMode)evt.newValue;
                UpdateModeSpecificUI();
            });
            modeSection.Add(modeField);

            root.Add(modeSection);

            // Object settings
            var objectSection = new Foldout { text = "Object Settings", value = true };
            
            var prefabField = new ObjectField("Prefab to Scatter") 
            { 
                objectType = typeof(GameObject),
                value = prefabToScatter
            };
            prefabField.RegisterValueChangedCallback(evt => prefabToScatter = evt.newValue as GameObject);
            objectSection.Add(prefabField);

            var countField = new IntegerField("Scatter Count") { value = scatterCount };
            countField.RegisterValueChangedCallback(evt => scatterCount = evt.newValue);
            objectSection.Add(countField);

            root.Add(objectSection);

            // Mode-specific options container
            var modeOptionsContainer = new VisualElement();
            modeOptionsContainer.name = "mode-options";
            root.Add(modeOptionsContainer);

            // Distribution settings
            var distributionSection = new Foldout { text = "Distribution", value = true };
            
            var distributionCurveField = new CurveField("Distribution Curve") { value = distributionCurve };
            distributionCurveField.RegisterValueChangedCallback(evt => distributionCurve = evt.newValue);
            distributionSection.Add(distributionCurveField);

            var seedField = new IntegerField("Random Seed") { value = seed };
            seedField.RegisterValueChangedCallback(evt => seed = evt.newValue);
            distributionSection.Add(seedField);

            root.Add(distributionSection);

            // Randomization settings
            var randomSection = new Foldout { text = "Randomization", value = true };
            
            var randomRotToggle = new Toggle("Random Rotation") { value = randomRotation };
            randomRotToggle.RegisterValueChangedCallback(evt => randomRotation = evt.newValue);
            randomSection.Add(randomRotToggle);

            var rotRangeField = new Vector2Field("Rotation Range") { value = rotationRange };
            rotRangeField.RegisterValueChangedCallback(evt => rotationRange = evt.newValue);
            randomSection.Add(rotRangeField);

            var randomScaleToggle = new Toggle("Random Scale") { value = randomScale };
            randomScaleToggle.RegisterValueChangedCallback(evt => randomScale = evt.newValue);
            randomSection.Add(randomScaleToggle);

            var scaleRangeField = new Vector2Field("Scale Range") { value = scaleRange };
            scaleRangeField.RegisterValueChangedCallback(evt => scaleRange = evt.newValue);
            randomSection.Add(scaleRangeField);

            root.Add(randomSection);

            // Noise settings
            var noiseSection = new Foldout { text = "Noise Offset", value = false };
            
            var noiseToggle = new Toggle("Use Noise Offset") { value = useNoiseOffset };
            noiseToggle.RegisterValueChangedCallback(evt => useNoiseOffset = evt.newValue);
            noiseSection.Add(noiseToggle);

            var noiseStrengthField = new FloatField("Noise Strength") { value = noiseStrength };
            noiseStrengthField.RegisterValueChangedCallback(evt => noiseStrength = evt.newValue);
            noiseSection.Add(noiseStrengthField);

            var noiseScaleField = new FloatField("Noise Scale") { value = noiseScale };
            noiseScaleField.RegisterValueChangedCallback(evt => noiseScale = evt.newValue);
            noiseSection.Add(noiseScaleField);

            root.Add(noiseSection);

            // Preview settings
            var previewSection = new Foldout { text = "Preview", value = true };
            
            var previewToggle = new Toggle("Show Preview") { value = showPreview };
            previewToggle.RegisterValueChangedCallback(evt => 
            {
                showPreview = evt.newValue;
                if (!showPreview) ClearPreview();
            });
            previewSection.Add(previewToggle);

            root.Add(previewSection);

            // Actions
            var actionsContainer = new VisualElement();
            actionsContainer.style.flexDirection = FlexDirection.Row;
            actionsContainer.style.marginTop = 10;

            var previewButton = new Button(() => UpdatePreview()) { text = "Update Preview" };
            previewButton.AddToClassList("secondary-button");
            actionsContainer.Add(previewButton);

            var scatterButton = new Button(() => ApplyScatter()) { text = "Apply Scatter" };
            scatterButton.AddToClassList("primary-button");
            actionsContainer.Add(scatterButton);

            var clearButton = new Button(() => ClearPreview()) { text = "Clear Preview" };
            clearButton.AddToClassList("warning-button");
            actionsContainer.Add(clearButton);

            root.Add(actionsContainer);

            // Spline editing
            var splineSection = new Foldout { text = "Spline Editing", value = false };
            
            var splineButtonsContainer = new VisualElement();
            splineButtonsContainer.style.flexDirection = FlexDirection.Row;

            var addPointButton = new Button(() => AddSplinePoint()) { text = "Add Point" };
            splineButtonsContainer.Add(addPointButton);

            var clearSplineButton = new Button(() => ClearSpline()) { text = "Clear Spline" };
            splineButtonsContainer.Add(clearSplineButton);

            splineSection.Add(splineButtonsContainer);

            var splineInfo = new Label($"Spline has {splinePoints.Count} points");
            splineInfo.name = "spline-info";
            splineSection.Add(splineInfo);

            root.Add(splineSection);

            // Initialize mode-specific UI
            UpdateModeSpecificUI();

            return root;
        }

        private void UpdateModeSpecificUI()
        {
            var container = GetUIElement<VisualElement>("mode-options");
            if (container == null) return;

            container.Clear();

            switch (scatterMode)
            {
                case ScatterMode.Surface:
                    var layerMaskField = new LayerMaskField("Surface Layer Mask") { value = surfaceLayerMask };
                    layerMaskField.RegisterValueChangedCallback(evt => surfaceLayerMask = evt.newValue);
                    container.Add(layerMaskField);

                    var radiusField = new FloatField("Scatter Radius") { value = scatterRadius };
                    radiusField.RegisterValueChangedCallback(evt => scatterRadius = evt.newValue);
                    container.Add(radiusField);

                    var alignToggle = new Toggle("Align to Surface Normal") { value = alignToSurface };
                    alignToggle.RegisterValueChangedCallback(evt => alignToSurface = evt.newValue);
                    container.Add(alignToggle);
                    break;

                case ScatterMode.Spline:
                    var lengthField = new FloatField("Spline Length") { value = splineLength };
                    lengthField.RegisterValueChangedCallback(evt => splineLength = evt.newValue);
                    container.Add(lengthField);                    var splineHelp = new Label("Use Scene View to add spline points by clicking while holding Shift");
                    splineHelp.AddToClassList("help-text");
                    container.Add(splineHelp);
                    break;

                case ScatterMode.Circle:
                    var circleRadiusField = new FloatField("Circle Radius") { value = scatterRadius };
                    circleRadiusField.RegisterValueChangedCallback(evt => scatterRadius = evt.newValue);
                    container.Add(circleRadiusField);
                    break;

                case ScatterMode.Grid:
                    var gridSpacingField = new FloatField("Grid Spacing") { value = scatterRadius };
                    gridSpacingField.RegisterValueChangedCallback(evt => scatterRadius = evt.newValue);
                    container.Add(gridSpacingField);
                    break;
            }
        }

        private void UpdatePreview()
        {
            if (!showPreview) return;

            ClearPreview();

            if (prefabToScatter == null)
            {
                Debug.LogWarning("No prefab selected to scatter.");
                return;
            }

            try
            {
                var positions = GenerateScatterPositions();
                
                foreach (var position in positions)
                {
                    var preview = CreatePreviewObject(position);
                    previewObjects.Add(preview);
                }

                Debug.Log($"Generated preview with {positions.Count} objects");
            }
            catch (Exception e)
            {
                Debug.LogError($"Error generating preview: {e.Message}");
            }
        }

        private List<Vector3> GenerateScatterPositions()
        {
            var positions = new List<Vector3>();
            var random = new System.Random(seed);

            switch (scatterMode)
            {
                case ScatterMode.Surface:
                    positions = GenerateSurfacePositions(random);
                    break;

                case ScatterMode.Spline:
                    positions = GenerateSplinePositions(random);
                    break;

                case ScatterMode.Circle:
                    positions = GenerateCirclePositions(random);
                    break;

                case ScatterMode.Grid:
                    positions = GenerateGridPositions(random);
                    break;

                case ScatterMode.Custom:
                    positions = GenerateCustomPositions(random);
                    break;
            }

            // Apply noise offset if enabled
            if (useNoiseOffset)
            {
                for (int i = 0; i < positions.Count; i++)
                {
                    var noise = new Vector3(
                        Mathf.PerlinNoise(positions[i].x * noiseScale, positions[i].z * noiseScale),
                        Mathf.PerlinNoise(positions[i].z * noiseScale, positions[i].y * noiseScale),
                        Mathf.PerlinNoise(positions[i].y * noiseScale, positions[i].x * noiseScale)
                    ) * noiseStrength;
                    
                    positions[i] += noise;
                }
            }

            return positions;
        }

        private List<Vector3> GenerateSurfacePositions(System.Random random)
        {
            var positions = new List<Vector3>();
            var center = Selection.activeTransform ? Selection.activeTransform.position : Vector3.zero;

            for (int i = 0; i < scatterCount; i++)
            {
                var randomPoint = center + new Vector3(
                    (float)(random.NextDouble() - 0.5) * scatterRadius * 2,
                    0,
                    (float)(random.NextDouble() - 0.5) * scatterRadius * 2
                );

                // Raycast down to find surface
                if (Physics.Raycast(randomPoint + Vector3.up * 100, Vector3.down, out RaycastHit hit, 200f, surfaceLayerMask))
                {
                    positions.Add(hit.point);
                }
            }

            return positions;
        }

        private List<Vector3> GenerateSplinePositions(System.Random random)
        {
            var positions = new List<Vector3>();
            
            if (splinePoints.Count < 2)
            {
                Debug.LogWarning("Need at least 2 spline points to generate positions.");
                return positions;
            }

            for (int i = 0; i < scatterCount; i++)
            {
                var t = (float)i / (scatterCount - 1);
                var curveValue = distributionCurve.Evaluate(t);
                var position = EvaluateSpline(curveValue);
                positions.Add(position);
            }

            return positions;
        }

        private List<Vector3> GenerateCirclePositions(System.Random random)
        {
            var positions = new List<Vector3>();
            var center = Selection.activeTransform ? Selection.activeTransform.position : Vector3.zero;

            for (int i = 0; i < scatterCount; i++)
            {
                var angle = (float)i / scatterCount * 2 * Mathf.PI;
                var radius = scatterRadius * distributionCurve.Evaluate((float)i / scatterCount);
                
                var position = center + new Vector3(
                    Mathf.Cos(angle) * radius,
                    0,
                    Mathf.Sin(angle) * radius
                );
                
                positions.Add(position);
            }

            return positions;
        }

        private List<Vector3> GenerateGridPositions(System.Random random)
        {
            var positions = new List<Vector3>();
            var center = Selection.activeTransform ? Selection.activeTransform.position : Vector3.zero;
            var gridSize = Mathf.CeilToInt(Mathf.Sqrt(scatterCount));

            for (int x = 0; x < gridSize; x++)
            {
                for (int z = 0; z < gridSize; z++)
                {
                    if (positions.Count >= scatterCount) break;

                    var position = center + new Vector3(
                        (x - gridSize * 0.5f) * scatterRadius,
                        0,
                        (z - gridSize * 0.5f) * scatterRadius
                    );
                    
                    positions.Add(position);
                }
                if (positions.Count >= scatterCount) break;
            }

            return positions;
        }

        private List<Vector3> GenerateCustomPositions(System.Random random)
        {
            // Placeholder for custom scatter logic
            return new List<Vector3>();
        }

        private Vector3 EvaluateSpline(float t)
        {
            if (splinePoints.Count < 2) return Vector3.zero;

            t = Mathf.Clamp01(t);
            var segmentCount = splinePoints.Count - 1;
            var segment = Mathf.FloorToInt(t * segmentCount);
            var localT = (t * segmentCount) - segment;

            if (segment >= segmentCount)
            {
                return splinePoints[splinePoints.Count - 1];
            }

            return Vector3.Lerp(splinePoints[segment], splinePoints[segment + 1], localT);
        }

        private GameObject CreatePreviewObject(Vector3 position)
        {
            var preview = UnityEngine.Object.Instantiate(prefabToScatter);
            preview.name = prefabToScatter.name + "_Preview";
            preview.transform.position = position;

            // Apply random rotation
            if (randomRotation)
            {
                var rotation = Quaternion.Euler(
                    UnityEngine.Random.Range(rotationRange.x, rotationRange.y),
                    UnityEngine.Random.Range(rotationRange.x, rotationRange.y),
                    UnityEngine.Random.Range(rotationRange.x, rotationRange.y)
                );
                preview.transform.rotation = rotation;
            }

            // Apply random scale
            if (randomScale)
            {
                var scale = UnityEngine.Random.Range(scaleRange.x, scaleRange.y);
                preview.transform.localScale = Vector3.one * scale;
            }

            // Apply surface alignment
            if (alignToSurface && scatterMode == ScatterMode.Surface)
            {
                if (Physics.Raycast(position + Vector3.up, Vector3.down, out RaycastHit hit, 2f, surfaceLayerMask))
                {
                    preview.transform.up = hit.normal;
                }
            }

            // Make it a preview object
            preview.hideFlags = HideFlags.HideAndDontSave;

            return preview;
        }

        private void ApplyScatter()
        {
            if (prefabToScatter == null)
            {
                Debug.LogWarning("No prefab selected to scatter.");
                return;
            }

            using (var scope = new UndoScope("Scatter Objects"))
            {
                var positions = GenerateScatterPositions();
                var createdObjects = new List<GameObject>();

                foreach (var position in positions)
                {
                    var obj = PrefabUtility.InstantiatePrefab(prefabToScatter) as GameObject;
                    obj.transform.position = position;

                    // Apply random rotation
                    if (randomRotation)
                    {
                        var rotation = Quaternion.Euler(
                            UnityEngine.Random.Range(rotationRange.x, rotationRange.y),
                            UnityEngine.Random.Range(rotationRange.x, rotationRange.y),
                            UnityEngine.Random.Range(rotationRange.x, rotationRange.y)
                        );
                        obj.transform.rotation = rotation;
                    }

                    // Apply random scale
                    if (randomScale)
                    {
                        var scale = UnityEngine.Random.Range(scaleRange.x, scaleRange.y);
                        obj.transform.localScale = Vector3.one * scale;
                    }

                    // Apply surface alignment
                    if (alignToSurface && scatterMode == ScatterMode.Surface)
                    {
                        if (Physics.Raycast(position + Vector3.up, Vector3.down, out RaycastHit hit, 2f, surfaceLayerMask))
                        {
                            obj.transform.up = hit.normal;
                        }
                    }

                    Undo.RegisterCreatedObjectUndo(obj, "Scatter Object");
                    createdObjects.Add(obj);
                }

                Selection.objects = createdObjects.ToArray();
                Debug.Log($"Scattered {createdObjects.Count} objects");
                LogOperation($"Scattered {createdObjects.Count} objects");
            }

            ClearPreview();
        }

        private void ClearPreview()
        {
            foreach (var obj in previewObjects)
            {
                if (obj != null)
                {
                    UnityEngine.Object.DestroyImmediate(obj);
                }
            }
            previewObjects.Clear();
        }

        private void AddSplinePoint()
        {
            var point = Selection.activeTransform ? Selection.activeTransform.position : Vector3.zero;
            splinePoints.Add(point);
            
            var splineInfo = GetUIElement<Label>("spline-info");
            if (splineInfo != null)
            {
                splineInfo.text = $"Spline has {splinePoints.Count} points";
            }
        }

        private void ClearSpline()
        {
            splinePoints.Clear();
            
            var splineInfo = GetUIElement<Label>("spline-info");
            if (splineInfo != null)
            {
                splineInfo.text = $"Spline has {splinePoints.Count} points";
            }
        }

        private T GetUIElement<T>(string name) where T : VisualElement
        {
            // This would need to be implemented based on how the UI is structured
            // For now, returning null as placeholder
            return null;
        }

        // Scene view integration for spline editing
        [DrawGizmo(GizmoType.InSelectionHierarchy | GizmoType.NotInSelectionHierarchy)]
        static void DrawSplineGizmos(UnityEngine.Transform transform, GizmoType gizmoType)
        {
            // This would draw the spline in the scene view
            // Implementation would depend on having access to the tool instance
        }
    }
}
