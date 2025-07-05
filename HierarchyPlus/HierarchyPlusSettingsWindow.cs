using System;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace HierarchyPlus.Editor
{
    public class HierarchyPlusSettingsWindow : EditorWindow
    {
        private HierarchyPlusSettings _settings;
        private Vector2 _scrollPosition;
        private GUIStyle _headerStyle;
        private GUIStyle _boxStyle;
        private GUIStyle _removeButtonStyle;
 
        private Texture2D _headerBackground;
        private Texture2D _sectionBackground;

        private bool _hasChanges;

        [MenuItem("Tools/HierarchyPlus/Settings")]
        public static void ShowWindow()
        {
            var window = GetWindow<HierarchyPlusSettingsWindow>("HierarchyPlus Settings");
            window.minSize = new Vector2(400, 500);
            window.Show();
        }

        private void DrawGradientOverlaySettings()
        {
            DrawSectionHeader("Gradient Overlay Settings");
            
            EditorGUILayout.BeginVertical(_boxStyle);
            
            _settings.EnableGradientOverlay = EditorGUILayout.Toggle(
                new GUIContent("Enable Gradient Overlay", "Toggle gradient overlay drawing on/off"),
                _settings.EnableGradientOverlay
            );
            
            EditorGUI.BeginDisabledGroup(!_settings.EnableGradientOverlay);
            
            EditorGUILayout.Space(10);
            
            DrawGradientMappings();
            
            EditorGUI.EndDisabledGroup();
            
            EditorGUILayout.EndVertical();
        }
        
        private void DrawGradientMappings()
        {
            EditorGUILayout.LabelField("Gradient Mappings", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);
            
            if (_settings.GradientMappings == null)
            {
                _settings.GradientMappings = new List<HierarchyPlusSettings.GradientMapping>();
            }
            
            for (int i = 0; i < _settings.GradientMappings.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                
                var mapping = _settings.GradientMappings[i];
                
                mapping.ComponentName = EditorGUILayout.TextField(
                    new GUIContent("", "Component type name"),
                    mapping.ComponentName,
                    GUILayout.MinWidth(150)
                );
                
                mapping.Gradient = EditorGUILayout.GradientField(
                    new GUIContent("", "Gradient overlay"),
                    mapping.Gradient,
                    GUILayout.MinWidth(100)
                );
                
                if (GUILayout.Button("✕", _removeButtonStyle))
                {
                    _settings.GradientMappings.RemoveAt(i);
                    i--;
                }
                
                EditorGUILayout.EndHorizontal();
                
                EditorGUILayout.Space(2);
            }
            
            EditorGUILayout.Space(5);
            
            if (GUILayout.Button("+ Add Gradient Mapping", GUILayout.Height(25)))
            {
                _settings.GradientMappings.Add(new HierarchyPlusSettings.GradientMapping());
            }
        }
        
        private void OnEnable()
        {
            LoadSettings();
            InitializeStyles();
            
            EditorApplication.hierarchyChanged += OnHierarchyChanged;
        }
        
        private void OnDisable()
        {
            EditorApplication.hierarchyChanged -= OnHierarchyChanged;
            
            if (_settings != null && _hasChanges)
            {
                EditorUtility.SetDirty(_settings);
                AssetDatabase.SaveAssets();
                _hasChanges = false;
            }
            
            if (_headerBackground != null) DestroyImmediate(_headerBackground);
            if (_sectionBackground != null) DestroyImmediate(_sectionBackground);
        }
        
        private void LoadSettings()
        {
            _settings = HierarchyPlusSettings.LoadOrCreateSettings();
            if (_settings == null)
            {
                Debug.LogError("[HierarchyPlus] Failed to load or create settings!");
            }
        }
        
        private void InitializeStyles()
        {
            _headerBackground = CreateTexture(new Color(0.2f, 0.2f, 0.2f, 0.3f));
            _sectionBackground = CreateTexture(new Color(0.15f, 0.15f, 0.15f, 0.2f));
        }
        
        private Texture2D CreateTexture(Color color)
        {
            var texture = new Texture2D(1, 1);
            texture.SetPixel(0, 0, color);
            texture.Apply();
            return texture;
        }
        
        private void OnGUI()
        {
            if (_settings == null)
            {
                LoadSettings();
                return;
            }
            
            if (_headerStyle == null)
            {
                _headerStyle = new GUIStyle(EditorStyles.boldLabel)
                {
                    fontSize = 14,
                    alignment = TextAnchor.MiddleLeft,
                    padding = new RectOffset(10, 10, 5, 5)
                };
                
                _boxStyle = new GUIStyle("Box")
                {
                    padding = new RectOffset(10, 10, 10, 10),
                    margin = new RectOffset(5, 5, 5, 5)
                };
                
                _removeButtonStyle = new GUIStyle(GUI.skin.button)
                {
                    fixedWidth = 20,
                    fixedHeight = 20,
                    padding = new RectOffset(0, 0, 0, 0),
                    margin = new RectOffset(5, 0, 0, 0)
                };
            }
            
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
            
            EditorGUI.BeginChangeCheck();
            
            DrawHeader();
            
            if (GUILayout.Button("Refresh Hierarchy", GUILayout.Height(30)))
            {
                RefreshHierarchy();
            }
            
            EditorGUILayout.Space(10);
            
            DrawHierarchyLineSettings();
            
            EditorGUILayout.Space(20);
            
            DrawRowSettings();
            
            EditorGUILayout.Space(20);
            
            DrawComponentIconSettings();
            
            EditorGUILayout.Space(20);
            
            DrawGradientOverlaySettings();
            
            if (EditorGUI.EndChangeCheck())
            {
                _hasChanges = true;
                EditorApplication.RepaintHierarchyWindow();
            }
            
            EditorGUILayout.EndScrollView();
        }

        private void OnLostFocus()
        {
            if (_settings != null && _hasChanges)
            {
                EditorUtility.SetDirty(_settings);
                AssetDatabase.SaveAssets();
                _hasChanges = false;
            }
        }

        private void DrawHeader()
        {
            var headerRect = GUILayoutUtility.GetRect(0, 40, GUILayout.ExpandWidth(true));
            GUI.DrawTexture(headerRect, _headerBackground);
            
            var labelRect = new Rect(headerRect.x + 10, headerRect.y, headerRect.width - 20, headerRect.height);
            GUI.Label(labelRect, "HierarchyPlus Configuration", new GUIStyle(EditorStyles.largeLabel)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 16,
                fontStyle = FontStyle.Bold
            });
        }
        
        private void DrawHierarchyLineSettings()
        {
            DrawSectionHeader("Hierarchy Line Settings");
            
            EditorGUILayout.BeginVertical(_boxStyle);
            
            _settings.EnableHierarchyLines = EditorGUILayout.Toggle(
                new GUIContent("Enable Hierarchy Lines", "Toggle hierarchy line drawing on/off"),
                _settings.EnableHierarchyLines
            );
            
            EditorGUI.BeginDisabledGroup(!_settings.EnableHierarchyLines);
            
            EditorGUILayout.Space(5);
            
            _settings.LineColor = EditorGUILayout.ColorField(
                new GUIContent("Line Color", "Color of the hierarchy lines (including alpha)"),
                _settings.LineColor,
                true,
                true,
                false  
            );
            
            EditorGUILayout.Space(5);
            
            _settings.LineStyle = (LineStyle)EditorGUILayout.EnumPopup(
                new GUIContent("Line Style", "Choose between straight or dotted lines"),
                _settings.LineStyle
            );
            
            EditorGUILayout.Space(5);
            
            _settings.LineWidth = EditorGUILayout.Slider(
                new GUIContent("Line Width", "Width of the hierarchy lines"),
                _settings.LineWidth,
                0.5f,
                3f
            );
            
            EditorGUI.EndDisabledGroup();
            
            EditorGUILayout.EndVertical();
        }
        
        private void DrawRowSettings()
        {
            DrawSectionHeader("Row Settings");
            
            EditorGUILayout.BeginVertical(_boxStyle);
            
            _settings.ShowAlternateRowBackgrounds = EditorGUILayout.Toggle(
                new GUIContent("Show Alternate Row Backgrounds", "Show alternating background colors for hierarchy rows"),
                _settings.ShowAlternateRowBackgrounds
            );
            
            EditorGUI.BeginDisabledGroup(!_settings.ShowAlternateRowBackgrounds);
            
            _settings.AlternateRowColor = EditorGUILayout.ColorField(
                new GUIContent("Alternate Row Color", "Background color for alternate rows"),
                _settings.AlternateRowColor,
                true, true, false
            );
            
            EditorGUI.EndDisabledGroup();
            
            EditorGUILayout.Space(10);
            
            _settings.ShowSeparatorLines = EditorGUILayout.Toggle(
                new GUIContent("Show Separator Lines", "Show thin lines between hierarchy rows"),
                _settings.ShowSeparatorLines
            );
            
            EditorGUI.BeginDisabledGroup(!_settings.ShowSeparatorLines);
            
            _settings.SeparatorLineColor = EditorGUILayout.ColorField(
                new GUIContent("Separator Color", "Color of the separator lines"),
                _settings.SeparatorLineColor,
                true, true, false
            );
            
            EditorGUI.EndDisabledGroup();
            
            EditorGUILayout.EndVertical();
        }
        
        private void DrawComponentIconSettings()
        {
            DrawSectionHeader("Component Icon Settings");
            
            EditorGUILayout.BeginVertical(_boxStyle);
            
            _settings.EnableComponentIcons = EditorGUILayout.Toggle(
                new GUIContent("Enable Component Icons", "Toggle component icon drawing on/off"),
                _settings.EnableComponentIcons
            );
            
            EditorGUI.BeginDisabledGroup(!_settings.EnableComponentIcons);
            
            EditorGUILayout.Space(5);
            
            _settings.IconSize = EditorGUILayout.Slider(
                new GUIContent("Icon Size", "Size of the component icons in pixels"),
                _settings.IconSize,
                8f,
                32f
            );
            
            _settings.IconPaddingRight = EditorGUILayout.FloatField(
                new GUIContent("Right Padding", "Padding from the right edge (negative values allowed)"),
                _settings.IconPaddingRight
            );
            
            EditorGUILayout.Space(10);
            
            DrawComponentMappings();
            
            EditorGUI.EndDisabledGroup();
            
            EditorGUILayout.EndVertical();
        }
        
        private void DrawComponentMappings()
        {
            EditorGUILayout.LabelField("Component Mappings", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);
            
            if (_settings.ComponentMappings == null)
            {
                _settings.ComponentMappings = new List<ComponentIconMapping>();
            }
  
            for (int i = 0; i < _settings.ComponentMappings.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                
                var mapping = _settings.ComponentMappings[i];
                
                mapping.ComponentName = EditorGUILayout.TextField(
                    new GUIContent("", "Component type name (e.g., 'Camera', 'Light')"),
                    mapping.ComponentName,
                    GUILayout.MinWidth(150)
                );
                
                mapping.Icon = (Texture2D)EditorGUILayout.ObjectField(
                    mapping.Icon,
                    typeof(Texture2D),
                    false,
                    GUILayout.Width(70),
                    GUILayout.Height(18)
                );
                
                if (GUILayout.Button("✕", _removeButtonStyle))
                {
                    _settings.ComponentMappings.RemoveAt(i);
                    i--;
                }
                
                EditorGUILayout.EndHorizontal();
                
                EditorGUILayout.Space(2);
            }
            
            EditorGUILayout.Space(5);
      
            if (GUILayout.Button("+ Add Mapping", GUILayout.Height(25)))
            {
                _settings.ComponentMappings.Add(new ComponentIconMapping());
            }
        }
        
        private void DrawSectionHeader(string title)
        {
            var rect = GUILayoutUtility.GetRect(0, 30, GUILayout.ExpandWidth(true));
            GUI.DrawTexture(rect, _sectionBackground);
            
            var labelRect = new Rect(rect.x + 10, rect.y, rect.width - 20, rect.height);
            GUI.Label(labelRect, title, _headerStyle);
        }
        
        private void OnHierarchyChanged()
        {
            Repaint();
        }
        
        private void RefreshHierarchy()
        {
            HierarchyPlusDrawer.ClearAllCaches();
            EditorApplication.RepaintHierarchyWindow();
            EditorApplication.DirtyHierarchyWindowSorting();
        }
    }
}