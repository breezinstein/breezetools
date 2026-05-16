using UnityEditor;
using UnityEngine;

namespace Breezinstein.Tools.Inventory.Editor
{
    [CustomEditor(typeof(InventoryItemData))]
    [CanEditMultipleObjects]
    public class InventoryItemDataEditor : UnityEditor.Editor
    {
        private SerializedProperty idProp;
        private SerializedProperty iconProp;

        private void OnEnable()
        {
            idProp = serializedObject.FindProperty("id");
            iconProp = serializedObject.FindProperty("icon");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            if (idProp != null)
            {
                EditorGUILayout.BeginHorizontal();
                using (new EditorGUI.DisabledScope(true))
                {
                    EditorGUILayout.PropertyField(idProp);
                }
                if (GUILayout.Button("Regenerate", GUILayout.Width(90)))
                {
                    if (EditorUtility.DisplayDialog(
                            "Regenerate Item ID?",
                            "Existing save files will no longer recognize this item. Continue?",
                            "Regenerate",
                            "Cancel"))
                    {
                        foreach (var obj in targets)
                        {
                            if (obj is InventoryItemData data)
                            {
                                Undo.RecordObject(data, "Regenerate Item ID");
                                data.RegenerateId();
                                EditorUtility.SetDirty(data);
                            }
                        }
                    }
                }
                EditorGUILayout.EndHorizontal();
            }

            DrawPropertiesExcluding(serializedObject, "m_Script", "id");

            serializedObject.ApplyModifiedProperties();

            if (iconProp != null && iconProp.objectReferenceValue is Sprite sprite && sprite != null)
            {
                GUILayout.Space(8);
                EditorGUILayout.LabelField("Preview", EditorStyles.boldLabel);
                Rect rect = GUILayoutUtility.GetRect(96, 96, GUILayout.ExpandWidth(false));
                DrawSpritePreview(rect, sprite);
            }
        }

        private static void DrawSpritePreview(Rect rect, Sprite sprite)
        {
            if (sprite == null || sprite.texture == null) return;
            var tex = sprite.texture;
            var tr = sprite.textureRect;
            var uv = new Rect(
                tr.x / tex.width,
                tr.y / tex.height,
                tr.width / tex.width,
                tr.height / tex.height);
            GUI.DrawTextureWithTexCoords(rect, tex, uv, true);
        }
    }
}
