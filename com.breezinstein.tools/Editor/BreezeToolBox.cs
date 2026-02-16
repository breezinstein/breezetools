using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UIToggle = UnityEngine.UIElements.Toggle;
using UIButton = UnityEngine.UIElements.Button;

namespace Breezinstein.Tools
{
    public class BreezeToolBox : EditorWindow
    {
        private VisualTreeAsset visualTreeAsset;
        private StyleSheet styleSheet;
        
        // Randomizer fields
        private UIToggle rotXToggle, rotYToggle, rotZToggle;
        private Vector3Field minRotationField, maxRotationField;
        private UIToggle posXToggle, posYToggle, posZToggle;
        private Vector3Field minPositionField, maxPositionField;
        private UIToggle uniformScaleToggle, perAxisScaleToggle;
        private Vector3Field minScaleField, maxScaleField;
        
        // Distribute fields
        private FloatField radialRadiusField, radialStartAngleField, radialEndAngleField;
        private UIToggle radialFaceCenterToggle, previewToggle;
        
        // Populate fields
        private LayerMaskField surfaceLayersField;
        private FloatField surfaceAngleLimitField, minSpacingField, maxSpacingField;
        private UIToggle alignToNormalToggle;
        private IntegerField objectCountField;
        private ObjectField primaryPrefabField;
        private VisualElement secondaryPrefabsContainer;
        private List<ObjectField> secondaryPrefabFields = new List<ObjectField>();
        
        // Replace fields
        private ObjectField replaceGameObjectField, replaceFontField, replaceMaterialField;
        private UIToggle maintainPositionToggle, maintainRotationToggle, maintainScaleToggle;
        private ObjectField findFontField, sceneReplaceFontField, findMaterialField, sceneReplaceMaterialField;
        
        // Missing references
        private VisualElement missingRefsResults;
        
        // Preview system
        private List<GameObject> previewObjects = new List<GameObject>();
        private Material previewMaterial;
        
        // Populate tracking
        private List<GameObject> populatedObjects = new List<GameObject>();

        [MenuItem("Breeze Tools/Toolbox v3")]
        static void Init()
        {
            BreezeToolBox window = GetWindow<BreezeToolBox>();
            window.titleContent = new GUIContent("Breeze Toolbox");
            window.minSize = new Vector2(400, 600);
            window.Show();
        }

        public void CreateGUI()
        {
            // Load UXML and USS
            var packagePath = "Packages/com.breezinstein.tools/Editor/";
            visualTreeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(packagePath + "BreezeToolBox.uxml");
            styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(packagePath + "BreezeToolBox.uss");
            
            if (visualTreeAsset != null)
            {
                visualTreeAsset.CloneTree(rootVisualElement);
                
                if (styleSheet != null)
                {
                    rootVisualElement.styleSheets.Add(styleSheet);
                }
                
                SetupUIReferences();
                SetupObjectFields();
                RegisterCallbacks();
                InitializeDefaultValues();
            }
            else
            {
                // Fallback UI
                CreateFallbackUI();
            }
        }

        private void SetupUIReferences()
        {
            // Randomizer references
            rotXToggle = rootVisualElement.Q<UIToggle>("rot-x-toggle");
            rotYToggle = rootVisualElement.Q<UIToggle>("rot-y-toggle");
            rotZToggle = rootVisualElement.Q<UIToggle>("rot-z-toggle");
            minRotationField = rootVisualElement.Q<Vector3Field>("min-rotation");
            maxRotationField = rootVisualElement.Q<Vector3Field>("max-rotation");
            
            posXToggle = rootVisualElement.Q<UIToggle>("pos-x-toggle");
            posYToggle = rootVisualElement.Q<UIToggle>("pos-y-toggle");
            posZToggle = rootVisualElement.Q<UIToggle>("pos-z-toggle");
            minPositionField = rootVisualElement.Q<Vector3Field>("min-position");
            maxPositionField = rootVisualElement.Q<Vector3Field>("max-position");
            
            uniformScaleToggle = rootVisualElement.Q<UIToggle>("uniform-scale-toggle");
            perAxisScaleToggle = rootVisualElement.Q<UIToggle>("per-axis-scale-toggle");
            minScaleField = rootVisualElement.Q<Vector3Field>("min-scale");
            maxScaleField = rootVisualElement.Q<Vector3Field>("max-scale");
            
            // Distribute references
            radialRadiusField = rootVisualElement.Q<FloatField>("radial-radius");
            radialStartAngleField = rootVisualElement.Q<FloatField>("radial-start-angle");
            radialEndAngleField = rootVisualElement.Q<FloatField>("radial-end-angle");
            radialFaceCenterToggle = rootVisualElement.Q<UIToggle>("radial-face-center");
            previewToggle = rootVisualElement.Q<UIToggle>("preview-toggle");
            
            // Populate references
            surfaceLayersField = rootVisualElement.Q<LayerMaskField>("surface-layers");
            surfaceAngleLimitField = rootVisualElement.Q<FloatField>("surface-angle-limit");
            alignToNormalToggle = rootVisualElement.Q<UIToggle>("align-to-normal");
            objectCountField = rootVisualElement.Q<IntegerField>("object-count");
            minSpacingField = rootVisualElement.Q<FloatField>("min-spacing");
            maxSpacingField = rootVisualElement.Q<FloatField>("max-spacing");
            primaryPrefabField = rootVisualElement.Q<ObjectField>("primary-prefab");
            secondaryPrefabsContainer = rootVisualElement.Q<VisualElement>("secondary-prefabs-container");
            
            // Replace references
            replaceGameObjectField = rootVisualElement.Q<ObjectField>("replace-gameobject");
            maintainPositionToggle = rootVisualElement.Q<UIToggle>("maintain-position");
            maintainRotationToggle = rootVisualElement.Q<UIToggle>("maintain-rotation");
            maintainScaleToggle = rootVisualElement.Q<UIToggle>("maintain-scale");
            replaceFontField = rootVisualElement.Q<ObjectField>("replace-font");
            replaceMaterialField = rootVisualElement.Q<ObjectField>("replace-material");
            
            findFontField = rootVisualElement.Q<ObjectField>("find-font");
            sceneReplaceFontField = rootVisualElement.Q<ObjectField>("scene-replace-font");
            findMaterialField = rootVisualElement.Q<ObjectField>("find-material");
            sceneReplaceMaterialField = rootVisualElement.Q<ObjectField>("scene-replace-material");
            
            // Missing references
            missingRefsResults = rootVisualElement.Q<VisualElement>("missing-refs-results");
        }

