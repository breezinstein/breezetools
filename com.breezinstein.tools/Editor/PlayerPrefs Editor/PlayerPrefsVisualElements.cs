using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System;

namespace Breezinstein.Tools
{
    /// <summary>
    /// Custom VisualElement for displaying and editing PlayerPrefs entries
    /// </summary>
    public class PlayerPrefVisualElement : VisualElement
    {
        private PlayerPrefEntryData data;
        private UnifiedPlayerPrefsEditor editor;
        
        private Label keyLabel;
        private Label typeLabel;
        private Label valueLabel;
        private TextField editField;
        private TextField jsonEditField;
        private Button editButton;
        private Button saveButton;
        private Button cancelButton;
        private Button deleteButton;
        private Button jsonToggleButton;
        private VisualElement editContainer;
        private VisualElement jsonEditContainer;
        private VisualElement displayContainer;
        private Label jsonErrorLabel;

        public PlayerPrefVisualElement()
        {
            CreateUI();
            AddToClassList("playerprefs-item");
        }

        private void CreateUI()
        {
            // Main container
            this.style.flexDirection = FlexDirection.Row;
            this.style.alignItems = Align.Center;
            this.style.paddingLeft = 8;
            this.style.paddingRight = 8;
            this.style.paddingTop = 4;
            this.style.paddingBottom = 4;
            this.style.minHeight = 32;

            // Key container (fixed width)
            var keyContainer = new VisualElement();
            keyContainer.style.width = 200;
            keyContainer.style.minWidth = 200;
            keyContainer.style.maxWidth = 200;
            keyLabel = new Label();
            keyLabel.style.unityTextAlign = TextAnchor.MiddleLeft;
            keyLabel.style.overflow = Overflow.Hidden;
            keyLabel.style.textOverflow = TextOverflow.Ellipsis;
            keyContainer.Add(keyLabel);
            Add(keyContainer);

            // Type container (fixed width)
            var typeContainer = new VisualElement();
            typeContainer.style.width = 100;
            typeContainer.style.minWidth = 100;
            typeContainer.style.maxWidth = 100;
            typeLabel = new Label();
            typeLabel.style.unityTextAlign = TextAnchor.MiddleLeft;
            typeLabel.AddToClassList("type-label");
            typeContainer.Add(typeLabel);
            Add(typeContainer);

            // Value display container (flexible)
            displayContainer = new VisualElement();
            displayContainer.style.flexGrow = 1;
            displayContainer.style.flexDirection = FlexDirection.Row;
            displayContainer.style.alignItems = Align.Center;
            
            valueLabel = new Label();
            valueLabel.style.flexGrow = 1;
            valueLabel.style.unityTextAlign = TextAnchor.MiddleLeft;
            valueLabel.style.overflow = Overflow.Hidden;
            valueLabel.style.textOverflow = TextOverflow.Ellipsis;
            valueLabel.AddToClassList("value-label");
            displayContainer.Add(valueLabel);

            // JSON toggle button (only shown for strings)
            jsonToggleButton = new Button(ToggleJsonMode) { text = "JSON" };
            jsonToggleButton.style.width = 40;
            jsonToggleButton.style.height = 20;
            jsonToggleButton.style.marginLeft = 4;
            jsonToggleButton.style.display = DisplayStyle.None;
            jsonToggleButton.AddToClassList("json-toggle-button");
            displayContainer.Add(jsonToggleButton);
            
            Add(displayContainer);

            // Regular edit container (hidden by default)
            editContainer = new VisualElement();
            editContainer.style.flexGrow = 1;
            editContainer.style.flexDirection = FlexDirection.Row;
            editContainer.style.alignItems = Align.Center;
            editContainer.style.display = DisplayStyle.None;
            
            editField = new TextField();
            editField.style.flexGrow = 1;
            editField.style.marginRight = 4;
            editContainer.Add(editField);
            Add(editContainer);

            // JSON edit container (hidden by default)
            jsonEditContainer = new VisualElement();
            jsonEditContainer.style.flexGrow = 1;
            jsonEditContainer.style.flexDirection = FlexDirection.Column;
            jsonEditContainer.style.display = DisplayStyle.None;
            
            jsonEditField = new TextField();
            jsonEditField.multiline = true;
            jsonEditField.style.flexGrow = 1;
            jsonEditField.style.minHeight = 100;
            jsonEditField.style.marginRight = 4;
            jsonEditField.style.marginBottom = 2;
            jsonEditField.AddToClassList("json-edit-field");
            
            // JSON error label
            jsonErrorLabel = new Label();
            jsonErrorLabel.style.color = Color.red;
            jsonErrorLabel.style.fontSize = 12;
            jsonErrorLabel.style.display = DisplayStyle.None;
            jsonErrorLabel.AddToClassList("json-error-label");
            
            jsonEditContainer.Add(jsonEditField);
            jsonEditContainer.Add(jsonErrorLabel);
            Add(jsonEditContainer);

            // Action buttons container
            var buttonContainer = new VisualElement();
            buttonContainer.style.flexDirection = FlexDirection.Row;
            buttonContainer.style.alignItems = Align.Center;

            editButton = new Button(StartEdit) { text = "Edit" };
            editButton.style.width = 50;
            editButton.style.height = 20;
            editButton.AddToClassList("edit-button");
            buttonContainer.Add(editButton);

            saveButton = new Button(SaveEdit) { text = "Save" };
            saveButton.style.width = 50;
            saveButton.style.height = 20;
            saveButton.style.display = DisplayStyle.None;
            saveButton.AddToClassList("save-button");
            buttonContainer.Add(saveButton);

            cancelButton = new Button(CancelEdit) { text = "Cancel" };
            cancelButton.style.width = 50;
            cancelButton.style.height = 20;
            cancelButton.style.display = DisplayStyle.None;
            cancelButton.style.marginLeft = 2;
            cancelButton.AddToClassList("cancel-button");
            buttonContainer.Add(cancelButton);

            deleteButton = new Button(DeleteEntry) { text = "Delete" };
            deleteButton.style.width = 50;
            deleteButton.style.height = 20;
            deleteButton.style.marginLeft = 4;
            deleteButton.AddToClassList("delete-button");
            buttonContainer.Add(deleteButton);

            Add(buttonContainer);

            // Setup keyboard events for edit fields
            editField.RegisterCallback<KeyDownEvent>(OnEditFieldKeyDown);
            jsonEditField.RegisterCallback<KeyDownEvent>(OnEditFieldKeyDown);
        }

