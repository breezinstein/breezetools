using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Breezinstein.Tools.Core;


namespace Breezinstein.Tools
{
    public class BreezeToolBoxV2 : EditorWindow
    {
        private VisualElement _tabContainer;
        private VisualElement _tabContentContainer;
        private TextField _searchField;
        private Label _statusLabel;
        private Button _selectedTab;
        
        private string _currentCategory = "";
        private string _searchQuery = "";
        private IBreezeTool _selectedTool;
        
        private List<string> _categories = new List<string>();
        private Dictionary<string, VisualElement> _tabContents = new Dictionary<string, VisualElement>();
        private Dictionary<string, Button> _tabButtons = new Dictionary<string, Button>();
        
        [MenuItem("Breeze Tools/BreezeToolBox v2")]
        public static void ShowWindow()
        {
            var window = GetWindow<BreezeToolBoxV2>();
            window.titleContent = new GUIContent("BreezeToolBox v2");
            window.minSize = new Vector2(800, 600);
            window.Show();
        }
          public void CreateGUI()
        {
            var root = rootVisualElement;
            root.style.flexDirection = FlexDirection.Column;
            
            // Load USS styles
            var styleSheet = Resources.Load<StyleSheet>("BreezeToolBoxStyles");
            if (styleSheet != null)
            {
                root.styleSheets.Add(styleSheet);
            }
            
            CreateHeader(root);
            CreateTabContainer(root);
            CreateMainContent(root);
            CreateFooter(root);
            
            RefreshUI();
            
            // Subscribe to selection changes
            Selection.selectionChanged += OnSelectionChanged;
        }
          private void OnDestroy()
        {
            Selection.selectionChanged -= OnSelectionChanged;
        }

