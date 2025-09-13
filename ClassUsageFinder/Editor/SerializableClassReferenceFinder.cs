using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

public class SerializableClassReferenceFinder : EditorWindow
{
    private Dictionary<string, HashSet<string>> _classReferences = new Dictionary<string, HashSet<string>>();
    private List<Type> _serializableClasses = new List<Type>();

    private SerializableClassReferenceCache _cache;

    private const string CachePath = "Assets/GameplayTagCache.asset";
    
    private static string _searchText = "";
    private Vector2 _scrollPosition;
    private bool _showSceneReferences = false;

    [MenuItem("Tools/Serializable Class Finder")]
    public static void OpenWindow()
    {
        SerializableClassReferenceFinder window = GetWindow<SerializableClassReferenceFinder>("Serializable Finder");
        window.Show();
    }
    
    public static void OpenWindowExternal(string searchText)
    {
        SerializableClassReferenceFinder window = GetWindow<SerializableClassReferenceFinder>("Serializable Finder");
        window.Show();
        _searchText = searchText;
    }

    private void OnEnable()
    {
        LoadSerializableClasses();
        LoadCache();
        Selection.selectionChanged += OnSelectionChanged;
    }

    private void OnDisable()
    {
        Selection.selectionChanged -= OnSelectionChanged;
    }

    private void OnSelectionChanged()
    {
        if (Selection.activeObject != null && EditorWindow.focusedWindow != this)
        {
            _searchText = Selection.activeObject.name;
        }
    }

    private void OnGUI()
    {
        EditorGUILayout.Space();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Search Serializable Class:", GUILayout.Width(180));
        _searchText = EditorGUILayout.TextField(_searchText);
        if(_cache != null) _cache.LastSearchText = _searchText;
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("Scan Project"))
        {
            FindSerializableClassUsages();
            SaveCache();
        }