        public void SetData(PlayerPrefEntryData entryData, UnifiedPlayerPrefsEditor editorWindow)
        {
            data = entryData;
            editor = editorWindow;
            UpdateDisplay();
        }

        private void UpdateDisplay()
        {
            if (data == null) return;

            keyLabel.text = data.Key;
            keyLabel.tooltip = data.Key;
            
            typeLabel.text = data.TypeDisplayName;
            
            valueLabel.text = data.DisplayValue;
            valueLabel.tooltip = data.DisplayValue;
            
            editField.value = data.EditValue;
            jsonEditField.value = data.EditValue;

            // Show/hide JSON toggle button for string types
            if (data.CanUseJsonMode())
            {
                jsonToggleButton.style.display = DisplayStyle.Flex;
                UpdateJsonToggleButton();
            }
            else
            {
                jsonToggleButton.style.display = DisplayStyle.None;
            }

            // Update JSON validation display
            UpdateJsonValidation();

            // Add type-specific styling
            RemoveTypeClasses();
            AddToClassList($"type-{data.Type.ToString().ToLower()}");
            
            // Add JSON mode styling
            RemoveFromClassList("json-mode");
            if (data.IsJsonMode && data.CanUseJsonMode())
            {
                AddToClassList("json-mode");
            }
        }

        private void RemoveTypeClasses()
        {
            var typeNames = Enum.GetNames(typeof(ExtendedPrefType));
            foreach (var typeName in typeNames)
            {
                RemoveFromClassList($"type-{typeName.ToLower()}");
            }
        }

        private void StartEdit()
        {
            if (data == null) return;

            displayContainer.style.display = DisplayStyle.None;
            
            // Show appropriate edit container based on JSON mode
            if (data.IsJsonMode && data.CanUseJsonMode())
            {
                jsonEditContainer.style.display = DisplayStyle.Flex;
                editContainer.style.display = DisplayStyle.None;
                jsonEditField.value = data.EditValue;
                jsonEditField.Focus();
                jsonEditField.SelectAll();
            }
            else
            {
                editContainer.style.display = DisplayStyle.Flex;
                jsonEditContainer.style.display = DisplayStyle.None;
                editField.value = data.EditValue;
                editField.Focus();
                editField.SelectAll();
            }

            editButton.style.display = DisplayStyle.None;
            saveButton.style.display = DisplayStyle.Flex;
            cancelButton.style.display = DisplayStyle.Flex;

            AddToClassList("editing");
        }

