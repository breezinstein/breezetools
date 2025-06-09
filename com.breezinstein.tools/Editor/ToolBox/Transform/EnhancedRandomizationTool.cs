using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using Breezinstein.Tools.Core;

namespace Breezinstein.Tools.TransformTools
{
    public class EnhancedRandomizationTool : BaseBreezeTool
    {
        public override string ToolName => "Enhanced Randomization";
        public override string ToolDescription => "Advanced randomization with noise, seeds, and per-axis control";
        public override string Category => "Transform Tools";
        public override int Priority => 30;
        
        // Position settings
        private Vector3 _minPosition = Vector3.zero;
        private Vector3 _maxPosition = Vector3.one;
        private bool _enablePositionX = true;
        private bool _enablePositionY = true;
        private bool _enablePositionZ = true;
        private bool _useLocalPosition = true;
        
        // Rotation settings
        private Vector3 _minRotation = Vector3.zero;
        private Vector3 _maxRotation = new Vector3(360, 360, 360);
        private bool _enableRotationX = true;
        private bool _enableRotationY = true;
        private bool _enableRotationZ = true;
        
        // Scale settings
        private Vector3 _minScale = Vector3.one;
        private Vector3 _maxScale = Vector3.one * 2f;
        private bool _enableScaleX = true;
        private bool _enableScaleY = true;
        private bool _enableScaleZ = true;
        private bool _uniformScale = true;
        
        // Noise settings
        private bool _usePerlinNoise = false;
        private float _noiseScale = 1f;
        private Vector3 _noiseOffset = Vector3.zero;
        
        // Seed settings
        private bool _useSeed = false;
        private int _seed = 12345;
        
        public override VisualElement CreateUI()
        {
            var container = new VisualElement();
            container.AddToClassList("tool-section");
            
            CreateSeedSection(container);
            CreatePositionSection(container);
            CreateRotationSection(container);
            CreateScaleSection(container);
            CreateNoiseSection(container);
            CreateActionButtons(container);
            
            return container;
        }
        
        private void CreateSeedSection(VisualElement parent)
        {
            var section = new VisualElement();
            section.AddToClassList("tool-section");
            
            var header = new Label("Seed Control");
            header.AddToClassList("tool-section-header");
            section.Add(header);
            
            var seedToggle = new Toggle("Use Seed (Reproducible)");
            seedToggle.value = _useSeed;
            seedToggle.RegisterValueChangedCallback(evt => _useSeed = evt.newValue);
            section.Add(seedToggle);
              var seedField = new IntegerField("Seed");
            seedField.value = _seed;
            seedField.RegisterValueChangedCallback(evt => _seed = evt.newValue);
            section.Add(seedField);
            
            var randomSeedButton = new Button(() => {
                _seed = Random.Range(0, 999999);
                seedField.value = _seed;
            }) { text = "Random Seed" };
            section.Add(randomSeedButton);
            
            parent.Add(section);
        }
        
        private void CreatePositionSection(VisualElement parent)
        {
            var section = new VisualElement();
            section.AddToClassList("tool-section");
            
            var header = new Label("Position Randomization");
            header.AddToClassList("tool-section-header");
            section.Add(header);
            
            // Axis toggles
            var axisRow = new VisualElement();
            axisRow.style.flexDirection = FlexDirection.Row;
            
            var xToggle = new Toggle("X") { value = _enablePositionX };
            xToggle.RegisterValueChangedCallback(evt => _enablePositionX = evt.newValue);
            axisRow.Add(xToggle);
            
            var yToggle = new Toggle("Y") { value = _enablePositionY };
            yToggle.RegisterValueChangedCallback(evt => _enablePositionY = evt.newValue);
            axisRow.Add(yToggle);
            
            var zToggle = new Toggle("Z") { value = _enablePositionZ };
            zToggle.RegisterValueChangedCallback(evt => _enablePositionZ = evt.newValue);
            axisRow.Add(zToggle);
            
            section.Add(axisRow);
            
            var localToggle = new Toggle("Use Local Position");
            localToggle.value = _useLocalPosition;
            localToggle.RegisterValueChangedCallback(evt => _useLocalPosition = evt.newValue);
            section.Add(localToggle);
            
            var minField = new Vector3Field("Min Position");
            minField.value = _minPosition;
            minField.RegisterValueChangedCallback(evt => _minPosition = evt.newValue);
            section.Add(minField);
            
            var maxField = new Vector3Field("Max Position");
            maxField.value = _maxPosition;
            maxField.RegisterValueChangedCallback(evt => _maxPosition = evt.newValue);
            section.Add(maxField);
            
            parent.Add(section);
        }
        
