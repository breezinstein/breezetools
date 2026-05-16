using NUnit.Framework;
using UnityEngine;
using Breezinstein.Tools.Audio;

namespace Breezinstein.Tools.Tests
{
    [TestFixture]
    public class AudioLibraryTests
    {
        private AudioLibrary library;

        [SetUp]
        public void SetUp()
        {
            library = ScriptableObject.CreateInstance<AudioLibrary>();
        }

        [TearDown]
        public void TearDown()
        {
            if (library != null) Object.DestroyImmediate(library);
        }

        [Test]
        public void NewLibrary_StartsEmpty()
        {
            Assert.IsNotNull(library.clips);
            Assert.AreEqual(0, library.clips.Count);
        }

        [Test]
        public void Add_StoresClipsByKey()
        {
            var item = new AudioItem
            {
                category = AudioManager.AudioSourceType.MUSIC,
                volume = 0.75f,
                description = "Title screen loop",
            };

            library.clips.Add("title_music", item);

            Assert.AreEqual(1, library.clips.Count);
            Assert.IsTrue(library.clips.ContainsKey("title_music"));
            Assert.IsTrue(library.clips.TryGetValue("title_music", out var retrieved));
            Assert.AreSame(item, retrieved);
            Assert.AreEqual(0.75f, retrieved.volume, 1e-6f);
            Assert.AreEqual(AudioManager.AudioSourceType.MUSIC, retrieved.category);
        }

        [Test]
        public void TryGetValue_UnknownKey_ReturnsFalse()
        {
            Assert.IsFalse(library.clips.TryGetValue("nope", out _));
        }

        [Test]
        public void Remove_DropsClip()
        {
            library.clips.Add("a", new AudioItem());
            library.clips.Add("b", new AudioItem());

            Assert.IsTrue(library.clips.Remove("a"));
            Assert.AreEqual(1, library.clips.Count);
            Assert.IsFalse(library.clips.ContainsKey("a"));
            Assert.IsTrue(library.clips.ContainsKey("b"));
        }

        [Test]
        public void Iteration_VisitsEveryEntry()
        {
            library.clips.Add("a", new AudioItem { category = AudioManager.AudioSourceType.MUSIC });
            library.clips.Add("b", new AudioItem { category = AudioManager.AudioSourceType.EFFECT });
            library.clips.Add("c", new AudioItem { category = AudioManager.AudioSourceType.VOICE });

            int count = 0;
            foreach (var _ in library.clips) count++;
            Assert.AreEqual(3, count);
        }

        [Test]
        public void AudioItem_DefaultsVolumeToOne()
        {
            var item = new AudioItem();
            Assert.AreEqual(1f, item.volume, 1e-6f);
        }
    }
}
