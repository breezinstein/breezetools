using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.IO;

#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
using Microsoft.Win32;
#endif

#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
using System.Xml;
#endif

namespace Breezinstein.Tools
{
    /// <summary>
    /// Extended PlayerPrefs functionality combining standard and PlayerPrefsX features
    /// </summary>
    public static class PlayerPrefsExtensions
    {
        private static int endianDiff1;
        private static int idx;
        private static byte[] byteBlock;

        enum ArrayType { Float, Int32, Bool, String, Vector2, Vector3, Quaternion, Color }

        #region Type Detection and Value Retrieval

        public static ExtendedPrefType DetectPlayerPrefType(string key)
        {
            try
            {
                // Check for array types first by looking for Base64 encoded data
                string stringValue = PlayerPrefs.GetString(key, null);
                if (!string.IsNullOrEmpty(stringValue) && IsBase64String(stringValue))
                {
                    return DetectArrayType(key, stringValue);
                }

                // Check for long type (stored as two int keys)
                if (PlayerPrefs.HasKey(key + "_lowBits") && PlayerPrefs.HasKey(key + "_highBits"))
                {
                    return ExtendedPrefType.Long;
                }

                // Check for Vector/Quaternion/Color types (stored as float arrays)
                if (!string.IsNullOrEmpty(stringValue) && IsBase64String(stringValue))
                {
                    var floatArray = GetFloatArraySafe(key);
                    if (floatArray != null)
                    {
                        switch (floatArray.Length)
                        {
                            case 2: return ExtendedPrefType.Vector2;
                            case 3: return ExtendedPrefType.Vector3;
                            case 4: 
                                // Could be Quaternion or Color, check for typical quaternion values
                                if (IsLikelyQuaternion(floatArray))
                                    return ExtendedPrefType.Quaternion;
                                return ExtendedPrefType.Color;
                        }
                    }
                }

                // Check for basic types
                if (string.IsNullOrEmpty(stringValue))
                {
                    // Try int first
                    int intValue = PlayerPrefs.GetInt(key, int.MinValue);
                    if (intValue != int.MinValue)
                    {
                        // Check if it might be a bool (0 or 1)
                        if (intValue == 0 || intValue == 1)
                        {
                            return ExtendedPrefType.Bool;
                        }
                        return ExtendedPrefType.Int;
                    }

                    // Try float
                    float floatValue = PlayerPrefs.GetFloat(key, float.MinValue);
                    if (!Mathf.Approximately(floatValue, float.MinValue))
                    {
                        return ExtendedPrefType.Float;
                    }

                    return ExtendedPrefType.String;
                }

                // Check if string represents a number
                if (int.TryParse(stringValue, out int parsedInt))
                {
                    int storedInt = PlayerPrefs.GetInt(key, int.MinValue);
                    if (storedInt == parsedInt)
                    {
                        return (parsedInt == 0 || parsedInt == 1) ? ExtendedPrefType.Bool : ExtendedPrefType.Int;
                    }
                }

                if (float.TryParse(stringValue, out float parsedFloat))
                {
                    float storedFloat = PlayerPrefs.GetFloat(key, float.MinValue);
                    if (Mathf.Approximately(storedFloat, parsedFloat))
                    {
                        return ExtendedPrefType.Float;
                    }
                }

                return ExtendedPrefType.String;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"PlayerPrefs Extensions: Error detecting type for key '{key}' - {ex.Message}");
                return ExtendedPrefType.String;
            }
        }

        private static ExtendedPrefType DetectArrayType(string key, string base64Value)
        {
            try
            {
                var bytes = Convert.FromBase64String(base64Value.Split('|')[0]);
                if (bytes.Length > 0)
                {
                    ArrayType arrayType = (ArrayType)bytes[0];
                    switch (arrayType)
                    {
                        case ArrayType.Bool: return ExtendedPrefType.BoolArray;
                        case ArrayType.Int32: return ExtendedPrefType.IntArray;
                        case ArrayType.Float: return ExtendedPrefType.FloatArray;
                        case ArrayType.String: return ExtendedPrefType.StringArray;
                        case ArrayType.Vector2: return ExtendedPrefType.Vector2Array;
                        case ArrayType.Vector3: return ExtendedPrefType.Vector3Array;
                        case ArrayType.Quaternion: return ExtendedPrefType.QuaternionArray;
                        case ArrayType.Color: return ExtendedPrefType.ColorArray;
                    }
                }
            }
            catch
            {
                // Not an array type
            }
            return ExtendedPrefType.String;
        }

