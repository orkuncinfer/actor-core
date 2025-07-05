using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine.Profiling;

namespace HierarchyPlus.Editor
{
    [InitializeOnLoad]
    public static class HierarchyPlusDrawer
    {
        private static EditorWindow _cachedHierarchyWindow;

        private static HierarchyPlusSettings _settings;
        private static Dictionary<string, Type> _componentTypeCache;
        private static Dictionary<int, List<ComponentInfo>> _gameObjectComponentCache;
        private static Dictionary<int, GradientInfo> _gameObjectGradientCache;
        private static Texture2D _lineTexture;
        private static Texture2D _alternateRowTexture;
        private static Texture2D _separatorTexture;
        private static Texture2D _treeLineTexture;
        private static Texture2D _treeBranchTexture;
        private static Texture2D _treeEndTexture;
        private static float _lastCacheTime;
        private static readonly float CACHE_DURATION = 0.5f;
        private static int _currentRow = 0;
        private static HashSet<int> _visibleInstanceIDs;
        private static float _lastHierarchyWidth = 0f;
        
        private struct ComponentInfo
        {
            public string TypeName;
            public Texture2D Icon;
            
            public ComponentInfo(string typeName, Texture2D icon)
            {
                TypeName = typeName;
                Icon = icon;
            }
        }
        
        private struct GradientInfo
        {
            public Gradient Gradient;
            public Texture2D CachedTexture;
            
            public GradientInfo(Gradient gradient)
            {
                Gradient = gradient;
                CachedTexture = null;
            }
        }
        
        static HierarchyPlusDrawer()
        {
            EditorApplication.delayCall += Initialize;
        }
        
        private static void Initialize()
        {
            LoadSettings();
            InitializeCaches();
            CreateTextures();

            EditorApplication.hierarchyWindowItemOnGUI -= OnHierarchyGUI;
            EditorApplication.hierarchyWindowItemOnGUI += OnHierarchyGUI;

            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;

            EditorApplication.hierarchyChanged -= OnHierarchyChanged;
            EditorApplication.hierarchyChanged += OnHierarchyChanged;
            
            EditorApplication.update -= Update;
            EditorApplication.update += Update;

            EditorApplication.RepaintHierarchyWindow();
        }
        
        private static void Update()
        {
            if (Time.realtimeSinceStartup - _lastCacheTime > CACHE_DURATION)
            {
                CleanupCache();
                _lastCacheTime = Time.realtimeSinceStartup;
            }

            UpdateHierarchyWindowCache();
        }

        private static void LoadSettings()
        {
            _settings = HierarchyPlusSettings.LoadOrCreateSettings();
        }
        
        private static void UpdateHierarchyWindowCache()
        {
            if (_cachedHierarchyWindow == null)
            {
                var windows = Resources.FindObjectsOfTypeAll<EditorWindow>();
                foreach (var window in windows)
                {
                    if (window.GetType().Name == "SceneHierarchyWindow")
                    {
                        _cachedHierarchyWindow = window;
                        break;
                    }
                }
            }
            else if (!_cachedHierarchyWindow) // Window destroyed
            {
                _cachedHierarchyWindow = null;
            }
        }
        
        private static void InitializeCaches()
        {
            _componentTypeCache = new Dictionary<string, Type>(StringComparer.Ordinal);
            _gameObjectComponentCache = new Dictionary<int, List<ComponentInfo>>();
            _gameObjectGradientCache = new Dictionary<int, GradientInfo>();
            _visibleInstanceIDs = new HashSet<int>();
            
            CacheCommonComponentTypes();
        }
        
        private static void CacheCommonComponentTypes()
        {
            var commonTypes = new[]
            {
                typeof(Transform), typeof(Camera), typeof(Light),
                typeof(MeshRenderer), typeof(SkinnedMeshRenderer),
                typeof(Animator), typeof(Rigidbody), typeof(Collider),
                typeof(AudioSource), typeof(ParticleSystem)
            };
            
            foreach (var type in commonTypes)
            {
                _componentTypeCache[type.Name] = type;
            }
        }
        
        private static void CreateTextures()
        {
            _lineTexture = CreateSolidTexture(Color.white);
            _alternateRowTexture = CreateSolidTexture(Color.white);
            _separatorTexture = CreateSolidTexture(Color.white);
            _treeLineTexture = CreateSolidTexture(Color.white);
            _treeBranchTexture = CreateSolidTexture(Color.white);
            _treeEndTexture = CreateSolidTexture(Color.white);
        }
        
