using UnityEngine;

namespace Breezinstein.Tools.Audio
{
    [CreateAssetMenu(fileName = "New Audio Library", menuName = "Breeze Tools/Audio/Create Audio Library")]
    public class AudioLibrary : ScriptableObject
    {
        public SerializableDictionary<string, AudioItem> clips = new SerializableDictionary<string, AudioItem>();
    }

    [System.Serializable]
    public class AudioItem
    {
        public AudioCategory category;
        public AudioClip clip;
        [Range(0.0f, 1.0f)]
        public float volume = 1f;
    }

    public enum AudioCategory { SFX, MUSIC }
}