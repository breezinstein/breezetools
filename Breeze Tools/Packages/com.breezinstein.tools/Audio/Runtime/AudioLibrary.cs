using BreezeTools.Utils;
using UnityEngine;

namespace BreezeTools.Audio
{
    [CreateAssetMenu(fileName = "New Audio Library", menuName = "Breeze Tools/Audio/Create Audio Library")]
    public class AudioLibrary : ScriptableObject
    {
        public SerializableDictionary<string, AudioItem> clips = new SerializableDictionary<string, AudioItem>();
    }

    [System.Serializable]
    public class AudioItem
    {
        public AudioClip clip;
        [Range(0.0f,1.0f)]
        public float volume = 1f;
    }
}