        private static Texture2D CreateSolidTexture(Color color)
        {
            var texture = new Texture2D(1, 1)
            {
                hideFlags = HideFlags.HideAndDontSave
            };
            texture.SetPixel(0, 0, color);
            texture.Apply();
            return texture;
        }
        
        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            ClearAllCaches();
        }
        
        private static void OnHierarchyChanged()
        {
            _currentRow = 0;
            //EditorApplication.RepaintHierarchyWindow();
        }
        
        private static void OnHierarchyGUI(int instanceID, Rect selectionRect)
        {
          
            if (_settings == null)
            {
                LoadSettings();
                if (_settings == null) return;
            }
            
            var gameObject = EditorUtility.InstanceIDToObject(instanceID) as GameObject;
            if (gameObject == null) return;
            
            Profiler.BeginSample("HierarchyPlus.OnHierarchyGUI");
            float currentWidth = EditorGUIUtility.currentViewWidth;
            if (Mathf.Abs(currentWidth - _lastHierarchyWidth) > 0.1f)
            {
                _lastHierarchyWidth = currentWidth;
                _currentRow = 0;
            }
            
            _visibleInstanceIDs.Add(instanceID);
            
            if (_settings.ShowAlternateRowBackgrounds)
            {
                DrawAlternateRowBackground(selectionRect);
            }
            
            if (_settings.EnableGradientOverlay)
            {
                DrawGradientOverlay(gameObject, selectionRect, instanceID);
            }
            
            if (_settings.EnableHierarchyLines)
            {
                DrawHierarchyTreeLines(gameObject, selectionRect);
            }
            
            if (_settings.EnableComponentIcons)
            {
                DrawComponentIcons(gameObject, selectionRect, instanceID);
            }
            
            if (_settings.ShowSeparatorLines)
            {
                DrawSeparatorLine(selectionRect);
            }
            
            _currentRow++;
            
            Profiler.EndSample();
        }
        
        private static void DrawAlternateRowBackground(Rect selectionRect)
        {
            int rowIndex = Mathf.FloorToInt(selectionRect.y / selectionRect.height);
            
            if (rowIndex % 2 == 1)
            {
                var previousColor = GUI.color;
                GUI.color = _settings.AlternateRowColor;
                
                var bgRect = new Rect(32, selectionRect.y, EditorGUIUtility.currentViewWidth, selectionRect.height);
                GUI.DrawTexture(bgRect, _alternateRowTexture);
                GUI.color = previousColor;
            }
        }
        
        private static void DrawSeparatorLine(Rect selectionRect)
        {
            var previousColor = GUI.color;
            GUI.color = _settings.SeparatorLineColor;
            
            var lineRect = new Rect(32, selectionRect.y + selectionRect.height - 1f, 
                EditorGUIUtility.currentViewWidth, 1f);

            GUI.DrawTexture(lineRect, _separatorTexture);
            
            GUI.color = previousColor;
        }
        
        private static void DrawHierarchyTreeLines(GameObject gameObject, Rect selectionRect)
        {
            var transform = gameObject.transform;
            if (transform.parent == null) return;
            
            var lineColor = _settings.LineColor;
            var previousColor = GUI.color;
            GUI.color = gameObject.activeInHierarchy ? lineColor : new Color(lineColor.r, lineColor.g, lineColor.b, lineColor.a * 0.5f);
            
            float lineX = selectionRect.x - 20f;
            float lineWidth = _settings.LineWidth;
            
            DrawTreeLines(transform, selectionRect, lineX, lineWidth);
            
            GUI.color = previousColor;
        }
        