        private void SetupObjectFields()
        {
            // Configure object field types for specificity
            if (primaryPrefabField != null)
                primaryPrefabField.objectType = typeof(GameObject);

            if (replaceGameObjectField != null)
                replaceGameObjectField.objectType = typeof(GameObject);

            // Font fields only support Unity's legacy Font type
            if (replaceFontField != null)
                replaceFontField.objectType = typeof(Font);

            if (replaceMaterialField != null)
                replaceMaterialField.objectType = typeof(Material);

            if (findFontField != null)
                findFontField.objectType = typeof(Font);

            if (sceneReplaceFontField != null)
                sceneReplaceFontField.objectType = typeof(Font);

            if (findMaterialField != null)
                findMaterialField.objectType = typeof(Material);

            if (sceneReplaceMaterialField != null)
                sceneReplaceMaterialField.objectType = typeof(Material);
        }
        
        private void RegisterCallbacks()
        {
            // Pivot Utilities
            rootVisualElement.Q<Button>("center-pivot-btn").clicked += CenterPivot;
            rootVisualElement.Q<Button>("pivot-to-base-btn").clicked += PivotToBase;
            
            // Randomizer
            rootVisualElement.Q<Button>("apply-rotation-btn").clicked += ApplyRandomRotation;
            rootVisualElement.Q<Button>("apply-position-btn").clicked += ApplyRandomPosition;
            rootVisualElement.Q<Button>("apply-scale-btn").clicked += ApplyRandomScale;
            
            // Distribute
            rootVisualElement.Q<Button>("x-array-btn").clicked += () => CreateLinearArray(0);
            rootVisualElement.Q<Button>("y-array-btn").clicked += () => CreateLinearArray(1);
            rootVisualElement.Q<Button>("z-array-btn").clicked += () => CreateLinearArray(2);
            rootVisualElement.Q<Button>("radial-array-btn").clicked += CreateRadialArray;
            rootVisualElement.Q<Button>("apply-preview-btn").clicked += ApplyPreview;
            
            // Populate
            rootVisualElement.Q<Button>("add-secondary-prefab-btn").clicked += AddSecondaryPrefabField;
            rootVisualElement.Q<Button>("populate-btn").clicked += PopulateScene;
            rootVisualElement.Q<Button>("clear-population-btn").clicked += ClearPopulation;
            
            // Replace Selected
            rootVisualElement.Q<Button>("replace-objects-btn").clicked += ReplaceSelectedObjects;
            rootVisualElement.Q<Button>("replace-font-btn").clicked += ReplaceSelectedFont;
            rootVisualElement.Q<Button>("replace-material-btn").clicked += ReplaceSelectedMaterial;
            
            // Replace in Scene
            rootVisualElement.Q<Button>("replace-all-fonts-btn").clicked += ReplaceAllFonts;
            rootVisualElement.Q<Button>("replace-all-materials-btn").clicked += ReplaceAllMaterials;
            
            // Missing References
            rootVisualElement.Q<Button>("find-missing-refs-btn").clicked += FindMissingReferences;
            
            // Scale toggles exclusivity
            uniformScaleToggle.RegisterValueChangedCallback(evt => {
                if (evt.newValue) perAxisScaleToggle.value = false;
            });
            perAxisScaleToggle.RegisterValueChangedCallback(evt => {
                if (evt.newValue) uniformScaleToggle.value = false;
            });
            
            // Preview toggle
            previewToggle.RegisterValueChangedCallback(evt => {
                if (!evt.newValue) ClearPreview();
            });
        }