        EditorGUILayout.EndHorizontal();
        DrawResults();
        Repaint();
        EditorGUILayout.EndScrollView();

        
    }

    private void LoadSerializableClasses()
    {
        _serializableClasses.Clear();

        var assemblies = AppDomain.CurrentDomain.GetAssemblies();

        foreach (var assembly in assemblies)
        {
            Type[] types;

            try
            {
                types = assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                types = ex.Types.Where(t => t != null).ToArray();
            }

            foreach (var type in types)
            {
                if (type == null) continue;

                // Must be class, not abstract, and serializable by Unity
                if (!type.IsAbstract && !type.IsGenericTypeDefinition && IsUnitySerializable(type))
                {
                    _serializableClasses.Add(type);
                }
            }
        }
    }
    private bool IsUnitySerializable(Type type)
    {
        if (type == null || !type.IsClass || type.IsAbstract || type.IsGenericTypeDefinition)
            return false;

        // Unity serializes MonoBehaviours and ScriptableObjects
        if (typeof(MonoBehaviour).IsAssignableFrom(type) || typeof(ScriptableObject).IsAssignableFrom(type))
            return true;

        // Check for [Serializable] attribute on this type or any base type
        return HasSerializableAttributeRecursive(type);
    }
    private bool HasSerializableAttributeRecursive(Type type)
    {
        while (type != null && type != typeof(object))
        {
            if (type.IsDefined(typeof(SerializableAttribute), false))
                return true;

            type = type.BaseType;
        }

        return false;
    }
    private void LoadCache()
    {
        _cache = AssetDatabase.LoadAssetAtPath<SerializableClassReferenceCache>(CachePath);
        if (_cache == null)
        {
            _cache = ScriptableObject.CreateInstance<SerializableClassReferenceCache>();
            AssetDatabase.CreateAsset(_cache, CachePath);
            AssetDatabase.SaveAssets();
        }
        else
        {
            _classReferences.Clear();
            foreach (var entry in _cache.Entries)
            {
                if (entry.References.Count > 0)
                {
                    _classReferences[entry.Tag] = new HashSet<string>(entry.References);
                }
            }
        }

        if(string.IsNullOrEmpty(_searchText))_searchText = _cache.LastSearchText;
    }
    private void SaveCache()
    {
        if (_cache == null)
        {
            Debug.LogError("Cache not loaded. Cannot save results.");
            return;
        }

        _cache.Entries.Clear();
        
        foreach (var kvp in _classReferences)
        {
            if (kvp.Value.Count > 0)
            {
                SerializableClassReferenceCache.CacheEntry entry = new SerializableClassReferenceCache.CacheEntry
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
    private void FindSerializableClassUsages()
    {
        _classReferences.Clear();

        foreach (var type in _serializableClasses)
        {
            _classReferences[type.FullName] = new HashSet<string>(); // Use HashSet to prevent duplicates
        }

        string[] assetPaths = AssetDatabase.GetAllAssetPaths();
        
        
        StringBuilder scannedPathBuilder = new StringBuilder();

        foreach (string path in assetPaths)
        {
            if (path.EndsWith(".prefab") || path.EndsWith(".asset"))
            {
                if(path.Contains("Settings")) continue;
                Object asset = AssetDatabase.LoadAssetAtPath<Object>(path);
                if (asset != null)
                {
                    
                    scannedPathBuilder.AppendLine(asset.name);

                    if (asset is GameObject gameObject)
                    {
                        SearchPrefabForSerializableClasses(gameObject, path);
                    }
                    else
                    {
                        SearchObjectForSerializableClasses(asset, path);
                    }
                }
            }
        }

        Debug.Log(scannedPathBuilder.ToString());
    

        ScanScenesForSerializableClasses();
    }

    private void SearchPrefabForSerializableClasses(GameObject prefab, string context)
    {
        // Check if this is a nested prefab instance inside another prefab
        if (PrefabUtility.GetPrefabInstanceHandle(prefab) != null)
        {
            return; // Skip nested prefabs
        }

        Component[] components = prefab.GetComponentsInChildren<Component>(true);
        foreach (Component component in components)
        {
            if (component != null)
            {
                var foundClasses = FindSerializableClassesInObject(component, context);
                foreach (var className in foundClasses)
                {
                    if (_classReferences.ContainsKey(className))
                    {
                        _classReferences[className].Add(context); // Add only top-level prefab reference
                    }
                }
            }
        }
    }


    private void SearchObjectForSerializableClasses(Object obj, string context)
    {
        var foundClasses = FindSerializableClassesInObject(obj, context);
        foreach (var className in foundClasses)
        {
            if (_classReferences.ContainsKey(className))
            {
                _classReferences[className].Add(context);
            }
        }
    }

    private void ScanScenesForSerializableClasses()
    {
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene scene = SceneManager.GetSceneAt(i);
            if (!scene.isLoaded) continue;

            GameObject[] rootObjects = scene.GetRootGameObjects();
            foreach (GameObject rootObject in rootObjects)
            {
                Component[] components = rootObject.GetComponentsInChildren<Component>(true);
                foreach (Component component in components)
                {
                    if (component != null)
                    {
                        var foundClasses = FindSerializableClassesInObject(component, $"Scene: {scene.path}, GameObject: {rootObject.name}");
                        foreach (var className in foundClasses)
                        {
                            if (_classReferences.ContainsKey(className))
                            {
                                _classReferences[className].Add($"{scene.path}/{rootObject.name}");
                            }
                        }
                    }
                }
            }
        }
    }

    private List<string> FindSerializableClassesInObject(Object obj, string context)
    {
        HashSet<string> foundClasses = new HashSet<string>();
        if (obj == null) return foundClasses.ToList();

        Type objType = obj.GetType();
        const BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly;

        // Direct field references (non-[SerializeReference])
        FieldInfo[] fields = objType.GetFields(bindingFlags);
        foreach (var field in fields)
        {
            if (_serializableClasses.Contains(field.FieldType))
            {
                foundClasses.Add(field.FieldType.FullName);
                //Debug.Log($"Found direct reference to {field.FieldType.Name} in field {field.Name} of {obj.name} ({objType.Name}) in {context}");
            }
        }

        // [SerializeReference] managed references
        using (SerializedObject serializedObject = new SerializedObject(obj))
        {
            SerializedProperty property = serializedObject.GetIterator();
            bool enterChildren = true;

            while (property.NextVisible(enterChildren))
            {
                enterChildren = true;

                if (property.propertyType == SerializedPropertyType.ManagedReference)
                {
                    object managedRef = property.managedReferenceValue;
                    if (managedRef != null)
                    {
                        Type managedType = managedRef.GetType();
                        if (_serializableClasses.Contains(managedType))
                        {
                            foundClasses.Add(managedType.FullName);
                            //Debug.Log($"Found [SerializeReference] reference to {managedType.Name} in {obj.name} in {context}");
                        }
                    }
                }
            }
        }

        return foundClasses.ToList();
    }

    private void DrawResults()
    {
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Serializable Class References", EditorStyles.boldLabel);

        _showSceneReferences = EditorGUILayout.Toggle("Show Scene References", _showSceneReferences);

        foreach (var kvp in _classReferences)
        {
            var className = kvp.Key;
            var references = kvp.Value.ToList(); // Convert HashSet to List

            if (references.Count == 0) continue;

            if (!string.IsNullOrEmpty(_searchText) && !className.ToLower().Contains(_searchText.ToLower()))
            {
                continue;
            }
           
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField(className, EditorStyles.boldLabel);

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
                    assetPath = TrimPathAfterUnity(assetPath);
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
                        PingOrOpenReference(reference);
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
            string objectName = reference.Substring(reference.LastIndexOf('/') + 1);
            if (!EditorSceneManager.GetSceneByPath(TrimPathAfterUnity(reference)).isLoaded)
            {
                EditorSceneManager.OpenScene(TrimPathAfterUnity(reference), OpenSceneMode.Single);
            }
            GameObject target = GameObject.Find(objectName);
            if (target != null)
            {
                //Selection.activeObject = target;
                EditorGUIUtility.PingObject(target);
            }
        }
        else
        {
            Object obj = AssetDatabase.LoadAssetAtPath<Object>(reference);
            if (obj != null)
            {
                //Selection.activeObject = obj;
                EditorGUIUtility.PingObject(obj);
            }
        }
    }

    private string TrimPathAfterUnity(string path)
    {
        int index = path.IndexOf(".unity");
        return index != -1 ? path.Substring(0, index + 6) : path;
    }
}