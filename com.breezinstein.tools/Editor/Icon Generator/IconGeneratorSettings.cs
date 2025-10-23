using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace BreezeTools.Editor.IconGenerator
{
    /// <summary>
    /// Settings data for the Icon Generator tool
    /// </summary>
    [Serializable]
    public class IconGeneratorSettings
    {
        [Header("Icon Resolution")]
        public IconResolution resolution = IconResolution.Resolution256;
        public Vector2Int customResolution = new Vector2Int(256, 256);
        
        [Header("Camera Settings")]
        public Color backgroundColor = new Color(0, 0, 0, 0); // Transparent by default
        public bool useTransparentBackground = true;
        public float cameraDistance = 5f;
        public Vector3 cameraRotation = new Vector3(15f, -30f, 0f);
        public float cameraZoom = 1f;
        
        [Header("Output Settings")]
        public OutputPathMode outputPathMode = OutputPathMode.MirrorPrefabPath;
        public string customOutputFolder = "Assets/GeneratedIcons";
        public string iconSuffix = "_Icon";
        public bool createSubfolderPerCategory = true;
        
        [Header("Post-Processing")]
        public bool autoRefreshAssetDatabase = true;
        public bool configureAsUISprite = true;
        public FilterMode filterMode = FilterMode.Bilinear;
        public int maxTextureSize = 512;
        public TextureImporterCompression compression = TextureImporterCompression.Compressed;
        
        /// <summary>
        /// Get the actual resolution based on settings
        /// </summary>
        public Vector2Int GetResolution()
        {
            if (resolution == IconResolution.Custom)
                return customResolution;
            
            return GetPresetResolution(resolution);
        }
        
        public static Vector2Int GetPresetResolution(IconResolution preset)
        {
            switch (preset)
            {
                case IconResolution.Resolution64:
                    return new Vector2Int(64, 64);
                case IconResolution.Resolution128:
                    return new Vector2Int(128, 128);
                case IconResolution.Resolution256:
                    return new Vector2Int(256, 256);
                case IconResolution.Resolution512:
                    return new Vector2Int(512, 512);
                default:
                    return new Vector2Int(256, 256);
            }
        }
    }
    
    [Serializable]
    public enum IconResolution
    {
        Resolution64,
        Resolution128,
        Resolution256,
        Resolution512,
        Custom
    }
    
    [Serializable]
    public enum OutputPathMode
    {
        MirrorPrefabPath,
        CustomFolder,
        PrefabFolderWithSubfolder
    }
}