        private void InitializeDefaultValues()
        {
            // Set default values for Vector3 fields
            minRotationField.value = Vector3.zero;
            maxRotationField.value = new Vector3(360, 360, 360);
            minPositionField.value = Vector3.zero;
            maxPositionField.value = Vector3.one;
            minScaleField.value = new Vector3(0.8f, 0.8f, 0.8f);
            maxScaleField.value = new Vector3(1.2f, 1.2f, 1.2f);
            
            // Create preview material
            CreatePreviewMaterial();
        }

        #region Pivot Utilities
        private void CenterPivot()
        {
            var selectedTransforms = Selection.transforms;
            if (selectedTransforms.Length == 0)
            {
                EditorUtility.DisplayDialog("No Selection", "Please select one or more objects.", "OK");
                return;
            }

            foreach (var transform in selectedTransforms)
            {
                CenterPivotForObject(transform);
            }
        }

        private void CenterPivotForObject(Transform transform)
        {
            var meshFilter = transform.GetComponent<MeshFilter>();
            if (meshFilter == null || meshFilter.sharedMesh == null) return;

            var mesh = meshFilter.sharedMesh;
            var renderer = transform.GetComponent<Renderer>();
            if (renderer == null) return;

            // Calculate the center of the mesh bounds in local space
            var bounds = mesh.bounds;
            var center = bounds.center;
            
            // Create a new mesh with adjusted vertices
            var newMesh = Object.Instantiate(mesh);
            var vertices = newMesh.vertices;
            
            // Offset all vertices by the negative center to move pivot to center
            for (int i = 0; i < vertices.Length; i++)
            {
                vertices[i] -= center;
            }
            
            newMesh.vertices = vertices;
            newMesh.RecalculateBounds();
            newMesh.RecalculateNormals();
            newMesh.RecalculateTangents();
            
            // Record the undo operation
            Undo.RecordObject(meshFilter, "Center Pivot");
            Undo.RecordObject(transform, "Center Pivot Transform");
            
            // Move the transform to compensate for the mesh change
            transform.position += transform.TransformDirection(center);
            
            // Apply the new mesh
            meshFilter.mesh = newMesh;
            
            // Handle colliders
            var meshCollider = transform.GetComponent<MeshCollider>();
            if (meshCollider != null)
            {
                Undo.RecordObject(meshCollider, "Center Pivot Collider");
                meshCollider.sharedMesh = newMesh;
            }
        }

        private void PivotToBase()
        {
            var selectedTransforms = Selection.transforms;
            if (selectedTransforms.Length == 0)
            {
                EditorUtility.DisplayDialog("No Selection", "Please select one or more objects.", "OK");
                return;
            }

            foreach (var transform in selectedTransforms)
            {
                PivotToBaseForObject(transform);
            }
        }

        private void PivotToBaseForObject(Transform transform)
        {
            var meshFilter = transform.GetComponent<MeshFilter>();
            if (meshFilter == null || meshFilter.sharedMesh == null) return;

            var mesh = meshFilter.sharedMesh;
            var renderer = transform.GetComponent<Renderer>();
            if (renderer == null) return;

            // Calculate the base point of the mesh bounds in local space
            var bounds = mesh.bounds;
            var baseOffset = new Vector3(0, -bounds.min.y, 0); // Move pivot to bottom
            
            // Create a new mesh with adjusted vertices
            var newMesh = Object.Instantiate(mesh);
            var vertices = newMesh.vertices;
            
            // Offset all vertices to move pivot to base
            for (int i = 0; i < vertices.Length; i++)
            {
                vertices[i] += baseOffset;
            }
            
            newMesh.vertices = vertices;
            newMesh.RecalculateBounds();
            newMesh.RecalculateNormals();
            newMesh.RecalculateTangents();
            
            // Record the undo operation
            Undo.RecordObject(meshFilter, "Pivot to Base");
            Undo.RecordObject(transform, "Pivot to Base Transform");
            
            // Move the transform to compensate for the mesh change
            transform.position += transform.TransformDirection(new Vector3(0, bounds.min.y, 0));
            
            // Apply the new mesh
            meshFilter.mesh = newMesh;
            
            // Handle colliders
            var meshCollider = transform.GetComponent<MeshCollider>();
            if (meshCollider != null)
            {
                Undo.RecordObject(meshCollider, "Pivot to Base Collider");
                meshCollider.sharedMesh = newMesh;
            }
        }
        #endregion