        private static bool IsBase64String(string value)
        {
            if (string.IsNullOrEmpty(value)) return false;
            try
            {
                Convert.FromBase64String(value.Split('|')[0]);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static bool IsLikelyQuaternion(float[] values)
        {
            if (values.Length != 4) return false;
            // Check if it's normalized (quaternions should be unit length)
            float magnitude = Mathf.Sqrt(values[0] * values[0] + values[1] * values[1] + values[2] * values[2] + values[3] * values[3]);
            return Mathf.Approximately(magnitude, 1.0f);
        }

        public static object GetPlayerPrefValue(string key, ExtendedPrefType type)
        {
            try
            {
                switch (type)
                {
                    case ExtendedPrefType.Int:
                        return PlayerPrefs.GetInt(key);
                    case ExtendedPrefType.Float:
                        return PlayerPrefs.GetFloat(key);
                    case ExtendedPrefType.String:
                        return PlayerPrefs.GetString(key);
                    case ExtendedPrefType.Bool:
                        return PlayerPrefs.GetInt(key) == 1;
                    case ExtendedPrefType.Long:
                        return GetLong(key);
                    case ExtendedPrefType.Vector2:
                        return GetVector2(key);
                    case ExtendedPrefType.Vector3:
                        return GetVector3(key);
                    case ExtendedPrefType.Quaternion:
                        return GetQuaternion(key);
                    case ExtendedPrefType.Color:
                        return GetColor(key);
                    case ExtendedPrefType.IntArray:
                        return GetIntArray(key);
                    case ExtendedPrefType.FloatArray:
                        return GetFloatArray(key);
                    case ExtendedPrefType.BoolArray:
                        return GetBoolArray(key);
                    case ExtendedPrefType.StringArray:
                        return GetStringArray(key);
                    case ExtendedPrefType.Vector2Array:
                        return GetVector2Array(key);
                    case ExtendedPrefType.Vector3Array:
                        return GetVector3Array(key);
                    case ExtendedPrefType.QuaternionArray:
                        return GetQuaternionArray(key);
                    case ExtendedPrefType.ColorArray:
                        return GetColorArray(key);
                    default:
                        return PlayerPrefs.GetString(key);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"PlayerPrefs Extensions: Error getting value for key '{key}' - {ex.Message}");
                return GetDefaultValue(type);
            }
        }

        public static void SetPlayerPref(string key, string value, ExtendedPrefType type)
        {
            switch (type)
            {
                case ExtendedPrefType.Int:
                    PlayerPrefs.SetInt(key, int.Parse(value));
                    break;
                case ExtendedPrefType.Float:
                    PlayerPrefs.SetFloat(key, float.Parse(value));
                    break;
                case ExtendedPrefType.String:
                    PlayerPrefs.SetString(key, value);
                    break;
                case ExtendedPrefType.Bool:
                    PlayerPrefs.SetInt(key, bool.Parse(value) ? 1 : 0);
                    break;
                case ExtendedPrefType.Long:
                    SetLong(key, long.Parse(value));
                    break;
                case ExtendedPrefType.Vector2:
                    SetVector2(key, ParseVector2(value));
                    break;
                case ExtendedPrefType.Vector3:
                    SetVector3(key, ParseVector3(value));
                    break;
                case ExtendedPrefType.Quaternion:
                    SetQuaternion(key, ParseQuaternion(value));
                    break;
                case ExtendedPrefType.Color:
                    SetColor(key, ParseColor(value));
                    break;
                case ExtendedPrefType.IntArray:
                    SetIntArray(key, ParseIntArray(value));
                    break;
                case ExtendedPrefType.FloatArray:
                    SetFloatArray(key, ParseFloatArray(value));
                    break;
                case ExtendedPrefType.BoolArray:
                    SetBoolArray(key, ParseBoolArray(value));
                    break;
                case ExtendedPrefType.StringArray:
                    SetStringArray(key, ParseStringArray(value));
                    break;
                case ExtendedPrefType.Vector2Array:
                    SetVector2Array(key, ParseVector2Array(value));
                    break;
                case ExtendedPrefType.Vector3Array:
                    SetVector3Array(key, ParseVector3Array(value));
                    break;
                case ExtendedPrefType.QuaternionArray:
                    SetQuaternionArray(key, ParseQuaternionArray(value));
                    break;
                case ExtendedPrefType.ColorArray:
                    SetColorArray(key, ParseColorArray(value));
                    break;
                default:
                    PlayerPrefs.SetString(key, value);
                    break;
            }
        }

        private static object GetDefaultValue(ExtendedPrefType type)
        {
            switch (type)
            {
                case ExtendedPrefType.Int: return 0;
                case ExtendedPrefType.Float: return 0f;
                case ExtendedPrefType.String: return "";
                case ExtendedPrefType.Bool: return false;
                case ExtendedPrefType.Long: return 0L;
                case ExtendedPrefType.Vector2: return Vector2.zero;
                case ExtendedPrefType.Vector3: return Vector3.zero;
                case ExtendedPrefType.Quaternion: return Quaternion.identity;
                case ExtendedPrefType.Color: return Color.white;
                case ExtendedPrefType.IntArray: return new int[0];
                case ExtendedPrefType.FloatArray: return new float[0];
                case ExtendedPrefType.BoolArray: return new bool[0];
                case ExtendedPrefType.StringArray: return new string[0];
                case ExtendedPrefType.Vector2Array: return new Vector2[0];
                case ExtendedPrefType.Vector3Array: return new Vector3[0];
                case ExtendedPrefType.QuaternionArray: return new Quaternion[0];
                case ExtendedPrefType.ColorArray: return new Color[0];
                default: return "";
            }
        }

        #endregion

        #region Basic Extended Types (Bool, Long)

        public static void SetBool(string name, bool value)
        {
            PlayerPrefs.SetInt(name, value ? 1 : 0);
        }

        public static bool GetBool(string name, bool defaultValue = false)
        {
            return PlayerPrefs.GetInt(name, defaultValue ? 1 : 0) == 1;
        }

        public static void SetLong(string key, long value)
        {
            SplitLong(value, out int lowBits, out int highBits);
            PlayerPrefs.SetInt(key + "_lowBits", lowBits);
            PlayerPrefs.SetInt(key + "_highBits", highBits);
        }

        public static long GetLong(string key, long defaultValue = 0)
        {
            SplitLong(defaultValue, out int lowBits, out int highBits);
            lowBits = PlayerPrefs.GetInt(key + "_lowBits", lowBits);
            highBits = PlayerPrefs.GetInt(key + "_highBits", highBits);

            ulong ret = (uint)highBits;
            ret = (ret << 32);
            return (long)(ret | (ulong)(uint)lowBits);
        }

        private static void SplitLong(long input, out int lowBits, out int highBits)
        {
            lowBits = (int)(uint)(ulong)input;
            highBits = (int)(uint)(input >> 32);
        }

        #endregion

        #region Vector Types

        public static void SetVector2(string key, Vector2 vector)
        {
            SetFloatArray(key, new float[] { vector.x, vector.y });
        }

        public static Vector2 GetVector2(string key, Vector2 defaultValue = default)
        {
            var floatArray = GetFloatArraySafe(key);
            if (floatArray == null || floatArray.Length < 2)
                return defaultValue;
            return new Vector2(floatArray[0], floatArray[1]);
        }

        public static void SetVector3(string key, Vector3 vector)
        {
            SetFloatArray(key, new float[] { vector.x, vector.y, vector.z });
        }

        public static Vector3 GetVector3(string key, Vector3 defaultValue = default)
        {
            var floatArray = GetFloatArraySafe(key);
            if (floatArray == null || floatArray.Length < 3)
                return defaultValue;
            return new Vector3(floatArray[0], floatArray[1], floatArray[2]);
        }

        public static void SetQuaternion(string key, Quaternion quaternion)
        {
            SetFloatArray(key, new float[] { quaternion.x, quaternion.y, quaternion.z, quaternion.w });
        }

        public static Quaternion GetQuaternion(string key, Quaternion defaultValue = default)
        {
            var floatArray = GetFloatArraySafe(key);
            if (floatArray == null || floatArray.Length < 4)
                return defaultValue;
            return new Quaternion(floatArray[0], floatArray[1], floatArray[2], floatArray[3]);
        }

        public static void SetColor(string key, Color color)
        {
            SetFloatArray(key, new float[] { color.r, color.g, color.b, color.a });
        }

        public static Color GetColor(string key, Color defaultValue = default)
        {
            var floatArray = GetFloatArraySafe(key);
            if (floatArray == null || floatArray.Length < 4)
                return defaultValue;
            return new Color(floatArray[0], floatArray[1], floatArray[2], floatArray[3]);
        }

        #endregion

        #region Array Types Implementation (Simplified from PlayerPrefsX)

        public static void SetBoolArray(string key, bool[] boolArray)
        {
            var bytes = new byte[(boolArray.Length + 7) / 8 + 5];
            bytes[0] = Convert.ToByte(ArrayType.Bool);
            var bits = new BitArray(boolArray);
            bits.CopyTo(bytes, 5);
            Initialize();
            ConvertInt32ToBytes(boolArray.Length, bytes);
            SaveBytes(key, bytes);
        }

        public static bool[] GetBoolArray(string key)
        {
            if (!PlayerPrefs.HasKey(key)) return new bool[0];
            
            try
            {
                var bytes = Convert.FromBase64String(PlayerPrefs.GetString(key));
                if (bytes.Length < 5 || (ArrayType)bytes[0] != ArrayType.Bool)
                    return new bool[0];

                Initialize();
                var bytes2 = new byte[bytes.Length - 5];
                Array.Copy(bytes, 5, bytes2, 0, bytes2.Length);
                var bits = new BitArray(bytes2) { Length = ConvertBytesToInt32(bytes) };
                var boolArray = new bool[bits.Count];
                bits.CopyTo(boolArray, 0);
                return boolArray;
            }
            catch
            {
                return new bool[0];
            }
        }

        public static void SetStringArray(string key, string[] stringArray)
        {
            var bytes = new byte[stringArray.Length + 1];
            bytes[0] = Convert.ToByte(ArrayType.String);
            Initialize();

            for (int i = 0; i < stringArray.Length; i++)
            {
                if (stringArray[i] == null || stringArray[i].Length > 255)
                    throw new ArgumentException($"String at index {i} is null or too long (max 255 characters)");
                bytes[idx++] = (byte)stringArray[i].Length;
            }

            PlayerPrefs.SetString(key, Convert.ToBase64String(bytes) + "|" + string.Join("", stringArray));
        }

        public static string[] GetStringArray(string key)
        {
            if (!PlayerPrefs.HasKey(key)) return new string[0];
            
            try
            {
                var completeString = PlayerPrefs.GetString(key);
                var separatorIndex = completeString.IndexOf('|');
                if (separatorIndex < 4) return new string[0];

                var bytes = Convert.FromBase64String(completeString.Substring(0, separatorIndex));
                if ((ArrayType)bytes[0] != ArrayType.String) return new string[0];

                Initialize();
                var numberOfEntries = bytes.Length - 1;
                var stringArray = new string[numberOfEntries];
                var stringIndex = separatorIndex + 1;

                for (int i = 0; i < numberOfEntries; i++)
                {
                    int stringLength = bytes[idx++];
                    if (stringIndex + stringLength > completeString.Length)
                        return new string[0];
                    stringArray[i] = completeString.Substring(stringIndex, stringLength);
                    stringIndex += stringLength;
                }

                return stringArray;
            }
            catch
            {
                return new string[0];
            }
        }

        public static void SetIntArray(string key, int[] intArray)
        {
            SetValue(key, intArray, ArrayType.Int32, 1, ConvertFromInt);
        }

        public static int[] GetIntArray(string key)
        {
            var intList = new List<int>();
            GetValue(key, intList, ArrayType.Int32, 1, ConvertToInt);
            return intList.ToArray();
        }

        public static void SetFloatArray(string key, float[] floatArray)
        {
            SetValue(key, floatArray, ArrayType.Float, 1, ConvertFromFloat);
        }

        public static float[] GetFloatArray(string key)
        {
            var floatList = new List<float>();
            GetValue(key, floatList, ArrayType.Float, 1, ConvertToFloat);
            return floatList.ToArray();
        }

        private static float[] GetFloatArraySafe(string key)
        {
            try
            {
                return GetFloatArray(key);
            }
            catch
            {
                return null;
            }
        }

        public static void SetVector2Array(string key, Vector2[] vector2Array)
        {
            SetValue(key, vector2Array, ArrayType.Vector2, 2, ConvertFromVector2);
        }

        public static Vector2[] GetVector2Array(string key)
        {
            var vector2List = new List<Vector2>();
            GetValue(key, vector2List, ArrayType.Vector2, 2, ConvertToVector2);
            return vector2List.ToArray();
        }

        public static void SetVector3Array(string key, Vector3[] vector3Array)
        {
            SetValue(key, vector3Array, ArrayType.Vector3, 3, ConvertFromVector3);
        }

        public static Vector3[] GetVector3Array(string key)
        {
            var vector3List = new List<Vector3>();
            GetValue(key, vector3List, ArrayType.Vector3, 3, ConvertToVector3);
            return vector3List.ToArray();
        }

        public static void SetQuaternionArray(string key, Quaternion[] quaternionArray)
        {
            SetValue(key, quaternionArray, ArrayType.Quaternion, 4, ConvertFromQuaternion);
        }

        public static Quaternion[] GetQuaternionArray(string key)
        {
            var quaternionList = new List<Quaternion>();
            GetValue(key, quaternionList, ArrayType.Quaternion, 4, ConvertToQuaternion);
            return quaternionList.ToArray();
        }

        public static void SetColorArray(string key, Color[] colorArray)
        {
            SetValue(key, colorArray, ArrayType.Color, 4, ConvertFromColor);
        }

        public static Color[] GetColorArray(string key)
        {
            var colorList = new List<Color>();
            GetValue(key, colorList, ArrayType.Color, 4, ConvertToColor);
            return colorList.ToArray();
        }

        #endregion

        #region Parsing Methods

        private static Vector2 ParseVector2(string value)
        {
            var parts = value.Split(',');
            if (parts.Length >= 2)
                return new Vector2(float.Parse(parts[0]), float.Parse(parts[1]));
            throw new ArgumentException("Invalid Vector2 format");
        }

        private static Vector3 ParseVector3(string value)
        {
            var parts = value.Split(',');
            if (parts.Length >= 3)
                return new Vector3(float.Parse(parts[0]), float.Parse(parts[1]), float.Parse(parts[2]));
            throw new ArgumentException("Invalid Vector3 format");
        }

        private static Quaternion ParseQuaternion(string value)
        {
            var parts = value.Split(',');
            if (parts.Length >= 4)
                return new Quaternion(float.Parse(parts[0]), float.Parse(parts[1]), float.Parse(parts[2]), float.Parse(parts[3]));
            throw new ArgumentException("Invalid Quaternion format");
        }

        private static Color ParseColor(string value)
        {
            var parts = value.Split(',');
            if (parts.Length >= 4)
                return new Color(float.Parse(parts[0]), float.Parse(parts[1]), float.Parse(parts[2]), float.Parse(parts[3]));
            throw new ArgumentException("Invalid Color format");
        }

        private static int[] ParseIntArray(string value)
        {
            if (string.IsNullOrEmpty(value)) return new int[0];
            return value.Split(',').Select(int.Parse).ToArray();
        }

        private static float[] ParseFloatArray(string value)
        {
            if (string.IsNullOrEmpty(value)) return new float[0];
            return value.Split(',').Select(float.Parse).ToArray();
        }

        private static bool[] ParseBoolArray(string value)
        {
            if (string.IsNullOrEmpty(value)) return new bool[0];
            return value.Split(',').Select(bool.Parse).ToArray();
        }

        private static string[] ParseStringArray(string value)
        {
            if (string.IsNullOrEmpty(value)) return new string[0];
            return value.Split('|');
        }

        private static Vector2[] ParseVector2Array(string value)
        {
            if (string.IsNullOrEmpty(value)) return new Vector2[0];
            return value.Split(';').Select(ParseVector2).ToArray();
        }

        private static Vector3[] ParseVector3Array(string value)
        {
            if (string.IsNullOrEmpty(value)) return new Vector3[0];
            return value.Split(';').Select(ParseVector3).ToArray();
        }

        private static Quaternion[] ParseQuaternionArray(string value)
        {
            if (string.IsNullOrEmpty(value)) return new Quaternion[0];
            return value.Split(';').Select(ParseQuaternion).ToArray();
        }

        private static Color[] ParseColorArray(string value)
        {
            if (string.IsNullOrEmpty(value)) return new Color[0];
            return value.Split(';').Select(ParseColor).ToArray();
        }

        #endregion

        #region Helper Methods (From PlayerPrefsX)

        private static void SetValue<T>(string key, T array, ArrayType arrayType, int vectorNumber, Action<T, byte[], int> convert) where T : IList
        {
            var bytes = new byte[(4 * array.Count) * vectorNumber + 1];
            bytes[0] = Convert.ToByte(arrayType);
            Initialize();

            for (int i = 0; i < array.Count; i++)
            {
                convert(array, bytes, i);
            }
            SaveBytes(key, bytes);
        }

        private static void GetValue<T>(string key, T list, ArrayType arrayType, int vectorNumber, Action<T, byte[]> convert) where T : IList
        {
            if (!PlayerPrefs.HasKey(key)) return;

            try
            {
                var bytes = Convert.FromBase64String(PlayerPrefs.GetString(key));
                if (bytes.Length <= 0 || (ArrayType)bytes[0] != arrayType) return;

                Initialize();
                var end = bytes.Length - 1;
                while (idx <= end)
                {
                    convert(list, bytes);
                }
            }
            catch
            {
                // Handle errors silently
            }
        }

        private static void Initialize()
        {
            if (BitConverter.IsLittleEndian)
            {
                endianDiff1 = 0;
            }
            else
            {
                endianDiff1 = 3;
            }
            idx = 1;
        }

        private static bool SaveBytes(string key, byte[] bytes)
        {
            try
            {
                PlayerPrefs.SetString(key, Convert.ToBase64String(bytes));
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static void ConvertInt32ToBytes(int value, byte[] bytes)
        {
            byteBlock = BitConverter.GetBytes(value);
            ConvertTo4Bytes(byteBlock);
            byteBlock.CopyTo(bytes, idx);
            idx += 4;
        }

        private static int ConvertBytesToInt32(byte[] bytes)
        {
            byteBlock = new byte[4];
            Array.Copy(bytes, idx, byteBlock, 0, 4);
            ConvertFrom4Bytes(byteBlock);
            idx += 4;
            return BitConverter.ToInt32(byteBlock, 0);
        }

        private static void ConvertFloatToBytes(float value, byte[] bytes)
        {
            byteBlock = BitConverter.GetBytes(value);
            ConvertTo4Bytes(byteBlock);
            byteBlock.CopyTo(bytes, idx);
            idx += 4;
        }

        private static float ConvertBytesToFloat(byte[] bytes)
        {
            byteBlock = new byte[4];
            Array.Copy(bytes, idx, byteBlock, 0, 4);
            ConvertFrom4Bytes(byteBlock);
            idx += 4;
            return BitConverter.ToSingle(byteBlock, 0);
        }

        private static void ConvertTo4Bytes(byte[] bytes)
        {
            if (endianDiff1 != 0)
            {
                byte temp = bytes[0];
                bytes[0] = bytes[3];
                bytes[3] = temp;
                temp = bytes[1];
                bytes[1] = bytes[2];
                bytes[2] = temp;
            }
        }

        private static void ConvertFrom4Bytes(byte[] bytes)
        {
            if (endianDiff1 != 0)
            {
                byte temp = bytes[0];
                bytes[0] = bytes[3];
                bytes[3] = temp;
                temp = bytes[1];
                bytes[1] = bytes[2];
                bytes[2] = temp;
            }
        }

        // Conversion methods for arrays
        private static void ConvertFromInt(int[] array, byte[] bytes, int i) => ConvertInt32ToBytes(array[i], bytes);
        private static void ConvertFromFloat(float[] array, byte[] bytes, int i) => ConvertFloatToBytes(array[i], bytes);
        private static void ConvertFromVector2(Vector2[] array, byte[] bytes, int i)
        {
            ConvertFloatToBytes(array[i].x, bytes);
            ConvertFloatToBytes(array[i].y, bytes);
        }
        private static void ConvertFromVector3(Vector3[] array, byte[] bytes, int i)
        {
            ConvertFloatToBytes(array[i].x, bytes);
            ConvertFloatToBytes(array[i].y, bytes);
            ConvertFloatToBytes(array[i].z, bytes);
        }
        private static void ConvertFromQuaternion(Quaternion[] array, byte[] bytes, int i)
        {
            ConvertFloatToBytes(array[i].x, bytes);
            ConvertFloatToBytes(array[i].y, bytes);
            ConvertFloatToBytes(array[i].z, bytes);
            ConvertFloatToBytes(array[i].w, bytes);
        }
        private static void ConvertFromColor(Color[] array, byte[] bytes, int i)
        {
            ConvertFloatToBytes(array[i].r, bytes);
            ConvertFloatToBytes(array[i].g, bytes);
            ConvertFloatToBytes(array[i].b, bytes);
            ConvertFloatToBytes(array[i].a, bytes);
        }

        private static void ConvertToInt(List<int> list, byte[] bytes) => list.Add(ConvertBytesToInt32(bytes));
        private static void ConvertToFloat(List<float> list, byte[] bytes) => list.Add(ConvertBytesToFloat(bytes));
        private static void ConvertToVector2(List<Vector2> list, byte[] bytes) =>
            list.Add(new Vector2(ConvertBytesToFloat(bytes), ConvertBytesToFloat(bytes)));
        private static void ConvertToVector3(List<Vector3> list, byte[] bytes) =>
            list.Add(new Vector3(ConvertBytesToFloat(bytes), ConvertBytesToFloat(bytes), ConvertBytesToFloat(bytes)));
        private static void ConvertToQuaternion(List<Quaternion> list, byte[] bytes) =>
            list.Add(new Quaternion(ConvertBytesToFloat(bytes), ConvertBytesToFloat(bytes), ConvertBytesToFloat(bytes), ConvertBytesToFloat(bytes)));
        private static void ConvertToColor(List<Color> list, byte[] bytes) =>
            list.Add(new Color(ConvertBytesToFloat(bytes), ConvertBytesToFloat(bytes), ConvertBytesToFloat(bytes), ConvertBytesToFloat(bytes)));

        #endregion
    }

    /// <summary>
    /// Platform-specific PlayerPrefs discovery system
    /// </summary>
    public static class PlayerPrefsDiscovery
    {
        public static List<string> DiscoverAllPlayerPrefsKeys()
        {
            List<string> keys = new List<string>();

            try
            {
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
                keys = DiscoverWindowsPlayerPrefsKeys();
#elif UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
                keys = DiscoverMacOSPlayerPrefsKeys();
#else
                keys = DiscoverPlayerPrefsKeysFallback();
#endif
            }
            catch (Exception ex)
            {
                Debug.LogError($"PlayerPrefs Discovery: Error discovering keys - {ex.Message}");
                keys = DiscoverPlayerPrefsKeysFallback();
            }

            return keys;
        }

#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
        private static List<string> DiscoverWindowsPlayerPrefsKeys()
        {
            List<string> keys = new List<string>();

            try
            {
                // Use PlayerSettings instead of Application for consistency with Unity's approach
                string companyName = PlayerSettings.companyName;
                string productName = PlayerSettings.productName;
                string registryPath = $@"Software\Unity\UnityEditor\{companyName}\{productName}";

                // Use CreateSubKey like the working approach - it returns an existing key or creates it
                using (RegistryKey key = Registry.CurrentUser.CreateSubKey(registryPath))
                {
                    if (key != null)
                    {
                        string[] rawKeys = key.GetValueNames();
                        
                        // Process keys to strip Unity's hash suffixes (like "_h1669003810")
                        foreach (string rawKey in rawKeys)
                        {
                            if (!string.IsNullOrEmpty(rawKey))
                            {
                                string processedKey = rawKey;
                                int lastUnderscoreIndex = rawKey.LastIndexOf("_");
                                
                                // Strip the suffix if it exists (Unity adds hash suffixes to registry keys)
                                if (lastUnderscoreIndex > 0)
                                {
                                    processedKey = rawKey.Substring(0, lastUnderscoreIndex);
                                }
                                
                                if (!string.IsNullOrEmpty(processedKey) && !keys.Contains(processedKey))
                                {
                                    keys.Add(processedKey);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"PlayerPrefs Discovery: Error accessing Windows Registry - {ex.Message}");
                return DiscoverPlayerPrefsKeysFallback();
            }

            return keys;
        }
#endif

#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
        private static List<string> DiscoverMacOSPlayerPrefsKeys()
        {
            List<string> keys = new List<string>();

            try
            {
                string companyName = Application.companyName;
                string productName = Application.productName;
                string homeDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                string plistPath = Path.Combine(homeDirectory, "Library", "Preferences", $"unity.{companyName}.{productName}.plist");

                if (File.Exists(plistPath))
                {
                    string plistContent = File.ReadAllText(plistPath);
                    keys = ParseMacOSPlistKeys(plistContent);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"PlayerPrefs Discovery: Error accessing macOS plist - {ex.Message}");
                return DiscoverPlayerPrefsKeysFallback();
            }

            return keys;
        }

        private static List<string> ParseMacOSPlistKeys(string plistContent)
        {
            List<string> keys = new List<string>();

            try
            {
                Regex keyRegex = new Regex(@"<key>(.*?)</key>");
                MatchCollection matches = keyRegex.Matches(plistContent);

                foreach (Match match in matches)
                {
                    if (match.Groups.Count > 1)
                    {
                        string key = match.Groups[1].Value;
                        if (!string.IsNullOrEmpty(key) && !keys.Contains(key))
                        {
                            keys.Add(key);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"PlayerPrefs Discovery: Error parsing plist content - {ex.Message}");
            }

            return keys;
        }
#endif

        private static List<string> DiscoverPlayerPrefsKeysFallback()
        {
            List<string> keys = new List<string>();

            // Common PlayerPrefs key patterns
            string[] commonKeys = {
                "AdsEnabled", "sfxEnabled", "bgmEnabled", "PlayerCurrency", "UnlockedItemsProgression_V1",
                "MasterVolume", "SFXVolume", "MusicVolume", "QualityLevel", "ResolutionWidth", "ResolutionHeight",
                "Fullscreen", "Language", "FirstRun", "PlayerName", "HighScore", "LastPlayedLevel",
                "TutorialCompleted", "SettingsInitialized", "GameVersion", "PlayerLevel", "PlayerXP",
                "UnityGraphicsQuality_h2169741474", "Screenmanager Resolution Width_h182942802",
                "Screenmanager Resolution Height_h2627697771", "Screenmanager Fullscreen mode_h3630240806"
            };

            foreach (string key in commonKeys)
            {
                if (PlayerPrefs.HasKey(key))
                {
                    keys.Add(key);
                }
            }

            // Try common prefixes and suffixes
            string[] prefixes = { "Player", "Game", "Settings", "Audio", "Graphics", "Level", "Score", "Save" };
            string[] suffixes = { "Enabled", "Volume", "Count", "Level", "Score", "Time", "Data", "Progress" };

            foreach (string prefix in prefixes)
            {
                foreach (string suffix in suffixes)
                {
                    string testKey = prefix + suffix;
                    if (PlayerPrefs.HasKey(testKey) && !keys.Contains(testKey))
                    {
                        keys.Add(testKey);
                    }
                }
            }

            return keys;
        }
    }

    /// <summary>
    /// Import/Export functionality for PlayerPrefs
    /// </summary>
    public static class PlayerPrefsImportExport
    {
        public static void ExportToFile(List<PlayerPrefEntryData> playerPrefs, string filePath)
        {
            var exportData = new PlayerPrefsExportData();
            
            foreach (var pref in playerPrefs)
            {
                exportData.playerPrefs.Add(new ExportedPlayerPref(pref));
            }

            string json = JsonUtility.ToJson(exportData, true);
            File.WriteAllText(filePath, json);
        }

        public static void ImportFromFile(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException("Import file not found");

            string json = File.ReadAllText(filePath);
            var importData = JsonUtility.FromJson<PlayerPrefsExportData>(json);

            foreach (var pref in importData.playerPrefs)
            {
                if (Enum.TryParse(pref.type, out ExtendedPrefType type))
                {
                    PlayerPrefsExtensions.SetPlayerPref(pref.key, pref.value, type);
                }
            }

            PlayerPrefs.Save();
        }
    }
}