using System.IO;
using NUnit.Framework;
using UnityEngine;

namespace Breezinstein.Tools.Tests
{
    /// <summary>
    /// Shared helpers for tests that touch <c>Application.persistentDataPath</c>.
    /// Each instance backs up the file at construction and restores it on Dispose,
    /// so tests don't trample a real user's save file or each other's writes.
    /// </summary>
    internal sealed class PersistentFileScope : System.IDisposable
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

        public string FilePath => filePath;
        public bool FileExists => File.Exists(filePath);

        public void Dispose()
        {
            if (File.Exists(filePath)) File.Delete(filePath);
            if (hadOriginal && File.Exists(backupPath))
            {
                File.Move(backupPath, filePath);
            }
        }
    }
}