        #region Randomizer
        private void ApplyRandomRotation()
        {
            var selectedTransforms = Selection.transforms;
            if (selectedTransforms.Length == 0) return;

            Undo.RecordObjects(selectedTransforms, "Apply Random Rotation");
            
            var minRot = minRotationField.value;
            var maxRot = maxRotationField.value;
            
            foreach (var transform in selectedTransforms)
            {
                var newRotation = new Vector3(
                    Random.Range(minRot.x, maxRot.x),
                    Random.Range(minRot.y, maxRot.y),
                    Random.Range(minRot.z, maxRot.z)
                );
                
                var originalRotation = transform.rotation.eulerAngles;
                var finalRotation = new Vector3(
                    rotXToggle.value ? newRotation.x : originalRotation.x,
                    rotYToggle.value ? newRotation.y : originalRotation.y,
                    rotZToggle.value ? newRotation.z : originalRotation.z
                );
                
                transform.rotation = Quaternion.Euler(finalRotation);
            }
        }

        private void ApplyRandomPosition()
        {
            var selectedTransforms = Selection.transforms;
            if (selectedTransforms.Length == 0) return;

            Undo.RecordObjects(selectedTransforms, "Apply Random Position");
            
            var minPos = minPositionField.value;
            var maxPos = maxPositionField.value;
            
            foreach (var transform in selectedTransforms)
            {
                var pos = transform.localPosition;
                if (posXToggle.value) pos.x = Random.Range(minPos.x, maxPos.x);
                if (posYToggle.value) pos.y = Random.Range(minPos.y, maxPos.y);
                if (posZToggle.value) pos.z = Random.Range(minPos.z, maxPos.z);
                transform.localPosition = pos;
            }
        }

        private void ApplyRandomScale()
        {
            var selectedTransforms = Selection.transforms;
            if (selectedTransforms.Length == 0) return;

            Undo.RecordObjects(selectedTransforms, "Apply Random Scale");
            
            var minScale = minScaleField.value;
            var maxScale = maxScaleField.value;
            
            foreach (var transform in selectedTransforms)
            {
                Vector3 newScale;
                
                if (uniformScaleToggle.value)
                {
                    var uniformScale = Random.Range(minScale.x, maxScale.x);
                    newScale = Vector3.one * uniformScale;
                }
                else
                {
                    newScale = new Vector3(
                        Random.Range(minScale.x, maxScale.x),
                        Random.Range(minScale.y, maxScale.y),
                        Random.Range(minScale.z, maxScale.z)
                    );
                }
                
                transform.localScale = newScale;
            }
        }
        #endregion

        #region Distribute
        private void CreateLinearArray(int axis)
        {
            var selectedTransforms = Selection.transforms;
            if (selectedTransforms.Length < 2)
            {
                EditorUtility.DisplayDialog("Insufficient Selection", "Please select at least 2 objects.", "OK");
                return;
            }

            if (previewToggle.value)
            {
                CreateLinearArrayPreview(axis);
                return;
            }

            Undo.RecordObjects(selectedTransforms, "Create Linear Array");
            
            // Get min and max positions for the selected axis
            float min = GetAxisValue(selectedTransforms[0].position, axis);
            float max = min;

            foreach (var transform in selectedTransforms)
            {
                float axisValue = GetAxisValue(transform.position, axis);
                if (axisValue < min) min = axisValue;
                if (axisValue > max) max = axisValue;
            }

            // Distribute objects evenly
            float increment = (max - min) / (selectedTransforms.Length - 1);
            for (int i = 0; i < selectedTransforms.Length; i++)
            {
                var position = selectedTransforms[i].position;
                SetAxisValue(ref position, axis, min + (increment * i));
                selectedTransforms[i].position = position;
            }
        }

        private void CreateLinearArrayPreview(int axis)
        {
            ClearPreview();
            var selectedTransforms = Selection.transforms;
            
            // Get min and max positions for the selected axis
            float min = GetAxisValue(selectedTransforms[0].position, axis);
            float max = min;

            foreach (var transform in selectedTransforms)
            {
                float axisValue = GetAxisValue(transform.position, axis);
                if (axisValue < min) min = axisValue;
                if (axisValue > max) max = axisValue;
            }

            // Create preview objects
            float increment = (max - min) / (selectedTransforms.Length - 1);
            for (int i = 0; i < selectedTransforms.Length; i++)
            {
                var previewObj = CreatePreviewObject(selectedTransforms[i]);
                var position = selectedTransforms[i].position;
                SetAxisValue(ref position, axis, min + (increment * i));
                previewObj.transform.position = position;
                previewObjects.Add(previewObj);
            }
        }

