using Breezinstein.Tools;
using UnityEditor;
using UnityEngine;

namespace Breezinstein.Tools.Editor
{
    /// <summary>
    /// Renders a dropdown of every scene currently in Build Settings for string fields
    /// decorated with <see cref="SceneAttribute"/>. Stores the scene's file name (without
    /// extension or path) as the serialized value so it can be passed directly to
    /// <c>SceneManager.LoadScene(string)</c>.
    /// </summary>
    [CustomPropertyDrawer(typeof(SceneAttribute))]
    public class SceneAttributeDrawer : PropertyDrawer
    {
        /// <summary>
        /// Returns the file name (without extension or path) of every scene currently in
        /// <see cref="EditorBuildSettings.scenes"/>. Public so tests can validate the
        /// extraction logic without rendering an Inspector.
        /// </summary>
        public static string[] GetBuildSceneNames()
        {
            EditorBuildSettingsScene[] scenes = EditorBuildSettings.scenes;
            string[] sceneNames = new string[scenes.Length];
            for (int i = 0; i < scenes.Length; i++)
            {
                sceneNames[i] = System.IO.Path.GetFileNameWithoutExtension(scenes[i].path);
            }
            return sceneNames;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType != SerializedPropertyType.String)
            {
                EditorGUI.HelpBox(position, "[Scene] only works on string fields.", MessageType.Error);
                return;
            }

            string[] sceneNames = GetBuildSceneNames();

            if (sceneNames.Length == 0)
            {
                EditorGUI.HelpBox(position, "No scenes in Build Settings.", MessageType.Warning);
                return;
            }

            int currentIndex = System.Array.IndexOf(sceneNames, property.stringValue);
            if (currentIndex < 0) currentIndex = 0;

            EditorGUI.BeginProperty(position, label, property);
            int selectedIndex = EditorGUI.Popup(position, label.text, currentIndex, sceneNames);
            property.stringValue = sceneNames[selectedIndex];
            EditorGUI.EndProperty();
        }
    }
}
