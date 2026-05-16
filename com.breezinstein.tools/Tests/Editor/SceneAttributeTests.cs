using System.Linq;
using System.Reflection;
using Breezinstein.Tools;
using Breezinstein.Tools.Editor;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Breezinstein.Tools.Tests
{
    [TestFixture]
    public class SceneAttributeTests
    {
        private EditorBuildSettingsScene[] originalScenes;

        [SetUp]
        public void SetUp()
        {
            originalScenes = EditorBuildSettings.scenes;
        }

        [TearDown]
        public void TearDown()
        {
            EditorBuildSettings.scenes = originalScenes;
        }

        [Test]
        public void Attribute_IsAPropertyAttribute()
        {
            Assert.IsTrue(typeof(PropertyAttribute).IsAssignableFrom(typeof(SceneAttribute)));
        }

        [Test]
        public void Attribute_LivesInToolsNamespace()
        {
            Assert.AreEqual("Breezinstein.Tools", typeof(SceneAttribute).Namespace);
        }

        [Test]
        public void Drawer_IsRegisteredForSceneAttribute()
        {
            var custom = typeof(SceneAttributeDrawer)
                .GetCustomAttributes(typeof(CustomPropertyDrawer), false)
                .OfType<CustomPropertyDrawer>()
                .FirstOrDefault();

            Assert.IsNotNull(custom, "SceneAttributeDrawer must be marked [CustomPropertyDrawer].");

            FieldInfo typeField = typeof(CustomPropertyDrawer)
                .GetField("m_Type", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(typeField);
            Assert.AreEqual(typeof(SceneAttribute), typeField.GetValue(custom));
        }

        [Test]
        public void GetBuildSceneNames_StripsPathAndExtension()
        {
            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene("Assets/Scenes/Title.unity", true),
                new EditorBuildSettingsScene("Assets/Scenes/Sub/Game.unity", true),
                new EditorBuildSettingsScene("Assets/Scenes/Credits.unity", false),
            };

            string[] names = SceneAttributeDrawer.GetBuildSceneNames();

            Assert.AreEqual(3, names.Length);
            Assert.AreEqual("Title", names[0]);
            Assert.AreEqual("Game", names[1]);
            Assert.AreEqual("Credits", names[2]);
        }

        [Test]
        public void GetBuildSceneNames_PreservesOrder()
        {
            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene("Assets/Scenes/C.unity", true),
                new EditorBuildSettingsScene("Assets/Scenes/A.unity", true),
                new EditorBuildSettingsScene("Assets/Scenes/B.unity", true),
            };

            string[] names = SceneAttributeDrawer.GetBuildSceneNames();

            Assert.AreEqual(new[] { "C", "A", "B" }, names);
        }

        [Test]
        public void GetBuildSceneNames_ReturnsEmptyArrayWhenNoScenes()
        {
            EditorBuildSettings.scenes = new EditorBuildSettingsScene[0];

            string[] names = SceneAttributeDrawer.GetBuildSceneNames();

            Assert.IsNotNull(names);
            Assert.AreEqual(0, names.Length);
        }
    }
}
