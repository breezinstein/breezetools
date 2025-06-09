using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System;

namespace Breezinstein.Tools
{
    public class PlayerPrefsEditor : EditorWindow
    {
        private enum PrefType { Int, Float, String }
        private PrefType selectedType = PrefType.Int;
        private string newKey = "";
        private string newValue = "";
        private int newIntValue = 0;
        private float newFloatValue = 0f;

        private Vector2 scrollPosition;

        // PlayerPrefs data storage
        private List<PlayerPrefEntry> playerPrefsEntries = new List<PlayerPrefEntry>();
        private string searchKey = "";

        private class PlayerPrefEntry
        {
            public string Key { get; set; }
            public object Value { get; set; }
            public PrefType Type { get; set; }
            public bool IsEditing { get; set; }
            public string EditValue { get; set; }

            public PlayerPrefEntry(string key, object value, PrefType type)
            {
                Key = key;
                Value = value;
                Type = type;
                IsEditing = false;
                EditValue = value.ToString();
            }
        }

        [MenuItem("Breeze Tools/PlayerPrefs Editor")]
        public static void ShowWindow()
        {
            GetWindow<PlayerPrefsEditor>("PlayerPrefs Editor");
        }

        private void OnEnable()
        {
            LoadPlayerPrefs();
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("PlayerPrefs Manager", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            // Section: Add New PlayerPref
            EditorGUILayout.LabelField("Add New PlayerPref", EditorStyles.boldLabel);
            newKey = EditorGUILayout.TextField("Key", newKey);
            selectedType = (PrefType)EditorGUILayout.EnumPopup("Type", selectedType);

            switch (selectedType)
            {
                case PrefType.Int:
                    newIntValue = EditorGUILayout.IntField("Value", newIntValue);
                    newValue = newIntValue.ToString();
                    break;
                case PrefType.Float:
                    newFloatValue = EditorGUILayout.FloatField("Value", newFloatValue);
                    newValue = newFloatValue.ToString();
                    break;
                case PrefType.String:
                    newValue = EditorGUILayout.TextField("Value", newValue);
                    break;
            }

            if (GUILayout.Button("Add PlayerPref"))
            {
                if (!string.IsNullOrEmpty(newKey))
                {
                    AddPlayerPref(newKey, newValue, selectedType);
                    newKey = ""; // Reset fields
                    newValue = "";
                    newIntValue = 0;
                    newFloatValue = 0f;
                    GUI.FocusControl(null); // Remove focus from text fields
                }
                else
                {
                    ShowNotification(new GUIContent("Key cannot be empty."));
                }
            }

            EditorGUILayout.Space();
            EditorGUILayout.Separator();
            EditorGUILayout.Space();

            // Section: View/Edit/Delete PlayerPrefs
            EditorGUILayout.LabelField("Existing PlayerPrefs", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            searchKey = EditorGUILayout.TextField("Search Key", searchKey, GUILayout.ExpandWidth(true));
            if (GUILayout.Button("Clear Search", GUILayout.Width(100)))
            {
                searchKey = "";
                GUI.FocusControl(null);
            }
            EditorGUILayout.EndHorizontal();


            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            if (playerPrefsEntries.Count == 0)
            {
                EditorGUILayout.HelpBox("No PlayerPrefs found or loaded. Add some or refresh.", MessageType.Info);
            }
            else
            {
                List<PlayerPrefEntry> filteredEntries = string.IsNullOrEmpty(searchKey)
                    ? playerPrefsEntries
                    : playerPrefsEntries.FindAll(entry => entry.Key.ToLower().Contains(searchKey.ToLower()));

                if (filteredEntries.Count == 0 && !string.IsNullOrEmpty(searchKey))
                {
                    EditorGUILayout.HelpBox($"No PlayerPrefs found with key containing '{searchKey}'.", MessageType.Info);
                }

                for (int i = 0; i < filteredEntries.Count; i++)
                {
                    PlayerPrefEntry entry = filteredEntries[i];
                    EditorGUILayout.BeginHorizontal();

                    if (entry.IsEditing)
                    {
                        EditorGUILayout.LabelField(entry.Key, GUILayout.Width(150));
                        entry.EditValue = EditorGUILayout.TextField(entry.EditValue, GUILayout.ExpandWidth(true));
                        if (GUILayout.Button("Save", GUILayout.Width(60)))
                        {
                            TryUpdatePlayerPref(entry);
                            entry.IsEditing = false;
                            GUI.FocusControl(null);
                        }
                        if (GUILayout.Button("Cancel", GUILayout.Width(60)))
                        {
                            entry.IsEditing = false;
                            entry.EditValue = entry.Value.ToString(); // Reset edit value
                            GUI.FocusControl(null);
                        }
                    }
                    else
                    {
                        EditorGUILayout.LabelField(new GUIContent(entry.Key, $"Type: {entry.Type}"), GUILayout.Width(150));
                        EditorGUILayout.SelectableLabel(entry.Value.ToString(), EditorStyles.textField, GUILayout.ExpandWidth(true), GUILayout.Height(EditorGUIUtility.singleLineHeight));

                        if (GUILayout.Button("Edit", GUILayout.Width(60)))
                        {
                            // Ensure only one item is in edit mode at a time for simplicity
                            playerPrefsEntries.ForEach(e => e.IsEditing = false);
                            entry.IsEditing = true;
                            entry.EditValue = entry.Value.ToString(); // Initialize edit field
                        }
                    }

                    if (GUILayout.Button("Delete", GUILayout.Width(60)))
                    {
                        if (EditorUtility.DisplayDialog("Delete PlayerPref", $"Are you sure you want to delete '{entry.Key}'?", "Delete", "Cancel"))
                        {
                            DeletePlayerPref(entry.Key);
                            // No need to manually remove from list here, LoadPlayerPrefs will refresh
                        }
                    }
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.Separator();
                }
            }
            EditorGUILayout.EndScrollView();

            EditorGUILayout.Space();

            // Section: Actions
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Refresh List"))
            {
                LoadPlayerPrefs();
                ShowNotification(new GUIContent("PlayerPrefs list refreshed."));
            }

            if (GUILayout.Button("Delete All PlayerPrefs", GUILayout.Height(30)))
            {
                if (EditorUtility.DisplayDialog("Delete All PlayerPrefs", "Are you sure you want to delete ALL PlayerPrefs? This action cannot be undone.", "DELETE ALL", "Cancel"))
                {
                    PlayerPrefs.DeleteAll();
                    PlayerPrefs.Save();
                    LoadPlayerPrefs(); // Refresh the list
                    ShowNotification(new GUIContent("All PlayerPrefs deleted."));
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        private void AddPlayerPref(string key, string value, PrefType type)
        {
            switch (type)
            {
                case PrefType.Int:
                    if (int.TryParse(value, out int intVal))
                    {
                        PlayerPrefs.SetInt(key, intVal);
                    }
                    else
                    {
                        ShowNotification(new GUIContent($"Invalid integer value for key '{key}'."));
                        return;
                    }
                    break;
                case PrefType.Float:
                    if (float.TryParse(value, out float floatVal))
                    {
                        PlayerPrefs.SetFloat(key, floatVal);
                    }
                    else
                    {
                        ShowNotification(new GUIContent($"Invalid float value for key '{key}'."));
                        return;
                    }
                    break;
                case PrefType.String:
                    PlayerPrefs.SetString(key, value);
                    break;
            }
            PlayerPrefs.Save();
            LoadPlayerPrefs(); // Refresh list
            ShowNotification(new GUIContent($"PlayerPref '{key}' added/updated."));
        }

        private void TryUpdatePlayerPref(PlayerPrefEntry entry)
        {
            switch (entry.Type)
            {
                case PrefType.Int:
                    if (int.TryParse(entry.EditValue, out int intVal))
                    {
                        PlayerPrefs.SetInt(entry.Key, intVal);
                        entry.Value = intVal;
                    }
                    else
                    {
                        ShowNotification(new GUIContent($"Invalid integer value for key '{entry.Key}'. Update failed."));
                        return;
                    }
                    break;
                case PrefType.Float:
                    if (float.TryParse(entry.EditValue, out float floatVal))
                    {
                        PlayerPrefs.SetFloat(entry.Key, floatVal);
                        entry.Value = floatVal;
                    }
                    else
                    {
                        ShowNotification(new GUIContent($"Invalid float value for key '{entry.Key}'. Update failed."));
                        return;
                    }
                    break;
                case PrefType.String:
                    PlayerPrefs.SetString(entry.Key, entry.EditValue);
                    entry.Value = entry.EditValue;
                    break;
            }
            PlayerPrefs.Save();
            ShowNotification(new GUIContent($"PlayerPref '{entry.Key}' updated."));
            // No need to call LoadPlayerPrefs() here as we updated the entry in place
        }


        private void DeletePlayerPref(string key)
        {
            PlayerPrefs.DeleteKey(key);
            PlayerPrefs.Save();
            LoadPlayerPrefs(); // Refresh list
            ShowNotification(new GUIContent($"PlayerPref '{key}' deleted."));
        }

        // This is a simplified way to load PlayerPrefs.
        // Unity does not provide a way to get all keys directly.
        // This method relies on knowing the keys or having them stored elsewhere if you need a comprehensive list.
        // For a true "view all", you'd need to iterate through known keys or use platform-specific methods (e.g., registry on Windows, plist on macOS/iOS).
        // This editor will only show keys it knows about (e.g. added through this editor or common ones).
        // A more robust solution would involve saving a list of all PlayerPref keys managed by the game into another PlayerPref string.
        private void LoadPlayerPrefs()
        {
            playerPrefsEntries.Clear();
            // This is a common workaround: save a list of all keys in a separate PlayerPref.
            // For this example, we'll just demonstrate with a few known keys from the project.
            // In a real project, you would need a more robust way to track all PlayerPrefs keys.

            // Attempt to load keys that might exist from the provided scripts
            string[] knownKeys = {
            "AdsEnabled", "sfxEnabled", "bgmEnabled", "PlayerCurrency", "UnlockedItemsProgression_V1",
            // Add keys for QuestionLibrary stats if they follow a predictable pattern
            // For example, if saveID is "MyGameStats":
            // "MyGameStats_easy", "MyGameStats_medium", "MyGameStats_hard", "MyGameStats_marathon"
            // Since we don't know the saveID for QuestionLibrary, we can't list them generically here.
            // Also, PlayerPrefs are often dynamically generated (e.g., based on level names, item IDs).
        };

            foreach (string key in knownKeys)
            {
                if (PlayerPrefs.HasKey(key))
                {
                    // Try to determine type. This is imperfect.
                    // If it can be parsed as int, assume int. Then float. Else string.
                    string strVal = PlayerPrefs.GetString(key, ""); // Get as string first
                    if (int.TryParse(strVal, out int intVal) && PlayerPrefs.GetInt(key) == intVal) // Double check with GetInt
                    {
                        playerPrefsEntries.Add(new PlayerPrefEntry(key, PlayerPrefs.GetInt(key), PrefType.Int));
                    }
                    else if (float.TryParse(strVal, out float floatVal) && Mathf.Approximately(PlayerPrefs.GetFloat(key), floatVal)) // Double check
                    {
                        playerPrefsEntries.Add(new PlayerPrefEntry(key, PlayerPrefs.GetFloat(key), PrefType.Float));
                    }
                    else
                    {
                        playerPrefsEntries.Add(new PlayerPrefEntry(key, PlayerPrefs.GetString(key), PrefType.String));
                    }
                }
            }
            // It's impossible to get *all* PlayerPrefs keys without knowing them beforehand or using native OS calls.
            // This editor will primarily manage keys added/known through its interface or common game keys.
            // For a more complete view, you would need to modify your game to store a list of all PlayerPrefs keys it uses.
            // For example, whenever a PlayerPref is set, add its key to a comma-separated string in another PlayerPref.

            // Sort entries by key for consistent display
            playerPrefsEntries.Sort((a, b) => a.Key.CompareTo(b.Key));

            Repaint(); // Refresh the window display
        }
    }
}