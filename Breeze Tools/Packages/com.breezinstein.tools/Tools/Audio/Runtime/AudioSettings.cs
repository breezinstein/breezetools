using System;
using UnityEngine;

namespace Breezinstein.Tools.Audio
{
    [Serializable]
    public class AudioSettings
    {
        private static string saveKey = "AudioSettings";
        public bool MusicEnabled = true;
        public bool EffectsEnabled = true;
        public bool MainEnabled = true;

        private float musicVolume = 1;
        private float effectsVolume = 1;
        private float mainVolume = 1;

        public float MusicVolume { get => MusicEnabled ? musicVolume : 0.0001f; set => musicVolume = value; }
        public float EffectsVolume { get => EffectsEnabled ? effectsVolume : 0.0001f; set => effectsVolume = value; }
        public float MainVolume { get => MainEnabled ? mainVolume : 0.0001f; set => mainVolume = value; }

        public static AudioSettings Load()
        {
            AudioSettings settings = null;
            if (PlayerPrefs.HasKey(saveKey))
            {
                settings = new AudioSettings();
                settings = PlayerPrefs.GetString(saveKey).Deserialize<AudioSettings>();
            }
            if (settings == null)
            {
                settings = new AudioSettings();
                AudioSettings.Save(settings);
            }
            return settings;
        }

        public static void Save(AudioSettings settings)
        {
            string saveSerialized = BreezeHelper.Serialize<AudioSettings>(settings);
            PlayerPrefs.SetString(saveKey, saveSerialized);
            PlayerPrefs.Save();
        }
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