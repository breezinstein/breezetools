using System;
using UnityEngine;

namespace Breezinstein.Tools.Audio
{
    [Serializable]
    public class AudioSettings
    {
        private static string saveKey = "AudioSettings";
        public const float minVolume = 0.0001f;

        public bool MusicEnabled = true;
        public bool EffectsEnabled = true;
        public bool MainEnabled = true;

        protected float musicVolume = 1f;
        protected float effectsVolume = 1f;
        protected float mainVolume = 1f;

        public float MusicVolume { get => MusicEnabled ? musicVolume : minVolume; set => musicVolume = MusicEnabled ? value : musicVolume; }
        public float EffectsVolume { get => EffectsEnabled ? effectsVolume : minVolume; set => effectsVolume = EffectsEnabled ? value : effectsVolume; }
        public float MainVolume { get => MainEnabled ? mainVolume : minVolume; set => mainVolume = MainEnabled ? value : mainVolume; }

        public static AudioSettings Load()
        {
            AudioSettings settings = new AudioSettings();

            if (BreezeHelper.FileExists(saveKey))
            {
                settings = BreezeHelper.LoadFile(saveKey).Deserialize<AudioSettings>();
            }
            else
            {
                Debug.Log("The file " + saveKey + "does not exists");
                settings = new AudioSettings();
            }
            Debug.Log("Audio Settings Loaded");
            return settings;
        }

        public static void Save(AudioSettings settings)
        {
            BreezeHelper.SaveFile(saveKey, BreezeHelper.Serialize(settings));
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