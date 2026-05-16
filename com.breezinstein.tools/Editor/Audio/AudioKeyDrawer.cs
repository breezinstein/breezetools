using System.Collections.Generic;
using Breezinstein.Tools.Audio;
using UnityEditor;
using UnityEngine;

namespace Breezinstein.Tools.Audio.Editor
{
    /// <summary>
    /// Renders a dropdown of every clip key present in any <see cref="AudioLibrary"/> in the
    /// project for string fields decorated with <see cref="AudioKeyAttribute"/>. Shows a text
    /// field with a "Key not found" warning when the stored value doesn't exist in any
    /// library, and falls back to a normal text field when no libraries are found.
    /// </summary>
    [CustomPropertyDrawer(typeof(AudioKeyAttribute))]
    public class AudioKeyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType != SerializedPropertyType.String)
            {
                EditorGUI.HelpBox(position, "[AudioKey] only works on string fields.", MessageType.Error);
                return;
            }

            var keys = GetAllKeys();

            if (keys.Count == 0)
            {
                EditorGUI.PropertyField(position, property, label);
                return;
            }

            var options = new List<string> { "(none)" };
            options.AddRange(keys);

            int currentIndex = string.IsNullOrEmpty(property.stringValue)
                ? 0
                : options.IndexOf(property.stringValue);

            if (currentIndex < 0)
            {
                var splitRect = new Rect(position.x, position.y,
                    position.width * 0.75f, position.height);
                var warnRect = new Rect(position.x + position.width * 0.75f + 2f, position.y,
                    position.width * 0.25f - 2f, position.height);

                EditorGUI.BeginProperty(splitRect, label, property);
                property.stringValue = EditorGUI.TextField(splitRect, label, property.stringValue);
                EditorGUI.EndProperty();

                EditorGUI.HelpBox(warnRect, "Key not found", MessageType.Warning);
                return;
            }

            EditorGUI.BeginProperty(position, label, property);
            int selected = EditorGUI.Popup(position, label.text, currentIndex, options.ToArray());
            property.stringValue = selected == 0 ? string.Empty : options[selected];
            EditorGUI.EndProperty();
        }

        private static List<string> GetAllKeys()
        {
            var keys = new List<string>();
            string[] guids = AssetDatabase.FindAssets("t:AudioLibrary");
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var lib = AssetDatabase.LoadAssetAtPath<AudioLibrary>(path);
                if (lib?.clips == null) continue;
                foreach (var kvp in lib.clips)
                {
                    if (!keys.Contains(kvp.Key)) keys.Add(kvp.Key);
                }
            }
            keys.Sort();
            return keys;
        }
    }
}
