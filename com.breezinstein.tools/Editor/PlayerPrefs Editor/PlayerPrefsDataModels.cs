using UnityEngine;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Breezinstein.Tools
{
    /// <summary>
    /// Extended PlayerPrefs types including all PlayerPrefsX functionality
    /// </summary>
    public enum ExtendedPrefType
    {
        All,
        String,
        Int,
        Float,
        Bool,
        Long,
        Vector2,
        Vector3,
        Quaternion,
        Color,
        IntArray,
        FloatArray,
        BoolArray,
        StringArray,
        Vector2Array,
        Vector3Array,
        QuaternionArray,
        ColorArray
    }

    /// <summary>
    /// Data structure representing a PlayerPrefs entry with extended type support
    /// </summary>
    [Serializable]
    public class PlayerPrefEntryData
    {
        public string Key { get; set; }
        public object Value { get; set; }
        public ExtendedPrefType Type { get; set; }
        public bool IsEditing { get; set; }
        public string EditValue { get; set; }
        public DateTime LastModified { get; set; }
        public string DisplayValue { get; private set; }
        public string TypeDisplayName { get; private set; }

        public PlayerPrefEntryData(string key, object value, ExtendedPrefType type)
        {
            Key = key;
            Value = value;
            Type = type;
            IsEditing = false;
            LastModified = DateTime.Now;
            UpdateDisplayValues();
        }

        public void UpdateDisplayValues()
        {
            EditValue = FormatValueForEditing(Value, Type);
            DisplayValue = FormatValueForDisplay(Value, Type);
            TypeDisplayName = GetTypeDisplayName(Type);
        }

        private string FormatValueForEditing(object value, ExtendedPrefType type)
        {
            if (value == null) return "";

            switch (type)
            {
                case ExtendedPrefType.Vector2:
                    var v2 = (Vector2)value;
                    return $"{v2.x},{v2.y}";
                case ExtendedPrefType.Vector3:
                    var v3 = (Vector3)value;
                    return $"{v3.x},{v3.y},{v3.z}";
                case ExtendedPrefType.Quaternion:
                    var q = (Quaternion)value;
                    return $"{q.x},{q.y},{q.z},{q.w}";
                case ExtendedPrefType.Color:
                    var c = (Color)value;
                    return $"{c.r},{c.g},{c.b},{c.a}";
                case ExtendedPrefType.IntArray:
                    return string.Join(",", (int[])value);
                case ExtendedPrefType.FloatArray:
                    return string.Join(",", (float[])value);
                case ExtendedPrefType.BoolArray:
                    return string.Join(",", (bool[])value);
                case ExtendedPrefType.StringArray:
                    return string.Join("|", (string[])value);
                case ExtendedPrefType.Vector2Array:
                    var v2Array = (Vector2[])value;
                    return string.Join(";", Array.ConvertAll(v2Array, v => $"{v.x},{v.y}"));
                case ExtendedPrefType.Vector3Array:
                    var v3Array = (Vector3[])value;
                    return string.Join(";", Array.ConvertAll(v3Array, v => $"{v.x},{v.y},{v.z}"));
                case ExtendedPrefType.QuaternionArray:
                    var qArray = (Quaternion[])value;
                    return string.Join(";", Array.ConvertAll(qArray, q => $"{q.x},{q.y},{q.z},{q.w}"));
                case ExtendedPrefType.ColorArray:
                    var cArray = (Color[])value;
                    return string.Join(";", Array.ConvertAll(cArray, c => $"{c.r},{c.g},{c.b},{c.a}"));
                default:
                    return value.ToString();
            }
        }

        private string FormatValueForDisplay(object value, ExtendedPrefType type)
        {
            if (value == null) return "<null>";

            switch (type)
            {
                case ExtendedPrefType.Bool:
                    return (bool)value ? "True" : "False";
                case ExtendedPrefType.Vector2:
                    var v2 = (Vector2)value;
                    return $"({v2.x:F2}, {v2.y:F2})";
                case ExtendedPrefType.Vector3:
                    var v3 = (Vector3)value;
                    return $"({v3.x:F2}, {v3.y:F2}, {v3.z:F2})";
                case ExtendedPrefType.Quaternion:
                    var q = (Quaternion)value;
                    return $"({q.x:F2}, {q.y:F2}, {q.z:F2}, {q.w:F2})";
                case ExtendedPrefType.Color:
                    var c = (Color)value;
                    return $"RGBA({c.r:F2}, {c.g:F2}, {c.b:F2}, {c.a:F2})";
                case ExtendedPrefType.IntArray:
                    var intArray = (int[])value;
                    return $"[{string.Join(", ", intArray)}] ({intArray.Length} items)";
                case ExtendedPrefType.FloatArray:
                    var floatArray = (float[])value;
                    var displayArray = Array.ConvertAll(floatArray, f => f.ToString("F2"));
                    return $"[{string.Join(", ", displayArray)}] ({floatArray.Length} items)";
                case ExtendedPrefType.BoolArray:
                    var boolArray = (bool[])value;
                    return $"[{string.Join(", ", boolArray)}] ({boolArray.Length} items)";
                case ExtendedPrefType.StringArray:
                    var stringArray = (string[])value;
                    var preview = stringArray.Length > 3 ? 
                        string.Join(", ", stringArray, 0, 3) + "..." :
                        string.Join(", ", stringArray);
                    return $"[{preview}] ({stringArray.Length} items)";
                case ExtendedPrefType.Vector2Array:
                case ExtendedPrefType.Vector3Array:
                case ExtendedPrefType.QuaternionArray:
                case ExtendedPrefType.ColorArray:
                    var array = (Array)value;
                    return $"{type} ({array.Length} items)";
                case ExtendedPrefType.Long:
                    return $"{value}L";
                case ExtendedPrefType.Float:
                    return $"{(float)value:F3}f";
                default:
                    var str = value.ToString();
                    return str.Length > 50 ? str.Substring(0, 47) + "..." : str;
            }
        }

        private string GetTypeDisplayName(ExtendedPrefType type)
        {
            switch (type)
            {
                case ExtendedPrefType.Int: return "Integer";
                case ExtendedPrefType.Float: return "Float";
                case ExtendedPrefType.String: return "String";
                case ExtendedPrefType.Bool: return "Boolean";
                case ExtendedPrefType.Long: return "Long";
                case ExtendedPrefType.Vector2: return "Vector2";
                case ExtendedPrefType.Vector3: return "Vector3";
                case ExtendedPrefType.Quaternion: return "Quaternion";
                case ExtendedPrefType.Color: return "Color";
                case ExtendedPrefType.IntArray: return "Int[]";
                case ExtendedPrefType.FloatArray: return "Float[]";
                case ExtendedPrefType.BoolArray: return "Bool[]";
                case ExtendedPrefType.StringArray: return "String[]";
                case ExtendedPrefType.Vector2Array: return "Vector2[]";
                case ExtendedPrefType.Vector3Array: return "Vector3[]";
                case ExtendedPrefType.QuaternionArray: return "Quaternion[]";
                case ExtendedPrefType.ColorArray: return "Color[]";
                default: return type.ToString();
            }
        }
    }

    /// <summary>
    /// Data structure for import/export operations
    /// </summary>
    [Serializable]
    public class PlayerPrefsExportData
    {
        public string exportDate;
        public string unityVersion;
        public string companyName;
        public string productName;
        public List<ExportedPlayerPref> playerPrefs;

        public PlayerPrefsExportData()
        {
            exportDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            unityVersion = Application.unityVersion;
            companyName = Application.companyName;
            productName = Application.productName;
            playerPrefs = new List<ExportedPlayerPref>();
        }
    }

    [Serializable]
    public class ExportedPlayerPref
    {
        public string key;
        public string value;
        public string type;
        public string lastModified;

        public ExportedPlayerPref(PlayerPrefEntryData entry)
        {
            key = entry.Key;
            value = entry.EditValue;
            type = entry.Type.ToString();
            lastModified = entry.LastModified.ToString("yyyy-MM-dd HH:mm:ss");
        }
    }

    /// <summary>
    /// Search and filter configuration
    /// </summary>
    public class PlayerPrefsFilter
    {
        public string SearchText { get; set; } = "";
        public ExtendedPrefType TypeFilter { get; set; } = ExtendedPrefType.All;
        public bool UseRegex { get; set; } = false;
        public bool CaseSensitive { get; set; } = false;
        public DateTime? ModifiedAfter { get; set; }
        public DateTime? ModifiedBefore { get; set; }

        public bool Matches(PlayerPrefEntryData entry)
        {
            // Type filter
            if (TypeFilter != ExtendedPrefType.All && entry.Type != TypeFilter)
                return false;

            // Date filters
            if (ModifiedAfter.HasValue && entry.LastModified < ModifiedAfter.Value)
                return false;

            if (ModifiedBefore.HasValue && entry.LastModified > ModifiedBefore.Value)
                return false;

            // Text search
            if (!string.IsNullOrEmpty(SearchText))
            {
                var comparison = CaseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;
                
                if (UseRegex)
                {
                    try
                    {
                        var options = CaseSensitive ? RegexOptions.None : RegexOptions.IgnoreCase;
                        if (!Regex.IsMatch(entry.Key, SearchText, options))
                            return false;
                    }
                    catch
                    {
                        // Invalid regex, fall back to simple contains
                        if (entry.Key.IndexOf(SearchText, comparison) < 0)
                            return false;
                    }
                }
                else
                {
                    if (entry.Key.IndexOf(SearchText, comparison) < 0)
                        return false;
                }
            }

            return true;
        }
    }
}