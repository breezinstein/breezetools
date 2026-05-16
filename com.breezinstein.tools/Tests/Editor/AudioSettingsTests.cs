using NUnit.Framework;
using Breezinstein.Tools.Audio;

namespace Breezinstein.Tools.Tests
{
    [TestFixture]
    public class AudioSettingsTests
    {
        // AudioSettings persists itself to "AudioSettings.json" via BreezeHelper, so back up
        // any existing file for the duration of these tests.
        private PersistentFileScope scope;

        [SetUp]
        public void SetUp()
        {
            scope = new PersistentFileScope("AudioSettings");
        }

        [TearDown]
        public void TearDown()
        {
            scope?.Dispose();
        }

        [Test]
        public void Defaults_AreSensible()
        {
            var s = new AudioSettings();
            // Reset() initializes the protected backing fields and persists; mimic that path.
            s.Reset();

            Assert.IsTrue(s.MainEnabled);
            Assert.IsTrue(s.MusicEnabled);
            Assert.IsTrue(s.EffectsEnabled);
            Assert.IsTrue(s.VoiceEnabled);

            Assert.AreEqual(1f, s.MainVolume);
            Assert.AreEqual(1f, s.MusicVolume);
            Assert.AreEqual(1f, s.EffectsVolume);
            Assert.AreEqual(1f, s.VoiceVolume);
        }

        [Test]
        public void MusicVolume_WhenDisabled_ReadsAsMinVolume()
        {
            var s = new AudioSettings();
            s.Reset();
            s.MusicVolume = 0.7f;

            s.MusicEnabled = false;
            Assert.AreEqual(AudioSettings.MIN_VOLUME, s.MusicVolume, 1e-6f);

            s.MusicEnabled = true;
            Assert.AreEqual(0.7f, s.MusicVolume, 1e-6f);
        }

        [Test]
        public void EffectsVolume_SetterIgnoredWhenDisabled()
        {
            var s = new AudioSettings();
            s.Reset();
            s.EffectsVolume = 0.5f;

            s.EffectsEnabled = false;
            s.EffectsVolume = 0.9f; // assignment should be ignored while disabled

            s.EffectsEnabled = true;
            Assert.AreEqual(0.5f, s.EffectsVolume, 1e-6f,
                "Setting volume while disabled should not mutate the stored value.");
        }

        [Test]
        public void MainAndVoiceVolume_BehaveLikeOtherChannels()
        {
            var s = new AudioSettings();
            s.Reset();

            s.MainVolume = 0.3f;
            s.VoiceVolume = 0.6f;
            s.MainEnabled = false;
            s.VoiceEnabled = false;

            Assert.AreEqual(AudioSettings.MIN_VOLUME, s.MainVolume, 1e-6f);
            Assert.AreEqual(AudioSettings.MIN_VOLUME, s.VoiceVolume, 1e-6f);

            s.MainEnabled = true;
            s.VoiceEnabled = true;
            Assert.AreEqual(0.3f, s.MainVolume, 1e-6f);
            Assert.AreEqual(0.6f, s.VoiceVolume, 1e-6f);
        }

        [Test]
        public void Reset_PersistsToDisk()
        {
            var s = new AudioSettings();
            s.Reset();
            Assert.IsTrue(BreezeHelper.FileExists("AudioSettings"),
                "Reset() should write the defaults to persistentDataPath.");
        }

        [Test]
        public void SaveAndLoad_RoundTripsValues()
        {
            var s = new AudioSettings();
            s.Reset();
            s.MusicVolume = 0.42f;
            s.EffectsEnabled = false;
            AudioSettings.Save(s);

            var loaded = AudioSettings.Load();

            Assert.AreEqual(0.42f, loaded.MusicVolume, 1e-6f);
            Assert.IsFalse(loaded.EffectsEnabled);
            // EffectsVolume reads as MIN_VOLUME because the channel is disabled.
            Assert.AreEqual(AudioSettings.MIN_VOLUME, loaded.EffectsVolume, 1e-6f);
        }

        [Test]
        public void Load_NoFile_ReturnsFreshDefaults()
        {
            UnityEngine.TestTools.LogAssert.Expect(
                UnityEngine.LogType.Warning,
                new System.Text.RegularExpressions.Regex(".*does not exist.*"));

            var loaded = AudioSettings.Load();

            Assert.IsTrue(loaded.MainEnabled);
            Assert.AreEqual(1f, loaded.MainVolume);
        }
    }
}
