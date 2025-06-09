using UnityEngine;
using UnityEngine.UIElements;

namespace Breezinstein.Tools.Core
{
    /// <summary>
    /// Interface for all BreezeToolBox modules/tools
    /// </summary>
    public interface IBreezeTool
    {
        string ToolName { get; }
        string ToolDescription { get; }
        string Category { get; }
        int Priority { get; }
        bool IsEnabled { get; set; }
        
        /// <summary>
        /// Creates the UI elements for this tool
        /// </summary>
        VisualElement CreateUI();
        
        /// <summary>
        /// Called when tool is selected/activated
        /// </summary>
        void OnToolSelected();
        
        /// <summary>
        /// Called when tool is deselected
        /// </summary>
        void OnToolDeselected();
        
        /// <summary>
        /// Called during scene GUI for tools that need scene interaction
        /// </summary>
        void OnSceneGUI();
        
        /// <summary>
        /// Called to refresh tool state
        /// </summary>
        void RefreshTool();
        
        /// <summary>
        /// Validates if tool can be used with current selection
        /// </summary>
        bool CanExecute();
    }
}