        private void SaveEdit()
        {
            if (data == null || editor == null) return;

            try
            {
                string valueToSave;
                
                if (data.IsJsonMode && data.CanUseJsonMode())
                {
                    valueToSave = jsonEditField.value;
                    
                    // Validate JSON before saving
                    if (!string.IsNullOrWhiteSpace(valueToSave) && !PlayerPrefEntryData.IsValidJsonString(valueToSave))
                    {
                        EditorUtility.DisplayDialog("Invalid JSON",
                            "The JSON content is not valid. Please fix the syntax errors before saving.",
                            "OK");
                        return;
                    }
                }
                else
                {
                    valueToSave = editField.value;
                }
                
                editor.UpdatePlayerPref(data.Key, valueToSave, data.Type);
                EndEdit();
            }
            catch (Exception ex)
            {
                EditorUtility.DisplayDialog("Error", $"Failed to update PlayerPref: {ex.Message}", "OK");
            }
        }

        private void CancelEdit()
        {
            if (data == null) return;
            editField.value = data.EditValue;
            jsonEditField.value = data.EditValue;
            EndEdit();
        }

        private void EndEdit()
        {
            displayContainer.style.display = DisplayStyle.Flex;
            editContainer.style.display = DisplayStyle.None;
            jsonEditContainer.style.display = DisplayStyle.None;
            editButton.style.display = DisplayStyle.Flex;
            saveButton.style.display = DisplayStyle.None;
            cancelButton.style.display = DisplayStyle.None;

            RemoveFromClassList("editing");
        }

        private void DeleteEntry()
        {
            if (data == null || editor == null) return;

            if (EditorUtility.DisplayDialog("Delete PlayerPref", 
                $"Are you sure you want to delete '{data.Key}'?", 
                "Delete", "Cancel"))
            {
                editor.DeletePlayerPref(data.Key);
            }
        }

        private void OnEditFieldKeyDown(KeyDownEvent evt)
        {
            switch (evt.keyCode)
            {
                case KeyCode.Return:
                case KeyCode.KeypadEnter:
                    if (!evt.shiftKey) // Allow Shift+Enter for new lines in JSON mode
                    {
                        SaveEdit();
                        evt.StopPropagation();
                    }
                    break;
                case KeyCode.Escape:
                    CancelEdit();
                    evt.StopPropagation();
                    break;
            }
        }

        private void ToggleJsonMode()
        {
            if (data == null || !data.CanUseJsonMode()) return;

            data.SetJsonMode(!data.IsJsonMode);
            UpdateDisplay();
        }

        private void UpdateJsonToggleButton()
        {
            if (data == null || !data.CanUseJsonMode()) return;

            jsonToggleButton.text = data.IsJsonMode ? "{}⚙" : "{}";
            
            // Update button styling based on JSON mode
            jsonToggleButton.RemoveFromClassList("json-active");
            jsonToggleButton.RemoveFromClassList("json-inactive");
            
            if (data.IsJsonMode)
            {
                jsonToggleButton.AddToClassList("json-active");
                jsonToggleButton.tooltip = "Switch to text view";
            }
            else
            {
                jsonToggleButton.AddToClassList("json-inactive");
                jsonToggleButton.tooltip = "Switch to JSON view";
            }
        }

        private void UpdateJsonValidation()
        {
            if (data == null || !data.IsJsonMode || !data.CanUseJsonMode())
            {
                jsonErrorLabel.style.display = DisplayStyle.None;
                return;
            }

            if (!string.IsNullOrWhiteSpace(data.JsonError))
            {
                jsonErrorLabel.text = $"JSON Error: {data.JsonError}";
                jsonErrorLabel.style.display = DisplayStyle.Flex;
            }
            else
            {
                jsonErrorLabel.style.display = DisplayStyle.None;
            }
        }
    }

    /// <summary>
    /// Custom ToolbarSearchField with enhanced search capabilities
    /// </summary>
    public class AdvancedSearchField : VisualElement
    {
        private TextField searchField;
        private ToolbarMenu optionsMenu;
        private Toggle regexToggle;
        private Toggle caseSensitiveToggle;
        
        public event Action<string> OnSearchChanged;
        public event Action<PlayerPrefsFilter> OnFilterChanged;

