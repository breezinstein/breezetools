using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

namespace Breezinstein.Tools.Audio
{
    [AddComponentMenu("Breeze's Tools/Audio/Audio UI Slider")]
    [RequireComponent(typeof(Slider))]
    public class AudioSlider : MonoBehaviour
    {

        [SerializeField]
        private AudioManager.AudioSourceType sourceType;
        private Slider slider;

        void Start()
        {
            slider = GetComponent<Slider>();
            slider.minValue = 0.0001f;
            slider.maxValue = 1f;
            slider.onValueChanged.AddListener(SetVolume);
            UpdateUI();
        }

        public void SetVolume(float volume)
        {
            AudioManager.Instance.SetVolume(sourceType, volume);
            UpdateUI();
        }
        void UpdateUI()
        {
            switch (sourceType)
            {
                case AudioManager.AudioSourceType.MUSIC:
                    if (AudioManager.Settings.MusicEnabled)
                    {
                        slider.value = AudioManager.Settings.MusicVolume;
                    }
                    break;
                case AudioManager.AudioSourceType.EFFECT:
                    if (AudioManager.Settings.EffectsEnabled)
                    {
                        slider.value = AudioManager.Settings.EffectsVolume;
                    }
                    break;
                case AudioManager.AudioSourceType.MAIN:
                    if (AudioManager.Settings.MainEnabled)
                    {
                        slider.value = AudioManager.Settings.MainVolume;
                    }
                    break;
                default:
                    break;
            }
        }
    }
}