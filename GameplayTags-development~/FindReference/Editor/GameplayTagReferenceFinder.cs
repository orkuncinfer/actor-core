using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BandoWare.GameplayTags;
using Object = UnityEngine.Object;

public class GameplayTagReferenceFinder : EditorWindow
{
    private Dictionary<string, List<string>> _tagReferences = new Dictionary<string, List<string>>();
    private List<string> _allGameplayTags = new List<string>();
    private GameplayTagCache _cache;

    private const string CachePath = "Assets/GameplayTagCache.asset";
    
    private string _searchText = "";
    private Vector2 _scrollPosition;
    
    private bool _showSceneReferences = false;

    [MenuItem("Tools/Gameplay Tag Container Finder")]
    public static void OpenWindow()
    {
        GameplayTagReferenceFinder window = GetWindow<GameplayTagReferenceFinder>("GameplayTag FindRef");
        window.Show();
    }

    private void OnEnable()
    {
        LoadAllGameplayTags();
        LoadCache();
    }

    private void OnGUI()
    {
        EditorGUILayout.Space();
        
        // Search bar
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Search Gameplay Tags:", GUILayout.Width(150));
        _searchText = EditorGUILayout.TextField(_searchText);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        // Scroll view for results
        _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Light Scan (Active Scenes Only)"))
        {
            FindGameplayTagUsages(activeScenesOnly: true);
            //SaveCache();
        }

        if (GUILayout.Button("Scan Project and Cache"))
        {
            FindGameplayTagUsages(activeScenesOnly: false);
            SaveCache();
        }
        EditorGUILayout.EndHorizontal();
        DrawGameplayTags();

        Repaint();
        EditorGUILayout.EndScrollView();
    }

    private void LoadAllGameplayTags()
    {
        _allGameplayTags.Clear();
        var assemblies = System.AppDomain.CurrentDomain.GetAssemblies();

        foreach (var assembly in assemblies)
        {
            var attributes = assembly.GetCustomAttributes(typeof(GameplayTagAttribute), false) as GameplayTagAttribute[];
            if (attributes != null)
            {
                foreach (var attribute in attributes)
                {
                    Debug.Log("added gameplay tag " + attribute.TagName);
                    _allGameplayTags.Add(attribute.TagName);
                }
            }
        }
    }

    private void LoadCache()
    {
        _cache = AssetDatabase.LoadAssetAtPath<GameplayTagCache>(CachePath);
        if (_cache == null)
        {
            _cache = ScriptableObject.CreateInstance<GameplayTagCache>();
            AssetDatabase.CreateAsset(_cache, CachePath);
            AssetDatabase.SaveAssets();
        }
        else
        {
            _tagReferences.Clear();
            foreach (var entry in _cache.Entries)
            {
                if (entry.References.Count > 0)
                {
                    _tagReferences[entry.Tag] = new List<string>(entry.References);
                }
            }
        }
    }

    private void SaveCache()
    {
        if (_cache == null)
        {
            Debug.LogError("Cache not loaded. Cannot save results.");
            return;
        }

        _cache.Entries.Clear();
        
        foreach (var kvp in _tagReferences)
        {
            if (kvp.Value.Count > 0)
            {
                GameplayTagCache.CacheEntry entry = new GameplayTagCache.CacheEntry
                {
                    Tag = kvp.Key,
                    References = new List<string>(kvp.Value)
                };
                _cache.Entries.Add(entry);
            }
        }

        EditorUtility.SetDirty(_cache);
        AssetDatabase.SaveAssets();
    }

