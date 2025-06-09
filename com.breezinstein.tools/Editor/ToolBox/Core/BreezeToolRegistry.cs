using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Breezinstein.Tools.Core
{
    /// <summary>
    /// Manages registration and execution of BreezeTools
    /// </summary>
    public static class BreezeToolRegistry
    {
        private static Dictionary<string, IBreezeTool> _registeredTools = new Dictionary<string, IBreezeTool>();
        private static Dictionary<string, List<IBreezeTool>> _toolsByCategory = new Dictionary<string, List<IBreezeTool>>();
        
        public static IReadOnlyDictionary<string, IBreezeTool> RegisteredTools => _registeredTools;
        public static IReadOnlyDictionary<string, List<IBreezeTool>> ToolsByCategory => _toolsByCategory;
        
        /// <summary>
        /// Register a tool with the system
        /// </summary>
        public static void RegisterTool(IBreezeTool tool)
        {
            if (tool == null)
            {
                Debug.LogError("Cannot register null tool");
                return;
            }
            
            var toolKey = $"{tool.Category}/{tool.ToolName}";
            
            if (_registeredTools.ContainsKey(toolKey))
            {
                Debug.LogWarning($"Tool {toolKey} is already registered. Replacing...");
            }
            
            _registeredTools[toolKey] = tool;
            
            // Add to category
            if (!_toolsByCategory.ContainsKey(tool.Category))
            {
                _toolsByCategory[tool.Category] = new List<IBreezeTool>();
            }
            
            _toolsByCategory[tool.Category].Add(tool);
            
            // Sort by priority
            _toolsByCategory[tool.Category] = _toolsByCategory[tool.Category]
                .OrderBy(t => t.Priority)
                .ToList();
        }
        
        /// <summary>
        /// Unregister a tool
        /// </summary>
        public static void UnregisterTool(IBreezeTool tool)
        {
            if (tool == null) return;
            
            var toolKey = $"{tool.Category}/{tool.ToolName}";
            _registeredTools.Remove(toolKey);
            
            if (_toolsByCategory.ContainsKey(tool.Category))
            {
                _toolsByCategory[tool.Category].Remove(tool);
                if (_toolsByCategory[tool.Category].Count == 0)
                {
                    _toolsByCategory.Remove(tool.Category);
                }
            }
        }
        
        /// <summary>
        /// Get tool by category and name
        /// </summary>
        public static IBreezeTool GetTool(string category, string name)
        {
            var toolKey = $"{category}/{name}";
            return _registeredTools.TryGetValue(toolKey, out var tool) ? tool : null;
        }
        
        /// <summary>
        /// Get all tools in a category
        /// </summary>
        public static List<IBreezeTool> GetToolsInCategory(string category)
        {
            return _toolsByCategory.TryGetValue(category, out var tools) ? tools : new List<IBreezeTool>();
        }
        
        /// <summary>
        /// Search tools by name or description
        /// </summary>
        public static List<IBreezeTool> SearchTools(string query)
        {
            if (string.IsNullOrEmpty(query))
                return _registeredTools.Values.ToList();
            
            query = query.ToLower();
            
            return _registeredTools.Values
                .Where(tool => tool.ToolName.ToLower().Contains(query) || 
                              tool.ToolDescription.ToLower().Contains(query) ||
                              tool.Category.ToLower().Contains(query))
                .OrderBy(tool => tool.Category)
                .ThenBy(tool => tool.Priority)
                .ToList();
        }
        
        /// <summary>
        /// Auto-discover and register tools
        /// </summary>
        [InitializeOnLoadMethod]
        private static void AutoRegisterTools()
        {
            // Clear existing registrations
            _registeredTools.Clear();
            _toolsByCategory.Clear();
            
            // Find all types implementing IBreezeTool
            var toolTypes = TypeCache.GetTypesDerivedFrom<IBreezeTool>()
                .Where(type => !type.IsAbstract && !type.IsInterface);
            
            foreach (var toolType in toolTypes)
            {
                try
                {
                    var tool = (IBreezeTool)Activator.CreateInstance(toolType);
                    RegisterTool(tool);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Failed to register tool {toolType.Name}: {ex.Message}");
                }
            }
            
            Debug.Log($"BreezeToolBox: Registered {_registeredTools.Count} tools across {_toolsByCategory.Count} categories");
        }
    }
}
