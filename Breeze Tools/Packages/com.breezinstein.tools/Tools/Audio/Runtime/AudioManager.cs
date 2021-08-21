using System;
using UnityEngine;

namespace Breezinstein.Tools.Audio
{
    [AddComponentMenu("Breeze's Tools/Audio/Audio Manager")]
    public class AudioManager : MonoBehaviour
    {
        public AudioLibrary audioLibrary;
        public static bool MusicEnabled = true;
        public static bool EffectsEnabled = true;
        private AudioSource effectsSource;
        private AudioSource musicSource;
        public static AudioManager Instance;

        // Save key consts
        private const string musicEnabledKey = "musicEnabled";
        private const string effectsEnabledKey = "effectsEnabled";

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
            LoadSettings();
        }
        private void LoadSettings()
        {
            MusicEnabled = Convert.ToBoolean(PlayerPrefs.GetInt(musicEnabledKey, 1));
            EffectsEnabled = Convert.ToBoolean(PlayerPrefs.GetInt(effectsEnabledKey, 1));
            UpdateSourceMute();
        }

        private void SaveSettings()
        {
            PlayerPrefs.SetInt(musicEnabledKey, Convert.ToInt32(MusicEnabled));
            PlayerPrefs.SetInt(effectsEnabledKey, Convert.ToInt32(EffectsEnabled));
            PlayerPrefs.Save();
            UpdateSourceMute();
        }

        private void UpdateSourceMute()
        {
            musicSource.mute = !MusicEnabled;
            effectsSource.mute = !EffectsEnabled;
        }
        public void EnableSource(AudioSourceType sourceType, bool enable)
        {
            switch (sourceType)
            {
                case AudioSourceType.music:
                    MusicEnabled = enable;
                    break;
                case AudioSourceType.effect:
                    EffectsEnabled = enable;
                    break;
                default:
                    break;
            }
            SaveSettings();
        }

        public void ToggleSource(AudioSourceType sourceType)
        {
            switch (sourceType)
            {
                case AudioSourceType.music:
                    MusicEnabled = !MusicEnabled;
                    break;
                case AudioSourceType.effect:
                    EffectsEnabled = !EffectsEnabled;
                    break;
                default:
                    break;
            }
            SaveSettings();
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
                    
                    //TODO configure source;
                    break;
                case AudioSourceType.effect:
                    source.name = "effects source";
                    source.playOnAwake = false;
                    
                    //TODO configure source;
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
            Instance.musicSource.clip = GetRandomAudioClip();
            Instance.musicSource.Play();
        }

        //TODO
        private static AudioClip GetRandomAudioClip()
        {
            throw new NotImplementedException();
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
            effect
        }
    }
}