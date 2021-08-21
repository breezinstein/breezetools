using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Breezinstein.Tools.Audio
{
    [AddComponentMenu("Breeze's Tools/Audio/Audio UI Toggle")]
    public class AudioUIButton : MonoBehaviour
    {
        [SerializeField]
        private AudioManager.AudioSourceType sourceType;
        [SerializeField]
        private Image buttonIcon;
        [SerializeField]
        private Sprite onSprite;
        [SerializeField]
        private Sprite offSprite;

        // Use this for initialization
        void Start()
        {
            UpdateUI();
        }

        public void ToggleAudio()
        {
            AudioManager.Instance.ToggleSource(sourceType);
            UpdateUI();
        }

        void UpdateUI()
        {
            switch (sourceType)
            {
                case AudioManager.AudioSourceType.music:
                    buttonIcon.sprite = AudioManager.MusicEnabled ? onSprite : offSprite;
                    break;
                case AudioManager.AudioSourceType.effect:
                    buttonIcon.sprite = AudioManager.EffectsEnabled ? onSprite : offSprite;
                    break;
                default:
                    break;
            }
        }
    }
}