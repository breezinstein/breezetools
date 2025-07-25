using UnityEngine;
using UnityEditor;
using System.Linq;

namespace Breezinstein.Tools
{
    /// <summary>
    /// Test script to verify PlayerPrefs editor functionality
    /// </summary>
    public static class PlayerPrefsEditorTest
    {
        [MenuItem("Breeze Tools/Test PlayerPrefs Editor")]
        public static void TestPlayerPrefsEditor()
        {
            Debug.Log("Testing PlayerPrefs Editor functionality...");
            
            // Test basic types
            TestBasicTypes();
            
            // Test extended types
            TestExtendedTypes();
            
            // Test array types
            TestArrayTypes();
            
            // Test discovery
            TestDiscovery();
            
            Debug.Log("PlayerPrefs Editor test completed!");
        }

        private static void TestBasicTypes()
        {
            // Set some test values
            PlayerPrefs.SetInt("TestInt", 42);
            PlayerPrefs.SetFloat("TestFloat", 3.14f);
            PlayerPrefs.SetString("TestString", "Hello World");
            
            // Test extended bool
            PlayerPrefsExtensions.SetBool("TestBool", true);
            
            // Test long
            PlayerPrefsExtensions.SetLong("TestLong", 9223372036854775807L);
            
            PlayerPrefs.Save();
            Debug.Log("Basic types test completed");
        }

        private static void TestExtendedTypes()
        {
            // Test Vector types
            PlayerPrefsExtensions.SetVector2("TestVector2", new Vector2(1.5f, 2.5f));
            PlayerPrefsExtensions.SetVector3("TestVector3", new Vector3(1.0f, 2.0f, 3.0f));
            PlayerPrefsExtensions.SetQuaternion("TestQuaternion", Quaternion.identity);
            PlayerPrefsExtensions.SetColor("TestColor", Color.red);
            
            PlayerPrefs.Save();
            Debug.Log("Extended types test completed");
        }

        private static void TestArrayTypes()
        {
            // Test array types
            PlayerPrefsExtensions.SetIntArray("TestIntArray", new int[] { 1, 2, 3, 4, 5 });
            PlayerPrefsExtensions.SetFloatArray("TestFloatArray", new float[] { 1.1f, 2.2f, 3.3f });
            PlayerPrefsExtensions.SetBoolArray("TestBoolArray", new bool[] { true, false, true });
            PlayerPrefsExtensions.SetStringArray("TestStringArray", new string[] { "One", "Two", "Three" });
            
            PlayerPrefs.Save();
            Debug.Log("Array types test completed");
        }

        private static void TestDiscovery()
        {
            var discoveredKeys = PlayerPrefsDiscovery.DiscoverAllPlayerPrefsKeys();
            Debug.Log($"Discovered {discoveredKeys.Count} PlayerPrefs keys:");
            
            foreach (var key in discoveredKeys.Take(10)) // Show first 10
            {
                var type = PlayerPrefsExtensions.DetectPlayerPrefType(key);
                var value = PlayerPrefsExtensions.GetPlayerPrefValue(key, type);
                Debug.Log($"  {key} ({type}): {value}");
            }
            
            if (discoveredKeys.Count > 10)
            {
                Debug.Log($"  ... and {discoveredKeys.Count - 10} more");
            }
        }

        [MenuItem("Breeze Tools/Clear Test PlayerPrefs")]
        public static void ClearTestPlayerPrefs()
        {
            string[] testKeys = {
                "TestInt", "TestFloat", "TestString", "TestBool", "TestLong",
                "TestVector2", "TestVector3", "TestQuaternion", "TestColor",
                "TestIntArray", "TestFloatArray", "TestBoolArray", "TestStringArray"
            };

            foreach (var key in testKeys)
            {
                if (PlayerPrefs.HasKey(key))
                {
                    PlayerPrefs.DeleteKey(key);
                }
                
                // Also check for long type keys
                if (PlayerPrefs.HasKey(key + "_lowBits"))
                {
                    PlayerPrefs.DeleteKey(key + "_lowBits");
                }
                if (PlayerPrefs.HasKey(key + "_highBits"))
                {
                    PlayerPrefs.DeleteKey(key + "_highBits");
                }
            }

            PlayerPrefs.Save();
            Debug.Log("Test PlayerPrefs cleared");
        }
    }
}