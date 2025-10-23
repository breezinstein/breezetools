using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace BreezeTools.Editor.IconGenerator
{
    /// <summary>
    /// Editor window for generating item icon sprites from prefabs
    /// </summary>
    public class IconGeneratorWindow : EditorWindow
    {
        private List<GameObject> prefabs = new List<GameObject>();
        private IconGeneratorSettings settings = new IconGeneratorSettings();
        
        // UI Elements
        private ScrollView prefabListScrollView;
        private VisualElement prefabListContainer;
        private Button generateButton;
        private ProgressBar progressBar;
        
        // Accordion state
        private bool prefabSectionExpanded = true;
        private bool settingsSectionExpanded = true;
        
        [MenuItem("Tools/BreezeTools/Icon Generator")]
        public static void ShowWindow()
        {
            IconGeneratorWindow window = GetWindow<IconGeneratorWindow>();
            window.titleContent = new GUIContent("Icon Generator");
            window.minSize = new Vector2(400, 600);
        }
        
        public void CreateGUI()
        {
            // Root container
            var root = rootVisualElement;
            root.style.paddingTop = 0;
            root.style.paddingBottom = 0;
            root.style.paddingLeft = 0;
            root.style.paddingRight = 0;
            
            // Header section with gradient-like effect
            var header = new VisualElement
            {
                style =
                {
                    backgroundColor = new StyleColor(new Color(0.15f, 0.15f, 0.15f, 1f)),
                    paddingTop = 15,
                    paddingBottom = 15,
                    paddingLeft = 20,
                    paddingRight = 20,
                    marginBottom = 0,
                    borderBottomWidth = 2,
                    borderBottomColor = new StyleColor(new Color(0.3f, 0.5f, 0.8f, 0.5f))
                }
            };

            // Title with icon
            var titleContainer = new VisualElement
            {
                style = { flexDirection = FlexDirection.Row, alignItems = Align.Center }
            };
            
            var iconLabel = new Label("🎨")
            {
                style =
                {
                    fontSize = 20,
                    marginRight = 10,
                    unityTextAlign = TextAnchor.MiddleCenter
                }
            };
            titleContainer.Add(iconLabel);

            var titleLabel = new Label("Icon Generator")
            {
                style =
                {
                    fontSize = 20,
                    unityFontStyleAndWeight = FontStyle.Bold,
                    color = new StyleColor(new Color(0.9f, 0.9f, 0.9f, 1f))
                }
            };
            titleContainer.Add(titleLabel);
            
            header.Add(titleContainer);

            // Description
            var description = new Label("Generate icon sprites from prefabs with customizable settings")
            {
                style =
                {
                    marginTop = 8,
                    fontSize = 11,
                    color = new StyleColor(new Color(0.7f, 0.7f, 0.7f, 1f)),
                    unityFontStyleAndWeight = FontStyle.Italic
                }
            };
            header.Add(description);
            
            root.Add(header);
            
            // Main content area with scroll view
            var mainScrollView = new ScrollView(ScrollViewMode.Vertical)
            {
                style =
                {
                    flexGrow = 1
                }
            };
            
            var contentArea = new VisualElement
            {
                style =
                {
                    paddingTop = 10,
                    paddingBottom = 10,
                    paddingLeft = 10,
                    paddingRight = 10
                }
            };
            
            // Create sections
            CreatePrefabListSection(contentArea);
            CreateSettingsSection(contentArea);
            CreateActionSection(contentArea);
            
            mainScrollView.Add(contentArea);
            root.Add(mainScrollView);
            
            // Progress bar (initially hidden)
            progressBar = new ProgressBar();
            progressBar.title = "Generating Icons...";
            progressBar.style.marginTop = 10;
            progressBar.style.marginLeft = 10;
            progressBar.style.marginRight = 10;
            progressBar.style.display = DisplayStyle.None;
            root.Add(progressBar);
        }
        
        private void CreatePrefabListSection(VisualElement root)
        {
            var section = CreateCollapsibleSection("📦 Prefabs", root, ref prefabSectionExpanded, (content) =>
            {
                // Add prefab button with better styling
                var addButton = new Button(() => ShowAddPrefabMenu())
                {
                    text = "➕ Add Prefab",
                    style =
                    {
                        marginBottom = 8,
                        backgroundColor = new StyleColor(new Color(0.2f, 0.4f, 0.7f, 1f)),
                        borderTopLeftRadius = 4,
                        borderTopRightRadius = 4,
                        borderBottomLeftRadius = 4,
                        borderBottomRightRadius = 4,
                        paddingTop = 6,
                        paddingBottom = 6,
                        unityFontStyleAndWeight = FontStyle.Bold,
                        color = new StyleColor(Color.white)
                    }
                };
                content.Add(addButton);
                
                // Drag and drop area
                var dropArea = new VisualElement();
                dropArea.style.backgroundColor = new Color(0.18f, 0.18f, 0.18f, 1f);
                dropArea.style.borderBottomWidth = 2;
                dropArea.style.borderTopWidth = 2;
                dropArea.style.borderLeftWidth = 2;
                dropArea.style.borderRightWidth = 2;
                dropArea.style.borderBottomColor = new StyleColor(new Color(0.3f, 0.5f, 0.8f, 0.5f));
                dropArea.style.borderTopColor = new StyleColor(new Color(0.3f, 0.5f, 0.8f, 0.5f));
                dropArea.style.borderLeftColor = new StyleColor(new Color(0.3f, 0.5f, 0.8f, 0.5f));
                dropArea.style.borderRightColor = new StyleColor(new Color(0.3f, 0.5f, 0.8f, 0.5f));
                dropArea.style.borderBottomLeftRadius = 6;
                dropArea.style.borderBottomRightRadius = 6;
                dropArea.style.borderTopLeftRadius = 6;
                dropArea.style.borderTopRightRadius = 6;
                dropArea.style.paddingTop = 25;
                dropArea.style.paddingBottom = 25;
                dropArea.style.marginBottom = 10;
                dropArea.style.alignItems = Align.Center;
                dropArea.style.justifyContent = Justify.Center;
                
                var dropLabel = new Label("🎯 Drag and drop prefabs here");
                dropLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
                dropLabel.style.fontSize = 12;
                dropLabel.style.color = new StyleColor(new Color(0.7f, 0.7f, 0.7f, 1f));
                dropArea.Add(dropLabel);
                
                dropArea.RegisterCallback<DragUpdatedEvent>(OnDragUpdate);
                dropArea.RegisterCallback<DragPerformEvent>(OnDragPerform);
                
                content.Add(dropArea);
                
                // Prefab list
                prefabListScrollView = new ScrollView(ScrollViewMode.Vertical);
                prefabListScrollView.style.maxHeight = 200;
                prefabListScrollView.style.marginBottom = 10;
                prefabListScrollView.style.backgroundColor = new StyleColor(new Color(0.16f, 0.16f, 0.16f, 1f));
                prefabListScrollView.style.borderTopLeftRadius = 4;
                prefabListScrollView.style.borderTopRightRadius = 4;
                prefabListScrollView.style.borderBottomLeftRadius = 4;
                prefabListScrollView.style.borderBottomRightRadius = 4;
                prefabListScrollView.style.borderBottomWidth = 1;
                prefabListScrollView.style.borderTopWidth = 1;
                prefabListScrollView.style.borderLeftWidth = 1;
                prefabListScrollView.style.borderRightWidth = 1;
                prefabListScrollView.style.borderBottomColor = new StyleColor(new Color(0.3f, 0.3f, 0.3f, 0.5f));
                prefabListScrollView.style.borderTopColor = new StyleColor(new Color(0.3f, 0.3f, 0.3f, 0.5f));
                prefabListScrollView.style.borderLeftColor = new StyleColor(new Color(0.3f, 0.3f, 0.3f, 0.5f));
                prefabListScrollView.style.borderRightColor = new StyleColor(new Color(0.3f, 0.3f, 0.3f, 0.5f));
                prefabListScrollView.style.paddingTop = 5;
                prefabListScrollView.style.paddingBottom = 5;
                
                prefabListContainer = new VisualElement();
                prefabListScrollView.Add(prefabListContainer);
                content.Add(prefabListScrollView);
                
                // Clear all button
                var clearButton = new Button(() => ClearAllPrefabs())
                {
                    text = "🗑️ Clear All",
                    style =
                    {
                        backgroundColor = new StyleColor(new Color(0.6f, 0.2f, 0.2f, 1f)),
                        borderTopLeftRadius = 4,
                        borderTopRightRadius = 4,
                        borderBottomLeftRadius = 4,
                        borderBottomRightRadius = 4,
                        paddingTop = 4,
                        paddingBottom = 4,
                        color = new StyleColor(Color.white)
                    }
                };
                content.Add(clearButton);
                
                RefreshPrefabList();
            });
        }
        
        private void CreateSettingsSection(VisualElement root)
        {
            CreateCollapsibleSection("⚙️ Settings", root, ref settingsSectionExpanded, (content) =>
            {
                // Resolution settings
                var resolutionLabel = new Label("Icon Resolution");
                resolutionLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
                resolutionLabel.style.marginTop = 5;
                resolutionLabel.style.fontSize = 12;
                resolutionLabel.style.color = new StyleColor(new Color(0.8f, 0.8f, 0.8f, 1f));
                content.Add(resolutionLabel);
                
                var resolutionField = new EnumField("Preset", settings.resolution);
                resolutionField.RegisterValueChangedCallback(evt =>
                {
                    settings.resolution = (IconResolution)evt.newValue;
                    RefreshCustomResolutionVisibility();
                });
                StyleField(resolutionField);
                content.Add(resolutionField);
                
                var customResField = new Vector2IntField("Custom Size");
                customResField.value = settings.customResolution;
                customResField.RegisterValueChangedCallback(evt => settings.customResolution = evt.newValue);
                customResField.style.display = settings.resolution == IconResolution.Custom ? DisplayStyle.Flex : DisplayStyle.None;
                customResField.name = "customResolutionField";
                StyleField(customResField);
                content.Add(customResField);
                
                // Camera settings
                var cameraLabel = new Label("Camera Settings");
                cameraLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
                cameraLabel.style.marginTop = 10;
                cameraLabel.style.fontSize = 12;
                cameraLabel.style.color = new StyleColor(new Color(0.8f, 0.8f, 0.8f, 1f));
                content.Add(cameraLabel);
                
                var transparentBgToggle = new Toggle("Transparent Background");
                transparentBgToggle.value = settings.useTransparentBackground;
                transparentBgToggle.RegisterValueChangedCallback(evt =>
                {
                    settings.useTransparentBackground = evt.newValue;
                    RefreshBackgroundColorVisibility();
                });
                StyleField(transparentBgToggle);
                content.Add(transparentBgToggle);
                
                var bgColorField = new ColorField("Background Color");
                bgColorField.value = settings.backgroundColor;
                bgColorField.RegisterValueChangedCallback(evt => settings.backgroundColor = evt.newValue);
                bgColorField.style.display = settings.useTransparentBackground ? DisplayStyle.None : DisplayStyle.Flex;
                bgColorField.name = "backgroundColorField";
                StyleField(bgColorField);
                content.Add(bgColorField);
                
                var distanceSlider = new Slider("Camera Distance", 1f, 20f);
                distanceSlider.value = settings.cameraDistance;
                distanceSlider.RegisterValueChangedCallback(evt => settings.cameraDistance = evt.newValue);
                distanceSlider.showInputField = true;
                StyleField(distanceSlider);
                content.Add(distanceSlider);
                
                var rotationField = new Vector3Field("Camera Rotation");
                rotationField.value = settings.cameraRotation;
                rotationField.RegisterValueChangedCallback(evt => settings.cameraRotation = evt.newValue);
                StyleField(rotationField);
                content.Add(rotationField);
                
                var zoomSlider = new Slider("Camera Zoom", 0.1f, 3f);
                zoomSlider.value = settings.cameraZoom;
                zoomSlider.RegisterValueChangedCallback(evt => settings.cameraZoom = evt.newValue);
                zoomSlider.showInputField = true;
                StyleField(zoomSlider);
                content.Add(zoomSlider);
                
                // Output settings
                var outputLabel = new Label("Output Settings");
                outputLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
                outputLabel.style.marginTop = 10;
                outputLabel.style.fontSize = 12;
                outputLabel.style.color = new StyleColor(new Color(0.8f, 0.8f, 0.8f, 1f));
                content.Add(outputLabel);
                
                var outputModeField = new EnumField("Output Path Mode", settings.outputPathMode);
                outputModeField.RegisterValueChangedCallback(evt =>
                {
                    settings.outputPathMode = (OutputPathMode)evt.newValue;
                    RefreshOutputPathVisibility();
                });
                StyleField(outputModeField);
                content.Add(outputModeField);
                
                var customPathField = new TextField("Custom Output Folder");
                customPathField.value = settings.customOutputFolder;
                customPathField.RegisterValueChangedCallback(evt => settings.customOutputFolder = evt.newValue);
                customPathField.style.display = settings.outputPathMode == OutputPathMode.CustomFolder ? DisplayStyle.Flex : DisplayStyle.None;
                customPathField.name = "customOutputFolderField";
                StyleField(customPathField);
                content.Add(customPathField);
                
                var suffixField = new TextField("Icon Suffix");
                suffixField.value = settings.iconSuffix;
                suffixField.RegisterValueChangedCallback(evt => settings.iconSuffix = evt.newValue);
                StyleField(suffixField);
                content.Add(suffixField);
                
                var subfolderToggle = new Toggle("Create Icons Subfolder");
                subfolderToggle.value = settings.createSubfolderPerCategory;
                subfolderToggle.RegisterValueChangedCallback(evt => settings.createSubfolderPerCategory = evt.newValue);
                StyleField(subfolderToggle);
                content.Add(subfolderToggle);
                
                // Post-processing settings
                var postLabel = new Label("Post-Processing");
                postLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
                postLabel.style.marginTop = 10;
                postLabel.style.fontSize = 12;
                postLabel.style.color = new StyleColor(new Color(0.8f, 0.8f, 0.8f, 1f));
                content.Add(postLabel);
                
                var refreshToggle = new Toggle("Auto Refresh AssetDatabase");
                refreshToggle.value = settings.autoRefreshAssetDatabase;
                refreshToggle.RegisterValueChangedCallback(evt => settings.autoRefreshAssetDatabase = evt.newValue);
                StyleField(refreshToggle);
                content.Add(refreshToggle);
                
                var uiSpriteToggle = new Toggle("Configure as UI Sprite");
                uiSpriteToggle.value = settings.configureAsUISprite;
                uiSpriteToggle.RegisterValueChangedCallback(evt => settings.configureAsUISprite = evt.newValue);
                StyleField(uiSpriteToggle);
                content.Add(uiSpriteToggle);
            });
        }
        
        private void StyleField(VisualElement field)
        {
            field.style.marginBottom = 3;
            field.style.marginTop = 2;
        }
        
        private void CreateActionSection(VisualElement root)
        {
            var section = new VisualElement();
            section.style.marginTop = 20;
            section.style.marginBottom = 10;
            root.Add(section);
            
            generateButton = new Button(() => GenerateIcons())
            {
                text = "✨ Generate Icons",
                style =
                {
                    height = 35,
                    fontSize = 14,
                    unityFontStyleAndWeight = FontStyle.Bold,
                    backgroundColor = new StyleColor(new Color(0.2f, 0.6f, 0.3f, 1f)),
                    borderTopLeftRadius = 6,
                    borderTopRightRadius = 6,
                    borderBottomLeftRadius = 6,
                    borderBottomRightRadius = 6,
                    color = new StyleColor(Color.white)
                }
            };
            section.Add(generateButton);
            
            // Instructions with icon
            var instructionsLabel = new Label("💡 Add prefabs above and click Generate to create icons")
            {
                style =
                {
                    marginTop = 8,
                    fontSize = 10,
                    unityTextAlign = TextAnchor.MiddleCenter,
                    color = new StyleColor(new Color(0.6f, 0.6f, 0.6f, 1f)),
                    unityFontStyleAndWeight = FontStyle.Italic
                }
            };
            section.Add(instructionsLabel);
        }
        
        private VisualElement CreateCollapsibleSection(string title, VisualElement parent, ref bool isExpanded, Action<VisualElement> buildContent)
        {
            // Capture the expanded state as a local variable to avoid ref issues in callbacks
            bool expanded = isExpanded;
            
            var container = new VisualElement();
            container.style.marginBottom = 15;
            container.style.backgroundColor = new StyleColor(new Color(0.18f, 0.18f, 0.18f, 0.6f));
            container.style.borderTopLeftRadius = 6;
            container.style.borderTopRightRadius = 6;
            container.style.borderBottomLeftRadius = 6;
            container.style.borderBottomRightRadius = 6;
            container.style.paddingTop = 10;
            container.style.paddingBottom = 10;
            container.style.paddingLeft = 12;
            container.style.paddingRight = 12;
            container.style.borderLeftWidth = 3;
            container.style.borderLeftColor = new StyleColor(new Color(0.3f, 0.6f, 1f, 1f));
            
            // Header - clickable to toggle
            var header = new VisualElement();
            header.style.flexDirection = FlexDirection.Row;
            header.style.justifyContent = Justify.SpaceBetween;
            header.style.alignItems = Align.Center;
            header.style.marginBottom = 8;
            
            // Add hover effect to header
            header.RegisterCallback<MouseEnterEvent>(evt =>
            {
                var elem = evt.currentTarget as VisualElement;
                if (elem != null)
                {
                    container.style.backgroundColor = new StyleColor(new Color(0.20f, 0.20f, 0.20f, 0.6f));
                }
            });

            header.RegisterCallback<MouseLeaveEvent>(evt =>
            {
                var elem = evt.currentTarget as VisualElement;
                if (elem != null)
                {
                    container.style.backgroundColor = new StyleColor(new Color(0.18f, 0.18f, 0.18f, 0.6f));
                }
            });
            
            var titleLabel = new Label(title);
            titleLabel.style.fontSize = 13;
            titleLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            titleLabel.style.color = new StyleColor(new Color(0.9f, 0.9f, 0.9f, 1f));
            header.Add(titleLabel);
            
            var toggleIcon = new Label(expanded ? "▼" : "▶");
            toggleIcon.style.fontSize = 10;
            toggleIcon.style.color = new StyleColor(new Color(0.6f, 0.6f, 0.6f, 1f));
            toggleIcon.name = "toggleIcon";
            header.Add(toggleIcon);
            
            container.Add(header);
            
            // Content
            var content = new VisualElement();
            content.style.paddingLeft = 5;
            content.name = "content";
            content.style.display = expanded ? DisplayStyle.Flex : DisplayStyle.None;
            
            buildContent(content);
            container.Add(content);
            
            // Click handler for toggle
            header.RegisterCallback<MouseDownEvent>(evt =>
            {
                expanded = !expanded;
                content.style.display = expanded ? DisplayStyle.Flex : DisplayStyle.None;
                toggleIcon.text = expanded ? "▼" : "▶";
                
                // Update the ref value based on which section this is
                if (title.Contains("Prefabs"))
                    prefabSectionExpanded = expanded;
                else if (title.Contains("Settings"))
                    settingsSectionExpanded = expanded;
            });
            
            parent.Add(container);
            return content;
        }
        
        private VisualElement CreateSection(string title, VisualElement parent)
        {
            var container = new VisualElement();
            container.style.marginBottom = 15;
            container.style.backgroundColor = new StyleColor(new Color(0.18f, 0.18f, 0.18f, 0.6f));
            container.style.borderTopLeftRadius = 6;
            container.style.borderTopRightRadius = 6;
            container.style.borderBottomLeftRadius = 6;
            container.style.borderBottomRightRadius = 6;
            container.style.paddingTop = 10;
            container.style.paddingBottom = 10;
            container.style.paddingLeft = 12;
            container.style.paddingRight = 12;
            container.style.borderLeftWidth = 3;
            container.style.borderLeftColor = new StyleColor(new Color(0.3f, 0.6f, 1f, 1f));
            
            var titleLabel = new Label(title);
            titleLabel.style.fontSize = 13;
            titleLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            titleLabel.style.marginBottom = 8;
            titleLabel.style.color = new StyleColor(new Color(0.9f, 0.9f, 0.9f, 1f));
            container.Add(titleLabel);
            
            var content = new VisualElement();
            content.style.paddingLeft = 5;
            container.Add(content);
            
            parent.Add(container);
            return content;
        }
        
        private void ShowAddPrefabMenu()
        {
            // Open object picker for GameObjects
            EditorGUIUtility.ShowObjectPicker<GameObject>(null, false, "", 0);
            EditorApplication.update += CheckObjectPicker;
        }
        
        private void CheckObjectPicker()
        {
            if (Event.current == null)
                return;
                
            if (Event.current.commandName == "ObjectSelectorClosed")
            {
                EditorApplication.update -= CheckObjectPicker;
                var selectedObject = EditorGUIUtility.GetObjectPickerObject() as GameObject;
                
                if (selectedObject != null)
                {
                    AddPrefab(selectedObject);
                }
            }
        }
        
        private void OnDragUpdate(DragUpdatedEvent evt)
        {
            if (DragAndDrop.objectReferences.Length > 0)
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
            }
        }
        
        private void OnDragPerform(DragPerformEvent evt)
        {
            DragAndDrop.AcceptDrag();
            
            foreach (var obj in DragAndDrop.objectReferences)
            {
                if (obj is GameObject go)
                {
                    AddPrefab(go);
                }
            }
        }
        
        private void AddPrefab(GameObject prefab)
        {
            if (prefab == null)
                return;
                
            // Check if it's actually a prefab
            if (PrefabUtility.GetPrefabAssetType(prefab) == PrefabAssetType.NotAPrefab)
            {
                Debug.LogWarning($"{prefab.name} is not a prefab!");
                return;
            }
            
            if (!prefabs.Contains(prefab))
            {
                prefabs.Add(prefab);
                RefreshPrefabList();
            }
        }
        
        private void RemovePrefab(GameObject prefab)
        {
            prefabs.Remove(prefab);
            RefreshPrefabList();
        }
        
        private void ClearAllPrefabs()
        {
            prefabs.Clear();
            RefreshPrefabList();
        }
        
        private void RefreshPrefabList()
        {
            prefabListContainer.Clear();
            
            if (prefabs.Count == 0)
            {
                var emptyLabel = new Label("📭 No prefabs added");
                emptyLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
                emptyLabel.style.paddingTop = 20;
                emptyLabel.style.paddingBottom = 20;
                emptyLabel.style.color = new StyleColor(new Color(0.5f, 0.5f, 0.5f, 1f));
                emptyLabel.style.fontSize = 11;
                prefabListContainer.Add(emptyLabel);
                return;
            }
            
            foreach (var prefab in prefabs)
            {
                if (prefab == null)
                    continue;
                    
                var itemContainer = new VisualElement();
                itemContainer.style.flexDirection = FlexDirection.Row;
                itemContainer.style.justifyContent = Justify.SpaceBetween;
                itemContainer.style.alignItems = Align.Center;
                itemContainer.style.marginBottom = 4;
                itemContainer.style.marginLeft = 4;
                itemContainer.style.marginRight = 4;
                itemContainer.style.marginTop = 2;
                itemContainer.style.paddingLeft = 10;
                itemContainer.style.paddingRight = 8;
                itemContainer.style.paddingTop = 6;
                itemContainer.style.paddingBottom = 6;
                itemContainer.style.backgroundColor = new Color(0.22f, 0.22f, 0.22f, 1f);
                itemContainer.style.borderBottomLeftRadius = 4;
                itemContainer.style.borderBottomRightRadius = 4;
                itemContainer.style.borderTopLeftRadius = 4;
                itemContainer.style.borderTopRightRadius = 4;
                itemContainer.style.borderLeftWidth = 3;
                itemContainer.style.borderLeftColor = new StyleColor(new Color(0.3f, 0.6f, 1f, 1f));
                
                // Add hover effect
                itemContainer.RegisterCallback<MouseEnterEvent>(evt =>
                {
                    var elem = evt.currentTarget as VisualElement;
                    if (elem != null)
                    {
                        elem.style.backgroundColor = new StyleColor(new Color(0.26f, 0.26f, 0.26f, 1f));
                    }
                });

                itemContainer.RegisterCallback<MouseLeaveEvent>(evt =>
                {
                    var elem = evt.currentTarget as VisualElement;
                    if (elem != null)
                    {
                        elem.style.backgroundColor = new StyleColor(new Color(0.22f, 0.22f, 0.22f, 1f));
                    }
                });
                
                var nameLabel = new Label("📦 " + prefab.name);
                nameLabel.style.fontSize = 11;
                nameLabel.style.color = new StyleColor(new Color(0.85f, 0.85f, 0.85f, 1f));
                itemContainer.Add(nameLabel);
                
                var removeButton = new Button(() => RemovePrefab(prefab))
                {
                    text = "✖",
                    style =
                    {
                        width = 22,
                        height = 22,
                        fontSize = 10,
                        backgroundColor = new StyleColor(new Color(0.6f, 0.2f, 0.2f, 0.8f)),
                        borderTopLeftRadius = 3,
                        borderTopRightRadius = 3,
                        borderBottomLeftRadius = 3,
                        borderBottomRightRadius = 3,
                        color = new StyleColor(Color.white),
                        unityFontStyleAndWeight = FontStyle.Bold
                    }
                };
                itemContainer.Add(removeButton);
                
                prefabListContainer.Add(itemContainer);
            }
        }
        
        private void RefreshCustomResolutionVisibility()
        {
            var field = rootVisualElement.Q<Vector2IntField>("customResolutionField");
            if (field != null)
            {
                field.style.display = settings.resolution == IconResolution.Custom ? DisplayStyle.Flex : DisplayStyle.None;
            }
        }
        
        private void RefreshBackgroundColorVisibility()
        {
            var field = rootVisualElement.Q<ColorField>("backgroundColorField");
            if (field != null)
            {
                field.style.display = settings.useTransparentBackground ? DisplayStyle.None : DisplayStyle.Flex;
            }
        }
        
        private void RefreshOutputPathVisibility()
        {
            var field = rootVisualElement.Q<TextField>("customOutputFolderField");
            if (field != null)
            {
                field.style.display = settings.outputPathMode == OutputPathMode.CustomFolder ? DisplayStyle.Flex : DisplayStyle.None;
            }
        }
        
        private void GenerateIcons()
        {
            if (prefabs.Count == 0)
            {
                EditorUtility.DisplayDialog("No Prefabs", "Please add at least one prefab to generate icons.", "OK");
                return;
            }
            
            generateButton.SetEnabled(false);
            progressBar.style.display = DisplayStyle.Flex;
            
            try
            {
                IconGenerator generator = new IconGenerator(settings);
                generator.GenerateIcons(prefabs, (current, total) =>
                {
                    progressBar.value = (float)current / total * 100f;
                    progressBar.title = $"Generating Icons... {current}/{total}";
                });
                
                EditorUtility.DisplayDialog("Success", $"Successfully generated {prefabs.Count} icon(s)!", "OK");
            }
            catch (Exception e)
            {
                EditorUtility.DisplayDialog("Error", $"Error generating icons: {e.Message}", "OK");
                Debug.LogError($"Icon generation error: {e}");
            }
            finally
            {
                generateButton.SetEnabled(true);
                progressBar.style.display = DisplayStyle.None;
                progressBar.value = 0;
            }
        }
    }
}