        private void CreateRotationSection(VisualElement parent)
        {
            var section = new VisualElement();
            section.AddToClassList("tool-section");
            
            var header = new Label("Rotation Randomization");
            header.AddToClassList("tool-section-header");
            section.Add(header);
            
            // Axis toggles
            var axisRow = new VisualElement();
            axisRow.style.flexDirection = FlexDirection.Row;
            
            var xToggle = new Toggle("X") { value = _enableRotationX };
            xToggle.RegisterValueChangedCallback(evt => _enableRotationX = evt.newValue);
            axisRow.Add(xToggle);
            
            var yToggle = new Toggle("Y") { value = _enableRotationY };
            yToggle.RegisterValueChangedCallback(evt => _enableRotationY = evt.newValue);
            axisRow.Add(yToggle);
            
            var zToggle = new Toggle("Z") { value = _enableRotationZ };
            zToggle.RegisterValueChangedCallback(evt => _enableRotationZ = evt.newValue);
            axisRow.Add(zToggle);
            
            section.Add(axisRow);
            
            var minField = new Vector3Field("Min Rotation");
            minField.value = _minRotation;
            minField.RegisterValueChangedCallback(evt => _minRotation = evt.newValue);
            section.Add(minField);
            
            var maxField = new Vector3Field("Max Rotation");
            maxField.value = _maxRotation;
            maxField.RegisterValueChangedCallback(evt => _maxRotation = evt.newValue);
            section.Add(maxField);
            
            parent.Add(section);
        }
        
        private void CreateScaleSection(VisualElement parent)
        {
            var section = new VisualElement();
            section.AddToClassList("tool-section");
            
            var header = new Label("Scale Randomization");
            header.AddToClassList("tool-section-header");
            section.Add(header);
            
            var uniformToggle = new Toggle("Uniform Scale");
            uniformToggle.value = _uniformScale;
            uniformToggle.RegisterValueChangedCallback(evt => _uniformScale = evt.newValue);
            section.Add(uniformToggle);
            
            // Axis toggles (only shown if not uniform)
            var axisRow = new VisualElement();
            axisRow.style.flexDirection = FlexDirection.Row;
            axisRow.style.display = _uniformScale ? DisplayStyle.None : DisplayStyle.Flex;
            
            var xToggle = new Toggle("X") { value = _enableScaleX };
            xToggle.RegisterValueChangedCallback(evt => _enableScaleX = evt.newValue);
            axisRow.Add(xToggle);
            
            var yToggle = new Toggle("Y") { value = _enableScaleY };
            yToggle.RegisterValueChangedCallback(evt => _enableScaleY = evt.newValue);
            axisRow.Add(yToggle);
            
            var zToggle = new Toggle("Z") { value = _enableScaleZ };
            zToggle.RegisterValueChangedCallback(evt => _enableScaleZ = evt.newValue);
            axisRow.Add(zToggle);
            
            section.Add(axisRow);
            
            // Update visibility when uniform toggle changes
            uniformToggle.RegisterValueChangedCallback(evt => 
            {
                axisRow.style.display = evt.newValue ? DisplayStyle.None : DisplayStyle.Flex;
            });
            
            var minField = new Vector3Field("Min Scale");
            minField.value = _minScale;
            minField.RegisterValueChangedCallback(evt => _minScale = evt.newValue);
            section.Add(minField);
            
            var maxField = new Vector3Field("Max Scale");
            maxField.value = _maxScale;
            maxField.RegisterValueChangedCallback(evt => _maxScale = evt.newValue);
            section.Add(maxField);
            
            parent.Add(section);
        }
        
        private void CreateNoiseSection(VisualElement parent)
        {
            var section = new VisualElement();
            section.AddToClassList("tool-section");
            
            var header = new Label("Perlin Noise");
            header.AddToClassList("tool-section-header");
            section.Add(header);
            
            var noiseToggle = new Toggle("Use Perlin Noise");
            noiseToggle.value = _usePerlinNoise;
            noiseToggle.RegisterValueChangedCallback(evt => _usePerlinNoise = evt.newValue);
            section.Add(noiseToggle);
            
            var scaleField = new FloatField("Noise Scale");
            scaleField.value = _noiseScale;
            scaleField.RegisterValueChangedCallback(evt => _noiseScale = Mathf.Max(0.01f, evt.newValue));
            section.Add(scaleField);
            
            var offsetField = new Vector3Field("Noise Offset");
            offsetField.value = _noiseOffset;
            offsetField.RegisterValueChangedCallback(evt => _noiseOffset = evt.newValue);
            section.Add(offsetField);
            
            parent.Add(section);
        }
        
        private void CreateActionButtons(VisualElement parent)
        {
            var section = new VisualElement();
            section.AddToClassList("tool-section");
            
            var buttonRow = new VisualElement();
            buttonRow.AddToClassList("button-row");
            
            buttonRow.Add(new Button(() => RandomizePosition()) { text = "Randomize Position" });
            buttonRow.Add(new Button(() => RandomizeRotation()) { text = "Randomize Rotation" });
            section.Add(buttonRow);
            
            var buttonRow2 = new VisualElement();
            buttonRow2.AddToClassList("button-row");
            
            buttonRow2.Add(new Button(() => RandomizeScale()) { text = "Randomize Scale" });
            buttonRow2.Add(new Button(() => RandomizeAll()) { text = "Randomize All" });
            section.Add(buttonRow2);
            
            parent.Add(section);
        }
        
