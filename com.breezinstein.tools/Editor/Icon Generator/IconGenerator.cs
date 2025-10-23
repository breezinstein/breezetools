using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace BreezeTools.Editor.IconGenerator
{
    /// <summary>
    /// Core icon generation logic
    /// </summary>
    public class IconGenerator
    {
        private readonly IconGeneratorSettings settings;
        private Camera renderCamera;
        private GameObject cameraObject;
        private RenderTexture renderTexture;
        
        public IconGenerator(IconGeneratorSettings settings)
        {
            this.settings = settings;
        }
        
        /// <summary>
        /// Generate icons for all provided prefabs
        /// </summary>
        public void GenerateIcons(List<GameObject> prefabs, Action<int, int> progressCallback = null)
        {
            if (prefabs == null || prefabs.Count == 0)
            {
                Debug.LogWarning("No prefabs to generate icons for.");
                return;
            }
            
            SetupRenderCamera();
            
            try
            {
                for (int i = 0; i < prefabs.Count; i++)
                {
                    var prefab = prefabs[i];
                    
                    if (prefab == null)
                    {
                        Debug.LogWarning($"Skipping null prefab at index {i}");
                        continue;
                    }
                    
                    try
                    {
                        progressCallback?.Invoke(i, prefabs.Count);
                        GenerateIconForPrefab(prefab);
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"Failed to generate icon for {prefab.name}: {e.Message}");
                    }
                }
                
                progressCallback?.Invoke(prefabs.Count, prefabs.Count);
                
                if (settings.autoRefreshAssetDatabase)
                {
                    AssetDatabase.Refresh();
                }
            }
            finally
            {
                CleanupRenderCamera();
            }
        }
        
        /// <summary>
        /// Generate icon for a single prefab
        /// </summary>
        private void GenerateIconForPrefab(GameObject prefab)
        {
            // Validate prefab
            if (PrefabUtility.GetPrefabAssetType(prefab) == PrefabAssetType.NotAPrefab)
            {
                Debug.LogWarning($"{prefab.name} is not a valid prefab. Skipping.");
                return;
            }
            
            // Instantiate prefab in isolated position
            GameObject instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            if (instance == null)
            {
                Debug.LogError($"Failed to instantiate prefab {prefab.name}");
                return;
            }
            
            try
            {
                // Position far away from scene origin to avoid interference
                Vector3 isolatedPosition = new Vector3(10000, 10000, 10000);
                instance.transform.position = isolatedPosition;
                
                // Get all renderers to calculate bounds
                Renderer[] renderers = instance.GetComponentsInChildren<Renderer>();
                
                if (renderers.Length == 0)
                {
                    Debug.LogWarning($"Prefab {prefab.name} has no renderers. Cannot generate icon.");
                    return;
                }
                
                // Calculate combined bounds
                Bounds combinedBounds = renderers[0].bounds;
                for (int i = 1; i < renderers.Length; i++)
                {
                    combinedBounds.Encapsulate(renderers[i].bounds);
                }
                
                // Position camera to capture the object
                PositionCamera(combinedBounds);
                
                // Render to texture
                RenderTexture iconTexture = RenderPrefabToTexture(instance);
                
                // Convert to Texture2D and save
                Texture2D texture2D = ConvertRenderTextureToTexture2D(iconTexture);
                string outputPath = GetOutputPath(prefab);
                SaveTextureAsPNG(texture2D, outputPath);
                
                // Cleanup temporary texture
                RenderTexture.ReleaseTemporary(iconTexture);
                UnityEngine.Object.DestroyImmediate(texture2D);
                
                // Configure sprite settings if needed
                if (settings.configureAsUISprite)
                {
                    ConfigureSpriteImportSettings(outputPath);
                }
                
                Debug.Log($"Generated icon for {prefab.name} at {outputPath}");
            }
            finally
            {
                // Always cleanup instantiated object
                UnityEngine.Object.DestroyImmediate(instance);
            }
        }
        
        /// <summary>
        /// Setup the render camera
        /// </summary>
        private void SetupRenderCamera()
        {
            cameraObject = new GameObject("IconGeneratorCamera");
            cameraObject.hideFlags = HideFlags.HideAndDontSave;
            
            renderCamera = cameraObject.AddComponent<Camera>();
            renderCamera.orthographic = true;
            renderCamera.clearFlags = settings.useTransparentBackground 
                ? CameraClearFlags.SolidColor 
                : CameraClearFlags.SolidColor;
            renderCamera.backgroundColor = settings.useTransparentBackground 
                ? new Color(0, 0, 0, 0) 
                : settings.backgroundColor;
            renderCamera.cullingMask = -1; // Render everything
            renderCamera.enabled = false; // Manual rendering
        }
        
        /// <summary>
        /// Position camera to capture object bounds
        /// </summary>
        private void PositionCamera(Bounds bounds)
        {
            // Calculate camera position based on bounds and distance
            Vector3 objectCenter = bounds.center;
            float maxBoundsSize = Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z);
            float cameraDistance = maxBoundsSize * settings.cameraDistance;
            
            // Apply rotation to determine camera direction
            Quaternion rotation = Quaternion.Euler(settings.cameraRotation);
            Vector3 cameraDirection = rotation * Vector3.back; // Camera looks forward (negative Z)
            
            // Position camera
            cameraObject.transform.position = objectCenter - cameraDirection * cameraDistance;
            cameraObject.transform.rotation = rotation;
            
            // Ensure camera is looking at the object
            cameraObject.transform.LookAt(objectCenter);
            
            // Set orthographic size based on bounds and zoom
            float orthoSize = (maxBoundsSize * 0.6f) / settings.cameraZoom;
            renderCamera.orthographicSize = orthoSize;
        }
        
        /// <summary>
        /// Render prefab to a RenderTexture
        /// </summary>
        private RenderTexture RenderPrefabToTexture(GameObject prefab)
        {
            Vector2Int resolution = settings.GetResolution();
            
            // Create render texture with transparency support
            RenderTexture rt = RenderTexture.GetTemporary(
                resolution.x, 
                resolution.y, 
                24, 
                RenderTextureFormat.ARGB32
            );
            rt.antiAliasing = 4;
            
            // Render
            renderCamera.targetTexture = rt;
            renderCamera.Render();
            renderCamera.targetTexture = null;
            
            return rt;
        }
        
        /// <summary>
        /// Convert RenderTexture to Texture2D
        /// </summary>
        private Texture2D ConvertRenderTextureToTexture2D(RenderTexture renderTexture)
        {
            RenderTexture previous = RenderTexture.active;
            RenderTexture.active = renderTexture;
            
            Texture2D texture = new Texture2D(
                renderTexture.width, 
                renderTexture.height, 
                TextureFormat.ARGB32, 
                false
            );
            
            texture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
            texture.Apply();
            
            RenderTexture.active = previous;
            
            return texture;
        }
        
        /// <summary>
        /// Save Texture2D as PNG file
        /// </summary>
        private void SaveTextureAsPNG(Texture2D texture, string path)
        {
            byte[] bytes = texture.EncodeToPNG();
            
            // Ensure directory exists
            string directory = Path.GetDirectoryName(path);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            
            File.WriteAllBytes(path, bytes);
            AssetDatabase.ImportAsset(path);
        }
        
        /// <summary>
        /// Get output path for a prefab based on settings
        /// </summary>
        private string GetOutputPath(GameObject prefab)
        {
            string prefabPath = AssetDatabase.GetAssetPath(prefab);
            string prefabDirectory = Path.GetDirectoryName(prefabPath);
            string prefabName = Path.GetFileNameWithoutExtension(prefabPath);
            string iconFileName = prefabName + settings.iconSuffix + ".png";
            
            string outputDirectory;
            
            switch (settings.outputPathMode)
            {
                case OutputPathMode.MirrorPrefabPath:
                    outputDirectory = prefabDirectory;
                    if (settings.createSubfolderPerCategory)
                    {
                        outputDirectory = Path.Combine(prefabDirectory, "Icons");
                    }
                    break;
                    
                case OutputPathMode.CustomFolder:
                    outputDirectory = settings.customOutputFolder;
                    if (settings.createSubfolderPerCategory)
                    {
                        // Extract category from prefab path (parent folder name)
                        string parentFolder = Path.GetFileName(prefabDirectory);
                        outputDirectory = Path.Combine(settings.customOutputFolder, parentFolder);
                    }
                    break;
                    
                case OutputPathMode.PrefabFolderWithSubfolder:
                    outputDirectory = Path.Combine(prefabDirectory, "Icons");
                    break;
                    
                default:
                    outputDirectory = prefabDirectory;
                    break;
            }
            
            return Path.Combine(outputDirectory, iconFileName).Replace("\\", "/");
        }
        
        /// <summary>
        /// Configure sprite import settings for UI usage
        /// </summary>
        private void ConfigureSpriteImportSettings(string assetPath)
        {
            TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            
            if (importer == null)
            {
                Debug.LogWarning($"Could not get TextureImporter for {assetPath}");
                return;
            }
            
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.filterMode = settings.filterMode;
            importer.textureCompression = settings.compression;
            importer.maxTextureSize = settings.maxTextureSize;
            
            // Platform-specific settings
            var platformSettings = importer.GetDefaultPlatformTextureSettings();
            platformSettings.maxTextureSize = settings.maxTextureSize;
            platformSettings.format = TextureImporterFormat.RGBA32;
            importer.SetPlatformTextureSettings(platformSettings);
            
            importer.SaveAndReimport();
        }
        
        /// <summary>
        /// Cleanup render camera resources
        /// </summary>
        private void CleanupRenderCamera()
        {
            if (renderTexture != null)
            {
                RenderTexture.ReleaseTemporary(renderTexture);
                renderTexture = null;
            }
            
            if (cameraObject != null)
            {
                UnityEngine.Object.DestroyImmediate(cameraObject);
                cameraObject = null;
            }
            
            renderCamera = null;
        }
    }
}
