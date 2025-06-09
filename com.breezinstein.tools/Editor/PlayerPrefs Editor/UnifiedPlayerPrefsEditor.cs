using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System.Collections.Generic;
using System.Linq;
using System;
using System.IO;
using System.Text.RegularExpressions;

#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
using Microsoft.Win32;
#endif

namespace Breezinstein.Tools
{
    public class UnifiedPlayerPrefsEditor : EditorWindow
    {
        private VisualElement root;
        private ListView playerPrefsListView;
        private TextField searchField;
        private DropdownField typeDropdown;
        private TextField keyField;
        private TextField valueField;
        private Button addButton;
        private Button refreshButton;
        private Button deleteAllButton;
        private Button importButton;
        private Button exportButton;
        private Label statusLabel;
        private Toggle autoRefreshToggle;
        private DropdownField filterTypeDropdown;
        
        private List<PlayerPrefEntryData> allPlayerPrefs = new List<PlayerPrefEntryData>();
        private List<PlayerPrefEntryData> filteredPlayerPrefs = new List<PlayerPrefEntryData>();
        private List<PlayerPrefEntryData> selectedEntries = new List<PlayerPrefEntryData>();
        
        private string searchText = "";
        private ExtendedPrefType selectedType = ExtendedPrefType.String;
        private ExtendedPrefType filterType = ExtendedPrefType.All;
        private bool autoRefresh = false;
        private double lastRefreshTime = 0;
        private const double REFRESH_INTERVAL = 2.0; // seconds
        
        [MenuItem("Breeze Tools/PlayerPrefs Editor")]
        public static void ShowWindow()
        {
            var window = GetWindow<UnifiedPlayerPrefsEditor>("Unified PlayerPrefs Editor");
            window.minSize = new Vector2(800, 600);
        }

        public void CreateGUI()
        {
            root = rootVisualElement;
            
            // Try to load UXML and USS from the package
            var visualTree = Resources.Load<VisualTreeAsset>("UnifiedPlayerPrefsEditor");
            var styleSheet = Resources.Load<StyleSheet>("UnifiedPlayerPrefsEditor");
            
            // Fallback to direct path loading
            if (visualTree == null)
            {
                string[] guids = AssetDatabase.FindAssets("UnifiedPlayerPrefsEditor t:VisualTreeAsset");
                if (guids.Length > 0)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                    visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(path);
                }
            }
            
            if (styleSheet == null)
            {
                string[] guids = AssetDatabase.FindAssets("UnifiedPlayerPrefsEditor t:StyleSheet");
                if (guids.Length > 0)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                    styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(path);
                }
            }
            
            if (visualTree != null)
            {
                visualTree.CloneTree(root);
            }
            else
            {
                CreateUIFallback();
            }
            
            if (styleSheet != null)
            {
                root.styleSheets.Add(styleSheet);
            }
            
