// Based on https://github.com/Kimi2016/unity-screenshot

using UnityEngine;

namespace BreezeTools
{
    public class ScreenshotEditorConfig : ScriptableObject
    {
        public static readonly int DEFAULTRESOLUTIONWIDTH = 1024;
        public static readonly int DEFAULTRESOLUTIONHEIGHT = 1024;

        [SerializeField]
        private string saveFolderPath;

        [SerializeField]
        private int resolutionWidth;

        [SerializeField]
        private int resolutionHeight;

        [SerializeField]
        private int scale;

        [SerializeField]
        private bool isTransparent;

        public string SaveFolderPath { get { return saveFolderPath; } set { saveFolderPath = value; } }
        public int ResolutionWidth { get { return resolutionWidth; } set { resolutionWidth = value; } }
        public int ResolutionHeight { get { return resolutionHeight; } set { resolutionHeight = value; } }
        public int Scale { get { return scale; } set { scale = value; } }
        public bool IsTransparent { get { return isTransparent; } set { isTransparent = value; } }
    }
}

