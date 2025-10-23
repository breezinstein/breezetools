using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace BreezeTools.Editor
{
    public class TODOFinderWindow : EditorWindow
    {
        private ListView _listView;
        private Label _statusLabel;
        private TextField _searchField;
        private Button _refreshButton;
        private DropdownField _filterDropdown;
        
        private List<TodoItem> _allTodoItems = new List<TodoItem>();
        private List<TodoItem> _filteredTodoItems = new List<TodoItem>();
        
        [MenuItem("Breeze Tools/TODO Finder")]
        public static void ShowWindow()
        {
            var window = GetWindow<TODOFinderWindow>();
            window.titleContent = new GUIContent("TODO Finder");
            window.minSize = new Vector2(600, 400);
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

            // Title with icon-like prefix
            var titleContainer = new VisualElement
            {
                style = { flexDirection = FlexDirection.Row, alignItems = Align.Center }
            };
            
            var iconLabel = new Label("📋")
            {
                style =
                {
                    fontSize = 20,
                    marginRight = 10,
                    unityTextAlign = TextAnchor.MiddleCenter
                }
            };
            titleContainer.Add(iconLabel);

            var titleLabel = new Label("TODO Finder")
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
            root.Add(header);

            // Main content area
            var contentArea = new VisualElement
            {
                style =
                {
                    flexGrow = 1,
                    paddingTop = 10,
                    paddingBottom = 10,
                    paddingLeft = 10,
                    paddingRight = 10
                }
            };

            // Toolbar container with better styling
            var toolbar = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    marginBottom = 10,
                    backgroundColor = new StyleColor(new Color(0.18f, 0.18f, 0.18f, 1f)),
                    paddingTop = 8,
                    paddingBottom = 8,
                    paddingLeft = 10,
                    paddingRight = 10,
                    borderTopLeftRadius = 4,
                    borderTopRightRadius = 4,
                    borderBottomLeftRadius = 4,
                    borderBottomRightRadius = 4,
                    borderBottomWidth = 1,
                    borderTopWidth = 1,
                    borderLeftWidth = 1,
                    borderRightWidth = 1,
                    borderBottomColor = new StyleColor(new Color(0.3f, 0.3f, 0.3f, 0.5f)),
                    borderTopColor = new StyleColor(new Color(0.3f, 0.3f, 0.3f, 0.5f)),
                    borderLeftColor = new StyleColor(new Color(0.3f, 0.3f, 0.3f, 0.5f)),
                    borderRightColor = new StyleColor(new Color(0.3f, 0.3f, 0.3f, 0.5f))
                }
            };

            // Search field with icon
            var searchContainer = new VisualElement
            {
                style = { flexGrow = 1, flexDirection = FlexDirection.Row, marginRight = 10 }
            };
            
            _searchField = new TextField
            {
                style = { flexGrow = 1 }
            };
            _searchField.value = "";
            var searchInput = _searchField.Q(className: "unity-text-field__input");
            if (searchInput != null)
            {
                searchInput.style.backgroundColor = new StyleColor(new Color(0.25f, 0.25f, 0.25f, 1f));
                searchInput.style.borderTopLeftRadius = 3;
                searchInput.style.borderTopRightRadius = 3;
                searchInput.style.borderBottomLeftRadius = 3;
                searchInput.style.borderBottomRightRadius = 3;
                searchInput.style.paddingLeft = 8;
                searchInput.style.paddingRight = 8;
            }
            _searchField.RegisterValueChangedCallback(evt => FilterTodos());
            
            var searchLabel = new Label("🔍 Search:");
            searchLabel.style.marginRight = 5;
            searchLabel.style.alignSelf = Align.Center;
            searchContainer.Add(searchLabel);
            searchContainer.Add(_searchField);
            toolbar.Add(searchContainer);

            // Filter dropdown
            _filterDropdown = new DropdownField(
                new List<string> { "All", "TODO", "FIXME", "HACK", "NOTE" },
                0
            )
            {
                style = { 
                    width = 120, 
                    marginRight = 10
                }
            };
            var filterInput = _filterDropdown.Q(className: "unity-base-popup-field__input");
            if (filterInput != null)
            {
                filterInput.style.backgroundColor = new StyleColor(new Color(0.25f, 0.25f, 0.25f, 1f));
                filterInput.style.borderTopLeftRadius = 3;
                filterInput.style.borderTopRightRadius = 3;
                filterInput.style.borderBottomLeftRadius = 3;
                filterInput.style.borderBottomRightRadius = 3;
            }
            _filterDropdown.RegisterValueChangedCallback(evt => FilterTodos());
            
            var filterLabel = new Label("Filter:");
            filterLabel.style.marginRight = 5;
            filterLabel.style.alignSelf = Align.Center;
            var filterContainer = new VisualElement { style = { flexDirection = FlexDirection.Row } };
            filterContainer.Add(filterLabel);
            filterContainer.Add(_filterDropdown);
            toolbar.Add(filterContainer);

            // Refresh button with better styling
            _refreshButton = new Button(ScanForTodos)
            {
                text = "🔄 Refresh",
                style = { 
                    width = 100,
                    backgroundColor = new StyleColor(new Color(0.2f, 0.4f, 0.7f, 1f)),
                    borderTopLeftRadius = 4,
                    borderTopRightRadius = 4,
                    borderBottomLeftRadius = 4,
                    borderBottomRightRadius = 4,
                    paddingTop = 6,
                    paddingBottom = 6,
                    paddingLeft = 12,
                    paddingRight = 12,
                    unityFontStyleAndWeight = FontStyle.Bold,
                    color = new StyleColor(Color.white)
                }
            };
            toolbar.Add(_refreshButton);

            contentArea.Add(toolbar);

            // Status label with improved styling
            _statusLabel = new Label("Click Refresh to scan for TODOs")
            {
                style =
                {
                    marginBottom = 10,
                    paddingLeft = 12,
                    paddingTop = 8,
                    paddingBottom = 8,
                    paddingRight = 12,
                    backgroundColor = new StyleColor(new Color(0.2f, 0.3f, 0.4f, 0.3f)),
                    borderTopLeftRadius = 4,
                    borderTopRightRadius = 4,
                    borderBottomLeftRadius = 4,
                    borderBottomRightRadius = 4,
                    borderLeftWidth = 3,
                    borderLeftColor = new StyleColor(new Color(0.3f, 0.6f, 1f, 1f)),
                    fontSize = 11,
                    color = new StyleColor(new Color(0.85f, 0.85f, 0.85f, 1f))
                }
            };
            contentArea.Add(_statusLabel);

            // ListView with improved styling
            _listView = new ListView
            {
                virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight,
                style = { 
                    flexGrow = 1,
                    backgroundColor = new StyleColor(new Color(0.16f, 0.16f, 0.16f, 1f)),
                    borderTopLeftRadius = 4,
                    borderTopRightRadius = 4,
                    borderBottomLeftRadius = 4,
                    borderBottomRightRadius = 4,
                    borderBottomWidth = 1,
                    borderTopWidth = 1,
                    borderLeftWidth = 1,
                    borderRightWidth = 1,
                    borderBottomColor = new StyleColor(new Color(0.3f, 0.3f, 0.3f, 0.5f)),
                    borderTopColor = new StyleColor(new Color(0.3f, 0.3f, 0.3f, 0.5f)),
                    borderLeftColor = new StyleColor(new Color(0.3f, 0.3f, 0.3f, 0.5f)),
                    borderRightColor = new StyleColor(new Color(0.3f, 0.3f, 0.3f, 0.5f))
                },
                showBorder = false,
                selectionType = SelectionType.Single,
                fixedItemHeight = 70
            };

            _listView.makeItem = MakeListItem;
            _listView.bindItem = BindListItem;
            _listView.itemsSource = _filteredTodoItems;
            
            _listView.selectionChanged += OnSelectionChanged;

            contentArea.Add(_listView);

            // Instructions with icon
            var instructionsLabel = new Label("💡 Double-click any item to jump to the code")
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
            contentArea.Add(instructionsLabel);

            root.Add(contentArea);
        }

        private VisualElement MakeListItem()
        {
            var container = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Column,
                    paddingTop = 10,
                    paddingBottom = 10,
                    paddingLeft = 15,
                    paddingRight = 15,
                    marginBottom = 2,
                    marginLeft = 4,
                    marginRight = 4,
                    marginTop = 2,
                    backgroundColor = new StyleColor(new Color(0.2f, 0.2f, 0.2f, 1f)),
                    borderTopLeftRadius = 6,
                    borderTopRightRadius = 6,
                    borderBottomLeftRadius = 6,
                    borderBottomRightRadius = 6,
                    borderLeftWidth = 4
                }
            };

            // Add hover effect
            container.RegisterCallback<MouseEnterEvent>(evt =>
            {
                var elem = evt.currentTarget as VisualElement;
                if (elem != null)
                {
                    elem.style.backgroundColor = new StyleColor(new Color(0.25f, 0.25f, 0.25f, 1f));
                }
            });

            container.RegisterCallback<MouseLeaveEvent>(evt =>
            {
                var elem = evt.currentTarget as VisualElement;
                if (elem != null)
                {
                    elem.style.backgroundColor = new StyleColor(new Color(0.2f, 0.2f, 0.2f, 1f));
                }
            });

            // Header row with tag and file path
            var headerRow = new VisualElement
            {
                style = { 
                    flexDirection = FlexDirection.Row, 
                    marginBottom = 6,
                    alignItems = Align.Center
                }
            };

            var tagLabel = new Label
            {
                name = "tag",
                style =
                {
                    fontSize = 10,
                    unityFontStyleAndWeight = FontStyle.Bold,
                    minWidth = 65,
                    maxWidth = 65,
                    paddingLeft = 8,
                    paddingRight = 8,
                    paddingTop = 4,
                    paddingBottom = 4,
                    borderTopLeftRadius = 10,
                    borderTopRightRadius = 10,
                    borderBottomLeftRadius = 10,
                    borderBottomRightRadius = 10,
                    marginRight = 12,
                    unityTextAlign = TextAnchor.MiddleCenter
                }
            };
            headerRow.Add(tagLabel);

            var fileLabel = new Label
            {
                name = "file",
                style =
                {
                    fontSize = 10,
                    color = new StyleColor(new Color(0.6f, 0.7f, 1f, 1f)),
                    flexGrow = 1,
                    unityFontStyleAndWeight = FontStyle.Bold
                }
            };
            headerRow.Add(fileLabel);

            container.Add(headerRow);

            // Message
            var messageLabel = new Label
            {
                name = "message",
                style =
                {
                    fontSize = 12,
                    whiteSpace = WhiteSpace.Normal,
                    marginLeft = 77,
                    color = new StyleColor(new Color(0.85f, 0.85f, 0.85f, 1f)),
                    marginTop = -2
                }
            };
            container.Add(messageLabel);

            // Make the container clickable
            container.RegisterCallback<MouseDownEvent>(evt =>
            {
                if (evt.clickCount == 2)
                {
                    var todo = (TodoItem)container.userData;
                    if (todo != null)
                    {
                        OpenScriptAtLine(todo.FilePath, todo.LineNumber);
                    }
                }
            });

            return container;
        }

        private void BindListItem(VisualElement element, int index)
        {
            if (index < 0 || index >= _filteredTodoItems.Count)
                return;

            var todo = _filteredTodoItems[index];
            element.userData = todo;

            var tagLabel = element.Q<Label>("tag");
            var fileLabel = element.Q<Label>("file");
            var messageLabel = element.Q<Label>("message");

            tagLabel.text = todo.Tag;
            
            // Color code the tag with modern colors
            Color tagBgColor;
            Color borderColor;
            switch (todo.Tag)
            {
                case "TODO":
                    tagBgColor = new Color(0.2f, 0.5f, 0.9f, 1f);
                    borderColor = new Color(0.3f, 0.6f, 1f, 1f);
                    break;
                case "FIXME":
                    tagBgColor = new Color(0.9f, 0.3f, 0.3f, 1f);
                    borderColor = new Color(1f, 0.4f, 0.4f, 1f);
                    break;
                case "HACK":
                    tagBgColor = new Color(0.9f, 0.7f, 0.2f, 1f);
                    borderColor = new Color(1f, 0.8f, 0.3f, 1f);
                    break;
                case "NOTE":
                    tagBgColor = new Color(0.3f, 0.8f, 0.5f, 1f);
                    borderColor = new Color(0.4f, 0.9f, 0.6f, 1f);
                    break;
                default:
                    tagBgColor = new Color(0.5f, 0.5f, 0.5f, 1f);
                    borderColor = new Color(0.6f, 0.6f, 0.6f, 1f);
                    break;
            }
            
            tagLabel.style.backgroundColor = new StyleColor(tagBgColor);
            tagLabel.style.color = new StyleColor(Color.white);
            element.style.borderLeftColor = new StyleColor(borderColor);

            // Add emoji indicators
            var emoji = todo.Tag switch
            {
                "TODO" => "📝",
                "FIXME" => "🔧",
                "HACK" => "⚠️",
                "NOTE" => "📌",
                _ => "•"
            };

            fileLabel.text = $"{emoji} {Path.GetFileName(todo.FilePath)} : {todo.LineNumber}";
            messageLabel.text = string.IsNullOrWhiteSpace(todo.Message) ? "(no description)" : todo.Message;
        }

        private void OnSelectionChanged(IEnumerable<object> selectedItems)
        {
            // Optional: Could add more interaction here
        }

        private void ScanForTodos()
        {
            _allTodoItems.Clear();
            _statusLabel.text = "Scanning...";

            var scriptFiles = Directory.GetFiles(Application.dataPath, "*.cs", SearchOption.AllDirectories);
            
            // Regex pattern to match TODO, FIXME, HACK, NOTE comments
            var pattern = @"//\s*(TODO|FIXME|HACK|NOTE)[:\s]*(.*)";
            var regex = new Regex(pattern, RegexOptions.IgnoreCase);

            int totalFiles = scriptFiles.Length;
            int processedFiles = 0;

            foreach (var filePath in scriptFiles)
            {
                processedFiles++;
                
                try
                {
                    var lines = File.ReadAllLines(filePath);
                    for (int i = 0; i < lines.Length; i++)
                    {
                        var match = regex.Match(lines[i]);
                        if (match.Success)
                        {
                            _allTodoItems.Add(new TodoItem
                            {
                                FilePath = filePath,
                                LineNumber = i + 1,
                                Tag = match.Groups[1].Value.ToUpper(),
                                Message = match.Groups[2].Value.Trim(),
                                FullLine = lines[i].Trim()
                            });
                        }
                    }
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"Error reading file {filePath}: {e.Message}");
                }
            }

            _statusLabel.text = $"Found {_allTodoItems.Count} items in {totalFiles} files";
            FilterTodos();
        }

        private void FilterTodos()
        {
            _filteredTodoItems.Clear();

            var searchText = _searchField?.value?.ToLower() ?? "";
            var filterType = _filterDropdown?.value ?? "All";

            foreach (var item in _allTodoItems)
            {
                // Apply type filter
                if (filterType != "All" && item.Tag != filterType)
                    continue;

                // Apply search filter
                if (!string.IsNullOrEmpty(searchText))
                {
                    bool matchesSearch = 
                        item.Message.ToLower().Contains(searchText) ||
                        item.FilePath.ToLower().Contains(searchText) ||
                        item.Tag.ToLower().Contains(searchText);
                    
                    if (!matchesSearch)
                        continue;
                }

                _filteredTodoItems.Add(item);
            }

            _listView?.RefreshItems();
            
            if (_statusLabel != null && _allTodoItems.Count > 0)
            {
                _statusLabel.text = _filteredTodoItems.Count == _allTodoItems.Count
                    ? $"Showing all {_allTodoItems.Count} items"
                    : $"Showing {_filteredTodoItems.Count} of {_allTodoItems.Count} items";
            }
        }

        private void OpenScriptAtLine(string filePath, int lineNumber)
        {
            // Convert absolute path to relative Unity asset path
            var relativePath = "Assets" + filePath.Replace(Application.dataPath, "").Replace("\\", "/");
            
            var script = AssetDatabase.LoadAssetAtPath<MonoScript>(relativePath);
            if (script != null)
            {
                AssetDatabase.OpenAsset(script, lineNumber);
            }
            else
            {
                Debug.LogWarning($"Could not open script at: {relativePath}");
            }
        }

        [Serializable]
        private class TodoItem
        {
            public string FilePath;
            public int LineNumber;
            public string Tag;
            public string Message;
            public string FullLine;
        }
    }
}
