using UnityEngine;

namespace Breezinstein.Tools.Core
{
    /// <summary>
    /// Base class for BreezeToolBox tools
    /// </summary>
    public abstract class BaseBreezeTool : IBreezeTool
    {
        public abstract string ToolName { get; }
        public abstract string ToolDescription { get; }
        public abstract string Category { get; }
        public virtual int Priority => 100;
        public virtual bool IsEnabled { get; set; } = true;
        
        public abstract UnityEngine.UIElements.VisualElement CreateUI();
        
        public virtual void OnToolSelected() { }
        public virtual void OnToolDeselected() { }
        public virtual void OnSceneGUI() { }
        public virtual void RefreshTool() { }
        public virtual bool CanExecute() => true;
        
        protected void LogOperation(string operation)
        {
            Debug.Log($"[{ToolName}] {operation}");
        }
        
        protected void LogError(string error)
        {
            Debug.LogError($"[{ToolName}] {error}");
        }
        
        protected void LogWarning(string warning)
        {
            Debug.LogWarning($"[{ToolName}] {warning}");
        }
    }
}
