// Based on https://github.com/Kimi2016/unity-screenshot
using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace BreezeTools
{
    public class ScreenshotEditor : EditorWindow
    {
        private static readonly string CONFIGDIRECTORYPATH = "Assets/BreezeTools/Config";
        private static readonly string CONFIGFILEPATH = "Assets/BreezeTools/Config/ScreenshotEditorConfig.asset";
        private static ScreenshotEditor screenshotEditor;

        [SerializeField]
        private ScreenshotEditorConfig config;
        private GameObject targetCamera;

        [MenuItem("Breeze Tools/Screenshot/Editor", false, 1)]
        private static void Open()
        {
            if (screenshotEditor == null)
            {
                screenshotEditor = CreateInstance<ScreenshotEditor>();
            }

            screenshotEditor.config = GetOrCreateToolConfig<ScreenshotEditorConfig>(CONFIGDIRECTORYPATH, CONFIGFILEPATH);

            screenshotEditor.minSize = new Vector2(500, 400);
            screenshotEditor.titleContent.text = "ScreenshotEditor";
            screenshotEditor.ShowUtility();
        }

        [MenuItem("Breeze Tools/Screenshot/Quick shot", false, 1)]
        private static void QuickScreenshot()
        {
            ScreenCapture.CaptureScreenshot(GetScreenshotFileNameFromDateTime());
        }

        #region Internal

        private void OnGUI()
        {
            EditorGUI.BeginChangeCheck();

            string path = config.SaveFolderPath;
            EditorGUILayout.TextField("Save destination", path);
            if (GUILayout.Button("Select Folder"))
            {
                config.SaveFolderPath = EditorUtility.OpenFolderPanel("Please select a save destination.", "", "");
            }
            if (GUILayout.Button("Open folder"))
            {
                if (Application.platform == RuntimePlatform.WindowsEditor)
                {
                    System.Diagnostics.Process.Start(config.SaveFolderPath);
                }
                else
                {
                    EditorUtility.RevealInFinder(config.SaveFolderPath);
                }

            }

            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal(GUI.skin.box);
            {
                EditorGUILayout.LabelField("Camera");
                targetCamera = (GameObject)EditorGUILayout.ObjectField(targetCamera, typeof(GameObject), true);

                if (targetCamera != null && targetCamera.GetComponent<Camera>() == null)
                {
                    targetCamera = null;
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal(GUI.skin.box);
            {
                EditorGUILayout.LabelField("Resolution");
                config.ResolutionWidth = EditorGUILayout.IntField(config.ResolutionWidth);
                config.ResolutionHeight = EditorGUILayout.IntField(config.ResolutionHeight);
            }
            EditorGUILayout.EndHorizontal();

            config.Scale = EditorGUILayout.IntSlider("Scale", config.Scale, 1, 10);

            EditorGUILayout.Space();
            EditorGUILayout.BeginVertical(GUI.skin.box);
            {
                EditorGUILayout.LabelField("Options");
                if (GUILayout.Button("Set resolution to screen size"))
                {
                    config.ResolutionWidth = Screen.currentResolution.width;
                    config.ResolutionHeight = Screen.currentResolution.height;
                }

                if (GUILayout.Button(string.Format("Set default resolution ( {0}x{1} )", ScreenshotEditorConfig.DEFAULTRESOLUTIONWIDTH, ScreenshotEditorConfig.DEFAULTRESOLUTIONHEIGHT)))
                {
                    config.ResolutionWidth = ScreenshotEditorConfig.DEFAULTRESOLUTIONWIDTH;
                    config.ResolutionHeight = ScreenshotEditorConfig.DEFAULTRESOLUTIONHEIGHT;
                }

                if (GUILayout.Button("Set scene main camera"))
                {
                    targetCamera = Camera.main.gameObject;
                }

                config.IsTransparent = GUILayout.Toggle(config.IsTransparent, "Transparent background");
            }

            EditorGUILayout.EndVertical();

            EditorGUILayout.Space();
            EditorGUILayout.Space();

            string explain = string.Format("Screenshot taken size : {0}x{1} px", config.ResolutionWidth * config.Scale, config.ResolutionHeight * config.Scale);
            GUILayout.Label(explain);
            if (GUILayout.Button("Take Screenshot", GUILayout.Width(200), GUILayout.Height(50)))
            {
                if (PrepareScreenshot())
                {
                    TakeScreenshot();
                }
            }

            if (EditorGUI.EndChangeCheck())
            {
                //Record that there has been a change.
                EditorUtility.SetDirty(config);
            }
        }

        private bool PrepareScreenshot()
        {
            if (targetCamera == null)
            {
                EditorUtility.DisplayDialog("ScreenshotEditor", "Failed take screenshot because targetCamera is null.", "Ok");
                return false;
            }

            if (config.SaveFolderPath == null || !Directory.Exists(config.SaveFolderPath))
            {
                EditorUtility.DisplayDialog("ScreenshotEditor", "Failed take screenshot because save destination is invalid.", "Ok");
                return false;
            }

            if (config.ResolutionWidth <= 0 || config.ResolutionHeight <= 0)
            {
                config.ResolutionWidth = ScreenshotEditorConfig.DEFAULTRESOLUTIONWIDTH;
                config.ResolutionHeight = ScreenshotEditorConfig.DEFAULTRESOLUTIONHEIGHT;
            }

            return true;
        }

        private void TakeScreenshot()
        {
            Camera camera = targetCamera.GetComponent<Camera>();
            CameraClearFlags clearflag = camera.clearFlags;
            Color background = camera.backgroundColor;
            int width = config.ResolutionWidth * config.Scale;
            int height = config.ResolutionHeight * config.Scale;
            RenderTexture current = camera.targetTexture;

            Texture2D ss = new Texture2D(width, height, TextureFormat.ARGB32, false);
            RenderTexture rt = new RenderTexture(width, height, 24);
            RenderTexture.active = rt;

            if (config.IsTransparent)
            {
                camera.clearFlags = CameraClearFlags.SolidColor;
                camera.backgroundColor = new Color(0, 0, 0, 0);
            }

            camera.targetTexture = rt;
            camera.Render();

            ss.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            ss.Apply();

            byte[] bytes = ss.EncodeToPNG();
            File.WriteAllBytes(Path.Combine(config.SaveFolderPath, GetScreenshotFileNameFromDateTime()), bytes);

            DestroyImmediate(ss);

            //Reset camera settings...
            camera.targetTexture = current;
            camera.clearFlags = clearflag;
            camera.backgroundColor = background;

        }

        private static string GetScreenshotFileNameFromDateTime()
        {
            string name = string.Format("{0}_{1}.png", DateTime.Now.ToShortDateString(), DateTime.Now.ToLongTimeString());
            name = name.Replace("/", "_");
            name = name.Replace(":", "_");
            return name;
        }

        /// <summary>
        /// Get or create tool config.
        /// </summary>
        /// <typeparam name="Type">Tool config type.</typeparam>
        /// <param name="directorypath">Path to save destination folder.</param>
        /// <param name="configpath">Path to save destination file.</param>
        /// <returns>Return Type of <see cref="ScriptableObject"/></returns>
        public static Type GetOrCreateToolConfig<Type>(string directorypath, string configpath)
            where Type : ScriptableObject

        {
            Type config =
                  (Type)AssetDatabase.FindAssets("t:ScriptableObject", new string[] { directorypath })
                  .Select(id => AssetDatabase.GUIDToAssetPath(id))
                  .Select(path => AssetDatabase.LoadAssetAtPath(path, typeof(Type)))
                  .Where(c => c != null)
                  .FirstOrDefault();

            if (config == null)
            {
                config = ScriptableObject.CreateInstance<Type>();
                Save(config, directorypath, configpath);
            }

            return config;
        }

        /// <summary>
        /// Save tool config.
        /// </summary>
        /// <typeparam name="Type">Tool config type.</typeparam>
        /// <param name="config">Save config instance.</param>
        /// <param name="directorypath">Path to save destination folder.</param>
        /// <param name="configpath">Path to save destination file.</param>
        public static void Save<Type>(Type config, string directorypath, string configpath)
              where Type : ScriptableObject
        {
            if (!Directory.Exists(directorypath))
            {
                Directory.CreateDirectory(directorypath);
            }

            if (!File.Exists(configpath))
            {
                AssetDatabase.CreateAsset(config, configpath);
                AssetDatabase.Refresh();
            }
        }
        #endregion
    }

}