        private void CreateRadialArray()
        {
            var selectedTransforms = Selection.transforms;
            if (selectedTransforms.Length == 0) return;

            if (previewToggle.value)
            {
                CreateRadialArrayPreview();
                return;
            }

            Undo.RecordObjects(selectedTransforms, "Create Radial Array");
            
            var radius = radialRadiusField.value;
            var startAngle = radialStartAngleField.value;
            var endAngle = radialEndAngleField.value;
            var faceCenter = radialFaceCenterToggle.value;
            
            var center = CalculateSelectionCenter(selectedTransforms);
            var angleStep = (endAngle - startAngle) / Mathf.Max(1, selectedTransforms.Length - 1);
            
            for (int i = 0; i < selectedTransforms.Length; i++)
            {
                var angle = startAngle + (angleStep * i);
                var radian = angle * Mathf.Deg2Rad;
                var position = center + new Vector3(
                    Mathf.Cos(radian) * radius,
                    0,
                    Mathf.Sin(radian) * radius
                );
                
                selectedTransforms[i].position = position;
                
                if (faceCenter)
                {
                    selectedTransforms[i].LookAt(center);
                }
            }
        }

        private void CreateRadialArrayPreview()
        {
            ClearPreview();
            var selectedTransforms = Selection.transforms;
            
            var radius = radialRadiusField.value;
            var startAngle = radialStartAngleField.value;
            var endAngle = radialEndAngleField.value;
            var faceCenter = radialFaceCenterToggle.value;
            
            var center = CalculateSelectionCenter(selectedTransforms);
            var angleStep = (endAngle - startAngle) / Mathf.Max(1, selectedTransforms.Length - 1);
            
            for (int i = 0; i < selectedTransforms.Length; i++)
            {
                var angle = startAngle + (angleStep * i);
                var radian = angle * Mathf.Deg2Rad;
                var position = center + new Vector3(
                    Mathf.Cos(radian) * radius,
                    0,
                    Mathf.Sin(radian) * radius
                );
                
                var previewObj = CreatePreviewObject(selectedTransforms[i]);
                previewObj.transform.position = position;
                
                if (faceCenter)
                {
                    previewObj.transform.LookAt(center);
                }
                
                previewObjects.Add(previewObj);
            }
        }

        private float GetAxisValue(Vector3 position, int axis)
        {
            return axis switch
            {
                0 => position.x,
                1 => position.y,
                2 => position.z,
                _ => 0f
            };
        }

        private void SetAxisValue(ref Vector3 position, int axis, float value)
        {
            switch (axis)
            {
                case 0: position.x = value; break;
                case 1: position.y = value; break;
                case 2: position.z = value; break;
            }
        }

        private Vector3 CalculateSelectionCenter(Transform[] transforms)
        {
            if (transforms.Length == 0) return Vector3.zero;
            
            var center = Vector3.zero;
            foreach (var transform in transforms)
            {
                center += transform.position;
            }
            return center / transforms.Length;
        }
        #endregion

        #region Preview System
        private void CreatePreviewMaterial()
        {
            previewMaterial = new Material(Shader.Find("Standard"));
            previewMaterial.color = new Color(0.3f, 0.8f, 1f, 0.5f);
            previewMaterial.SetFloat("_Mode", 3); // Transparent mode
            previewMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            previewMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            previewMaterial.SetInt("_ZWrite", 0);
            previewMaterial.DisableKeyword("_ALPHATEST_ON");
            previewMaterial.EnableKeyword("_ALPHABLEND_ON");
            previewMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            previewMaterial.renderQueue = 3000;
        }

        private GameObject CreatePreviewObject(Transform original)
        {
            var previewObj = new GameObject("Preview_" + original.name);
            previewObj.hideFlags = HideFlags.HideAndDontSave;
            
            // Copy mesh renderers
            var originalRenderers = original.GetComponentsInChildren<MeshRenderer>();
            foreach (var renderer in originalRenderers)
            {
                var meshFilter = renderer.GetComponent<MeshFilter>();
                if (meshFilter != null)
                {
                    var previewChild = new GameObject(renderer.name);
                    previewChild.transform.SetParent(previewObj.transform);
                    previewChild.transform.localPosition = renderer.transform.localPosition;
                    previewChild.transform.localRotation = renderer.transform.localRotation;
                    previewChild.transform.localScale = renderer.transform.localScale;
                    
                    var previewMeshFilter = previewChild.AddComponent<MeshFilter>();
                    previewMeshFilter.mesh = meshFilter.mesh;
                    
                    var previewRenderer = previewChild.AddComponent<MeshRenderer>();
                    previewRenderer.material = previewMaterial;
                }
            }
            
            return previewObj;
        }

        private void ClearPreview()
        {
            foreach (var obj in previewObjects)
            {
                if (obj != null) DestroyImmediate(obj);
            }
            previewObjects.Clear();
        }

        private void ApplyPreview()
        {
            if (previewObjects.Count == 0) return;
            
            var selectedTransforms = Selection.transforms;
            if (selectedTransforms.Length != previewObjects.Count) return;
            
            Undo.RecordObjects(selectedTransforms, "Apply Preview");
            
            for (int i = 0; i < selectedTransforms.Length; i++)
            {
                selectedTransforms[i].position = previewObjects[i].transform.position;
                selectedTransforms[i].rotation = previewObjects[i].transform.rotation;
            }
            
            ClearPreview();
            previewToggle.value = false;
        }
        #endregion