        private PlayerPrefsFilter currentFilter = new PlayerPrefsFilter();

        public string SearchText => searchField?.value ?? "";

        public AdvancedSearchField()
        {
            CreateUI();
            AddToClassList("advanced-search-field");
        }

        private void CreateUI()
        {
            style.flexDirection = FlexDirection.Row;
            style.alignItems = Align.Center;

            // Search field
            searchField = new TextField();
            searchField.style.flexGrow = 1;
            searchField.style.marginRight = 4;
            
            var searchIcon = new VisualElement();
            searchIcon.AddToClassList("search-icon");
            searchField.Add(searchIcon);
            
            searchField.RegisterValueChangedCallback(evt => 
            {
                currentFilter.SearchText = evt.newValue;
                OnSearchChanged?.Invoke(evt.newValue);
                OnFilterChanged?.Invoke(currentFilter);
            });
            
            Add(searchField);

            // Options menu
            optionsMenu = new ToolbarMenu { text = "⚙" };
            optionsMenu.style.width = 30;
            
            // Regex toggle
            optionsMenu.menu.AppendAction("Use Regex", 
                action => 
                {
                    currentFilter.UseRegex = !currentFilter.UseRegex;
                    OnFilterChanged?.Invoke(currentFilter);
                },
                action => currentFilter.UseRegex ? DropdownMenuAction.Status.Checked : DropdownMenuAction.Status.Normal);
            
            // Case sensitive toggle
            optionsMenu.menu.AppendAction("Case Sensitive", 
                action => 
                {
                    currentFilter.CaseSensitive = !currentFilter.CaseSensitive;
                    OnFilterChanged?.Invoke(currentFilter);
                },
                action => currentFilter.CaseSensitive ? DropdownMenuAction.Status.Checked : DropdownMenuAction.Status.Normal);

            Add(optionsMenu);

            // Clear button
            var clearButton = new Button(() => 
            {
                searchField.value = "";
                currentFilter.SearchText = "";
                OnSearchChanged?.Invoke("");
                OnFilterChanged?.Invoke(currentFilter);
            }) { text = "✕" };
            clearButton.style.width = 25;
            clearButton.AddToClassList("clear-button");
            Add(clearButton);
        }

        public void SetPlaceholderText(string placeholder)
        {
            if (searchField != null)
            {
                searchField.value = placeholder;
            }
        }

        public new void Clear()
        {
            if (searchField != null)
            {
                searchField.value = "";
                currentFilter.SearchText = "";
                OnSearchChanged?.Invoke("");
                OnFilterChanged?.Invoke(currentFilter);
            }
        }

        public PlayerPrefsFilter GetCurrentFilter()
        {
            return currentFilter;
        }
    }

    /// <summary>
    /// Custom toolbar with PlayerPrefs-specific actions
    /// </summary>
    public class PlayerPrefsToolbar : Toolbar
    {
        private AdvancedSearchField searchField;
        private DropdownField filterDropdown;
        private Toggle autoRefreshToggle;
        private ToolbarButton refreshButton;
        private ToolbarButton importButton;
        private ToolbarButton exportButton;
        private ToolbarButton deleteAllButton;

        public event Action OnRefresh;
        public event Action OnImport;
        public event Action OnExport;
        public event Action OnDeleteAll;
        public event Action<string> OnSearchChanged;
        public event Action<ExtendedPrefType> OnFilterChanged;
        public event Action<bool> OnAutoRefreshChanged;

        public PlayerPrefsToolbar()
        {
            CreateUI();
            AddToClassList("playerprefs-toolbar");
        }

