using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

namespace Breezinstein.Tools.Audio
{
    [AddComponentMenu("Breeze's Tools/Audio/Audio Manager")]
    public class AudioManager : MonoBehaviour
    {
        private static AudioSettings settings;
        public static AudioSettings Settings
        {
            get
            {
                if (settings == null)
                {
                    settings = AudioSettings.Load();
                }

                return settings;
            }
            set
            {
                settings = value;
                AudioSettings.Save(settings);
            }
        }

        public AudioLibrary audioLibrary;
        private AudioSource effectsSource;
        private AudioSource musicSource;

        public AudioMixer audioMixer;

        public static AudioManager Instance;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(this.gameObject);
            }
            DontDestroyOnLoad(this.gameObject);
            effectsSource = CreateAudioSource(AudioSourceType.effect);
            musicSource = CreateAudioSource(AudioSourceType.music);

            // Load music and effects settings
            AudioSettings.Load();
        }

        private void OnSceneWasLoaded(Scene arg0, LoadSceneMode arg1)
        {
            UpdateVolumes();
        }

        public void UpdateVolumes()
        {
            audioMixer.SetFloat("MainVolume", Mathf.Log10(settings.MainVolume) * 20);
            audioMixer.FindMatchingGroups("BGM")[0].audioMixer.SetFloat("BGMVolume", Mathf.Log10(settings.MusicVolume) * 20);
            audioMixer.FindMatchingGroups("SFX")[0].audioMixer.SetFloat("SFXVolume", Mathf.Log10(settings.EffectsVolume) * 20);
        }

        public void SetVolume(AudioSourceType sourceType, float volume)
        {
            volume = Mathf.Clamp(volume, 0.0001f, 1);
            switch (sourceType)
            {
                case AudioSourceType.music:
                    Settings.MusicVolume = volume;
                    break;
                case AudioSourceType.effect:
                    Settings.EffectsVolume = volume;
                    break;
                case AudioSourceType.main:
                    Settings.MainVolume = volume;
                    break;
                default:
                    break;
            }
            AudioSettings.Save(settings);
            UpdateVolumes();
        }


        public void ToggleSource(AudioSourceType sourceType)
        {
            switch (sourceType)
            {
                case AudioSourceType.music:
                    Settings.MusicEnabled = !Settings.MusicEnabled;
                    break;
                case AudioSourceType.effect:
                    Settings.EffectsEnabled = !Settings.EffectsEnabled;
                    break;
                case AudioSourceType.main:
                    Settings.MainEnabled = !Settings.MainEnabled;
                    break;
                default:
                    break;
            }
            UpdateVolumes();
        }

        private AudioSource CreateAudioSource(AudioSourceType sourceType)
        {
            AudioSource source = new GameObject().AddComponent<AudioSource>();
            source.transform.parent = transform;
            switch (sourceType)
            {
                case AudioSourceType.music:
                    source.name = "music source";
                    source.playOnAwake = true;
                    source.loop = true;
                    source.outputAudioMixerGroup = audioMixer.FindMatchingGroups("BGM")[0];
                    break;
                case AudioSourceType.effect:
                    source.name = "effects source";
                    source.playOnAwake = false;
                    source.outputAudioMixerGroup = audioMixer.FindMatchingGroups("SFX")[0];
                    break;
                default:
                    break;
            }
            return source;
        }

        public static void PlaySoundEffect(string clipName)
        {
            AudioItem item = GetAudioClip(clipName);
            Instance.effectsSource.PlayOneShot(item.clip, item.volume);
        }

        public static void PlayMusic(string clipName)
        {
            // TODO play random music
            AudioItem item = GetAudioClip(clipName);
            Instance.musicSource.clip = item.clip;
            Instance.musicSource.volume = item.volume;
            Instance.musicSource.Play();
        }

        public static void PlayRandomMusic()
        {
            // TODO play random music
            Instance.musicSource.clip = GetRandomAudioClip(AudioCategory.MUSIC);
            Instance.musicSource.Play();
        }

        public static void StopMusic()
        {
            Instance.musicSource.Stop();
        }


        private static AudioClip GetRandomAudioClip(AudioCategory category)
        {
            //Get all clips of that category
            List<AudioItem> clips = new List<AudioItem>();
            foreach (var item in Instance.audioLibrary.clips)
            {
                if (item.Value.category == category)
                {
                    clips.Add(item.Value);
                }
            }

            return clips[UnityEngine.Random.Range(0, clips.Count)].clip;
        }

        private static AudioItem GetAudioClip(string clipName)
        {
            AudioItem audioItem;
            Instance.audioLibrary.clips.TryGetValue(clipName, out audioItem);
            return audioItem;
        }

        public enum AudioSourceType
        {
            music = 0,
            effect,
            main
        }


    }
}