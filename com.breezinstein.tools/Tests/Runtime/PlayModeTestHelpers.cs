using System;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace Breezinstein.Tools.Tests.PlayMode
{
    /// <summary>
    /// Backs up <c>Application.persistentDataPath/&lt;saveKey&gt;.json</c> on construction
    /// and restores it on Dispose so PlayMode tests don't mutate real save data.
    /// </summary>
    internal sealed class PersistentFileScope : IDisposable
    {
        private readonly string filePath;
        private readonly string backupPath;
        private readonly bool hadOriginal;

        public PersistentFileScope(string saveKey)
        {
            filePath = Path.Combine(Application.persistentDataPath, $"{saveKey}.json");
            backupPath = filePath + ".testbackup";
            hadOriginal = File.Exists(filePath);
            if (hadOriginal)
            {
                if (File.Exists(backupPath)) File.Delete(backupPath);
                File.Move(filePath, backupPath);
            }
        }

        public void Dispose()
        {
            if (File.Exists(filePath)) File.Delete(filePath);
            if (hadOriginal && File.Exists(backupPath))
            {
                File.Move(backupPath, filePath);
            }
        }
    }

    /// <summary>
    /// Reflection helpers used to poke private state on singletons between PlayMode tests
    /// (Unity does not domain-reload between individual tests within a single run).
    /// </summary>
    internal static class TestReflection
    {
        public static void SetStaticField(Type type, string fieldName, object value)
        {
            var field = type.GetField(fieldName,
                BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
            if (field == null)
                throw new InvalidOperationException($"Field '{fieldName}' not found on {type}");
            field.SetValue(null, value);
        }

        public static void SetInstanceField(object target, string fieldName, object value)
        {
            var field = target.GetType().GetField(fieldName,
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            if (field == null)
                throw new InvalidOperationException(
                    $"Field '{fieldName}' not found on {target.GetType()}");
            field.SetValue(target, value);
        }

        public static T GetInstanceField<T>(object target, string fieldName)
        {
            var field = target.GetType().GetField(fieldName,
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            if (field == null)
                throw new InvalidOperationException(
                    $"Field '{fieldName}' not found on {target.GetType()}");
            return (T)field.GetValue(target);
        }
    }

    /// <summary>
    /// Helpers for safely tearing down GameObjects created inside a single test, and
    /// for reflectively resetting <see cref="Singleton{T}"/>'s private static state so
    /// later tests start from a clean slate.
    /// </summary>
    internal static class PlayTestUtil
    {
        public static void SafeDestroy(UnityEngine.Object obj)
        {
            if (obj == null) return;
            if (Application.isPlaying) UnityEngine.Object.Destroy(obj);
            else UnityEngine.Object.DestroyImmediate(obj);
        }

        public static void ResetSingleton<T>() where T : MonoBehaviour
        {
            var type = typeof(Singleton<T>);
            TestReflection.SetStaticField(type, "_instance", null);
            TestReflection.SetStaticField(type, "_quitting", false);
        }
    }
}
