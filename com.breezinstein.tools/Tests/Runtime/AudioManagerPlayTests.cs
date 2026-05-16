using System.Collections;
using System.Reflection;
using Breezinstein.Tools.Audio;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.TestTools;

namespace Breezinstein.Tools.Tests.PlayMode
{
    public class AudioManagerPlayTests
    {
        private GameObject managerGo;
        private PersistentFileScope settingsScope;
        private AudioClip silentClip;

        [SetUp]
        public void SetUp()
        {
            settingsScope = new PersistentFileScope("AudioSettings");
            // Forget cached settings + Instance between tests so each one fully re-Awakes.
            TestReflection.SetStaticField(typeof(AudioManager), "m_Settings", null);
            AudioManager.Instance = null;
        }

        [TearDown]
        public void TearDown()
        {
            if (managerGo != null) Object.Destroy(managerGo);
            if (silentClip != null) Object.Destroy(silentClip);
            AudioManager.Instance = null;
            TestReflection.SetStaticField(typeof(AudioManager), "m_Settings", null);
            settingsScope?.Dispose();
            settingsScope = null;
        }

        private AudioManager BuildManager(AudioLibrary library)
        {
            var mixer = Resources.Load<AudioMixer>("Mixers/DefaultMixer");
            Assert.IsNotNull(mixer,
                "DefaultMixer.mixer must be present under Tools/Audio/Runtime/Resources/Mixers/ for AudioManager PlayMode tests.");

            managerGo = new GameObject("AudioManagerUnderTest");
            managerGo.SetActive(false); // delay Awake until fields are set

            var manager = managerGo.AddComponent<AudioManager>();
            manager.AudioLibrary = library;

            var mixerField = typeof(AudioManager).GetField("m_AudioMixer",
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(mixerField, "AudioManager.m_AudioMixer field not found.");
            mixerField.SetValue(manager, mixer);

            managerGo.SetActive(true); // now Awake runs with all fields populated
            return manager;
        }

        private AudioLibrary CreateLibraryWithClip(string clipName, out AudioClip clip)
        {
            var library = ScriptableObject.CreateInstance<AudioLibrary>();
            clip = AudioClip.Create(clipName, 44100, 1, 44100, false);
            silentClip = clip;
            library.clips[clipName] = new AudioItem
            {
                category = AudioManager.AudioSourceType.EFFECT,
                clip = clip,
                volume = 1f,
            };
            return library;
        }

        [UnityTest]
        public IEnumerator Awake_SetsInstance_AndCreatesAudioSources()
        {
            var library = CreateLibraryWithClip("test_sfx", out _);
            var manager = BuildManager(library);

            yield return null;

            Assert.AreSame(manager, AudioManager.Instance);
            var sources = managerGo.GetComponentsInChildren<AudioSource>(true);
            Assert.GreaterOrEqual(sources.Length, 3,
                "Awake should create at least Music/Effects/Voice sources plus the SFX pool.");
        }

        [UnityTest]
        public IEnumerator PlaySoundEffect_ResolvesClip_AndAssignsToASource()
        {
            var library = CreateLibraryWithClip("test_sfx", out var clip);
            BuildManager(library);

            yield return null;

            AudioManager.PlaySoundEffect("test_sfx");

            var sources = managerGo.GetComponentsInChildren<AudioSource>(true);
            bool found = false;
            foreach (var source in sources)
            {
                if (source.clip == clip)
                {
                    found = true;
                    break;
                }
            }
            Assert.IsTrue(found, "An AudioSource in the pool should have received the requested clip.");
        }

        [UnityTest]
        public IEnumerator PlaySoundEffect_OnUnknownClip_LogsError_WithoutThrowing()
        {
            var library = CreateLibraryWithClip("known", out _);
            BuildManager(library);

            yield return null;

            LogAssert.Expect(LogType.Error, "Audio clip not found: unknown");
            Assert.DoesNotThrow(() => AudioManager.PlaySoundEffect("unknown"));
        }

        [UnityTest]
        public IEnumerator SetVolume_PersistsThroughSettings()
        {
            var library = CreateLibraryWithClip("test_sfx", out _);
            var manager = BuildManager(library);

            yield return null;

            manager.SetVolume(AudioManager.AudioSourceType.EFFECT, 0.5f);

            Assert.AreEqual(0.5f, AudioManager.Settings.EffectsVolume, 0.001f);
        }

        [UnityTest]
        public IEnumerator ToggleSource_FlipsEnabledFlag()
        {
            var library = CreateLibraryWithClip("test_sfx", out _);
            var manager = BuildManager(library);

            yield return null;

            bool initial = AudioManager.Settings.MusicEnabled;
            manager.ToggleSource(AudioManager.AudioSourceType.MUSIC);

            Assert.AreEqual(!initial, AudioManager.Settings.MusicEnabled);
        }

        [UnityTest]
        public IEnumerator SfxPool_GrowsOnDemand_WhenAllSourcesBusy()
        {
            var library = CreateLibraryWithClip("test_sfx", out _);
            BuildManager(library);

            yield return null;

            var poolField = typeof(AudioManager).GetField("m_SfxPool",
                BindingFlags.Instance | BindingFlags.NonPublic);
            var initialPool = (System.Collections.IList)poolField.GetValue(AudioManager.Instance);
            int initialSize = initialPool.Count;

            for (int i = 0; i < initialSize + 2; i++)
            {
                AudioManager.PlaySoundEffect("test_sfx");
            }

            var grownPool = (System.Collections.IList)poolField.GetValue(AudioManager.Instance);
            Assert.Greater(grownPool.Count, initialSize,
                "Pool should grow when more concurrent SFX are requested than initial capacity.");
        }
    }
}
