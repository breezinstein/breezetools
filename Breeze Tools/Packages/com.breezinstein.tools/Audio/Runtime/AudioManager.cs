using System;
using UnityEngine;

namespace BreezeTools.Audio
{
    public class AudioManager : MonoBehaviour
    {
        public AudioLibrary audioLibrary;
        public bool musicEnabled = true;
        public bool effectsEnabled = true;
        private static AudioSource effectsSource;
        private static AudioSource musicSource;
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
        }

        public void EnableSource(AudioSourceType sourceType, bool enable)
        {
            switch (sourceType)
            {
                case AudioSourceType.music:
                    musicEnabled = enable;
                    break;
                case AudioSourceType.effect:
                    effectsEnabled = enable;
                    break;
                default:
                    break;
            }
        }

        public void ToggleSource(AudioSourceType sourceType)
        {
            switch (sourceType)
            {
                case AudioSourceType.music:
                    musicEnabled = !musicEnabled;
                    break;
                case AudioSourceType.effect:
                    effectsEnabled = !effectsEnabled;
                    break;
                default:
                    break;
            }
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
            effectsSource.PlayOneShot(item.clip,item.volume);
        }

        public static void PlayMusic(string clipName)
        {
            // TODO play random music
            AudioItem item = GetAudioClip(clipName);
            musicSource.clip = item.clip;
            musicSource.volume = item.volume;
            musicSource.Play();
        }

        public static void PlayRandomMusic()
        {
            // TODO play random music
            musicSource.clip = GetRandomAudioClip();
            musicSource.Play();
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