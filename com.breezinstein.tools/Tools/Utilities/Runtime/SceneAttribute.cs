using UnityEngine;

namespace Breezinstein.Tools
{
    /// <summary>
    /// Marks a string field as a Scene name selector. The Inspector renders a dropdown of
    /// every scene currently listed in Build Settings instead of a free-form text field,
    /// keeping serialized scene names in sync with the build configuration.
    /// </summary>
    public class SceneAttribute : PropertyAttribute { }
}
