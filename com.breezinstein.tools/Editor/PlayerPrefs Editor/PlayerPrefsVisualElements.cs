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
        private Button editButton;
        private Button saveButton;
        private Button cancelButton;
        private Button deleteButton;
        private VisualElement editContainer;
        private VisualElement displayContainer;

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
            Add(displayContainer);

            // Edit container (hidden by default)
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

            // Setup keyboard events for edit field
            editField.RegisterCallback<KeyDownEvent>(OnEditFieldKeyDown);
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

            // Add type-specific styling
            RemoveTypeClasses();
            AddToClassList($"type-{data.Type.ToString().ToLower()}");
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
            editContainer.style.display = DisplayStyle.Flex;
            editButton.style.display = DisplayStyle.None;
            saveButton.style.display = DisplayStyle.Flex;
            cancelButton.style.display = DisplayStyle.Flex;

            editField.value = data.EditValue;
            editField.Focus();
            editField.SelectAll();

            AddToClassList("editing");
        }

        private void SaveEdit()
        {
            if (data == null || editor == null) return;

            try
            {
                editor.UpdatePlayerPref(data.Key, editField.value, data.Type);
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
            EndEdit();
        }

        private void EndEdit()
        {
            displayContainer.style.display = DisplayStyle.Flex;
            editContainer.style.display = DisplayStyle.None;
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
                    SaveEdit();
                    evt.StopPropagation();
                    break;
                case KeyCode.Escape:
                    CancelEdit();
                    evt.StopPropagation();
                    break;
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

        public void Clear()
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