        private void CreateHeader(VisualElement root)
        {
            var header = new VisualElement();
            header.AddToClassList("header");
            header.style.flexDirection = FlexDirection.Row;
            header.style.justifyContent = Justify.SpaceBetween;
            header.style.alignItems = Align.Center;
            header.style.paddingBottom = 20;
            header.style.paddingTop = 20;
            header.style.paddingLeft = 20;
            header.style.paddingRight = 20;
            header.style.borderBottomWidth = 2;
            header.style.borderBottomColor = new Color(0.4f, 0.4f, 0.4f, 1f);
            header.style.backgroundColor = new Color(0.25f, 0.25f, 0.25f, 1f);
            header.style.minHeight = 60;
            
            var title = new Label("BreezeToolBox v2");
            title.AddToClassList("title");
            title.style.fontSize = 24;            title.style.unityFontStyleAndWeight = FontStyle.Bold;
            title.style.color = new Color(0.9f, 0.9f, 0.9f, 1f);
            header.Add(title);
            
            // Search container
            var searchContainer = new VisualElement();
            searchContainer.style.flexDirection = FlexDirection.Row;
            searchContainer.style.alignItems = Align.Center;
            searchContainer.style.width = 350;
            searchContainer.style.height = 35;
            searchContainer.style.backgroundColor = new Color(0.15f, 0.15f, 0.15f, 1f);            searchContainer.style.borderTopLeftRadius = 6;
            searchContainer.style.borderTopRightRadius = 6;
            searchContainer.style.borderBottomLeftRadius = 6;
            searchContainer.style.borderBottomRightRadius = 6;
            searchContainer.style.borderTopWidth = 1;
            searchContainer.style.borderBottomWidth = 1;
            searchContainer.style.borderLeftWidth = 1;
            searchContainer.style.borderRightWidth = 1;
            searchContainer.style.borderTopColor = new Color(0.5f, 0.5f, 0.5f, 1f);
            searchContainer.style.borderBottomColor = new Color(0.5f, 0.5f, 0.5f, 1f);
            searchContainer.style.borderLeftColor = new Color(0.5f, 0.5f, 0.5f, 1f);
            searchContainer.style.borderRightColor = new Color(0.5f, 0.5f, 0.5f, 1f);
            
            _searchField = new TextField();
            _searchField.AddToClassList("search-field");
            _searchField.style.flexGrow = 1;
            _searchField.style.height = 33;            _searchField.style.fontSize = 14;
            _searchField.style.backgroundColor = Color.clear;
            _searchField.style.borderTopWidth = 0;
            _searchField.style.borderBottomWidth = 0;
            _searchField.style.borderLeftWidth = 0;
            _searchField.style.borderRightWidth = 0;
            _searchField.style.paddingLeft = 12;
            _searchField.style.paddingRight = 8;            _searchField.value = "";
            
            // Set placeholder text
            var placeholderLabel = new Label("Search tools...");
            placeholderLabel.style.position = Position.Absolute;
            placeholderLabel.style.left = 12;
            placeholderLabel.style.top = 8;
            placeholderLabel.style.fontSize = 14;
            placeholderLabel.style.color = new Color(0.6f, 0.6f, 0.6f, 1f);
            placeholderLabel.style.display = string.IsNullOrEmpty(_searchField.value) ? DisplayStyle.Flex : DisplayStyle.None;
            placeholderLabel.pickingMode = PickingMode.Ignore;
            searchContainer.Add(placeholderLabel);
            
            // Hide placeholder when typing
            _searchField.RegisterValueChangedCallback(evt => {
                placeholderLabel.style.display = string.IsNullOrEmpty(evt.newValue) ? DisplayStyle.Flex : DisplayStyle.None;
            });
            
            // Add placeholder text styling
            var textInput = _searchField.Q<VisualElement>("unity-text-input");            if (textInput != null)
            {
                textInput.style.backgroundColor = Color.clear;
                textInput.style.borderTopWidth = 0;
                textInput.style.borderBottomWidth = 0;
                textInput.style.borderLeftWidth = 0;
                textInput.style.borderRightWidth = 0;
            }
            
            _searchField.RegisterValueChangedCallback(OnSearchChanged);
            searchContainer.Add(_searchField);
            
            var searchButton = new Button(() => OpenCommandPalette()) { text = "🔍" };
            searchButton.AddToClassList("search-button");
            searchButton.style.width = 40;
            searchButton.style.height = 33;            searchButton.style.fontSize = 16;
            searchButton.style.backgroundColor = new Color(0.3f, 0.3f, 0.3f, 1f);
            searchButton.style.borderTopWidth = 0;
            searchButton.style.borderBottomWidth = 0;
            searchButton.style.borderLeftWidth = 0;
            searchButton.style.borderRightWidth = 0;
            searchButton.style.borderTopRightRadius = 5;
            searchButton.style.borderBottomRightRadius = 5;
            searchContainer.Add(searchButton);
            
            header.Add(searchContainer);
            root.Add(header);
        }
          private void CreateTabContainer(VisualElement root)
        {
            _tabContainer = new VisualElement();
            _tabContainer.AddToClassList("tab-container");
            _tabContainer.style.flexDirection = FlexDirection.Row;
            _tabContainer.style.borderBottomWidth = 1;
            _tabContainer.style.borderBottomColor = new Color(0.3f, 0.3f, 0.3f, 1f);
            _tabContainer.style.paddingLeft = 15;
            _tabContainer.style.paddingRight = 15;
            _tabContainer.style.backgroundColor = new Color(0.2f, 0.2f, 0.2f, 1f);
            
            root.Add(_tabContainer);
        }
          private void CreateMainContent(VisualElement root)
        {
            _tabContentContainer = new VisualElement();
            _tabContentContainer.style.flexGrow = 1;
            _tabContentContainer.style.paddingTop = 15;
            _tabContentContainer.style.paddingBottom = 15;
            _tabContentContainer.style.paddingLeft = 15;
            _tabContentContainer.style.paddingRight = 15;
              root.Add(_tabContentContainer);
        }

