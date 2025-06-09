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
        private VisualElement _toolContainer;
        private TextField _searchField;
        private ListView _categoryList;
        private ScrollView _toolsScrollView;
        private Label _statusLabel;
        
        private string _currentCategory = "";
        private string _searchQuery = "";
        private IBreezeTool _selectedTool;
        
        private List<string> _categories = new List<string>();
        
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
            
            var title = new Label("BreezeToolBox v2");
            title.AddToClassList("title");
            header.Add(title);
            
            // Command palette shortcut
            var searchContainer = new VisualElement();
            searchContainer.style.flexDirection = FlexDirection.Row;
            searchContainer.style.alignItems = Align.Center;            _searchField = new TextField();
            _searchField.AddToClassList("search-field");
            _searchField.RegisterValueChangedCallback(OnSearchChanged);
            searchContainer.Add(_searchField);
            
            var searchButton = new Button(() => OpenCommandPalette()) { text = "⌕" };
            searchButton.AddToClassList("search-button");
            searchContainer.Add(searchButton);
            
            header.Add(searchContainer);
            root.Add(header);
        }
        
        private void CreateMainContent(VisualElement root)
        {
            var mainContent = new VisualElement();
            mainContent.style.flexDirection = FlexDirection.Row;
            mainContent.style.flexGrow = 1;
            
            // Left sidebar - Categories
            CreateSidebar(mainContent);
            
            // Right content - Tools
            CreateToolsArea(mainContent);
            
            root.Add(mainContent);
        }
        
        private void CreateSidebar(VisualElement parent)
        {
            var sidebar = new VisualElement();
            sidebar.AddToClassList("sidebar");
            sidebar.style.width = 200;
            
            var categoryLabel = new Label("Categories");
            categoryLabel.AddToClassList("section-header");
            sidebar.Add(categoryLabel);
              _categoryList = new ListView();
            _categoryList.AddToClassList("category-list");
            _categoryList.selectionType = SelectionType.Single;
            _categoryList.selectionChanged += OnCategorySelectionChanged;
            sidebar.Add(_categoryList);
            
            parent.Add(sidebar);
        }
        
        private void CreateToolsArea(VisualElement parent)
        {
            var toolsArea = new VisualElement();
            toolsArea.style.flexGrow = 1;
            toolsArea.AddToClassList("tools-area");
            
            // Tools header
            var toolsHeader = new Label("Tools");
            toolsHeader.AddToClassList("section-header");
            toolsArea.Add(toolsHeader);
            
            // Tools container
            _toolsScrollView = new ScrollView();
            _toolsScrollView.AddToClassList("tools-scroll");
            _toolsScrollView.style.flexGrow = 1;
            
            _toolContainer = new VisualElement();
            _toolContainer.AddToClassList("tool-container");
            _toolsScrollView.Add(_toolContainer);
            
            toolsArea.Add(_toolsScrollView);
            parent.Add(toolsArea);
        }
        
        private void CreateFooter(VisualElement root)
        {
            var footer = new VisualElement();
            footer.AddToClassList("footer");
            
            _statusLabel = new Label("Ready");
            _statusLabel.AddToClassList("status-label");
            footer.Add(_statusLabel);
            
            root.Add(footer);
        }
        
        private void RefreshUI()
        {
            RefreshCategories();
            RefreshTools();
        }
        
        private void RefreshCategories()
        {
            _categories.Clear();
            _categories.Add("All Tools");
            _categories.AddRange(BreezeToolRegistry.ToolsByCategory.Keys.OrderBy(k => k));
            
            _categoryList.itemsSource = _categories;
            _categoryList.makeItem = () => new Label();
            _categoryList.bindItem = (element, index) =>
            {
                var label = element as Label;
                label.text = _categories[index];
            };
            
            if (_categories.Count > 0)
            {
                _categoryList.SetSelection(0);
                _currentCategory = "";
            }
        }
        
        private void RefreshTools()
        {
            _toolContainer.Clear();
            
            List<IBreezeTool> toolsToShow;
            
            if (!string.IsNullOrEmpty(_searchQuery))
            {
                toolsToShow = BreezeToolRegistry.SearchTools(_searchQuery);
            }
            else if (string.IsNullOrEmpty(_currentCategory) || _currentCategory == "All Tools")
            {
                toolsToShow = BreezeToolRegistry.RegisteredTools.Values.ToList();
            }
            else
            {
                toolsToShow = BreezeToolRegistry.GetToolsInCategory(_currentCategory);
            }
            
            foreach (var tool in toolsToShow.Where(t => t.IsEnabled))
            {
                CreateToolUI(tool);
            }
            
            UpdateStatus($"Showing {toolsToShow.Count(t => t.IsEnabled)} tools");
        }
        
        private void CreateToolUI(IBreezeTool tool)
        {
            var toolElement = new Foldout();
            toolElement.text = tool.ToolName;
            toolElement.AddToClassList("tool-foldout");
            
            // Tool description
            var description = new Label(tool.ToolDescription);
            description.AddToClassList("tool-description");
            toolElement.Add(description);
            
            // Tool content
            var toolContent = tool.CreateUI();
            if (toolContent != null)
            {
                toolElement.Add(toolContent);
            }
            
            _toolContainer.Add(toolElement);
        }
        
        private void OnCategorySelectionChanged(IEnumerable<object> selectedItems)
        {
            var selectedCategory = selectedItems.FirstOrDefault() as string;
            if (selectedCategory != null && selectedCategory != _currentCategory)
            {
                _currentCategory = selectedCategory == "All Tools" ? "" : selectedCategory;
                RefreshTools();
            }
        }
        
        private void OnSearchChanged(ChangeEvent<string> evt)
        {
            _searchQuery = evt.newValue;
            RefreshTools();
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
            }
        }
    }
}
