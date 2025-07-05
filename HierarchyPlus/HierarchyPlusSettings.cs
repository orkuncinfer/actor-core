using UnityEngine;
using System;
using System.Collections.Generic;

namespace HierarchyPlus
{
    [CreateAssetMenu(fileName = "HierarchyPlusSettings", menuName = "HierarchyPlus/Settings")]
    public class HierarchyPlusSettings : ScriptableObject
    {
        private const string RESOURCE_PATH = "HierarchyPlusSettings";
        private const string RESOURCE_FOLDER = "Resources";
        
        [Header("Hierarchy Line Settings")]
        [SerializeField] private bool _enableHierarchyLines = true;
        [SerializeField] private Color _lineColor = new Color(0.5f, 0.5f, 0.5f, 0.3f);
        [SerializeField] private LineStyle _lineStyle = LineStyle.Straight;
        [SerializeField] private float _lineWidth = 1f;
        
        [Header("Row Settings")]
        [SerializeField] private bool _showAlternateRowBackgrounds = false;
        [SerializeField] private Color _alternateRowColor = new Color(0f, 0f, 0f, 0.05f);
        [SerializeField] private bool _showSeparatorLines = false;
        [SerializeField] private Color _separatorLineColor = new Color(0f, 0f, 0f, 0.2f);
        
        [Header("Component Icon Settings")]
        [SerializeField] private bool _enableComponentIcons = true;
        [SerializeField] private float _iconSize = 16f;
        [SerializeField] private float _iconPaddingRight = 2f;
        [SerializeField] private List<ComponentIconMapping> _componentMappings = new List<ComponentIconMapping>();
        
        [Header("Gradient Overlay Settings")]
        [SerializeField] private bool _enableGradientOverlay = false;
        [SerializeField] private List<GradientMapping> _gradientMappings = new List<GradientMapping>();
   
        public bool EnableHierarchyLines
        {
            get => _enableHierarchyLines;
            set => _enableHierarchyLines = value;
        }
        
    [Serializable]
    public class GradientMapping
    {
        [SerializeField] private string _componentName = string.Empty;
        [SerializeField] private Gradient _gradient = new Gradient();
        
        public string ComponentName
        {
            get => _componentName;
            set => _componentName = value ?? string.Empty;
        }
        
        public Gradient Gradient
        {
            get => _gradient;
            set => _gradient = value ?? new Gradient();
        }
        
        public bool IsValid => !string.IsNullOrWhiteSpace(_componentName) && _gradient != null;
    }
        
        public Color LineColor
        {
            get => _lineColor;
            set => _lineColor = value;
        }
        
        public LineStyle LineStyle
        {
            get => _lineStyle;
            set => _lineStyle = value;
        }
        
        public float LineWidth
        {
            get => Mathf.Clamp(_lineWidth, 0.5f, 3f);
            set => _lineWidth = Mathf.Clamp(value, 0.5f, 3f);
        }
        
        public bool ShowAlternateRowBackgrounds
        {
            get => _showAlternateRowBackgrounds;
            set => _showAlternateRowBackgrounds = value;
        }
        
        public Color AlternateRowColor
        {
            get => _alternateRowColor;
            set => _alternateRowColor = value;
        }
        
        public bool ShowSeparatorLines
        {
            get => _showSeparatorLines;
            set => _showSeparatorLines = value;
        }
        
        public Color SeparatorLineColor
        {
            get => _separatorLineColor;
            set => _separatorLineColor = value;
        }
        
        public bool EnableComponentIcons
        {
            get => _enableComponentIcons;
            set => _enableComponentIcons = value;
        }
        
        public float IconSize
        {
            get => Mathf.Clamp(_iconSize, 8f, 32f);
            set => _iconSize = Mathf.Clamp(value, 8f, 32f);
        }
        
        public float IconPaddingRight
        {
            get => _iconPaddingRight;
            set => _iconPaddingRight = value;
        }
        
        public List<ComponentIconMapping> ComponentMappings
        {
            get => _componentMappings;
            set => _componentMappings = value ?? new List<ComponentIconMapping>();
        }
        
        public bool EnableGradientOverlay
        {
            get => _enableGradientOverlay;
            set => _enableGradientOverlay = value;
        }
        
        public List<GradientMapping> GradientMappings
        {
            get => _gradientMappings;
            set => _gradientMappings = value ?? new List<GradientMapping>();
        }
        
        public static HierarchyPlusSettings LoadOrCreateSettings()
        {
            var settings = Resources.Load<HierarchyPlusSettings>(RESOURCE_PATH);
            
            if (settings == null)
            {
                settings = CreateInstance<HierarchyPlusSettings>();
                
#if UNITY_EDITOR
                // Only create asset in editor
                string resourcesPath = $"Assets/{RESOURCE_FOLDER}";
                if (!UnityEditor.AssetDatabase.IsValidFolder(resourcesPath))
                {
                    UnityEditor.AssetDatabase.CreateFolder("Assets", RESOURCE_FOLDER);
                }
                
                string assetPath = $"{resourcesPath}/{RESOURCE_PATH}.asset";
                UnityEditor.AssetDatabase.CreateAsset(settings, assetPath);
                UnityEditor.AssetDatabase.SaveAssets();
                UnityEditor.AssetDatabase.Refresh();
#endif
            }
            
            return settings;
        }
        
        public void ValidateMappings()
        {
            if (_componentMappings == null)
            {
                _componentMappings = new List<ComponentIconMapping>();
                return;
            }
            
            _componentMappings.RemoveAll(m => m == null || string.IsNullOrWhiteSpace(m.ComponentName));
        }
        
#if UNITY_EDITOR
        private void OnValidate()
        {
            _iconSize = Mathf.Clamp(_iconSize, 8f, 32f);
            _lineWidth = Mathf.Clamp(_lineWidth, 0.5f, 3f);
            ValidateMappings();
        }
#endif
    }
    
    public enum LineStyle
    {
        Straight,
        Dotted
    }
    
    [Serializable]
    public class ComponentIconMapping
    {
        [SerializeField] private string _componentName = string.Empty;
        [SerializeField] private Texture2D _icon;
        
        public string ComponentName
        {
            get => _componentName;
            set => _componentName = value ?? string.Empty;
        }
        
        public Texture2D Icon
        {
            get => _icon;
            set => _icon = value;
        }
        
        /// <summary>
        /// Validates if this mapping is properly configured.
        /// </summary>
        public bool IsValid => !string.IsNullOrWhiteSpace(_componentName) && _icon != null;
    }
}