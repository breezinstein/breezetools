using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace BreezeTools.Editor.IconGenerator.Examples
{
    /// <summary>
    /// Example script demonstrating programmatic usage of the Icon Generator
    /// </summary>
    public static class IconGeneratorExamples
    {
        /// <summary>
        /// Example: Generate icons for all prefabs in a folder
        /// </summary>
        [MenuItem("Tools/BreezeTools/Icon Generator/Examples/Generate Icons for Folder")]
        public static void GenerateIconsForFolder()
        {
            // Define the folder path
            string folderPath = "Assets/Game/Items"; // Change this to your folder
            
            // Find all prefabs in the folder
            string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { folderPath });
            List<GameObject> prefabs = new List<GameObject>();
            
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                
                if (prefab != null)
                {
                    prefabs.Add(prefab);
                }
            }
            
            if (prefabs.Count == 0)
            {
                Debug.LogWarning($"No prefabs found in {folderPath}");
                return;
            }
            
            // Configure settings
            IconGeneratorSettings settings = new IconGeneratorSettings
            {
                resolution = IconResolution.Resolution256,
                useTransparentBackground = true,
                cameraDistance = 5f,
                cameraRotation = new Vector3(15f, -30f, 0f),
                cameraZoom = 1f,
                outputPathMode = OutputPathMode.PrefabFolderWithSubfolder,
                iconSuffix = "_Icon",
                createSubfolderPerCategory = true,
                autoRefreshAssetDatabase = true,
                configureAsUISprite = true
            };
            
            // Generate icons
            IconGenerator generator = new IconGenerator(settings);
            
            try
            {
                generator.GenerateIcons(prefabs, (current, total) =>
                {
                    EditorUtility.DisplayProgressBar(
                        "Generating Icons",
                        $"Processing {current}/{total}",
                        (float)current / total
                    );
                });
                
                Debug.Log($"Successfully generated {prefabs.Count} icons!");
                EditorUtility.DisplayDialog("Success", $"Generated {prefabs.Count} icons!", "OK");
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }
        
        /// <summary>
        /// Example: Generate a single high-quality icon
        /// </summary>
        [MenuItem("Tools/BreezeTools/Icon Generator/Examples/Generate High-Quality Icon")]
        public static void GenerateHighQualityIcon()
        {
            // Use object picker to select a prefab
            GameObject prefab = Selection.activeGameObject;
            
            if (prefab == null || PrefabUtility.GetPrefabAssetType(prefab) == PrefabAssetType.NotAPrefab)
            {
                EditorUtility.DisplayDialog("Error", "Please select a prefab in the Project window first.", "OK");
                return;
            }
            
            // High-quality settings
            IconGeneratorSettings settings = new IconGeneratorSettings
            {
                resolution = IconResolution.Resolution512,
                useTransparentBackground = true,
                cameraDistance = 4f,
                cameraRotation = new Vector3(20f, -45f, 0f),
                cameraZoom = 1.2f,
                outputPathMode = OutputPathMode.MirrorPrefabPath,
                iconSuffix = "_HighQuality",
                createSubfolderPerCategory = true,
                autoRefreshAssetDatabase = true,
                configureAsUISprite = true,
                maxTextureSize = 1024
            };
            
            // Generate
            IconGenerator generator = new IconGenerator(settings);
            List<GameObject> prefabs = new List<GameObject> { prefab };
            
            generator.GenerateIcons(prefabs);
            
            Debug.Log($"Generated high-quality icon for {prefab.name}");
        }
        
        /// <summary>
        /// Example: Generate icons with custom camera angles
        /// </summary>
        [MenuItem("Tools/BreezeTools/Icon Generator/Examples/Generate Multi-Angle Icons")]
        public static void GenerateMultiAngleIcons()
        {
            GameObject prefab = Selection.activeGameObject;
            
            if (prefab == null || PrefabUtility.GetPrefabAssetType(prefab) == PrefabAssetType.NotAPrefab)
            {
                EditorUtility.DisplayDialog("Error", "Please select a prefab first.", "OK");
                return;
            }
            
            // Define different camera angles
            Vector3[] angles = new Vector3[]
            {
                new Vector3(0, 0, 0),      // Front view
                new Vector3(0, 90, 0),     // Side view
                new Vector3(0, 180, 0),    // Back view
                new Vector3(90, 0, 0),     // Top view
                new Vector3(15, -30, 0)    // Isometric view
            };
            
            string[] suffixes = new string[]
            {
                "_Front",
                "_Side",
                "_Back",
                "_Top",
                "_Iso"
            };
            
            List<GameObject> prefabs = new List<GameObject> { prefab };
            
            for (int i = 0; i < angles.Length; i++)
            {
                IconGeneratorSettings settings = new IconGeneratorSettings
                {
                    resolution = IconResolution.Resolution256,
                    useTransparentBackground = true,
                    cameraRotation = angles[i],
                    cameraDistance = 5f,
                    cameraZoom = 1f,
                    outputPathMode = OutputPathMode.MirrorPrefabPath,
                    iconSuffix = suffixes[i],
                    createSubfolderPerCategory = true,
                    autoRefreshAssetDatabase = false // Only refresh at the end
                };
                
                IconGenerator generator = new IconGenerator(settings);
                generator.GenerateIcons(prefabs);
                
                Debug.Log($"Generated {suffixes[i]} view");
            }
            
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("Success", $"Generated {angles.Length} angle views for {prefab.name}!", "OK");
        }
        
        /// <summary>
        /// Example: Batch generate with custom resolution
        /// </summary>
        public static void GenerateCustomResolutionIcons(List<GameObject> prefabs, int width, int height)
        {
            IconGeneratorSettings settings = new IconGeneratorSettings
            {
                resolution = IconResolution.Custom,
                customResolution = new Vector2Int(width, height),
                useTransparentBackground = true,
                cameraDistance = 5f,
                cameraRotation = new Vector3(15f, -30f, 0f),
                outputPathMode = OutputPathMode.PrefabFolderWithSubfolder,
                autoRefreshAssetDatabase = true,
                configureAsUISprite = true
            };
            
            IconGenerator generator = new IconGenerator(settings);
            generator.GenerateIcons(prefabs);
        }
    }
}