        private void CreateFooter(VisualElement root)
        {
            var footer = new VisualElement();
            footer.AddToClassList("footer");
            footer.style.borderTopWidth = 1;
            footer.style.borderTopColor = new Color(0.3f, 0.3f, 0.3f, 1f);
            footer.style.paddingTop = 10;
            footer.style.paddingBottom = 10;
            footer.style.paddingLeft = 10;
            footer.style.paddingRight = 10;
            footer.style.backgroundColor = new Color(0.15f, 0.15f, 0.15f, 1f);
            footer.style.flexDirection = FlexDirection.Row;
            footer.style.justifyContent = Justify.SpaceBetween;
            
            _statusLabel = new Label("Ready");
            _statusLabel.AddToClassList("status-label");
            _statusLabel.style.fontSize = 12;
            footer.Add(_statusLabel);
              // Add some info text instead of shortcuts
            var infoLabel = new Label("Professional Unity Editor Tools");
            infoLabel.style.fontSize = 10;
            infoLabel.style.color = new Color(0.6f, 0.6f, 0.6f, 1f);
            footer.Add(infoLabel);
            
            root.Add(footer);
        }
          private void RefreshUI()
        {
            RefreshTabs();
            RefreshTabContent();
        }
        
        private void RefreshTabs()
        {
            _tabContainer.Clear();
            _tabButtons.Clear();
            _categories.Clear();
            
            // Add "All Tools" tab
            _categories.Add("All Tools");
            _categories.AddRange(BreezeToolRegistry.ToolsByCategory.Keys.OrderBy(k => k));
            
            // Create tab buttons
            foreach (var category in _categories)
            {
                var tabButton = CreateTabButton(category);
                _tabContainer.Add(tabButton);
                _tabButtons[category] = tabButton;
            }
            
            // Select first tab by default
            if (_categories.Count > 0)
            {
                SelectTab(_categories[0]);
            }
        }
        
        private Button CreateTabButton(string category)
        {
            var tabButton = new Button(() => SelectTab(category));
            tabButton.text = category;
            tabButton.AddToClassList("tab-button");
            
            // Style the tab button
            tabButton.style.paddingTop = 8;
            tabButton.style.paddingBottom = 8;
            tabButton.style.paddingLeft = 15;
            tabButton.style.paddingRight = 15;
            tabButton.style.marginRight = 2;
            tabButton.style.borderTopLeftRadius = 6;
            tabButton.style.borderTopRightRadius = 6;
            tabButton.style.borderBottomWidth = 0;
            tabButton.style.backgroundColor = new Color(0.25f, 0.25f, 0.25f, 1f);
            tabButton.style.color = new Color(0.8f, 0.8f, 0.8f, 1f);
            
            return tabButton;
        }
        
        private void SelectTab(string category)
        {
            // Update visual state of tabs
            foreach (var kvp in _tabButtons)
            {
                var button = kvp.Value;
                var isSelected = kvp.Key == category;
                
                if (isSelected)
                {
                    button.style.backgroundColor = new Color(0.4f, 0.4f, 0.4f, 1f);
                    button.style.color = Color.white;
                    _selectedTab = button;
                }
                else
                {
                    button.style.backgroundColor = new Color(0.25f, 0.25f, 0.25f, 1f);
                    button.style.color = new Color(0.8f, 0.8f, 0.8f, 1f);
                }
            }
            
            _currentCategory = category == "All Tools" ? "" : category;
            RefreshTabContent();
        }
        