        public override bool CanExecute()
        {
            return Selection.transforms.Length > 0;
        }
        
        private void RandomizePosition()
        {
            var selectedTransforms = Selection.transforms;
            if (selectedTransforms.Length == 0) return;
            
            SetupRandomSeed();
            
            using (var undoScope = new UndoScope("Randomize Position", selectedTransforms.Cast<Object>().ToArray()))
            {
                foreach (var transform in selectedTransforms)
                {
                    var currentPos = _useLocalPosition ? transform.localPosition : transform.position;
                    var newPos = currentPos;
                    
                    if (_usePerlinNoise)
                    {
                        var noisePos = transform.position + _noiseOffset;
                        if (_enablePositionX) newPos.x = Mathf.Lerp(_minPosition.x, _maxPosition.x, Mathf.PerlinNoise(noisePos.x * _noiseScale, noisePos.z * _noiseScale));
                        if (_enablePositionY) newPos.y = Mathf.Lerp(_minPosition.y, _maxPosition.y, Mathf.PerlinNoise(noisePos.y * _noiseScale, noisePos.x * _noiseScale));
                        if (_enablePositionZ) newPos.z = Mathf.Lerp(_minPosition.z, _maxPosition.z, Mathf.PerlinNoise(noisePos.z * _noiseScale, noisePos.y * _noiseScale));
                    }
                    else
                    {
                        if (_enablePositionX) newPos.x = Random.Range(_minPosition.x, _maxPosition.x);
                        if (_enablePositionY) newPos.y = Random.Range(_minPosition.y, _maxPosition.y);
                        if (_enablePositionZ) newPos.z = Random.Range(_minPosition.z, _maxPosition.z);
                    }
                    
                    if (_useLocalPosition)
                        transform.localPosition = newPos;
                    else
                        transform.position = newPos;
                }
            }
            
            BreezeEvents.EmitObjectsTransformed(selectedTransforms);
            LogOperation($"Randomized position for {selectedTransforms.Length} objects");
        }
        
        private void RandomizeRotation()
        {
            var selectedTransforms = Selection.transforms;
            if (selectedTransforms.Length == 0) return;
            
            SetupRandomSeed();
            
            using (var undoScope = new UndoScope("Randomize Rotation", selectedTransforms.Cast<Object>().ToArray()))
            {
                foreach (var transform in selectedTransforms)
                {
                    var currentRot = transform.rotation.eulerAngles;
                    var newRot = currentRot;
                    
                    if (_enableRotationX) newRot.x = Random.Range(_minRotation.x, _maxRotation.x);
                    if (_enableRotationY) newRot.y = Random.Range(_minRotation.y, _maxRotation.y);
                    if (_enableRotationZ) newRot.z = Random.Range(_minRotation.z, _maxRotation.z);
                    
                    transform.rotation = Quaternion.Euler(newRot);
                }
            }
            
            BreezeEvents.EmitObjectsTransformed(selectedTransforms);
            LogOperation($"Randomized rotation for {selectedTransforms.Length} objects");
        }
        
        private void RandomizeScale()
        {
            var selectedTransforms = Selection.transforms;
            if (selectedTransforms.Length == 0) return;
            
            SetupRandomSeed();
            
            using (var undoScope = new UndoScope("Randomize Scale", selectedTransforms.Cast<Object>().ToArray()))
            {
                foreach (var transform in selectedTransforms)
                {
                    var currentScale = transform.localScale;
                    var newScale = currentScale;
                    
                    if (_uniformScale)
                    {
                        var uniformValue = Random.Range(_minScale.x, _maxScale.x);
                        newScale = Vector3.one * uniformValue;
                    }
                    else
                    {
                        if (_enableScaleX) newScale.x = Random.Range(_minScale.x, _maxScale.x);
                        if (_enableScaleY) newScale.y = Random.Range(_minScale.y, _maxScale.y);
                        if (_enableScaleZ) newScale.z = Random.Range(_minScale.z, _maxScale.z);
                    }
                    
                    transform.localScale = newScale;
                }
            }
            
            BreezeEvents.EmitObjectsTransformed(selectedTransforms);
            LogOperation($"Randomized scale for {selectedTransforms.Length} objects");
        }
        
        private void RandomizeAll()
        {
            var selectedTransforms = Selection.transforms;
            if (selectedTransforms.Length == 0) return;
            
            SetupRandomSeed();
            
            using (var undoScope = new UndoScope("Randomize All Transform", selectedTransforms.Cast<Object>().ToArray()))
            {
                RandomizePosition();
                RandomizeRotation();
                RandomizeScale();
            }
            
            LogOperation($"Randomized all transform properties for {selectedTransforms.Length} objects");
        }
        
        private void SetupRandomSeed()
        {
            if (_useSeed)
            {
                Random.InitState(_seed);
            }
        }
    }
}
