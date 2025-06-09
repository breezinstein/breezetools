using System;
using UnityEngine;

namespace Breezinstein.Tools.Core
{
    /// <summary>
    /// Event system for BreezeToolBox operations
    /// </summary>
    public static class BreezeEvents
    {        // Transform events
        public static event Action<UnityEngine.Transform[]> OnObjectsTransformed;
        public static event Action<UnityEngine.Transform[]> OnObjectsReplaced;
        public static event Action<UnityEngine.Transform[]> OnObjectsRenamed;
        public static event Action<UnityEngine.Transform[]> OnObjectsGrouped;
        
        // Asset events
        public static event Action<UnityEngine.Object[]> OnMaterialsReplaced;
        public static event Action<UnityEngine.Object[]> OnFontsReplaced;
        public static event Action<GameObject[]> OnPrefabsUpdated;
        
        // Scene events
        public static event Action<string> OnSceneOperationStarted;
        public static event Action<string, bool> OnSceneOperationCompleted;
        
        // Component events
        public static event Action<Component[]> OnComponentsAdded;
        public static event Action<Component[]> OnComponentsRemoved;
        public static event Action<Component[]> OnComponentsModified;
          // Selection events
        public static event Action<UnityEngine.Transform[]> OnSelectionChanged;
        
        // Emit methods
        public static void EmitObjectsTransformed(UnityEngine.Transform[] objects) => OnObjectsTransformed?.Invoke(objects);
        public static void EmitObjectsReplaced(UnityEngine.Transform[] objects) => OnObjectsReplaced?.Invoke(objects);
        public static void EmitObjectsRenamed(UnityEngine.Transform[] objects) => OnObjectsRenamed?.Invoke(objects);
        public static void EmitObjectsGrouped(UnityEngine.Transform[] objects) => OnObjectsGrouped?.Invoke(objects);
        public static void EmitMaterialsReplaced(UnityEngine.Object[] materials) => OnMaterialsReplaced?.Invoke(materials);
        public static void EmitFontsReplaced(UnityEngine.Object[] fonts) => OnFontsReplaced?.Invoke(fonts);
        public static void EmitPrefabsUpdated(GameObject[] prefabs) => OnPrefabsUpdated?.Invoke(prefabs);
        public static void EmitSceneOperationStarted(string operation) => OnSceneOperationStarted?.Invoke(operation);
        public static void EmitSceneOperationCompleted(string operation, bool success) => OnSceneOperationCompleted?.Invoke(operation, success);
        public static void EmitComponentsAdded(Component[] components) => OnComponentsAdded?.Invoke(components);
        public static void EmitComponentsRemoved(Component[] components) => OnComponentsRemoved?.Invoke(components);
        public static void EmitComponentsModified(Component[] components) => OnComponentsModified?.Invoke(components);
        public static void EmitSelectionChanged(UnityEngine.Transform[] selection) => OnSelectionChanged?.Invoke(selection);
    }
}
