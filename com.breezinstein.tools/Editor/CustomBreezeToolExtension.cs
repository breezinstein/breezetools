using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace Breezinstein.Tools.Extensions
{
    /// <summary>
    /// Example of how to create a custom tool that integrates with BreezeToolBox
    /// This demonstrates extending the toolbox with custom functionality
    /// </summary>
    public class CustomBreezeToolExtension : EditorWindow
    {
        [MenuItem("Breeze Tools/Extensions/Custom Tool Example")]
        static void Init()
        {
            CustomBreezeToolExtension window = GetWindow<CustomBreezeToolExtension>();
            window.titleContent = new GUIContent("Custom Breeze Tool");
            window.minSize = new Vector2(300, 200);
            window.Show();
        }

        public void CreateGUI()
        {
            // Create UI programmatically (alternative to UXML)
            var root = rootVisualElement;
            
            // Title
            var title = new Label("Custom Breeze Tool Extension");
            title.style.fontSize = 16;
            title.style.unityFontStyleAndWeight = FontStyle.Bold;
            title.style.unityTextAlign = TextAnchor.UpperCenter;
            title.style.marginBottom = 20;
            root.Add(title);

            // Description
            var description = new Label("This demonstrates how to create custom tools that integrate with the BreezeToolBox ecosystem.");
            description.style.whiteSpace = WhiteSpace.Normal;
            description.style.marginBottom = 15;
            root.Add(description);

            // Custom functionality section
            var functionGroup = new VisualElement();
            functionGroup.style.backgroundColor = new Color(0.3f, 0.3f, 0.3f, 0.5f);
            functionGroup.style.paddingTop = 10;
            functionGroup.style.paddingBottom = 10;
            functionGroup.style.paddingLeft = 10;
            functionGroup.style.paddingRight = 10;
            functionGroup.style.borderTopLeftRadius = 5;
            functionGroup.style.borderTopRightRadius = 5;
            functionGroup.style.borderBottomLeftRadius = 5;
            functionGroup.style.borderBottomRightRadius = 5;
            functionGroup.style.marginBottom = 10;

            var groupTitle = new Label("Custom Functions");
            groupTitle.style.unityFontStyleAndWeight = FontStyle.Bold;
            groupTitle.style.marginBottom = 5;
            functionGroup.Add(groupTitle);

            // Align objects button
            var alignButton = new Button(() => AlignSelectedObjects())
            {
                text = "Align Selected to Grid"
            };
            alignButton.style.height = 30;
            alignButton.style.marginBottom = 5;
            functionGroup.Add(alignButton);

            // Rename sequence button
            var renameButton = new Button(() => RenameSequentially())
            {
                text = "Rename Selected Sequentially"
            };
            renameButton.style.height = 30;
            renameButton.style.marginBottom = 5;
            functionGroup.Add(renameButton);

            // Settings
            var gridSizeField = new FloatField("Grid Size");
            gridSizeField.value = 1.0f;
            gridSizeField.style.marginBottom = 5;
            functionGroup.Add(gridSizeField);

            var baseName = new TextField("Base Name");
            baseName.value = "Object";
            baseName.style.marginBottom = 5;
            functionGroup.Add(baseName);

            root.Add(functionGroup);

            // Integration note
            var integrationNote = new Label("💡 This tool can be combined with BreezeToolBox features for powerful workflows!");
            integrationNote.style.fontSize = 11;
            integrationNote.style.color = new Color(0.8f, 0.9f, 1f);
            integrationNote.style.whiteSpace = WhiteSpace.Normal;
            integrationNote.style.marginTop = 10;
            root.Add(integrationNote);

            // Open main toolbox button
            var openMainButton = new Button(() => {
                EditorWindow.GetWindow<BreezeToolBox>().Show();
            })
            {
                text = "Open Main BreezeToolBox"
            };
            openMainButton.style.height = 25;
            openMainButton.style.marginTop = 10;
            openMainButton.style.backgroundColor = new Color(0.2f, 0.4f, 0.8f);
            root.Add(openMainButton);

            // Store references for button callbacks
            alignButton.userData = gridSizeField;
            renameButton.userData = baseName;
        }

        private void AlignSelectedObjects()
        {
            var selected = Selection.transforms;
            if (selected.Length == 0)
            {
                EditorUtility.DisplayDialog("No Selection", "Please select objects to align.", "OK");
                return;
            }

            // Get grid size from UI (simplified for demo)
            float gridSize = 1.0f; // Would get from UI field in full implementation

            Undo.RecordObjects(selected, "Align to Grid");

            foreach (var transform in selected)
            {
                var pos = transform.position;
                pos.x = Mathf.Round(pos.x / gridSize) * gridSize;
                pos.z = Mathf.Round(pos.z / gridSize) * gridSize;
                transform.position = pos;
            }

            Debug.Log($"Aligned {selected.Length} objects to grid (size: {gridSize})");
        }

        private void RenameSequentially()
        {
            var selected = Selection.gameObjects;
            if (selected.Length == 0)
            {
                EditorUtility.DisplayDialog("No Selection", "Please select objects to rename.", "OK");
                return;
            }

            // Get base name from UI (simplified for demo)
            string baseName = "Object"; // Would get from UI field in full implementation

            Undo.RecordObjects(selected, "Rename Sequentially");

            for (int i = 0; i < selected.Length; i++)
            {
                selected[i].name = $"{baseName}_{i:000}";
            }

            Debug.Log($"Renamed {selected.Length} objects with base name '{baseName}'");
        }
    }

    /// <summary>
    /// Utility class showing how to create reusable functions for custom tools
    /// </summary>
    public static class BreezeToolUtilities
    {
        /// <summary>
        /// Snaps transform positions to a grid
        /// </summary>
        public static void SnapToGrid(Transform[] transforms, float gridSize)
        {
            foreach (var transform in transforms)
            {
                var pos = transform.position;
                pos.x = Mathf.Round(pos.x / gridSize) * gridSize;
                pos.y = Mathf.Round(pos.y / gridSize) * gridSize;
                pos.z = Mathf.Round(pos.z / gridSize) * gridSize;
                transform.position = pos;
            }
        }

        /// <summary>
        /// Creates a radial pattern with custom parameters
        /// </summary>
        public static void CreateRadialPattern(Transform[] transforms, Vector3 center, float radius, float startAngle = 0f, float endAngle = 360f)
        {
            if (transforms.Length == 0) return;

            float angleStep = (endAngle - startAngle) / Mathf.Max(1, transforms.Length - 1);

            for (int i = 0; i < transforms.Length; i++)
            {
                float angle = startAngle + (angleStep * i);
                float radian = angle * Mathf.Deg2Rad;
                
                Vector3 position = center + new Vector3(
                    Mathf.Cos(radian) * radius,
                    0,
                    Mathf.Sin(radian) * radius
                );

                transforms[i].position = position;
            }
        }

        /// <summary>
        /// Applies consistent naming with numbering
        /// </summary>
        public static void ApplySequentialNaming(GameObject[] objects, string baseName, int startNumber = 0, int digits = 3)
        {
            for (int i = 0; i < objects.Length; i++)
            {
                int number = startNumber + i;
                string numberString = number.ToString($"D{digits}");
                objects[i].name = $"{baseName}_{numberString}";
            }
        }
    }
}
