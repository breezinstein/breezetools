using System;
using UnityEditor;
using UnityEngine;

namespace Breezinstein.Tools.Core
{
    /// <summary>
    /// Centralized undo scope for all BreezeToolBox operations
    /// </summary>
    public class UndoScope : IDisposable
    {
        private string _operationName;
        private UnityEngine.Object[] _targets;
        private bool _recordObjects;
        
        public UndoScope(string operationName, UnityEngine.Object[] targets = null, bool recordObjects = true)
        {
            _operationName = operationName;
            _targets = targets;
            _recordObjects = recordObjects;
            
            if (_recordObjects && _targets != null && _targets.Length > 0)
            {
                Undo.RecordObjects(_targets, _operationName);
            }
            else if (_recordObjects)
            {
                Undo.IncrementCurrentGroup();
                Undo.SetCurrentGroupName(_operationName);
            }
        }
        
        public void Dispose()
        {
            if (_recordObjects)
            {
                Undo.CollapseUndoOperations(Undo.GetCurrentGroup());
            }
        }
        
        /// <summary>
        /// Register created object for undo
        /// </summary>
        public void RegisterCreatedObject(UnityEngine.Object obj, string name = null)
        {
            Undo.RegisterCreatedObjectUndo(obj, name ?? _operationName);
        }
        
        /// <summary>
        /// Register object for destruction with undo
        /// </summary>
        public void DestroyObject(UnityEngine.Object obj)
        {
            Undo.DestroyObjectImmediate(obj);
        }
    }
}