    private void FindGameplayTagUsages(bool activeScenesOnly)
    {
        _tagReferences.Clear();

        foreach (string tag in _allGameplayTags)
        {
            _tagReferences[tag] = new List<string>();
        }
        
        string[] assetPaths = AssetDatabase.GetAllAssetPaths();

        foreach (string path in assetPaths)
        {
            if (path.EndsWith(".prefab") || path.EndsWith(".asset"))
            {
                Object asset = AssetDatabase.LoadAssetAtPath<Object>(path);
                if (asset != null)
                {
                    if (asset is GameObject gameObject)
                    {
                        // This is a prefab, need to iterate through all components
                        SearchPrefabForTags(gameObject, path);
                    }
                    else
                    {
                        // Handle non-prefab assets (e.g., ScriptableObjects)
                        var foundTags = FindTagsInObject(asset, path);
                        foreach (var tag in foundTags)
                        {
                            if (_tagReferences.ContainsKey(tag))
                            {
                                _tagReferences[tag].Add(path);
                            }
                        }
                    }
                }
            }
        }

        if (activeScenesOnly)
        {
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Scene scene = SceneManager.GetSceneAt(i);

                if (!scene.isLoaded)
                    continue;

                SearchSceneForTags(scene);
            }
        }
        else
        {
            foreach (string path in assetPaths)
            {
                if (path.StartsWith("Assets/") && path.EndsWith(".unity"))
                {
                    Scene scene = EditorSceneManager.GetSceneByPath(path);
                    bool wasLoaded = scene.isLoaded;

                    if (!wasLoaded)
                    {
                        scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
                    }
                    
                    SearchSceneForTags(scene);
                    
                    if (!wasLoaded)
                    {
                        EditorSceneManager.CloseScene(scene, true);
                    }
                }
            }
        }
    }

    private void SearchPrefabForTags(GameObject prefab, string context)
    {
        Component[] components = prefab.GetComponentsInChildren<Component>(true);
        foreach (Component component in components)
        {
            if (component != null)
            {
                var foundTags = FindTagsInObject(component, context);
                foreach (var tag in foundTags)
                {
                    if (_tagReferences.ContainsKey(tag))
                    {
                        _tagReferences[tag].Add(context);
                    }
                }
            }
        }
    }

    private void SearchSceneForTags(Scene scene)
    {
        GameObject[] rootObjects = scene.GetRootGameObjects();
        foreach (GameObject rootObject in rootObjects)
        {
            Component[] components = rootObject.GetComponentsInChildren<Component>(true);
            foreach (Component component in components)
            {
                if (component != null)
                {
                    var foundTags = FindTagsInObject(component, $"Scene: {scene.path}, GameObject: {rootObject.name}");
                    foreach (var tag in foundTags)
                    {
                        if (_tagReferences.ContainsKey(tag))
                        {
                            _tagReferences[tag].Add($"{scene.path}/{rootObject.name}");
                        }
                    }
                }
            }
        }
    }

    private List<string> FindTagsInObject(Object obj, string context)
{
    List<string> tags = new List<string>();
    if (obj == null) return tags;

    // Enhanced reflection with inheritance support
    Type currentType = obj.GetType();
    const BindingFlags bindingFlags = BindingFlags.Public | 
                                    BindingFlags.NonPublic | 
                                    BindingFlags.Instance | 
                                    BindingFlags.DeclaredOnly;

    while (currentType != null && currentType != typeof(Object))
    {
        FieldInfo[] fields = currentType.GetFields(bindingFlags);
        foreach (var field in fields)
        {
            if (typeof(GameplayTagContainer).IsAssignableFrom(field.FieldType))
            {
                GameplayTagContainer container = field.GetValue(obj) as GameplayTagContainer;
                if (container != null)
                {
                    foreach (var tag in container.GetTags())
                    {
                        Debug.Log($"Found tag {tag} in {field.Name} of {obj.name} ({currentType.Name}) in {context}");
                        tags.Add(tag.ToString());
                    }
                }
            }
        }
        currentType = currentType.BaseType;
    }

    // Optimized serialized property handling
    using (SerializedObject serializedObject = new SerializedObject(obj))
    {
        SerializedProperty property = serializedObject.GetIterator();
        bool enterChildren = true;
        
        while (property.NextVisible(enterChildren))
        {
            enterChildren = true; // Reset for next property
            if (property.propertyType == SerializedPropertyType.ManagedReference &&
                property.managedReferenceFieldTypename == "GameplayTagContainer")
            {
                var container = property.managedReferenceValue as GameplayTagContainer;
                if (container != null)
                {
                    tags.AddRange(container.GetTags().Select(t => t.ToString()));
                }
                enterChildren = false; // Skip container's internal structure
            }
        }
    }

    return tags.Distinct().ToList(); // Remove duplicates from multiple inheritance levels
}

    // Helper method to get all fields (including private fields in base classes)
    private List<FieldInfo> GetAllFields(System.Type type)
    {
        var fields = new List<FieldInfo>();
        while (type != null)
        {
            // Get all fields (public, private, instance) from the current type
            fields.AddRange(type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly));
            // Move to the base type
            type = type.BaseType;
        }
        return fields;
    }

    private void DrawGameplayTags()
    {
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Gameplay Tags and Their References", EditorStyles.boldLabel);

        _showSceneReferences = EditorGUILayout.Toggle("Show Scene References",_showSceneReferences);
        
        foreach (var kvp in _tagReferences)
        {
            var tag = kvp.Key;
            var references = kvp.Value;
            
            if (references.Count == 0)
                continue;

            if (!string.IsNullOrEmpty(_searchText) && !tag.ToLower().Contains(_searchText.ToLower()))
            {
                continue;
            }
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField(tag, EditorStyles.boldLabel);

            references.Sort();
            foreach (var reference in references)
            {
                EditorGUILayout.BeginHorizontal();

                string assetPath = reference;
                bool show = true;
                if (assetPath.Contains(".unity"))
                {
                    if (!_showSceneReferences)
                    {
                        show = false;
                    }
                    assetPath = TrimPathAfterUnity(reference);
                }
                
                Object asset = AssetDatabase.LoadAssetAtPath<Object>(assetPath);
                if (asset != null && show)
                {
                    GUIContent assetContent = EditorGUIUtility.ObjectContent(asset, asset.GetType());

                    if (assetContent.image != null)
                    {
                        GUILayout.Label(assetContent.image, GUILayout.Width(20), GUILayout.Height(20));
                    }

                    // Define a GUIStyle to adjust the font size
                    GUIStyle linkStyle = new GUIStyle(EditorStyles.linkLabel);
                    linkStyle.fontSize = 11; // Set the desired font size
                    linkStyle.hover.textColor = linkStyle.hover.textColor * 1.5f;

                    if (GUILayout.Button(reference, linkStyle))
                    {
                        PingOrOpenReference(assetPath);
                    }
                }
                else if(show && asset == null)
                {
                    // Define a GUIStyle to adjust the font size for missing asset labels
                    GUIStyle missingStyle = new GUIStyle(GUI.skin.label);
                    missingStyle.fontSize = 14; // Set the desired font size

                    GUILayout.Label("Missing Asset : " + assetPath, missingStyle);
                }

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndVertical();
        }
    }

    private void PingOrOpenReference(string reference)
    {
        if (reference.Contains(".unity"))
        {
            if (!EditorSceneManager.GetSceneByPath(reference).isLoaded)
            {
                if (EditorUtility.DisplayDialog("Open Scene?", $"Do you want to open scene '{reference}'?", "Yes", "No"))
                {
                    EditorSceneManager.OpenScene(TrimPathAfterUnity(reference), OpenSceneMode.Single);
                }
            }
        }
        else if (reference.Contains("Scene: "))
        {
            string[] parts = reference.Split(new[] { ", GameObject: " }, System.StringSplitOptions.None);
            string scenePath = parts[0].Replace("Scene: ", "");

            if (!EditorSceneManager.GetSceneByPath(scenePath).isLoaded)
            {
                if (EditorUtility.DisplayDialog("Open Scene?", $"Do you want to open scene '{scenePath}'?", "Yes", "No"))
                {
                    EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);
                }
            }

            if (parts.Length > 1)
            {
                string gameObjectName = parts[1];
                GameObject target = GameObject.Find(gameObjectName);
                if (target != null)
                {
                    Selection.activeObject = target;
                    EditorGUIUtility.PingObject(target);
                }
            }
        }
        else
        {
            Object obj = AssetDatabase.LoadAssetAtPath<Object>(reference);
            if (obj != null)
            {
                Selection.activeObject = obj;
                EditorGUIUtility.PingObject(obj);
            }
        }
    }

    private string TrimPathAfterUnity(string path)
    {
        int index = path.IndexOf(".unity");

        if (index != -1)
        {
            return path.Substring(0, index + 6);
        }
        return path;
    }
}
