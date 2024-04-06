using UnityEngine;
/// <summary>
/// Represents an audio library that stores audio clips.
/// </summary>
namespace Breezinstein.Tools.Audio
{
    [CreateAssetMenu(fileName = "New Audio Library", menuName = "Breeze Tools/Audio/Create Audio Library")]
    public class AudioLibrary : ScriptableObject
    {
        /// <summary>
        /// Dictionary that maps audio clip names to audio items.
        /// </summary>
        public SerializableDictionary<string, AudioItem> clips = new SerializableDictionary<string, AudioItem>();
    }

    [System.Serializable]
    public class AudioItem
    {
        /// <summary>
        /// The category of the audio item.
        /// </summary>
        public AudioCategory category;

        /// <summary>
        /// The audio clip associated with the audio item.
        /// </summary>
        public AudioClip clip;

        /// <summary>
        /// The volume of the audio item.
        /// </summary>
        [Range(0.0f, 1.0f)]
        public float volume = 1f;
    }

    /// <summary>
    /// Represents the category of an audio item.
    /// </summary>
    public enum AudioCategory { EFFECT, MUSIC }
}