using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Breezinstein.Tools.Audio
{
    /// <summary>
    /// Represents a UI toggle for controlling audio sources.
    /// </summary>
    [AddComponentMenu("Breeze's Tools/Audio/Audio UI Toggle")]
    public class AudioUIToggle : MonoBehaviour
    {
        [SerializeField]
        private AudioManager.AudioSourceType sourceType;
        [SerializeField]
        private Image buttonIcon;
        [SerializeField]
        private Sprite onSprite;
        [SerializeField]
        private Sprite offSprite;

        /// <summary>
        /// Initializes the UI toggle and updates its state.
        /// </summary>
        void Start()
        {
            UpdateUI();
        }

        /// <summary>
        /// Toggles the audio source and updates the UI.
        /// </summary>
        public void ToggleAudio()
        {
            AudioManager.Instance.ToggleSource(sourceType);
            UpdateUI();
        }

        /// <summary>
        /// Updates the UI based on the current audio source state.
        /// </summary>
        void UpdateUI()
        {
            switch (sourceType)
            {
                case AudioManager.AudioSourceType.MUSIC:
                    buttonIcon.sprite = AudioManager.Settings.MusicEnabled ? onSprite : offSprite;
                    break;
                case AudioManager.AudioSourceType.EFFECT:
                    buttonIcon.sprite = AudioManager.Settings.EffectsEnabled ? onSprite : offSprite;
                    break;
                case AudioManager.AudioSourceType.MAIN:
                    buttonIcon.sprite = AudioManager.Settings.MainEnabled ? onSprite : offSprite;
                    break;
                default:
                    break;
            }
        }
    }
}