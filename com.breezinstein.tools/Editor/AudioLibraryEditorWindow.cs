using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Breezinstein.Tools.Audio.Editor
{
    /// <summary>
    /// Editor window for managing AudioLibrary assets.
    /// </summary>
    public class AudioLibraryEditorWindow : EditorWindow
    {
        private AudioLibrary selectedLibrary;
        private Vector2 scrollPosition;
        private string searchFilter = "";
        private int categoryFilter = -1; // -1 means "All"
        
        // New clip fields
        private string newClipKey = "";
        private AudioManager.AudioSourceType newClipCategory = AudioManager.AudioSourceType.EFFECT;
        private AudioClip newClipAudio;
        private float newClipVolume = 1f;
        
        // Category names for display
        private readonly string[] categoryNames = { "All", "MUSIC", "EFFECT", "MAIN", "VOICE" };
        private readonly string[] categoryNamesNoAll = { "MUSIC", "EFFECT", "MAIN", "VOICE" };
        
        // Export options
        private bool exportName = true;
        private bool exportCategory = true;
        private bool exportClipName = false;
        private bool exportVolume = false;
        
        // Styles
        private GUIStyle headerStyle;
        private GUIStyle boxStyle;
        private bool stylesInitialized = false;
        
        [MenuItem("Breeze Tools/Audio/Audio Library Editor")]
        public static void ShowWindow()
        {
            var window = GetWindow<AudioLibraryEditorWindow>("Audio Library Editor");
            window.minSize = new Vector2(600, 400);
            window.Show();
        }
        
        private void InitializeStyles()
        {
            if (stylesInitialized) return;
            
            headerStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 14,
                alignment = TextAnchor.MiddleLeft
            };
            
            boxStyle = new GUIStyle("box")
            {
                padding = new RectOffset(10, 10, 10, 10)
            };
            
            stylesInitialized = true;
        }
        
        private void OnGUI()
        {
            InitializeStyles();
            
            EditorGUILayout.Space(5);
            
            // Header Section
            DrawHeaderSection();
            
            EditorGUILayout.Space(5);
            
            if (selectedLibrary == null)
            {
                EditorGUILayout.HelpBox("Please select an AudioLibrary asset or create a new one.", MessageType.Info);
                return;
            }
            
            // Search/Filter Section
            DrawSearchFilterSection();
            
            EditorGUILayout.Space(5);
            
            // Clip List Section
            DrawClipListSection();
            
            EditorGUILayout.Space(5);
            
            // Add New Clip Section
            DrawAddNewClipSection();
            
            EditorGUILayout.Space(5);
            
            // Import/Export Section
            DrawImportExportSection();
            
            EditorGUILayout.Space(5);
            
            // Footer Statistics
            DrawFooterStatistics();
        }
        
        private void DrawHeaderSection()
        {
            EditorGUILayout.BeginHorizontal();
            
            EditorGUILayout.LabelField("Library:", GUILayout.Width(50));
            
            EditorGUI.BeginChangeCheck();
            selectedLibrary = (AudioLibrary)EditorGUILayout.ObjectField(
                selectedLibrary, 
                typeof(AudioLibrary), 
                false
            );
            if (EditorGUI.EndChangeCheck())
            {
                // Reset filters when library changes
                searchFilter = "";
                categoryFilter = -1;
            }
            
            if (GUILayout.Button("Create New", GUILayout.Width(100)))
            {
                CreateNewAudioLibrary();
            }
            
            EditorGUILayout.EndHorizontal();
        }
        
        private void CreateNewAudioLibrary()
        {
            string path = EditorUtility.SaveFilePanelInProject(
                "Create New Audio Library",
                "New Audio Library",
                "asset",
                "Choose a location to save the new Audio Library"
            );
            
            if (!string.IsNullOrEmpty(path))
            {
                AudioLibrary newLibrary = CreateInstance<AudioLibrary>();
                AssetDatabase.CreateAsset(newLibrary, path);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                
                selectedLibrary = newLibrary;
                EditorGUIUtility.PingObject(newLibrary);
            }
        }
        
        private void DrawSearchFilterSection()
        {
            EditorGUILayout.BeginHorizontal();
            
            EditorGUILayout.LabelField("Search:", GUILayout.Width(50));
            searchFilter = EditorGUILayout.TextField(searchFilter, GUILayout.MinWidth(150));
            
            GUILayout.FlexibleSpace();
            
            EditorGUILayout.LabelField("Category:", GUILayout.Width(60));
            categoryFilter = EditorGUILayout.Popup(categoryFilter + 1, categoryNames, GUILayout.Width(100)) - 1;
            
            EditorGUILayout.EndHorizontal();
        }
        
        private void DrawClipListSection()
        {
            // Column headers
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            EditorGUILayout.LabelField("Name", EditorStyles.boldLabel, GUILayout.MinWidth(120));
            EditorGUILayout.LabelField("Category", EditorStyles.boldLabel, GUILayout.Width(80));
            EditorGUILayout.LabelField("Clip", EditorStyles.boldLabel, GUILayout.MinWidth(150));
            EditorGUILayout.LabelField("Volume", EditorStyles.boldLabel, GUILayout.Width(80));
            EditorGUILayout.LabelField("Actions", EditorStyles.boldLabel, GUILayout.Width(60));
            EditorGUILayout.EndHorizontal();
            
            // Scrollable clip list
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.ExpandHeight(true));
            
            if (selectedLibrary.clips == null || selectedLibrary.clips.Count == 0)
            {
                EditorGUILayout.HelpBox("No clips in this library. Add clips using the section below.", MessageType.Info);
            }
            else
            {
                // Get filtered clips
                var filteredClips = GetFilteredClips();
                
                if (filteredClips.Count == 0)
                {
                    EditorGUILayout.HelpBox("No clips match the current filter.", MessageType.Info);
                }
                else
                {
                    string keyToDelete = null;
                    
                    foreach (var kvp in filteredClips)
                    {
                        EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
                        
                        // Name (read-only), with warning if no clip is assigned
                        if (kvp.Value.clip == null)
                        {
                            var warningStyle = new GUIStyle(EditorStyles.label) { normal = { textColor = new Color(0.9f, 0.6f, 0f) } };
                            EditorGUILayout.LabelField("⚠ " + kvp.Key, warningStyle, GUILayout.MinWidth(120));
                        }
                        else
                        {
                            EditorGUILayout.LabelField(kvp.Key, GUILayout.MinWidth(120));
                        }
                        
                        // Category dropdown
                        EditorGUI.BeginChangeCheck();
                        var newCategory = (AudioManager.AudioSourceType)EditorGUILayout.EnumPopup(
                            kvp.Value.category, 
                            GUILayout.Width(80)
                        );
                        if (EditorGUI.EndChangeCheck())
                        {
                            Undo.RecordObject(selectedLibrary, "Change Audio Category");
                            kvp.Value.category = newCategory;
                            EditorUtility.SetDirty(selectedLibrary);
                        }
                        
                        // Clip field
                        EditorGUI.BeginChangeCheck();
                        var newClip = (AudioClip)EditorGUILayout.ObjectField(
                            kvp.Value.clip, 
                            typeof(AudioClip), 
                            false, 
                            GUILayout.MinWidth(150)
                        );
                        if (EditorGUI.EndChangeCheck())
                        {
                            Undo.RecordObject(selectedLibrary, "Change Audio Clip");
                            kvp.Value.clip = newClip;
                            EditorUtility.SetDirty(selectedLibrary);
                        }
                        
                        // Volume slider
                        EditorGUI.BeginChangeCheck();
                        var newVolume = EditorGUILayout.Slider(kvp.Value.volume, 0f, 1f, GUILayout.Width(80));
                        if (EditorGUI.EndChangeCheck())
                        {
                            Undo.RecordObject(selectedLibrary, "Change Audio Volume");
                            kvp.Value.volume = newVolume;
                            EditorUtility.SetDirty(selectedLibrary);
                        }
                        
                        // Play button
                        if (GUILayout.Button("▶", GUILayout.Width(25)))
                        {
                            PlayPreviewClip(kvp.Value.clip);
                        }
                        
                        // Delete button
                        if (GUILayout.Button("✕", GUILayout.Width(25)))
                        {
                            if (EditorUtility.DisplayDialog(
                                "Delete Clip",
                                $"Are you sure you want to delete '{kvp.Key}'?",
                                "Delete",
                                "Cancel"))
                            {
                                keyToDelete = kvp.Key;
                            }
                        }
                        
                        EditorGUILayout.EndHorizontal();
                    }
                    
                    // Handle deletion outside the loop
                    if (keyToDelete != null)
                    {
                        Undo.RecordObject(selectedLibrary, "Delete Audio Clip");
                        selectedLibrary.clips.Remove(keyToDelete);
                        EditorUtility.SetDirty(selectedLibrary);
                    }
                }
            }
            
            EditorGUILayout.EndScrollView();
        }
        
        private List<KeyValuePair<string, AudioItem>> GetFilteredClips()
        {
            var result = new List<KeyValuePair<string, AudioItem>>();
            
            foreach (var kvp in selectedLibrary.clips)
            {
                // Apply search filter
                if (!string.IsNullOrEmpty(searchFilter))
                {
                    if (!kvp.Key.ToLower().Contains(searchFilter.ToLower()))
                    {
                        continue;
                    }
                }
                
                // Apply category filter
                if (categoryFilter >= 0)
                {
                    if ((int)kvp.Value.category != categoryFilter)
                    {
                        continue;
                    }
                }
                
                result.Add(kvp);
            }
            
            return result.OrderBy(x => x.Key).ToList();
        }
        
        private void DrawAddNewClipSection()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            EditorGUILayout.LabelField("+ Add New Clip", headerStyle);
            
            EditorGUILayout.Space(5);
            
            // First row: Key and Category
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Key:", GUILayout.Width(40));
            newClipKey = EditorGUILayout.TextField(newClipKey, GUILayout.MinWidth(150));
            
            GUILayout.FlexibleSpace();
            
            EditorGUILayout.LabelField("Category:", GUILayout.Width(60));
            newClipCategory = (AudioManager.AudioSourceType)EditorGUILayout.EnumPopup(newClipCategory, GUILayout.Width(100));
            EditorGUILayout.EndHorizontal();
            
            // Second row: Clip and Volume
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Clip:", GUILayout.Width(40));
            newClipAudio = (AudioClip)EditorGUILayout.ObjectField(newClipAudio, typeof(AudioClip), false, GUILayout.MinWidth(150));
            
            GUILayout.FlexibleSpace();
            
            EditorGUILayout.LabelField("Volume:", GUILayout.Width(50));
            newClipVolume = EditorGUILayout.Slider(newClipVolume, 0f, 1f, GUILayout.Width(120));
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space(5);
            
            // Validation messages
            bool canAdd = true;
            string validationMessage = "";
            
            if (string.IsNullOrWhiteSpace(newClipKey))
            {
                canAdd = false;
                validationMessage = "Please enter a key name for the clip.";
            }
            else if (selectedLibrary.clips != null && selectedLibrary.clips.ContainsKey(newClipKey))
            {
                canAdd = false;
                validationMessage = $"A clip with the key '{newClipKey}' already exists.";
            }
            else if (newClipAudio == null)
            {
                validationMessage = "No AudioClip assigned — entry will be added without a clip.";
            }
            
            if (!string.IsNullOrEmpty(validationMessage))
            {
                EditorGUILayout.HelpBox(validationMessage, canAdd ? MessageType.Info : MessageType.Warning);
            }
            
            // Add button
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            
            EditorGUI.BeginDisabledGroup(!canAdd);
            if (GUILayout.Button("Add Clip", GUILayout.Width(100)))
            {
                AddNewClip();
            }
            EditorGUI.EndDisabledGroup();
            
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.EndVertical();
        }
        
        private void AddNewClip()
        {
            Undo.RecordObject(selectedLibrary, "Add Audio Clip");
            
            var newItem = new AudioItem
            {
                category = newClipCategory,
                clip = newClipAudio,
                volume = newClipVolume
            };
            
            selectedLibrary.clips.Add(newClipKey, newItem);
            EditorUtility.SetDirty(selectedLibrary);
            
            // Reset fields
            newClipKey = "";
            newClipAudio = null;
            newClipVolume = 1f;
            
            // Focus on key field for quick entry
            GUI.FocusControl(null);
        }
        
        private void DrawFooterStatistics()
        {
            if (selectedLibrary.clips == null)
            {
                return;
            }
            
            int totalCount = selectedLibrary.clips.Count;
            int musicCount = 0;
            int effectCount = 0;
            int mainCount = 0;
            int voiceCount = 0;
            
            foreach (var kvp in selectedLibrary.clips)
            {
                switch (kvp.Value.category)
                {
                    case AudioManager.AudioSourceType.MUSIC:
                        musicCount++;
                        break;
                    case AudioManager.AudioSourceType.EFFECT:
                        effectCount++;
                        break;
                    case AudioManager.AudioSourceType.MAIN:
                        mainCount++;
                        break;
                    case AudioManager.AudioSourceType.VOICE:
                        voiceCount++;
                        break;
                }
            }
            
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            
            EditorGUILayout.LabelField($"Total: {totalCount} clips", GUILayout.Width(100));
            EditorGUILayout.LabelField("|", GUILayout.Width(10));
            EditorGUILayout.LabelField($"Music: {musicCount}", GUILayout.Width(70));
            EditorGUILayout.LabelField("|", GUILayout.Width(10));
            EditorGUILayout.LabelField($"Effects: {effectCount}", GUILayout.Width(80));
            EditorGUILayout.LabelField("|", GUILayout.Width(10));
            EditorGUILayout.LabelField($"Voice: {voiceCount}", GUILayout.Width(70));
            EditorGUILayout.LabelField("|", GUILayout.Width(10));
            EditorGUILayout.LabelField($"Main: {mainCount}", GUILayout.Width(70));
            
            GUILayout.FlexibleSpace();
            
            // Stop all previews button
            if (GUILayout.Button("Stop Preview", GUILayout.Width(90)))
            {
                StopAllPreviewClips();
            }
            
            EditorGUILayout.EndHorizontal();
        }
        
        private void DrawImportExportSection()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            EditorGUILayout.LabelField("Import / Export JSON", headerStyle);
            
            EditorGUILayout.Space(3);
            
            // Export row
            EditorGUILayout.BeginHorizontal();
            
            exportName = EditorGUILayout.ToggleLeft("Name", exportName, GUILayout.Width(80));
            exportCategory = EditorGUILayout.ToggleLeft("Category", exportCategory, GUILayout.Width(80));
            exportClipName = EditorGUILayout.ToggleLeft("Clip Name", exportClipName, GUILayout.Width(100));
            exportVolume = EditorGUILayout.ToggleLeft("Volume", exportVolume, GUILayout.Width(80));
            
            GUILayout.FlexibleSpace();
            
            EditorGUI.BeginDisabledGroup(selectedLibrary == null || selectedLibrary.clips == null || selectedLibrary.clips.Count == 0);
            if (GUILayout.Button("Export to JSON", GUILayout.Width(120)))
            {
                ExportToJson();
            }
            EditorGUI.EndDisabledGroup();
            
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space(3);
            
            // Import row
            EditorGUILayout.BeginHorizontal();
            
            EditorGUILayout.LabelField("Import clips from a JSON file. Audio clips are matched by filename.", EditorStyles.miniLabel);
            
            GUILayout.FlexibleSpace();
            
            if (GUILayout.Button("Import from JSON", GUILayout.Width(120)))
            {
                ImportFromJson();
            }
            
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.EndVertical();
        }
        
        private void ExportToJson()
        {
            if (selectedLibrary == null || selectedLibrary.clips == null || selectedLibrary.clips.Count == 0)
            {
                EditorUtility.DisplayDialog("Export Error", "No clips to export.", "OK");
                return;
            }
            
            // Check if at least one option is selected
            if (!exportName && !exportCategory && !exportClipName && !exportVolume)
            {
                EditorUtility.DisplayDialog("Export Error", "Please select at least one field to export.", "OK");
                return;
            }
            
            string path = EditorUtility.SaveFilePanel(
                "Export Audio Library to JSON",
                "",
                selectedLibrary.name + "_export.json",
                "json"
            );
            
            if (string.IsNullOrEmpty(path))
            {
                return;
            }
            
            var jsonBuilder = new System.Text.StringBuilder();
            jsonBuilder.AppendLine("{");
            jsonBuilder.AppendLine("  \"clips\": [");
            
            var clips = selectedLibrary.clips.ToList();
            for (int i = 0; i < clips.Count; i++)
            {
                var kvp = clips[i];
                jsonBuilder.Append("    {");
                
                var properties = new List<string>();
                
                if (exportName)
                    properties.Add($"\"name\": \"{EscapeJson(kvp.Key)}\"");
                
                if (exportCategory)
                    properties.Add($"\"category\": \"{kvp.Value.category}\"");
                
                if (exportClipName)
                    properties.Add($"\"clipName\": \"{(kvp.Value.clip != null ? EscapeJson(kvp.Value.clip.name) : "null")}\"");
                
                if (exportVolume)
                    properties.Add($"\"volume\": {kvp.Value.volume}");
                
                jsonBuilder.Append(string.Join(", ", properties));
                jsonBuilder.Append("}");
                
                if (i < clips.Count - 1)
                    jsonBuilder.AppendLine(",");
                else
                    jsonBuilder.AppendLine();
            }
            
            jsonBuilder.AppendLine("  ]");
            jsonBuilder.AppendLine("}");
            
            try
            {
                System.IO.File.WriteAllText(path, jsonBuilder.ToString());
                EditorUtility.DisplayDialog("Export Complete", $"Successfully exported {clips.Count} clips to:\n{path}", "OK");
            }
            catch (Exception e)
            {
                EditorUtility.DisplayDialog("Export Error", $"Failed to write file:\n{e.Message}", "OK");
            }
        }
        
        #region JSON Import
        
        [Serializable]
        private class JsonImportRoot
        {
            public string version;
            public JsonImportMapping[] mappings;
            public JsonUnmappedClip[] unmapped_clips;
        }
        
        [Serializable]
        private class JsonImportMapping
        {
            public string clip_name;
            public string category;
            public string audio_path;
        }
        
        [Serializable]
        private class JsonUnmappedClip
        {
            public string name;
            public string category;
        }
        
        private void ImportFromJson()
        {
            string path = EditorUtility.OpenFilePanel(
                "Import Audio Library from JSON",
                "",
                "json"
            );
            
            if (string.IsNullOrEmpty(path))
                return;
            
            string jsonText;
            try
            {
                jsonText = File.ReadAllText(path);
            }
            catch (Exception e)
            {
                EditorUtility.DisplayDialog("Import Error", $"Failed to read file:\n{e.Message}", "OK");
                return;
            }
            
            JsonImportRoot importData;
            try
            {
                importData = JsonUtility.FromJson<JsonImportRoot>(jsonText);
            }
            catch (Exception e)
            {
                EditorUtility.DisplayDialog("Import Error", $"Failed to parse JSON:\n{e.Message}", "OK");
                return;
            }
            
            // Combine mappings and unmapped clips into a single list
            var allEntries = new List<(string clipName, string category, string audioPath)>();
            
            if (importData.mappings != null)
            {
                foreach (var mapping in importData.mappings)
                {
                    allEntries.Add((mapping.clip_name, mapping.category, mapping.audio_path));
                }
            }
            
            if (importData.unmapped_clips != null)
            {
                foreach (var unmapped in importData.unmapped_clips)
                {
                    allEntries.Add((unmapped.name, unmapped.category, null));
                }
            }
            
            if (allEntries.Count == 0)
            {
                EditorUtility.DisplayDialog("Import Error", "No clip entries found in the JSON file.", "OK");
                return;
            }
            
            // Build a lookup cache of all AudioClips in the project by filename (without extension)
            var audioClipCache = BuildAudioClipCache();
            
            Undo.RegisterCompleteObjectUndo(selectedLibrary, "Import Audio Library from JSON");
            
            int addedCount = 0;
            int overwrittenCount = 0;
            int skippedCount = 0;
            int notFoundCount = 0;
            var notFoundNames = new List<string>();
            
            // Track user's "overwrite all" / "skip all" choice for batch operations
            // 0 = ask each time, 1 = overwrite all, 2 = skip all
            int overwritePolicy = 0;
            
            foreach (var entry in allEntries)
            {
                string clipName = entry.clipName;
                string categoryStr = entry.category;
                string audioPath = entry.audioPath;
                
                // Parse category
                AudioManager.AudioSourceType category = ParseCategory(categoryStr);
                
                // Try to find the AudioClip by filename
                AudioClip foundClip = null;
                if (!string.IsNullOrEmpty(audioPath))
                {
                    string fileName = Path.GetFileNameWithoutExtension(audioPath);
                    if (audioClipCache.TryGetValue(fileName.ToLowerInvariant(), out var candidates))
                    {
                        foundClip = candidates[0]; // Use first match
                        if (candidates.Count > 1)
                        {
                            Debug.LogWarning($"[AudioLibrary Import] Multiple AudioClips found for '{fileName}'. Using: {AssetDatabase.GetAssetPath(foundClip)}");
                        }
                    }
                }
                
                // Check if key already exists
                bool exists = selectedLibrary.clips != null && selectedLibrary.clips.ContainsKey(clipName);
                
                if (exists)
                {
                    var existingItem = selectedLibrary.clips[clipName];
                    
                    // Check if the existing entry already matches what we'd import
                    bool clipMatches = existingItem.clip == foundClip;
                    bool categoryMatches = existingItem.category == category;
                    
                    if (clipMatches && categoryMatches)
                    {
                        // Already identical — skip silently
                        skippedCount++;
                    }
                    else
                    {
                        // Build a description of what differs
                        var differences = new List<string>();
                        if (!categoryMatches)
                            differences.Add($"Category: {existingItem.category} → {category}");
                        if (!clipMatches)
                        {
                            string existingClipName = existingItem.clip != null ? existingItem.clip.name : "(none)";
                            string newClipName = foundClip != null ? foundClip.name : "(none)";
                            differences.Add($"Clip: {existingClipName} → {newClipName}");
                        }
                        string diffDescription = string.Join("\n", differences);
                        
                        bool shouldOverwrite = false;
                        
                        if (overwritePolicy == 1)
                        {
                            shouldOverwrite = true;
                        }
                        else if (overwritePolicy == 2)
                        {
                            shouldOverwrite = false;
                        }
                        else
                        {
                            // Ask the user, showing what's different
                            // DisplayDialogComplex: ok=0, cancel=1, alt=2
                            int choice = EditorUtility.DisplayDialogComplex(
                                "Clip Already Exists",
                                $"The clip '{clipName}' already exists in the library with different values:\n\n{diffDescription}\n\nWhat would you like to do?",
                                "Overwrite",       // option 0 (ok)
                                "Skip",            // option 1 (cancel)
                                "Skip All"         // option 2 (alt)
                            );
                            
                            switch (choice)
                            {
                                case 0: // Overwrite
                                    shouldOverwrite = true;
                                    break;
                                case 1: // Skip (just this one)
                                    shouldOverwrite = false;
                                    break;
                                case 2: // Skip All
                                    shouldOverwrite = false;
                                    overwritePolicy = 2;
                                    break;
                            }
                        }
                        
                        if (shouldOverwrite)
                        {
                            // Modify the existing AudioItem in-place (it's a class/reference type)
                            existingItem.category = category;
                            existingItem.clip = foundClip;
                            existingItem.volume = 1f;
                            overwrittenCount++;
                        }
                        else
                        {
                            skippedCount++;
                        }
                    }
                }
                else
                {
                    // Add new entry
                    var newItem = new AudioItem
                    {
                        category = category,
                        clip = foundClip,
                        volume = 1f
                    };
                    selectedLibrary.clips.Add(clipName, newItem);
                    addedCount++;
                }
                
                if (foundClip == null && !string.IsNullOrEmpty(audioPath))
                {
                    notFoundCount++;
                    notFoundNames.Add($"  • {clipName} ({Path.GetFileName(audioPath)})");
                }
            }
            
            EditorUtility.SetDirty(selectedLibrary);
            
            // Force serialization update via SerializedObject
            var serializedObj = new SerializedObject(selectedLibrary);
            serializedObj.Update();
            serializedObj.ApplyModifiedProperties();
            
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            // Build result message
            var resultMsg = new System.Text.StringBuilder();
            resultMsg.AppendLine($"Import complete!\n");
            resultMsg.AppendLine($"Added: {addedCount}");
            resultMsg.AppendLine($"Overwritten: {overwrittenCount}");
            resultMsg.AppendLine($"Skipped (duplicates): {skippedCount}");
            
            if (notFoundCount > 0)
            {
                resultMsg.AppendLine($"\nAudio files not found: {notFoundCount}");
                // Show up to 15 not-found entries
                int showCount = Math.Min(notFoundNames.Count, 15);
                for (int i = 0; i < showCount; i++)
                {
                    resultMsg.AppendLine(notFoundNames[i]);
                }
                if (notFoundNames.Count > showCount)
                {
                    resultMsg.AppendLine($"  ... and {notFoundNames.Count - showCount} more.");
                }
            }
            
            EditorUtility.DisplayDialog("Import Results", resultMsg.ToString(), "OK");
            
            Debug.Log($"[AudioLibrary Import] Added: {addedCount}, Overwritten: {overwrittenCount}, Skipped: {skippedCount}, Clips not found: {notFoundCount}");
        }
        
        /// <summary>
        /// Builds a dictionary mapping lowercase audio filenames (without extension) to lists of AudioClip assets.
        /// </summary>
        private Dictionary<string, List<AudioClip>> BuildAudioClipCache()
        {
            var cache = new Dictionary<string, List<AudioClip>>();
            
            string[] guids = AssetDatabase.FindAssets("t:AudioClip");
            
            foreach (string guid in guids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                string fileName = Path.GetFileNameWithoutExtension(assetPath).ToLowerInvariant();
                
                AudioClip clip = AssetDatabase.LoadAssetAtPath<AudioClip>(assetPath);
                if (clip == null) continue;
                
                if (!cache.ContainsKey(fileName))
                {
                    cache[fileName] = new List<AudioClip>();
                }
                cache[fileName].Add(clip);
            }
            
            return cache;
        }
        
        /// <summary>
        /// Parses a category string (e.g. "EFFECT", "MUSIC") into an AudioSourceType enum value.
        /// </summary>
        private AudioManager.AudioSourceType ParseCategory(string categoryStr)
        {
            if (string.IsNullOrEmpty(categoryStr))
                return AudioManager.AudioSourceType.EFFECT;
            
            switch (categoryStr.ToUpperInvariant())
            {
                case "MUSIC":
                    return AudioManager.AudioSourceType.MUSIC;
                case "EFFECT":
                    return AudioManager.AudioSourceType.EFFECT;
                case "MAIN":
                    return AudioManager.AudioSourceType.MAIN;
                case "VOICE":
                    return AudioManager.AudioSourceType.VOICE;
                default:
                    Debug.LogWarning($"[AudioLibrary Import] Unknown category '{categoryStr}', defaulting to EFFECT.");
                    return AudioManager.AudioSourceType.EFFECT;
            }
        }
        
        #endregion
        
        private string EscapeJson(string text)
        {
            return text.Replace("\\", "\\\\")
                       .Replace("\"", "\\\"")
                       .Replace("\n", "\\n")
                       .Replace("\r", "\\r")
                       .Replace("\t", "\\t");
        }
        
        #region Audio Preview
        
        private void PlayPreviewClip(AudioClip clip)
        {
            if (clip == null)
            {
                Debug.LogWarning("Cannot preview: AudioClip is null");
                return;
            }
            
            StopAllPreviewClips();
            
            try
            {
                var audioUtilType = typeof(AudioImporter).Assembly.GetType("UnityEditor.AudioUtil");
                if (audioUtilType != null)
                {
                    var playMethod = audioUtilType.GetMethod(
                        "PlayPreviewClip",
                        BindingFlags.Static | BindingFlags.Public,
                        null,
                        new Type[] { typeof(AudioClip), typeof(int), typeof(bool) },
                        null
                    );
                    
                    if (playMethod != null)
                    {
                        playMethod.Invoke(null, new object[] { clip, 0, false });
                        
                        // Schedule cleanup after clip duration
                        float clipLength = clip.length;
                        EditorApplication.delayCall += () =>
                        {
                            // This will be called after the editor updates
                            // We use a coroutine-like approach with delayCall
                            ScheduleStopPreview(clipLength);
                        };
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to play preview clip: {e.Message}");
            }
        }
        
        private void ScheduleStopPreview(float delay)
        {
            double startTime = EditorApplication.timeSinceStartup;
            EditorApplication.CallbackFunction callback = null;
            
            callback = () =>
            {
                if (EditorApplication.timeSinceStartup - startTime >= delay)
                {
                    StopAllPreviewClips();
                    EditorApplication.update -= callback;
                }
            };
            
            EditorApplication.update += callback;
        }
        
        private void StopAllPreviewClips()
        {
            try
            {
                var audioUtilType = typeof(AudioImporter).Assembly.GetType("UnityEditor.AudioUtil");
                if (audioUtilType != null)
                {
                    var stopMethod = audioUtilType.GetMethod(
                        "StopAllPreviewClips",
                        BindingFlags.Static | BindingFlags.Public
                    );
                    
                    if (stopMethod != null)
                    {
                        stopMethod.Invoke(null, null);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to stop preview clips: {e.Message}");
            }
        }
        
        #endregion
        
        private void OnDisable()
        {
            // Stop any playing previews when window is closed
            StopAllPreviewClips();
        }
        
        private void OnDestroy()
        {
            // Ensure previews are stopped when window is destroyed
            StopAllPreviewClips();
        }
    }
}