            SetupUI();
            LoadPlayerPrefs();
        }

        private void CreateUIFallback()
        {
            // Fallback UI creation if UXML is not found
            root.Add(new Label("Unified PlayerPrefs Editor") { name = "title" });
            
            // Toolbar
            var toolbar = new Toolbar() { name = "toolbar" };
            root.Add(toolbar);
            
            // Search section
            var searchContainer = new VisualElement() { name = "search-container" };
            searchField = new TextField("Search") { name = "search-field" };
            filterTypeDropdown = new DropdownField("Filter") { name = "filter-dropdown" };
            autoRefreshToggle = new Toggle("Auto Refresh") { name = "auto-refresh-toggle" };
            
            // Initialize filter dropdown choices
            var filterNames = Enum.GetNames(typeof(ExtendedPrefType)).ToList();
            filterTypeDropdown.choices = filterNames;
            filterTypeDropdown.value = ExtendedPrefType.All.ToString();
            
            searchContainer.Add(searchField);
            searchContainer.Add(filterTypeDropdown);
            searchContainer.Add(autoRefreshToggle);
            root.Add(searchContainer);
            
            // Add new entry section
            var addContainer = new VisualElement() { name = "add-container" };
            keyField = new TextField("Key") { name = "key-field" };
            typeDropdown = new DropdownField("Type") { name = "type-dropdown" };
            valueField = new TextField("Value") { name = "value-field" };
            addButton = new Button(() => AddPlayerPref()) { text = "Add", name = "add-button" };
            
            // Ensure dropdown is properly initialized
            typeDropdown.choices = new List<string> { "String", "Int", "Float", "Bool", "Long", "Vector2", "Vector3", "Quaternion", "Color" };
            typeDropdown.value = "String";
            
            addContainer.Add(keyField);
            addContainer.Add(typeDropdown);
            addContainer.Add(valueField);
            addContainer.Add(addButton);
            root.Add(addContainer);
            
            // ListView
            playerPrefsListView = new ListView() { name = "playerprefs-listview" };
            root.Add(playerPrefsListView);
            
            // Action buttons
            var actionContainer = new VisualElement() { name = "action-container" };
            refreshButton = new Button(() => LoadPlayerPrefs()) { text = "Refresh", name = "refresh-button" };
            deleteAllButton = new Button(() => DeleteAllPlayerPrefs()) { text = "Delete All", name = "delete-all-button" };
            importButton = new Button(() => ImportPlayerPrefs()) { text = "Import", name = "import-button" };
            exportButton = new Button(() => ExportPlayerPrefs()) { text = "Export", name = "export-button" };
            
            actionContainer.Add(refreshButton);
            actionContainer.Add(deleteAllButton);
            actionContainer.Add(importButton);
            actionContainer.Add(exportButton);
            root.Add(actionContainer);
            
            // Status bar
            statusLabel = new Label("Ready") { name = "status-label" };
            root.Add(statusLabel);
        }

        private void SetupUI()
        {
            // Find UI elements (either from UXML or fallback)
            searchField = root.Q<TextField>("search-field");
            filterTypeDropdown = root.Q<DropdownField>("filter-dropdown");
            autoRefreshToggle = root.Q<Toggle>("auto-refresh-toggle");
            keyField = root.Q<TextField>("key-field");
            typeDropdown = root.Q<DropdownField>("type-dropdown");
            valueField = root.Q<TextField>("value-field");
            addButton = root.Q<Button>("add-button");
            playerPrefsListView = root.Q<ListView>("playerprefs-listview");
            refreshButton = root.Q<Button>("refresh-button");
            deleteAllButton = root.Q<Button>("delete-all-button");
            importButton = root.Q<Button>("import-button");
            exportButton = root.Q<Button>("export-button");
            statusLabel = root.Q<Label>("status-label");
            
            // Ensure all required elements exist
            if (searchField == null || typeDropdown == null || playerPrefsListView == null)
            {
                Debug.LogWarning("[UnifiedPlayerPrefsEditor] Some UI elements not found, falling back to code creation");
                CreateUIFallback();
                return;
            }
            
            // Setup dropdowns
            SetupTypeDropdown();
            SetupFilterDropdown();
            SetupListView();
            SetupEventCallbacks();
        }

        private void SetupTypeDropdown()
        {
            if (typeDropdown == null)
            {
                Debug.LogError("[UnifiedPlayerPrefsEditor] typeDropdown is null! Cannot setup dropdown.");
                return;
            }
            
            // Define the available types for adding new PlayerPrefs
            // Include all basic types that users would commonly want to add
            var availableTypes = new[]
            {
                "String", "Int", "Float", "Bool", "Long",
                "Vector2", "Vector3", "Quaternion", "Color"
            };
            
            // Clear any existing choices and set new ones
            typeDropdown.choices = availableTypes.ToList();
            
            // Set default selection to String
            typeDropdown.index = 0;
            typeDropdown.value = availableTypes[0];
            selectedType = ExtendedPrefType.String;
            
            // Register value change callback
            typeDropdown.RegisterValueChangedCallback(evt =>
            {
                if (Enum.TryParse<ExtendedPrefType>(evt.newValue, out var parsedType))
                {
                    selectedType = parsedType;
                    UpdateValueFieldPlaceholder();
                }
                else
                {
                    Debug.LogWarning($"[UnifiedPlayerPrefsEditor] Failed to parse type: {evt.newValue}");
                    // Fallback to String if parsing fails
                    selectedType = ExtendedPrefType.String;
                }
            });
            
            // Set initial placeholder
            UpdateValueFieldPlaceholder();
        }

        private void SetupFilterDropdown()
        {
            if (filterTypeDropdown == null)
            {
                Debug.LogError("[UnifiedPlayerPrefsEditor] filterTypeDropdown is null! Cannot setup filter dropdown.");
                return;
            }
            
            // Get all enum values for filtering (including All and arrays)
            var filterNames = Enum.GetNames(typeof(ExtendedPrefType)).ToList();
            
            // Clear and set choices
            filterTypeDropdown.choices = filterNames;
            filterTypeDropdown.value = ExtendedPrefType.All.ToString();
            filterType = ExtendedPrefType.All;
            
            // Register callback
            filterTypeDropdown.RegisterValueChangedCallback(evt =>
            {
                if (Enum.TryParse<ExtendedPrefType>(evt.newValue, out var parsedType))
                {
                    filterType = parsedType;
                    FilterPlayerPrefs();
                }
                else
                {
                    Debug.LogWarning($"[UnifiedPlayerPrefsEditor] Failed to parse filter type: {evt.newValue}");
                    filterType = ExtendedPrefType.All;
                }
            });
        }

        private void SetupListView()
        {
            playerPrefsListView.itemsSource = filteredPlayerPrefs;
            playerPrefsListView.selectionType = SelectionType.Multiple;
            playerPrefsListView.virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight;
            
            playerPrefsListView.makeItem = () => new PlayerPrefVisualElement();
            playerPrefsListView.bindItem = (element, index) =>
            {
                if (element is PlayerPrefVisualElement prefElement && index < filteredPlayerPrefs.Count)
                {
                    prefElement.SetData(filteredPlayerPrefs[index], this);
                    
                    // Add even-row class for alternating row styling
                    if (index % 2 == 0)
                    {
                        prefElement.AddToClassList("even-row");
                    }
                    else
                    {
                        prefElement.RemoveFromClassList("even-row");
                    }
                }
            };
            
            playerPrefsListView.selectionChanged += OnSelectionChanged;
        }

        private void SetupEventCallbacks()
        {
            searchField.RegisterValueChangedCallback(evt => 
            {
                searchText = evt.newValue;
                FilterPlayerPrefs();
            });
            
            autoRefreshToggle.RegisterValueChangedCallback(evt => 
            {
                autoRefresh = evt.newValue;
            });
            
            addButton.clicked += AddPlayerPref;
            refreshButton.clicked += LoadPlayerPrefs;
            deleteAllButton.clicked += DeleteAllPlayerPrefs;
            importButton.clicked += ImportPlayerPrefs;
            exportButton.clicked += ExportPlayerPrefs;
            
            keyField.RegisterCallback<KeyDownEvent>(evt => 
            {
                if (evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.KeypadEnter)
                {
                    AddPlayerPref();
                }
            });
            
            valueField.RegisterCallback<KeyDownEvent>(evt => 
            {
                if (evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.KeypadEnter)
                {
                    AddPlayerPref();
                }
            });
        }

        private void OnSelectionChanged(IEnumerable<object> selectedItems)
        {
            selectedEntries.Clear();
            selectedEntries.AddRange(selectedItems.Cast<PlayerPrefEntryData>());
            UpdateStatusLabel();
        }

        private void UpdateValueFieldPlaceholder()
        {
            if (valueField == null) return;
            
            // Set placeholder text based on selected type
            switch (selectedType)
            {
                case ExtendedPrefType.String:
                    valueField.value = "";
                    valueField.SetValueWithoutNotify("");
                    break;
                case ExtendedPrefType.Int:
                    valueField.value = "0";
                    break;
                case ExtendedPrefType.Float:
                    valueField.value = "0.0";
                    break;
                case ExtendedPrefType.Bool:
                    valueField.value = "false";
                    break;
                case ExtendedPrefType.Long:
                    valueField.value = "0";
                    break;
                case ExtendedPrefType.Vector2:
                    valueField.value = "0,0";
                    break;
                case ExtendedPrefType.Vector3:
                    valueField.value = "0,0,0";
                    break;
                case ExtendedPrefType.Quaternion:
                    valueField.value = "0,0,0,1";
                    break;
                case ExtendedPrefType.Color:
                    valueField.value = "1,1,1,1";
                    break;
                default:
                    valueField.value = "";
                    break;
            }
        }

        private void AddPlayerPref()
        {
            string key = keyField.value;
            string value = valueField.value;
            
            if (string.IsNullOrEmpty(key))
            {
                UpdateStatusLabel("Key cannot be empty", true);
                return;
            }
            
            try
            {
                PlayerPrefsExtensions.SetPlayerPref(key, value, selectedType);
                PlayerPrefs.Save();
                
                keyField.value = "";
                valueField.value = "";
                UpdateValueFieldPlaceholder();
                
                LoadPlayerPrefs();
                UpdateStatusLabel($"Added PlayerPref '{key}'");
            }
            catch (Exception ex)
            {
                UpdateStatusLabel($"Error adding PlayerPref: {ex.Message}", true);
            }
        }

        private void LoadPlayerPrefs()
        {
            try
            {
                allPlayerPrefs.Clear();
                var discoveredKeys = PlayerPrefsDiscovery.DiscoverAllPlayerPrefsKeys();
                
                foreach (string key in discoveredKeys)
                {
                    if (PlayerPrefs.HasKey(key))
                    {
                        var type = PlayerPrefsExtensions.DetectPlayerPrefType(key);
                        var value = PlayerPrefsExtensions.GetPlayerPrefValue(key, type);
                        allPlayerPrefs.Add(new PlayerPrefEntryData(key, value, type));
                    }
                }
                
                allPlayerPrefs.Sort((a, b) => a.Key.CompareTo(b.Key));
                FilterPlayerPrefs();
                UpdateStatusLabel($"Loaded {allPlayerPrefs.Count} PlayerPrefs");
            }
            catch (Exception ex)
            {
                UpdateStatusLabel($"Error loading PlayerPrefs: {ex.Message}", true);
            }
        }

        private void FilterPlayerPrefs()
        {
            filteredPlayerPrefs.Clear();
            
            foreach (var pref in allPlayerPrefs)
            {
                bool matchesSearch = string.IsNullOrEmpty(searchText) || 
                                   pref.Key.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0;
                
                bool matchesFilter = filterType == ExtendedPrefType.All || pref.Type == filterType;
                
                if (matchesSearch && matchesFilter)
                {
                    filteredPlayerPrefs.Add(pref);
                }
            }
            
            playerPrefsListView.RefreshItems();
            UpdateStatusLabel($"Showing {filteredPlayerPrefs.Count} of {allPlayerPrefs.Count} PlayerPrefs");
        }

        private void DeleteAllPlayerPrefs()
        {
            if (EditorUtility.DisplayDialog("Delete All PlayerPrefs", 
                "Are you sure you want to delete ALL PlayerPrefs? This action cannot be undone.", 
                "DELETE ALL", "Cancel"))
            {
                PlayerPrefs.DeleteAll();
                PlayerPrefs.Save();
                LoadPlayerPrefs();
                UpdateStatusLabel("All PlayerPrefs deleted");
            }
        }

        private void ImportPlayerPrefs()
        {
            string path = EditorUtility.OpenFilePanel("Import PlayerPrefs", "", "json");
            if (!string.IsNullOrEmpty(path))
            {
                try
                {
                    PlayerPrefsImportExport.ImportFromFile(path);
                    LoadPlayerPrefs();
                    UpdateStatusLabel("PlayerPrefs imported successfully");
                }
                catch (Exception ex)
                {
                    UpdateStatusLabel($"Import failed: {ex.Message}", true);
                }
            }
        }

        private void ExportPlayerPrefs()
        {
            string path = EditorUtility.SaveFilePanel("Export PlayerPrefs", "", "PlayerPrefs", "json");
            if (!string.IsNullOrEmpty(path))
            {
                try
                {
                    PlayerPrefsImportExport.ExportToFile(allPlayerPrefs, path);
                    UpdateStatusLabel("PlayerPrefs exported successfully");
                }
                catch (Exception ex)
                {
                    UpdateStatusLabel($"Export failed: {ex.Message}", true);
                }
            }
        }

        public void DeletePlayerPref(string key)
        {
            PlayerPrefs.DeleteKey(key);
            PlayerPrefs.Save();
            LoadPlayerPrefs();
            UpdateStatusLabel($"Deleted PlayerPref '{key}'");
        }

        public void UpdatePlayerPref(string key, string newValue, ExtendedPrefType type)
        {
            try
            {
                PlayerPrefsExtensions.SetPlayerPref(key, newValue, type);
                PlayerPrefs.Save();
                LoadPlayerPrefs();
                UpdateStatusLabel($"Updated PlayerPref '{key}'");
            }
            catch (Exception ex)
            {
                UpdateStatusLabel($"Error updating PlayerPref: {ex.Message}", true);
            }
        }

        private void UpdateStatusLabel(string message = "", bool isError = false)
        {
            if (string.IsNullOrEmpty(message))
            {
                message = selectedEntries.Count > 0 ? 
                    $"{selectedEntries.Count} item(s) selected" : 
                    $"Showing {filteredPlayerPrefs.Count} of {allPlayerPrefs.Count} PlayerPrefs";
            }
            
            statusLabel.text = message;
            statusLabel.RemoveFromClassList("error");
            statusLabel.RemoveFromClassList("success");
            
            if (isError)
            {
                statusLabel.AddToClassList("error");
            }
            else if (message.Contains("Added") || message.Contains("Updated") || message.Contains("Deleted") || message.Contains("imported") || message.Contains("exported"))
            {
                statusLabel.AddToClassList("success");
            }
        }

        private void Update()
        {
            if (autoRefresh && EditorApplication.timeSinceStartup - lastRefreshTime > REFRESH_INTERVAL)
            {
                lastRefreshTime = EditorApplication.timeSinceStartup;
                LoadPlayerPrefs();
            }
        }
    }
}