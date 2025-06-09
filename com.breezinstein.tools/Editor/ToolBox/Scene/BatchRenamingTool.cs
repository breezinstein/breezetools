using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Breezinstein.Tools.Core;

namespace Breezinstein.Tools.Scene
{    public class BatchRenamingTool : BaseBreezeTool
    {
        public override string ToolName => "Batch Renaming";
        public override string ToolDescription => "Powerful renamer with regex, numbering, and case conversion";
        public override string Category => "Scene Organization";
        public override int Priority => 10;
        
        private string _findPattern = "";
        private string _replacePattern = "";
        private bool _useRegex = false;
        private string _prefix = "";
        private string _suffix = "";
        private bool _addNumbering = false;
        private int _startNumber = 1;
        private int _numberPadding = 2;
        private CaseConversion _caseConversion = CaseConversion.None;
        private bool _includeChildren = false;
        
        private Label _previewLabel;
        
        private enum CaseConversion
        {
            None,
            Uppercase,
            Lowercase,
            TitleCase,
            CamelCase,
            PascalCase,
            SnakeCase,
            KebabCase
        }
        
        public override VisualElement CreateUI()
        {
            var container = new VisualElement();
            container.AddToClassList("tool-section");
            
            CreateFindReplaceSection(container);
            CreatePrefixSuffixSection(container);
            CreateNumberingSection(container);
            CreateCaseSection(container);
            CreateOptionsSection(container);
            CreatePreviewSection(container);
            CreateActionButtons(container);
            
            return container;
        }
        
        private void CreateFindReplaceSection(VisualElement parent)
        {
            var section = new VisualElement();
            section.AddToClassList("tool-section");
            
            var header = new Label("Find & Replace");
            header.AddToClassList("tool-section-header");
            section.Add(header);
            
            var findField = new TextField("Find");
            findField.value = _findPattern;
            findField.RegisterValueChangedCallback(evt => _findPattern = evt.newValue);
            section.Add(findField);
            
            var replaceField = new TextField("Replace");
            replaceField.value = _replacePattern;
            replaceField.RegisterValueChangedCallback(evt => _replacePattern = evt.newValue);
            section.Add(replaceField);
            
            var regexToggle = new Toggle("Use Regular Expressions");
            regexToggle.value = _useRegex;
            regexToggle.RegisterValueChangedCallback(evt => _useRegex = evt.newValue);
            section.Add(regexToggle);
            
            parent.Add(section);
        }
        
        private void CreatePrefixSuffixSection(VisualElement parent)
        {
            var section = new VisualElement();
            section.AddToClassList("tool-section");
            
            var header = new Label("Prefix & Suffix");
            header.AddToClassList("tool-section-header");
            section.Add(header);
            
            var prefixField = new TextField("Prefix");
            prefixField.value = _prefix;
            prefixField.RegisterValueChangedCallback(evt => _prefix = evt.newValue);
            section.Add(prefixField);
            
            var suffixField = new TextField("Suffix");
            suffixField.value = _suffix;
            suffixField.RegisterValueChangedCallback(evt => _suffix = evt.newValue);
            section.Add(suffixField);
            
            parent.Add(section);
        }
        
        private void CreateNumberingSection(VisualElement parent)
        {
            var section = new VisualElement();
            section.AddToClassList("tool-section");
            
            var header = new Label("Sequential Numbering");
            header.AddToClassList("tool-section-header");
            section.Add(header);
            
            var numberingToggle = new Toggle("Add Sequential Numbers");
            numberingToggle.value = _addNumbering;
            numberingToggle.RegisterValueChangedCallback(evt => _addNumbering = evt.newValue);
            section.Add(numberingToggle);
            
            var startField = new IntegerField("Start Number");
            startField.value = _startNumber;
            startField.RegisterValueChangedCallback(evt => _startNumber = evt.newValue);
            section.Add(startField);
            
            var paddingField = new IntegerField("Number Padding");
            paddingField.value = _numberPadding;
            paddingField.RegisterValueChangedCallback(evt => _numberPadding = Mathf.Max(1, evt.newValue));
            section.Add(paddingField);
            
            parent.Add(section);
        }
        
        private void CreateCaseSection(VisualElement parent)
        {
            var section = new VisualElement();
            section.AddToClassList("tool-section");
            
            var header = new Label("Case Conversion");
            header.AddToClassList("tool-section-header");
            section.Add(header);
            
            var caseField = new EnumField("Case Style", _caseConversion);
            caseField.RegisterValueChangedCallback(evt => _caseConversion = (CaseConversion)evt.newValue);
            section.Add(caseField);
            
            parent.Add(section);
        }
        
        private void CreateOptionsSection(VisualElement parent)
        {
            var section = new VisualElement();
            section.AddToClassList("tool-section");
            
            var header = new Label("Options");
            header.AddToClassList("tool-section-header");
            section.Add(header);
            
            var childrenToggle = new Toggle("Include Children");
            childrenToggle.value = _includeChildren;
            childrenToggle.RegisterValueChangedCallback(evt => _includeChildren = evt.newValue);
            section.Add(childrenToggle);
            
            parent.Add(section);
        }
        
        private void CreatePreviewSection(VisualElement parent)
        {
            var section = new VisualElement();
            section.AddToClassList("preview-area");
            
            var header = new Label("Preview");
            header.AddToClassList("tool-section-header");
            section.Add(header);
              var previewLabel = new Label("Select objects to see preview");
            previewLabel.name = "preview-label";
            previewLabel.style.whiteSpace = WhiteSpace.Normal;
            _previewLabel = previewLabel;
            section.Add(previewLabel);
            
            parent.Add(section);
            
            // Update preview when selection changes
            Selection.selectionChanged += UpdatePreview;
        }
        
        private void CreateActionButtons(VisualElement parent)
        {
            var section = new VisualElement();
            section.AddToClassList("tool-section");
            
            var buttonRow = new VisualElement();
            buttonRow.AddToClassList("button-row");
            
            buttonRow.Add(new Button(() => UpdatePreview()) { text = "Update Preview" });
            buttonRow.Add(new Button(() => ApplyRenaming()) { text = "Apply Renaming" });
            
            section.Add(buttonRow);
            parent.Add(section);
        }
        
        public override bool CanExecute()
        {
            return Selection.transforms.Length > 0;
        }
        
        private void UpdatePreview()
        {
            var selectedObjects = GetTargetObjects();
            if (selectedObjects.Length == 0) return;
            
            var previewText = "Preview:\n";
            
            for (int i = 0; i < selectedObjects.Length && i < 10; i++) // Limit preview to 10 items
            {
                var obj = selectedObjects[i];
                var newName = GenerateNewName(obj.name, i);
                previewText += $"{obj.name} → {newName}\n";
            }
            
            if (selectedObjects.Length > 10)
            {
                previewText += $"... and {selectedObjects.Length - 10} more";
            }
              // Find and update preview label
            if (_previewLabel != null)
            {
                _previewLabel.text = previewText;
            }
        }
        
        private Transform[] GetTargetObjects()
        {
            var selectedTransforms = Selection.transforms;
            if (!_includeChildren) return selectedTransforms;
            
            var allTargets = new List<Transform>();
            foreach (var transform in selectedTransforms)
            {
                allTargets.Add(transform);
                allTargets.AddRange(transform.GetComponentsInChildren<Transform>().Skip(1));
            }
            
            return allTargets.ToArray();
        }
        
        private void ApplyRenaming()
        {
            var targetObjects = GetTargetObjects();
            if (targetObjects.Length == 0) return;
            
            using (var undoScope = new UndoScope("Batch Rename", targetObjects.Cast<Object>().ToArray()))
            {
                for (int i = 0; i < targetObjects.Length; i++)
                {
                    var obj = targetObjects[i];
                    var newName = GenerateNewName(obj.name, i);
                    obj.name = newName;
                }            }
            
            LogOperation($"Renamed {targetObjects.Length} objects");
            UpdatePreview();
        }
        
        private string GenerateNewName(string originalName, int index)
        {
            var result = originalName;
            
            // Apply find & replace
            if (!string.IsNullOrEmpty(_findPattern))
            {
                if (_useRegex)
                {
                    try
                    {
                        result = Regex.Replace(result, _findPattern, _replacePattern);
                    }                    catch (System.ArgumentException ex)
                    {
                        Debug.LogError($"Invalid regex pattern: {ex.Message}");
                        return originalName;
                    }
                }
                else
                {
                    result = result.Replace(_findPattern, _replacePattern);
                }
            }
            
            // Apply case conversion
            result = ApplyCaseConversion(result);
            
            // Add prefix
            if (!string.IsNullOrEmpty(_prefix))
            {
                result = _prefix + result;
            }
            
            // Add suffix
            if (!string.IsNullOrEmpty(_suffix))
            {
                result = result + _suffix;
            }
            
            // Add numbering
            if (_addNumbering)
            {
                var number = (_startNumber + index).ToString().PadLeft(_numberPadding, '0');
                result = result + "_" + number;
            }
            
            return result;
        }
        
        private string ApplyCaseConversion(string input)
        {
            switch (_caseConversion)
            {
                case CaseConversion.Uppercase:
                    return input.ToUpper();
                    
                case CaseConversion.Lowercase:
                    return input.ToLower();
                    
                case CaseConversion.TitleCase:
                    return System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(input.ToLower());
                    
                case CaseConversion.CamelCase:
                    return ToCamelCase(input);
                    
                case CaseConversion.PascalCase:
                    return ToPascalCase(input);
                    
                case CaseConversion.SnakeCase:
                    return ToSnakeCase(input);
                    
                case CaseConversion.KebabCase:
                    return ToKebabCase(input);
                    
                default:
                    return input;
            }
        }
        
        private string ToCamelCase(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;
            
            var words = Regex.Split(input, @"[\s_-]+");
            var result = words[0].ToLower();
            
            for (int i = 1; i < words.Length; i++)
            {
                if (!string.IsNullOrEmpty(words[i]))
                {
                    result += char.ToUpper(words[i][0]) + words[i].Substring(1).ToLower();
                }
            }
            
            return result;
        }
        
        private string ToPascalCase(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;
            
            var words = Regex.Split(input, @"[\s_-]+");
            var result = "";
            
            foreach (var word in words)
            {
                if (!string.IsNullOrEmpty(word))
                {
                    result += char.ToUpper(word[0]) + word.Substring(1).ToLower();
                }
            }
            
            return result;
        }
        
        private string ToSnakeCase(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;
            
            // Insert underscores before capital letters (except the first character)
            var result = Regex.Replace(input, @"(?<!^)(?=[A-Z])", "_");
            
            // Replace spaces and hyphens with underscores
            result = Regex.Replace(result, @"[\s-]+", "_");
            
            // Remove multiple consecutive underscores
            result = Regex.Replace(result, @"_+", "_");
            
            return result.ToLower();
        }
        
        private string ToKebabCase(string input)
        {
            return ToSnakeCase(input).Replace('_', '-');
        }
        
        public override void OnToolDeselected()
        {
            Selection.selectionChanged -= UpdatePreview;
        }
    }
}
