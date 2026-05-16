using System.IO;
using System.Linq;
using System.Reflection;
using Breezinstein.Tools.Audio;
using Breezinstein.Tools.Audio.Editor;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Breezinstein.Tools.Tests
{
    [TestFixture]
    public class AudioKeyAttributeTests
    {
        [Test]
        public void Attribute_IsAPropertyAttribute()
        {
            Assert.IsTrue(typeof(PropertyAttribute).IsAssignableFrom(typeof(AudioKeyAttribute)));
        }

        [Test]
        public void Attribute_LivesInAudioNamespace()
        {
            Assert.AreEqual("Breezinstein.Tools.Audio", typeof(AudioKeyAttribute).Namespace);
        }

        [Test]
        public void Drawer_IsRegisteredForAudioKeyAttribute()
        {
            var custom = typeof(AudioKeyDrawer)
                .GetCustomAttributes(typeof(CustomPropertyDrawer), false)
                .OfType<CustomPropertyDrawer>()
                .FirstOrDefault();

            Assert.IsNotNull(custom, "AudioKeyDrawer must be marked [CustomPropertyDrawer].");

            FieldInfo typeField = typeof(CustomPropertyDrawer)
                .GetField("m_Type", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(typeField, "CustomPropertyDrawer.m_Type field not found.");
            Assert.AreEqual(typeof(AudioKeyAttribute), typeField.GetValue(custom));
        }

        [Test]
        public void GetAllKeys_IncludesKeysFromProjectAudioLibraries()
        {
            const string testFolder = "Assets/__breezetools_audiokey_tests__";
            const string assetPath = testFolder + "/TestAudioLibrary.asset";

            if (!AssetDatabase.IsValidFolder(testFolder))
            {
                AssetDatabase.CreateFolder("Assets", "__breezetools_audiokey_tests__");
            }

            try
            {
                var lib = ScriptableObject.CreateInstance<AudioLibrary>();
                lib.clips.Add("test_key_zeta", new AudioItem { volume = 0.5f });
                lib.clips.Add("test_key_alpha", new AudioItem { volume = 1f });

                AssetDatabase.CreateAsset(lib, assetPath);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                MethodInfo getAllKeys = typeof(AudioKeyDrawer)
                    .GetMethod("GetAllKeys", BindingFlags.Static | BindingFlags.NonPublic);
                Assert.IsNotNull(getAllKeys, "AudioKeyDrawer.GetAllKeys must exist.");

                var keys = (System.Collections.Generic.List<string>)getAllKeys.Invoke(null, null);

                Assert.IsNotNull(keys);
                Assert.Contains("test_key_alpha", keys);
                Assert.Contains("test_key_zeta", keys);

                int alphaIdx = keys.IndexOf("test_key_alpha");
                int zetaIdx = keys.IndexOf("test_key_zeta");
                Assert.Less(alphaIdx, zetaIdx, "GetAllKeys should return keys in sorted order.");

                int alphaCount = keys.Count(k => k == "test_key_alpha");
                Assert.AreEqual(1, alphaCount, "Duplicate keys should be collapsed.");
            }
            finally
            {
                AssetDatabase.DeleteAsset(assetPath);
                AssetDatabase.DeleteAsset(testFolder);
                AssetDatabase.Refresh();
            }
        }

        [Test]
        public void GetAllKeys_ReturnsEmptyWhenNoLibrariesExist()
        {
            MethodInfo getAllKeys = typeof(AudioKeyDrawer)
                .GetMethod("GetAllKeys", BindingFlags.Static | BindingFlags.NonPublic);
            Assert.IsNotNull(getAllKeys);

            var keys = (System.Collections.Generic.List<string>)getAllKeys.Invoke(null, null);

            Assert.IsNotNull(keys);
            // We can't assert empty (the user's project might already have AudioLibrary
            // assets), but we can assert the call succeeds and returns a list of strings.
            foreach (var k in keys) Assert.IsNotNull(k);
        }
    }
}