        private void RefreshTabContent()
        {
            _tabContentContainer.Clear();
            
            List<IBreezeTool> toolsToShow;
            
            if (!string.IsNullOrEmpty(_searchQuery))
            {
                toolsToShow = BreezeToolRegistry.SearchTools(_searchQuery);
            }
            else if (string.IsNullOrEmpty(_currentCategory))
            {
                toolsToShow = BreezeToolRegistry.RegisteredTools.Values.ToList();
            }
            else
            {
                toolsToShow = BreezeToolRegistry.GetToolsInCategory(_currentCategory);
            }
            
            // Create scroll view for tools
            var scrollView = new ScrollView();
            scrollView.style.flexGrow = 1;
            
            var toolContainer = new VisualElement();
            toolContainer.style.paddingTop = 10;
            
            foreach (var tool in toolsToShow.Where(t => t.IsEnabled))
            {
                CreateToolUI(tool, toolContainer);
            }
            
            scrollView.Add(toolContainer);
            _tabContentContainer.Add(scrollView);
            
            UpdateStatus($"Showing {toolsToShow.Count(t => t.IsEnabled)} tools in {(_currentCategory.Length > 0 ? _currentCategory : "All Categories")}");
        }        private void CreateToolUI(IBreezeTool tool, VisualElement container)
        {
            var toolElement = new Foldout();
            toolElement.text = tool.ToolName;
            toolElement.value = false; // Start all foldouts in closed state
            toolElement.AddToClassList("tool-foldout");toolElement.style.marginBottom = 10;
            toolElement.style.backgroundColor = new Color(0.2f, 0.2f, 0.2f, 0.5f);
            toolElement.style.borderTopLeftRadius = 5;
            toolElement.style.borderTopRightRadius = 5;
            toolElement.style.borderBottomLeftRadius = 5;
            toolElement.style.borderBottomRightRadius = 5;
            toolElement.style.paddingTop = 10;
            toolElement.style.paddingBottom = 10;
            toolElement.style.paddingLeft = 10;
            toolElement.style.paddingRight = 10;
            
            // Tool description
            var description = new Label(tool.ToolDescription);
            description.AddToClassList("tool-description");
            description.style.fontSize = 12;
            description.style.color = new Color(0.8f, 0.8f, 0.8f, 1f);
            description.style.marginBottom = 8;
            description.style.whiteSpace = WhiteSpace.Normal;
            toolElement.Add(description);
            
            // Tool content
            var toolContent = tool.CreateUI();
            if (toolContent != null)
            {
                toolElement.Add(toolContent);
            }
            
            container.Add(toolElement);
        }
          private void OnSearchChanged(ChangeEvent<string> evt)
        {
            _searchQuery = evt.newValue;
            RefreshTabContent();
        }
          private void OnSelectionChanged()
        {
            // Notify tools of selection change
            var selectedTransforms = Selection.transforms;
            BreezeEvents.EmitSelectionChanged(selectedTransforms);
            
            // Update status
            UpdateStatus($"Selected: {selectedTransforms.Length} objects");
        }
        
        private void UpdateStatus(string message)
        {
            if (_statusLabel != null)
            {
                _statusLabel.text = message;
            }
        }
        
        private void OpenCommandPalette()
        {
            // Focus search field and select all text
            _searchField.Focus();
            _searchField.SelectAll();
        }
          // Handle keyboard shortcuts
        private void OnGUI()
        {
            var evt = Event.current;
            if (evt.type == EventType.KeyDown)
            {
                if (evt.keyCode == KeyCode.Quote && evt.control)
                {
                    OpenCommandPalette();
                    evt.Use();
                }
                // Tab navigation shortcuts
                else if (evt.keyCode == KeyCode.Tab && evt.control)
                {
                    var currentIndex = _categories.IndexOf(_currentCategory == "" ? "All Tools" : _currentCategory);
                    var nextIndex = evt.shift ? 
                        (currentIndex > 0 ? currentIndex - 1 : _categories.Count - 1) :
                        (currentIndex < _categories.Count - 1 ? currentIndex + 1 : 0);
                    SelectTab(_categories[nextIndex]);
                    evt.Use();
                }
                // Number key shortcuts for quick tab access (1-9)
                else if (evt.keyCode >= KeyCode.Alpha1 && evt.keyCode <= KeyCode.Alpha9 && evt.control)
                {
                    var tabIndex = evt.keyCode - KeyCode.Alpha1;
                    if (tabIndex < _categories.Count)
                    {
                        SelectTab(_categories[tabIndex]);
                        evt.Use();
                    }
                }
            }
        }
    }
}