        #region Populate
        private void AddSecondaryPrefabField()
        {
            var objectField = new ObjectField($"Secondary Prefab {secondaryPrefabFields.Count + 1}")
            {
                objectType = typeof(GameObject)
            };
            
            var container = new VisualElement();
            container.style.flexDirection = FlexDirection.Row;
            container.Add(objectField);
            
            var removeButton = new Button(() => {
                secondaryPrefabsContainer.Remove(container);
                secondaryPrefabFields.Remove(objectField);
            }) { text = "Remove" };
            removeButton.AddToClassList("action-button");
            removeButton.AddToClassList("secondary");
            removeButton.style.width = 80;
            
            container.Add(removeButton);
            secondaryPrefabsContainer.Add(container);
            secondaryPrefabFields.Add(objectField);
        }

        private void PopulateScene()
        {
            var primaryPrefab = primaryPrefabField.value as GameObject;
            if (primaryPrefab == null)
            {
                EditorUtility.DisplayDialog("No Primary Prefab", "Please assign a primary prefab.", "OK");
                return;
            }

            var selectedObjects = Selection.gameObjects;
            if (selectedObjects.Length == 0)
            {
                EditorUtility.DisplayDialog("No Selection", "Please select at least one GameObject in the scene to define the population area bounds.", "OK");
                return;
            }

            // Get all prefabs to spawn
            var allPrefabs = GetAllPrefabs();
            if (allPrefabs.Count == 0)
            {
                EditorUtility.DisplayDialog("No Prefabs", "No valid prefabs available for spawning.", "OK");
                return;
            }

            // Calculate bounds from selected objects
            var selectedTransforms = selectedObjects.Select(go => go.transform).ToArray();
            var bounds = CalculatePopulationBounds(selectedTransforms);

            if (bounds.size == Vector3.zero)
            {
                EditorUtility.DisplayDialog("Invalid Bounds", "Could not calculate valid bounds from selected objects.", "OK");
                return;
            }

            // Create parent object for organization
            var timestamp = System.DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var parentObject = new GameObject($"Population_{timestamp}");
            Undo.RegisterCreatedObjectUndo(parentObject, "Create Population Parent");

            int objectCount = objectCountField.value;
            float minSpacing = minSpacingField.value;
            float maxSpacing = maxSpacingField.value;
            float surfaceAngleLimit = surfaceAngleLimitField.value;
            int surfaceLayers = surfaceLayersField.value;
            bool alignToNormal = alignToNormalToggle.value;

            var spawnedPositions = new List<Vector3>();
            int spawnedCount = 0;
            int maxAttemptsPerObject = 100;

            try
            {
                for (int i = 0; i < objectCount; i++)
                {
                    EditorUtility.DisplayProgressBar("Populating Scene", $"Spawning object {i + 1} of {objectCount}", (float)i / objectCount);

                    bool spawned = false;
                    for (int attempt = 0; attempt < maxAttemptsPerObject && !spawned; attempt++)
                    {
                        // Generate random XZ position within bounds
                        float randomX = Random.Range(bounds.min.x, bounds.max.x);
                        float randomZ = Random.Range(bounds.min.z, bounds.max.z);
                        float raycastHeight = bounds.max.y + 10f;

                        Vector3 rayOrigin = new Vector3(randomX, raycastHeight, randomZ);
                        RaycastHit hit;

                        // Raycast downward to find surface
                        if (Physics.Raycast(rayOrigin, Vector3.down, out hit, raycastHeight - bounds.min.y + 20f, surfaceLayers))
                        {
                            // Check surface angle
                            float surfaceAngle = Vector3.Angle(hit.normal, Vector3.up);
                            if (surfaceAngle > surfaceAngleLimit)
                            {
                                continue; // Surface too steep
                            }

                            // Check spacing against already spawned objects
                            float requiredSpacing = Random.Range(minSpacing, maxSpacing);
                            bool tooClose = false;
                            foreach (var pos in spawnedPositions)
                            {
                                if (Vector3.Distance(hit.point, pos) < requiredSpacing)
                                {
                                    tooClose = true;
                                    break;
                                }
                            }

                            if (tooClose)
                            {
                                continue; // Too close to another object
                            }

                            // Select random prefab
                            var prefabToSpawn = allPrefabs[Random.Range(0, allPrefabs.Count)];

                            // Instantiate prefab
                            var spawnedObject = PrefabUtility.InstantiatePrefab(prefabToSpawn) as GameObject;
                            if (spawnedObject != null)
                            {
                                spawnedObject.transform.position = hit.point;

                                // Align to surface normal if enabled
                                if (alignToNormal)
                                {
                                    spawnedObject.transform.rotation = Quaternion.FromToRotation(Vector3.up, hit.normal);
                                }

                                // Parent under organization object
                                spawnedObject.transform.SetParent(parentObject.transform);

                                // Register for undo
                                Undo.RegisterCreatedObjectUndo(spawnedObject, "Spawn Population Object");

                                // Track spawned object
                                populatedObjects.Add(spawnedObject);
                                spawnedPositions.Add(hit.point);
                                spawnedCount++;
                                spawned = true;
                            }
                        }
                    }
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }

            Debug.Log($"Population complete: Spawned {spawnedCount} of {objectCount} requested objects.");
        }

        private void ClearPopulation()
        {
            if (populatedObjects.Count == 0)
            {
                EditorUtility.DisplayDialog("Nothing to Clear", "No populated objects to clear.", "OK");
                return;
            }

            Undo.SetCurrentGroupName("Clear Population");
            int undoGroup = Undo.GetCurrentGroup();

            foreach (var obj in populatedObjects)
            {
                if (obj != null)
                {
                    Undo.DestroyObjectImmediate(obj);
                }
            }

            populatedObjects.Clear();
            Undo.CollapseUndoOperations(undoGroup);

            Debug.Log("Population cleared.");
        }

        private List<GameObject> GetAllPrefabs()
        {
            var prefabs = new List<GameObject>();

            var primaryPrefab = primaryPrefabField.value as GameObject;
            if (primaryPrefab != null)
            {
                prefabs.Add(primaryPrefab);
            }

            foreach (var field in secondaryPrefabFields)
            {
                var secondaryPrefab = field.value as GameObject;
                if (secondaryPrefab != null)
                {
                    prefabs.Add(secondaryPrefab);
                }
            }

            return prefabs;
        }

        private Bounds CalculatePopulationBounds(Transform[] selectedTransforms)
        {
            if (selectedTransforms == null || selectedTransforms.Length == 0)
            {
                return new Bounds();
            }

            Bounds bounds = new Bounds();
            bool boundsInitialized = false;

            foreach (var transform in selectedTransforms)
            {
                if (transform == null) continue;

                // Try to get bounds from Renderer
                var renderer = transform.GetComponent<Renderer>();
                if (renderer != null)
                {
                    if (!boundsInitialized)
                    {
                        bounds = renderer.bounds;
                        boundsInitialized = true;
                    }
                    else
                    {
                        bounds.Encapsulate(renderer.bounds);
                    }
                    continue;
                }

                // Try to get bounds from Collider
                var collider = transform.GetComponent<Collider>();
                if (collider != null)
                {
                    if (!boundsInitialized)
                    {
                        bounds = collider.bounds;
                        boundsInitialized = true;
                    }
                    else
                    {
                        bounds.Encapsulate(collider.bounds);
                    }
                    continue;
                }

                // Fallback: use transform position
                if (!boundsInitialized)
                {
                    bounds = new Bounds(transform.position, Vector3.one);
                    boundsInitialized = true;
                }
                else
                {
                    bounds.Encapsulate(transform.position);
                }
            }

            return bounds;
        }
        #endregion

        #region Replace Selected
        private void ReplaceSelectedObjects()
        {
            var replacementPrefab = replaceGameObjectField.value as GameObject;
            var selectedObjects = Selection.gameObjects;
            
            if (replacementPrefab == null || selectedObjects.Length == 0) return;

            Undo.RecordObjects(selectedObjects, "Replace Selected Objects");
            
            var newObjects = new List<GameObject>();
            
            foreach (var obj in selectedObjects)
            {
                var newObj = PrefabUtility.InstantiatePrefab(replacementPrefab) as GameObject;
                
                if (maintainPositionToggle.value)
                    newObj.transform.position = obj.transform.position;
                if (maintainRotationToggle.value)
                    newObj.transform.rotation = obj.transform.rotation;
                if (maintainScaleToggle.value)
                    newObj.transform.localScale = obj.transform.localScale;
                
                newObj.transform.SetParent(obj.transform.parent);
                newObjects.Add(newObj);
                
                Undo.RegisterCreatedObjectUndo(newObj, "Replace Object");
                Undo.DestroyObjectImmediate(obj);
            }
            
            Selection.objects = newObjects.ToArray();
        }

        private void ReplaceSelectedFont()
        {
            var newFont = replaceFontField.value as Font;
            var selectedObjects = Selection.gameObjects;
            
            if (newFont == null || selectedObjects.Length == 0) return;

            var modifiedComponents = new List<Component>();
            
            foreach (var obj in selectedObjects)
            {
                var textComponents = obj.GetComponentsInChildren<UnityEngine.UI.Text>();
                modifiedComponents.AddRange(textComponents);
            }
            
            if (modifiedComponents.Count == 0) return;
            
            Undo.RecordObjects(modifiedComponents.ToArray(), "Replace Font");
            
            foreach (UnityEngine.UI.Text textComponent in modifiedComponents)
            {
                textComponent.font = newFont;
            }
        }

        private void ReplaceSelectedMaterial()
        {
            var newMaterial = replaceMaterialField.value as Material;
            var selectedObjects = Selection.gameObjects;
            
            if (newMaterial == null || selectedObjects.Length == 0) return;

            var modifiedRenderers = new List<Renderer>();
            
            foreach (var obj in selectedObjects)
            {
                var renderers = obj.GetComponentsInChildren<Renderer>();
                modifiedRenderers.AddRange(renderers);
            }
            
            Undo.RecordObjects(modifiedRenderers.ToArray(), "Replace Material");
            
            foreach (var renderer in modifiedRenderers)
            {
                var materials = renderer.materials;
                for (int i = 0; i < materials.Length; i++)
                {
                    materials[i] = newMaterial;
                }
                renderer.materials = materials;
            }
        }
        #endregion

        #region Replace in Scene
        private void ReplaceAllFonts()
        {
            var findFont = findFontField.value as Font;
            var replaceFont = sceneReplaceFontField.value as Font;
            
            if (findFont == null || replaceFont == null) return;

            var allTextComponents = FindObjectsByType<UnityEngine.UI.Text>(FindObjectsSortMode.None);
            var componentsToModify = allTextComponents.Where(t => t.font == findFont).ToArray();
            
            if (componentsToModify.Length == 0)
            {
                EditorUtility.DisplayDialog("No Matches", "No Text components found using the specified font.", "OK");
                return;
            }
            
            Undo.RecordObjects(componentsToModify.Cast<Object>().ToArray(), "Replace All Fonts");
            
            foreach (var textComponent in componentsToModify)
            {
                textComponent.font = replaceFont;
            }
            
            EditorUtility.DisplayDialog("Replace Complete", $"Replaced font on {componentsToModify.Length} Text components.", "OK");
        }

        private void ReplaceAllMaterials()
        {
            var findMaterial = findMaterialField.value as Material;
            var replaceMaterial = sceneReplaceMaterialField.value as Material;
            
            if (findMaterial == null || replaceMaterial == null) return;

            var allRenderers = FindObjectsByType<Renderer>(FindObjectsSortMode.None);
            var renderersToModify = new List<Renderer>();
            
            foreach (var renderer in allRenderers)
            {
                if (renderer.materials.Contains(findMaterial))
                {
                    renderersToModify.Add(renderer);
                }
            }
            
            if (renderersToModify.Count == 0)
            {
                EditorUtility.DisplayDialog("No Matches", "No Renderers found using the specified material.", "OK");
                return;
            }
            
            Undo.RecordObjects(renderersToModify.ToArray(), "Replace All Materials");
            
            foreach (var renderer in renderersToModify)
            {
                var materials = renderer.materials;
                for (int i = 0; i < materials.Length; i++)
                {
                    if (materials[i] == findMaterial)
                    {
                        materials[i] = replaceMaterial;
                    }
                }
                renderer.materials = materials;
            }
            
            EditorUtility.DisplayDialog("Replace Complete", $"Replaced material on {renderersToModify.Count} Renderers.", "OK");
        }
        #endregion

        #region Missing References
        private void FindMissingReferences()
        {
            missingRefsResults.Clear();
            
            var allGameObjects = FindObjectsByType<GameObject>(FindObjectsSortMode.None);
            var missingRefs = new List<string>();
            
            foreach (var obj in allGameObjects)
            {
                var components = obj.GetComponents<Component>();
                for (int i = 0; i < components.Length; i++)
                {
                    if (components[i] == null)
                    {
                        missingRefs.Add($"{obj.name} - Missing Script at index {i}");
                    }
                }
            }
            
            if (missingRefs.Count == 0)
            {
                var noIssuesLabel = new Label("No missing references found!");
                noIssuesLabel.style.color = Color.green;
                missingRefsResults.Add(noIssuesLabel);
            }
            else
            {
                var titleLabel = new Label($"Found {missingRefs.Count} missing references:");
                titleLabel.style.color = Color.red;
                titleLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
                missingRefsResults.Add(titleLabel);
                
                foreach (var missingRef in missingRefs)
                {
                    var refLabel = new Label($"• {missingRef}");
                    refLabel.style.whiteSpace = WhiteSpace.Normal;
                    refLabel.style.fontSize = 10;
                    missingRefsResults.Add(refLabel);
                }
            }
        }

        #endregion

        #region Fallback UI
        private void CreateFallbackUI()
        {
            var label = new Label("BreezeToolBox - UI files not found. Using fallback interface.");
            label.style.fontSize = 14;
            label.style.unityFontStyleAndWeight = FontStyle.Bold;
            label.style.color = Color.red;
            label.style.unityTextAlign = TextAnchor.UpperCenter;
            label.style.marginTop = 20;
            label.style.marginBottom = 20;
            
            rootVisualElement.Add(label);
            
            var fallbackButton = new Button(() => {
                EditorUtility.DisplayDialog("Fallback", "Please ensure UXML and USS files are in the correct location.", "OK");
            })
            {
                text = "Show Original Interface"
            };
            
            rootVisualElement.Add(fallbackButton);
        }
        #endregion

        private void OnDestroy()
        {
            ClearPreview();
        }
    }
}