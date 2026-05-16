using UnityEngine;

namespace Breezinstein.Tools.Audio
{
    /// <summary>
    /// Marks a string field as an <see cref="AudioLibrary"/> key, so the Inspector renders a
    /// dropdown of every clip name discovered across all AudioLibrary assets in the project
    /// instead of a free-form text field. Falls back to a text field with a "Key not found"
    /// warning when the current value isn't present in any library.
    /// </summary>
    public class AudioKeyAttribute : PropertyAttribute { }
}