        private static void DrawTreeLines(Transform transform, Rect selectionRect, float lineX, float lineWidth)
        {
            bool isLastChild = transform.GetSiblingIndex() == transform.parent.childCount - 1;
            float centerY = selectionRect.y + selectionRect.height * 0.5f;
            
            if (isLastChild)
            {
                GUI.DrawTexture(new Rect(lineX - lineWidth * 0.5f, selectionRect.y, lineWidth, 
                                        centerY - selectionRect.y), _treeLineTexture);
            }
            else
            {
                GUI.DrawTexture(new Rect(lineX - lineWidth * 0.5f, selectionRect.y, lineWidth, 
                                        selectionRect.height), _treeLineTexture);
            }
            
            float horizontalLineEndX = GameObjectHasVisibleChildren(transform) ? selectionRect.x - 14f : selectionRect.x - 4f;


            GUI.DrawTexture(new Rect(lineX, centerY - lineWidth * 0.5f, 
                                    horizontalLineEndX - lineX, lineWidth), _treeLineTexture);
            
            Transform parentTransform = transform.parent;
            float currentX = lineX;
            
            while (parentTransform != null && parentTransform.parent != null)
            {
                currentX -= 14f;
                
                bool parentIsLastChild = parentTransform.GetSiblingIndex() == parentTransform.parent.childCount - 1;
                if (!parentIsLastChild)
                {
                    GUI.DrawTexture(new Rect(currentX - lineWidth * 0.5f, selectionRect.y, lineWidth, 
                                            selectionRect.height), _treeLineTexture);
                }
                
                parentTransform = parentTransform.parent;
            }
        }
        private static bool GameObjectHasVisibleChildren(Transform transform)
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                var child = transform.GetChild(i);
                if ((child.hideFlags & HideFlags.HideInHierarchy) == 0)
                    return true;
            }
            return false;
        }


        private static void DrawGradientOverlay(GameObject gameObject, Rect selectionRect, int instanceID)
        {
            GradientInfo gradientInfo;
            if (!_gameObjectGradientCache.TryGetValue(instanceID, out gradientInfo))
            {
                gradientInfo = BuildGradientInfo(gameObject);
                _gameObjectGradientCache[instanceID] = gradientInfo;
            }
            
            if (gradientInfo.Gradient != null && gradientInfo.CachedTexture != null)
            {
                var previousColor = GUI.color;
                
                float overlayWidth = selectionRect.width * 0.5f;
                var overlayRect = new Rect(
                    selectionRect.x,
                    selectionRect.y,
                    overlayWidth,
                    selectionRect.height
                );
                
                GUI.DrawTexture(overlayRect, gradientInfo.CachedTexture);
                GUI.color = previousColor;
            }
        }
        
        private static GradientInfo BuildGradientInfo(GameObject gameObject)
        {
            var mappings = _settings.GradientMappings;
            if (mappings == null || mappings.Count == 0)
                return new GradientInfo();
            
            var components = gameObject.GetComponents<Component>();
            
            foreach (var mapping in mappings)
            {
                if (!mapping.IsValid) continue;
                
                Type mappingType = ResolveComponentType(mapping.ComponentName);
                if (mappingType == null) continue;
                
                foreach (var component in components)
                {
                    if (component == null) continue;
                    
                    var componentType = component.GetType();
                    
                    if (componentType.Name == mapping.ComponentName || 
                        mappingType.IsAssignableFrom(componentType))
                    {
                        var gradientInfo = new GradientInfo(mapping.Gradient);
                        gradientInfo.CachedTexture = CreateGradientTexture(mapping.Gradient);
                        return gradientInfo;
                    }
                }
            }
            
            return new GradientInfo();
        }
        
        private static Texture2D CreateGradientTexture(Gradient gradient)
        {
            var texture = new Texture2D(256, 1)
            {
                hideFlags = HideFlags.HideAndDontSave
            };
            
            for (int i = 0; i < 256; i++)
            {
                float t = i / 255f;
                var color = gradient.Evaluate(t);
                color.a *= t;
                texture.SetPixel(i, 0, color);
            }
            
            texture.Apply();
            texture.wrapMode = TextureWrapMode.Clamp;
            return texture;
        }
        
        private static void DrawComponentIcons(GameObject gameObject, Rect selectionRect, int instanceID)
        {
            List<ComponentInfo> componentInfos;
            if (!_gameObjectComponentCache.TryGetValue(instanceID, out componentInfos) || 
                HasComponentsChanged(gameObject, componentInfos))
            {
                componentInfos = BuildComponentInfoList(gameObject);
                _gameObjectComponentCache[instanceID] = componentInfos;
            }
            
            if (componentInfos == null || componentInfos.Count == 0)
                return;
            
            float iconSize = _settings.IconSize;
            float padding = _settings.IconPaddingRight;
            
            float windowWidth = _cachedHierarchyWindow != null 
                ? _cachedHierarchyWindow.position.width 
                : EditorGUIUtility.currentViewWidth;
            
            if (windowWidth == 0)
            {
                windowWidth = EditorGUIUtility.currentViewWidth;
            }
            
            float currentX = windowWidth - padding - iconSize;
            
            var previousColor = GUI.color;
            
            for (int i = 0; i < componentInfos.Count; i++)
            {
                var info = componentInfos[i];
                if (info.Icon != null)
                {
                    var iconRect = new Rect(
                        currentX,
                        selectionRect.y + (selectionRect.height - iconSize) * 0.5f,
                        iconSize,
                        iconSize
                    );
                    
                    GUI.DrawTexture(iconRect, info.Icon, ScaleMode.ScaleToFit);
                    currentX -= iconSize + 2f;
                }
            }
            
            GUI.color = previousColor;
        }
        
        private static bool HasComponentsChanged(GameObject gameObject, List<ComponentInfo> cachedInfos)
        {
            var currentComponents = gameObject.GetComponents<Component>();
            
            if (currentComponents.Length != cachedInfos.Count)
                return true;
        
            var currentComponentTypes = new HashSet<string>();
            foreach (var comp in currentComponents)
            {
                if (comp != null)
                {
                    currentComponentTypes.Add(comp.GetType().Name);
                }
            }
            
            foreach (var info in cachedInfos)
            {
                if (!currentComponentTypes.Contains(info.TypeName))
                {
                    return true;
                }
            }
            
            return false;
        }
        
        private static List<ComponentInfo> BuildComponentInfoList(GameObject gameObject)
        {
            var componentInfos = new List<ComponentInfo>();
            var mappings = _settings.ComponentMappings;
            
            if (mappings == null || mappings.Count == 0)
                return componentInfos;
            
            var components = gameObject.GetComponents<Component>();
            var processedComponents = new HashSet<Component>();
            
            foreach (var mapping in mappings)
            {
                if (!mapping.IsValid) continue;
                
                Type mappingType = ResolveComponentType(mapping.ComponentName);
                if (mappingType == null) continue;
                
                foreach (var component in components)
                {
                    if (component == null || processedComponents.Contains(component)) continue;
                    
                    var componentType = component.GetType();
                    
                    if (componentType.Name == mapping.ComponentName || 
                        mappingType.IsAssignableFrom(componentType))
                    {
                        componentInfos.Add(new ComponentInfo(componentType.Name, mapping.Icon));
                        processedComponents.Add(component);
                        break;
                    }
                }
            }
            
            return componentInfos;
        }
        
        private static void CleanupCache()
        {
            var keysToRemove = new List<int>();
            
            foreach (var kvp in _gameObjectComponentCache)
            {
                if (!_visibleInstanceIDs.Contains(kvp.Key))
                {
                    var obj = EditorUtility.InstanceIDToObject(kvp.Key);
                    if (obj == null)
                    {
                        keysToRemove.Add(kvp.Key);
                    }
                }
            }
            
            foreach (var key in keysToRemove)
            {
                _gameObjectComponentCache.Remove(key);
            }
            
            keysToRemove.Clear();
            foreach (var kvp in _gameObjectGradientCache)
            {
                if (!_visibleInstanceIDs.Contains(kvp.Key))
                {
                    var obj = EditorUtility.InstanceIDToObject(kvp.Key);
                    if (obj == null)
                    {
                        if (kvp.Value.CachedTexture != null)
                        {
                            UnityEngine.Object.DestroyImmediate(kvp.Value.CachedTexture);
                        }
                        keysToRemove.Add(kvp.Key);
                    }
                }
            }
            
            foreach (var key in keysToRemove)
            {
                _gameObjectGradientCache.Remove(key);
            }
            
            _visibleInstanceIDs.Clear();
            _currentRow = 0;
        }
        
        public static void ClearAllCaches()
        {
            _gameObjectComponentCache.Clear();
            
            foreach (var kvp in _gameObjectGradientCache)
            {
                if (kvp.Value.CachedTexture != null)
                {
                    UnityEngine.Object.DestroyImmediate(kvp.Value.CachedTexture);
                }
            }
            
            _gameObjectGradientCache.Clear();
            
            _visibleInstanceIDs.Clear();
            _currentRow = 0;
        }
        
        private static Type ResolveComponentType(string typeName)
        {
            if (string.IsNullOrWhiteSpace(typeName))
                return null;
            
            Type cachedType;
            if (_componentTypeCache.TryGetValue(typeName, out cachedType))
                return cachedType;
            
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    foreach (var type in assembly.GetTypes())
                    {
                        if (type.Name == typeName && typeof(Component).IsAssignableFrom(type))
                        {
                            _componentTypeCache[typeName] = type;
                            return type;
                        }
                    }
                }
                catch (ReflectionTypeLoadException)
                {
                    continue;
                }
            }
            
            Debug.LogError($"[HierarchyPlus] Component type '{typeName}' not found!");
            return null;
        }
    }
}