        private void CreateUI()
        {
            // Search section
            var searchContainer = new VisualElement();
            searchContainer.style.flexDirection = FlexDirection.Row;
            searchContainer.style.alignItems = Align.Center;
            searchContainer.style.flexGrow = 1;
            searchContainer.style.marginRight = 8;

            var searchLabel = new Label("Search:");
            searchLabel.style.marginRight = 4;
            searchContainer.Add(searchLabel);

            searchField = new AdvancedSearchField();
            searchField.style.flexGrow = 1;
            searchField.OnSearchChanged += text => OnSearchChanged?.Invoke(text);
            searchContainer.Add(searchField);

            Add(searchContainer);

            // Filter dropdown
            var filterContainer = new VisualElement();
            filterContainer.style.flexDirection = FlexDirection.Row;
            filterContainer.style.alignItems = Align.Center;
            filterContainer.style.marginRight = 8;

            var filterLabel = new Label("Filter:");
            filterLabel.style.marginRight = 4;
            filterContainer.Add(filterLabel);

            filterDropdown = new DropdownField();
            filterDropdown.style.width = 120;
            var filterNames = Enum.GetNames(typeof(ExtendedPrefType));
            filterDropdown.choices = new System.Collections.Generic.List<string>(filterNames);
            filterDropdown.value = ExtendedPrefType.All.ToString();
            filterDropdown.RegisterValueChangedCallback(evt => 
            {
                if (Enum.TryParse(evt.newValue, out ExtendedPrefType filterType))
                {
                    OnFilterChanged?.Invoke(filterType);
                }
            });
            filterContainer.Add(filterDropdown);

            Add(filterContainer);

            // Auto-refresh toggle
            autoRefreshToggle = new Toggle("Auto Refresh");
            autoRefreshToggle.style.marginRight = 8;
            autoRefreshToggle.RegisterValueChangedCallback(evt => OnAutoRefreshChanged?.Invoke(evt.newValue));
            Add(autoRefreshToggle);

            // Action buttons
            refreshButton = new ToolbarButton(() => OnRefresh?.Invoke()) { text = "Refresh" };
            Add(refreshButton);

            Add(new ToolbarSpacer());

            importButton = new ToolbarButton(() => OnImport?.Invoke()) { text = "Import" };
            Add(importButton);

            exportButton = new ToolbarButton(() => OnExport?.Invoke()) { text = "Export" };
            Add(exportButton);

            Add(new ToolbarSpacer());

            deleteAllButton = new ToolbarButton(() => OnDeleteAll?.Invoke()) { text = "Delete All" };
            deleteAllButton.AddToClassList("delete-all-button");
            Add(deleteAllButton);
        }

        public void SetAutoRefresh(bool enabled)
        {
            if (autoRefreshToggle != null)
            {
                autoRefreshToggle.value = enabled;
            }
        }

        public void SetFilter(ExtendedPrefType filterType)
        {
            if (filterDropdown != null)
            {
                filterDropdown.value = filterType.ToString();
            }
        }

        public void ClearSearch()
        {
            searchField?.Clear();
        }
    }

    /// <summary>
    /// Status bar for displaying information and progress
    /// </summary>
    public class PlayerPrefsStatusBar : VisualElement
    {
        private Label statusLabel;
        private Label countLabel;
        private ProgressBar progressBar;

        public PlayerPrefsStatusBar()
        {
            CreateUI();
            AddToClassList("playerprefs-status-bar");
        }

        private void CreateUI()
        {
            style.flexDirection = FlexDirection.Row;
            style.alignItems = Align.Center;
            style.paddingLeft = 8;
            style.paddingRight = 8;
            style.paddingTop = 4;
            style.paddingBottom = 4;
            style.borderTopWidth = 1;

            statusLabel = new Label("Ready");
            statusLabel.style.flexGrow = 1;
            Add(statusLabel);

            progressBar = new ProgressBar();
            progressBar.style.width = 200;
            progressBar.style.display = DisplayStyle.None;
            Add(progressBar);

            countLabel = new Label("0 items");
            countLabel.style.marginLeft = 8;
            countLabel.AddToClassList("count-label");
            Add(countLabel);
        }

        public void SetStatus(string message, bool isError = false)
        {
            statusLabel.text = message;
            statusLabel.RemoveFromClassList("error");
            statusLabel.RemoveFromClassList("success");
            
            if (isError)
            {
                statusLabel.AddToClassList("error");
            }
            else if (message.Contains("Added") || message.Contains("Updated") || 
                     message.Contains("Deleted") || message.Contains("imported") || 
                     message.Contains("exported"))
            {
                statusLabel.AddToClassList("success");
            }
        }

        public void SetCount(int displayed, int total)
        {
            countLabel.text = displayed == total ? 
                $"{total} items" : 
                $"{displayed} of {total} items";
        }

        public void ShowProgress(float progress, string text = "")
        {
            progressBar.style.display = DisplayStyle.Flex;
            progressBar.value = progress;
            if (!string.IsNullOrEmpty(text))
            {
                progressBar.title = text;
            }
        }

        public void HideProgress()
        {
            progressBar.style.display = DisplayStyle.None;
        }
    }
}