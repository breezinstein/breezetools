using System;
using UnityEngine;

namespace Breezinstein.Tools.Audio
{
    [Serializable]
    public class AudioSettings
    {
        private static readonly string SAVE_KEY = "AudioSettings";
        public const float MIN_VOLUME = 0.0001f;

        public bool MusicEnabled = true;
        public bool EffectsEnabled = true;
        public bool MainEnabled = true;

        private protected float musicVolume = 1f;
        private protected float effectsVolume = 1f;
        private protected float mainVolume = 1f;

        /// <summary>
        /// Gets or sets the music volume.
        /// </summary>
        public float MusicVolume
        {
            get => MusicEnabled ? musicVolume : MIN_VOLUME;
            set => musicVolume = MusicEnabled ? value : musicVolume;
        }

        /// <summary>
        /// Gets or sets the effects volume.
        /// </summary>
        public float EffectsVolume
        {
            get => EffectsEnabled ? effectsVolume : MIN_VOLUME;
            set => effectsVolume = EffectsEnabled ? value : effectsVolume;
        }

        /// <summary>
        /// Gets or sets the main volume.
        /// </summary>
        public float MainVolume
        {
            get => MainEnabled ? mainVolume : MIN_VOLUME;
            set => mainVolume = MainEnabled ? value : mainVolume;
        }

        /// <summary>
        /// Loads the audio settings.
        /// </summary>
        public static AudioSettings Load()
        {
            AudioSettings settings;

            if (BreezeHelper.FileExists(SAVE_KEY))
            {
                settings = BreezeHelper.LoadFile(SAVE_KEY).Deserialize<AudioSettings>();
            }
            else
            {
                Debug.LogWarning($"The file {SAVE_KEY} does not exist. Creating new settings");
                settings = new AudioSettings();
                settings.Reset();
            }

            return settings;
        }

        /// <summary>
        /// Saves the audio settings.
        /// </summary>
        /// <param name="settings">The settings to save.</param>
        public static void Save(AudioSettings settings)
        {
            BreezeHelper.SaveFile(SAVE_KEY, BreezeHelper.Serialize(settings));
        }

        /// <summary>
        /// Resets the audio settings to their defaults.
        /// </summary>
        public void Reset()
        {
            MusicEnabled = true;
            EffectsEnabled = true;
            MainEnabled = true;

            musicVolume = 1;
            effectsVolume = 1;
            mainVolume = 1;

            Save(this);

        }
    }
}