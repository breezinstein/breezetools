using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace Breezinstein.Tools.Audio
{
    /// <summary>
    /// Manages all audio related functionality.
    /// </summary>
    [AddComponentMenu("Breeze's Tools/Audio/Audio Manager")]
    public class AudioManager : MonoBehaviour
    {
        private static AudioSettings m_Settings;

        /// <summary>
        /// Gets or sets the audio settings.
        /// </summary>
        public static AudioSettings Settings
        {
            get
            {
                if (m_Settings == null)
                {
                    m_Settings = AudioSettings.Load();
                }

                return m_Settings;
            }
            set
            {
                m_Settings = value;
                AudioSettings.Save(m_Settings);
            }
        }

        public AudioLibrary AudioLibrary;

        private AudioSource m_EffectsSource;
        private AudioSource m_MusicSource;

        [SerializeField] private AudioMixer m_AudioMixer;
        private AudioMixerGroup m_MusicGroup;
        private AudioMixerGroup m_EffectsGroup;

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

            m_MusicGroup = m_AudioMixer.FindMatchingGroups("BGM")[0];
            m_EffectsGroup = m_AudioMixer.FindMatchingGroups("SFX")[0];

            m_EffectsSource = CreateAudioSource(AudioSourceType.EFFECT);
            m_MusicSource = CreateAudioSource(AudioSourceType.MUSIC);
            UpdateVolumes();

        }

        /// <summary>
        /// Updates the volume of the audio sources.
        /// </summary>
        public void UpdateVolumes()
        {
            m_AudioMixer.SetFloat("MainVolume", Mathf.Log10(Settings.MainVolume) * 20);
            m_MusicGroup.audioMixer.SetFloat("BGMVolume", Mathf.Log10(Settings.MusicVolume) * 20);
            m_EffectsGroup.audioMixer.SetFloat("SFXVolume", Mathf.Log10(Settings.EffectsVolume) * 20);
        }

        /// <summary>
        /// Sets the volume of the specified audio source.
        /// </summary>
        public void SetVolume(AudioSourceType sourceType, float volume)
        {
            volume = Mathf.Clamp(volume, AudioSettings.MIN_VOLUME, 1f);
            switch (sourceType)
            {
                case AudioSourceType.MUSIC:
                    Settings.MusicVolume = volume;
                    break;
                case AudioSourceType.EFFECT:
                    Settings.EffectsVolume = volume;
                    break;
                case AudioSourceType.MAIN:
                    Settings.MainVolume = volume;
                    break;
                default:
                    break;
            }
            AudioSettings.Save(Settings);
            UpdateVolumes();
        }

        /// <summary>
        /// Toggles the specified audio source.
        /// </summary>
        public void ToggleSource(AudioSourceType sourceType)
        {
            switch (sourceType)
            {
                case AudioSourceType.MUSIC:
                    Settings.MusicEnabled = !Settings.MusicEnabled;
                    break;
                case AudioSourceType.EFFECT:
                    Settings.EffectsEnabled = !Settings.EffectsEnabled;
                    break;
                case AudioSourceType.MAIN:
                    Settings.MainEnabled = !Settings.MainEnabled;
                    break;
                default:
                    break;
            }
            AudioSettings.Save(Settings);
            UpdateVolumes();
        }

        /// <summary>
        /// Creates an audio source of the specified type.
        /// </summary>
        private AudioSource CreateAudioSource(AudioSourceType sourceType)
        {
            AudioSource source = new GameObject().AddComponent<AudioSource>();
            source.transform.parent = transform;
            switch (sourceType)
            {
                case AudioSourceType.MUSIC:
                    source.name = "music source";
                    source.playOnAwake = true;
                    source.loop = true;
                    source.outputAudioMixerGroup = m_MusicGroup;
                    break;
                case AudioSourceType.EFFECT:
                    source.name = "effects source";
                    source.playOnAwake = false;
                    source.outputAudioMixerGroup = m_EffectsGroup;
                    break;
                default:
                    break;
            }
            return source;
        }

        /// <summary>
        /// Plays a sound effect with the specified clip name.
        /// </summary>
        public static void PlaySoundEffect(string clipName)
        {
            Instance.UpdateVolumes();
            AudioItem item = GetAudioClip(clipName);
            Instance.m_EffectsSource.PlayOneShot(item.clip, item.volume);
        }

        /// <summary>
        /// Plays music with the specified clip name.
        /// </summary>
        public static void PlayMusic(string clipName)
        {
            Instance.UpdateVolumes();
            AudioItem item = GetAudioClip(clipName);
            Instance.m_MusicSource.clip = item.clip;
            Instance.m_MusicSource.volume = item.volume;
            Instance.m_MusicSource.Play();
        }

        /// <summary>
        /// Plays random music.
        /// </summary>
        public static void PlayRandomMusic()
        {
            Instance.UpdateVolumes();
            // TODO play random music
            Instance.m_MusicSource.clip = GetRandomAudioClip(AudioCategory.MUSIC);
            Instance.m_MusicSource.Play();
        }

        /// <summary>
        /// Stops the music.
        /// </summary>
        public static void StopMusic()
        {
            Instance.m_MusicSource.Stop();
        }

        /// <summary>
        /// Gets a random audio clip of the specified category.
        /// </summary>
        private static AudioClip GetRandomAudioClip(AudioCategory category)
        {
            //Get all clips of that category
            List<AudioItem> clips = new List<AudioItem>();
            foreach (var item in Instance.AudioLibrary.clips)
            {
                if (item.Value.category == category)
                {
                    clips.Add(item.Value);
                }
            }

            return clips[UnityEngine.Random.Range(0, clips.Count)].clip;
        }

        /// <summary>
        /// Gets an audio clip with the specified name.
        /// </summary>
        private static AudioItem GetAudioClip(string clipName)
        {
            AudioItem audioItem;
            if (Instance.AudioLibrary.clips.TryGetValue(clipName, out audioItem))
            {
                return audioItem;
            }
            else
            {
                Debug.LogError("Audio clip not found: " + clipName);
                return null;
            }
        }

        /// <summary>
        /// Enum for audio source types.
        /// </summary>
        public enum AudioSourceType
        {
            MUSIC = 0,
            EFFECT,
            MAIN
        }
    }
}