using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

public class SerializeReferenceAssemblyFixer : EditorWindow
{
    [MenuItem("Tools/SerializeReference Fixer")]
    public static void ShowWindow()
    {
        GetWindow<SerializeReferenceAssemblyFixer>("SerializeRef Asm Fixer");
    }

    #region Private Fields
    private Vector2 _typeScrollPos;
    private Vector2 _assetScrollPos;
    private List<IBrokenAsset> _brokenAssets = new List<IBrokenAsset>();
    private bool _scanCompleted = false;
    
    private List<Type> _availableTypes = new List<Type>();
    private List<bool> _selectedTypes = new List<bool>();
    private string[] _typeNames;
    private bool _showTypeSelection = true;
    private string _searchFilter = "";
    private List<int> _filteredIndices = new List<int>();
    
    // Asset type selection
    private bool _scanScriptableObjects = true;
    private bool _scanPrefabs = true;
    
    // Type mapping cache
    private Dictionary<string, string> _typeToAssemblyMapping;
    private bool _mappingCached = false;
    #endregion

    #region Unity Lifecycle
    private void OnEnable()
    {
        RefreshAvailableTypes();
        InvalidateTypeMapping();
    }

    private void OnGUI()
    {
        DrawHeader();
        DrawAssetTypeSelection();
        DrawTypeSelection();
        DrawScanControls();
        DrawResults();
    }
    #endregion

    #region GUI Drawing Methods
    private void DrawHeader()
    {
        EditorGUILayout.LabelField("SerializeReference Assembly Fixer", EditorStyles.boldLabel);
        EditorGUILayout.Space();
    }

    private void DrawAssetTypeSelection()
    {
        EditorGUILayout.LabelField("Asset Types to Scan:", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        _scanScriptableObjects = EditorGUILayout.Toggle("ScriptableObjects", _scanScriptableObjects);
        _scanPrefabs = EditorGUILayout.Toggle("Prefabs", _scanPrefabs);
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space();

        if (!_scanScriptableObjects && !_scanPrefabs)
        {
            EditorGUILayout.HelpBox("Please select at least one asset type to scan.", MessageType.Warning);
            return;
        }
    }

    private void DrawTypeSelection()
    {
        string foldoutLabel = _scanScriptableObjects && _scanPrefabs 
            ? "Select Types to Scan (ScriptableObject & MonoBehaviour)"
            : _scanScriptableObjects 
                ? "Select ScriptableObject Types to Scan"
                : "Select MonoBehaviour Types to Scan";

        _showTypeSelection = EditorGUILayout.Foldout(_showTypeSelection, foldoutLabel);
        
        if (_showTypeSelection)
        {
            EditorGUILayout.BeginVertical("box");
            DrawSearchControls();
            DrawTypeList();
            EditorGUILayout.EndVertical();
        }

        EditorGUILayout.Space();
    }

    private void DrawSearchControls()
    {
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Search:", GUILayout.Width(50));
        string newSearchFilter = EditorGUILayout.TextField(_searchFilter);
        if (newSearchFilter != _searchFilter)
        {
            _searchFilter = newSearchFilter;
            UpdateFilteredList();
        }
        if (GUILayout.Button("Clear", GUILayout.Width(50)))
        {
            _searchFilter = "";
            UpdateFilteredList();
            GUI.FocusControl(null);
        }
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space();
        
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Select All"))
        {
            SetAllTypesSelection(true);
        }
        if (GUILayout.Button("Select None"))
        {
            SetAllTypesSelection(false);
        }
        EditorGUILayout.EndHorizontal();
        
        if (GUILayout.Button("Refresh Types"))
        {
            RefreshAvailableTypes();
        }
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField($"Showing {_filteredIndices.Count} of {_availableTypes.Count} types");
    }

    private void DrawTypeList()
    {
        _typeScrollPos = EditorGUILayout.BeginScrollView(_typeScrollPos, GUILayout.Height(200));
        
        foreach (int i in _filteredIndices)
        {
            EditorGUILayout.BeginHorizontal();
            _selectedTypes[i] = EditorGUILayout.Toggle(_selectedTypes[i], GUILayout.Width(20));
            
            string typeName = _typeNames[i];
            string assemblyName = _availableTypes[i].Assembly.GetName().Name;
            string baseTypeName = GetBaseTypeName(_availableTypes[i]);
            
            var style = CreateTypeDisplayStyle(typeName);
            EditorGUILayout.LabelField($"{typeName} [{baseTypeName}]", style);
            EditorGUILayout.LabelField($"({assemblyName})", EditorStyles.miniLabel);
            EditorGUILayout.EndHorizontal();
        }
        
        EditorGUILayout.EndScrollView();
    }

    private void DrawScanControls()
    {
        int selectedCount = _selectedTypes.Count(x => x);
        EditorGUILayout.LabelField($"Selected Types: {selectedCount}");

        if (selectedCount == 0)
        {
            EditorGUILayout.HelpBox("Please select at least one type to scan.", MessageType.Warning);
            return;
        }

        if (GUILayout.Button("Scan Selected Types for Broken References"))
        {
            ScanSelectedTypes();
        }

        EditorGUILayout.Space();
    }

    private void DrawResults()
    {
        if (!_scanCompleted) return;

        EditorGUILayout.LabelField($"Found {_brokenAssets.Count} assets with broken references:");
        
        if (_brokenAssets.Count > 0)
        {
            DrawBrokenAssetsList();
            DrawFixAllButton();
        }
        else
        {
            EditorGUILayout.HelpBox("No broken references found!", MessageType.Info);
        }
    }

    private void DrawBrokenAssetsList()
    {
        _assetScrollPos = EditorGUILayout.BeginScrollView(_assetScrollPos, GUILayout.MinHeight(100), GUILayout.MaxHeight(300));
        
        foreach (var asset in _brokenAssets)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.ObjectField(asset.GetUnityObject(), asset.GetAssetType(), false);
            EditorGUILayout.LabelField($"({asset.GetDisplayInfo()})", EditorStyles.miniLabel, GUILayout.Width(200));
            if (GUILayout.Button("Fix", GUILayout.Width(70)))
            {
                FixAssetDirectly(asset);
            }
            EditorGUILayout.EndHorizontal();
        }
        
        EditorGUILayout.EndScrollView();
    }

    private void DrawFixAllButton()
    {
        EditorGUILayout.Space();
        if (GUILayout.Button("Fix All", GUILayout.Height(30)))
        {
            FixAllAssets();
        }
    }
    #endregion

    #region Type Management
    private void RefreshAvailableTypes()
    {
        _availableTypes.Clear();
        _selectedTypes.Clear();
        
        var allTypes = new List<Type>();
        
        if (_scanScriptableObjects)
        {
            allTypes.AddRange(GetScriptableObjectTypes());
        }
        
        if (_scanPrefabs)
        {
            allTypes.AddRange(GetMonoBehaviourTypes());
        }
        
        _availableTypes = allTypes.OrderBy(t => t.Name).ToList();
        _selectedTypes = Enumerable.Repeat(false, _availableTypes.Count).ToList();
        _typeNames = _availableTypes.Select(t => t.Name).ToArray();
        
        UpdateFilteredList();
        
        Debug.Log($"Found {_availableTypes.Count} types ({GetScriptableObjectTypes().Count()} ScriptableObjects, {GetMonoBehaviourTypes().Count()} MonoBehaviours)");
    }

    private IEnumerable<Type> GetScriptableObjectTypes()
    {
        return GetTypesFromAssemblies(t => 
            typeof(ScriptableObject).IsAssignableFrom(t) && 
            !t.IsAbstract && 
            t != typeof(ScriptableObject));
    }

    private IEnumerable<Type> GetMonoBehaviourTypes()
    {
        return GetTypesFromAssemblies(t => 
            typeof(MonoBehaviour).IsAssignableFrom(t) && 
            !t.IsAbstract && 
            t != typeof(MonoBehaviour));
    }

    private IEnumerable<Type> GetTypesFromAssemblies(Func<Type, bool> predicate)
    {
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();
        var types = new List<Type>();
        
        foreach (var assembly in assemblies)
        {
            try
            {
                types.AddRange(assembly.GetTypes().Where(predicate));
            }
            catch (ReflectionTypeLoadException ex)
            {
                // Handle partial assembly loading
                types.AddRange(ex.Types.Where(t => t != null && predicate(t)));
            }
            catch
            {
                continue;
            }
        }
        
        return types;
    }

    private void UpdateFilteredList()
    {
        _filteredIndices.Clear();
        
        if (string.IsNullOrEmpty(_searchFilter))
        {
            _filteredIndices.AddRange(Enumerable.Range(0, _availableTypes.Count));
        }
        else
        {
            string lowerFilter = _searchFilter.ToLower();
            for (int i = 0; i < _availableTypes.Count; i++)
            {
                if (TypeMatchesFilter(_availableTypes[i], lowerFilter))
                {
                    _filteredIndices.Add(i);
                }
            }
        }
    }

    private bool TypeMatchesFilter(Type type, string lowerFilter)
    {
        return type.Name.ToLower().Contains(lowerFilter) ||
               type.Assembly.GetName().Name.ToLower().Contains(lowerFilter) ||
               (type.FullName?.ToLower().Contains(lowerFilter) ?? false);
    }

    private void SetAllTypesSelection(bool selected)
    {
        for (int i = 0; i < _selectedTypes.Count; i++)
        {
            _selectedTypes[i] = selected;
        }
    }

    private string GetBaseTypeName(Type type)
    {
        if (typeof(ScriptableObject).IsAssignableFrom(type))
            return "SO";
        if (typeof(MonoBehaviour).IsAssignableFrom(type))
            return "MB";
        return "?";
    }

    private GUIStyle CreateTypeDisplayStyle(string typeName)
    {
        var style = new GUIStyle(EditorStyles.label);
        if (!string.IsNullOrEmpty(_searchFilter) && 
            typeName.ToLower().Contains(_searchFilter.ToLower()))
        {
            style.normal.textColor = Color.yellow;
        }
        return style;
    }
    #endregion

    #region Scanning Logic
    private void ScanSelectedTypes()
    {
        _brokenAssets.Clear();
        InvalidateTypeMapping();
        
        var selectedTypesList = GetSelectedTypes();
        
        Debug.Log($"=== SCANNING {selectedTypesList.Count} SELECTED TYPES ===");
        
        var scriptableObjectTypes = selectedTypesList.Where(t => typeof(ScriptableObject).IsAssignableFrom(t)).ToList();
        var monoBehaviourTypes = selectedTypesList.Where(t => typeof(MonoBehaviour).IsAssignableFrom(t)).ToList();
        
        if (_scanScriptableObjects && scriptableObjectTypes.Any())
        {
            ScanScriptableObjects(scriptableObjectTypes);
        }
        
        if (_scanPrefabs && monoBehaviourTypes.Any())
        {
            ScanPrefabs(monoBehaviourTypes);
        }
        
        _scanCompleted = true;
        
        Debug.Log($"=== SCAN COMPLETED ===");
        Debug.Log($"Assets with broken assembly references: {_brokenAssets.Count}");
    }

    private List<Type> GetSelectedTypes()
    {
        var selectedTypes = new List<Type>();
        for (int i = 0; i < _availableTypes.Count; i++)
        {
            if (_selectedTypes[i])
                selectedTypes.Add(_availableTypes[i]);
        }
        return selectedTypes;
    }

    private void ScanScriptableObjects(List<Type> targetTypes)
    {
        string[] allGuids = AssetDatabase.FindAssets("t:ScriptableObject");
        var relevantAssets = FilterAssetsByType<ScriptableObject>(allGuids, targetTypes, "ScriptableObject");
        
        ScanAssetsForBrokenReferences(relevantAssets, 
            asset => new BrokenScriptableObject { Asset = asset, Path = AssetDatabase.GetAssetPath(asset) });
    }

    private void ScanPrefabs(List<Type> targetTypes)
    {
        string[] allGuids = AssetDatabase.FindAssets("t:Prefab");
        var relevantPrefabs = new List<GameObject>();
        
        for (int i = 0; i < allGuids.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(allGuids[i]);
            EditorUtility.DisplayProgressBar("Filtering Prefabs", $"Checking {path}", (float)i / allGuids.Length);
            
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab != null && PrefabContainsTargetTypes(prefab, targetTypes))
            {
                relevantPrefabs.Add(prefab);
            }
        }
        
        EditorUtility.ClearProgressBar();
        Debug.Log($"Found {relevantPrefabs.Count} prefabs with target MonoBehaviour types");
        
        ScanAssetsForBrokenReferences(relevantPrefabs, 
            prefab => new BrokenPrefab { Prefab = prefab, Path = AssetDatabase.GetAssetPath(prefab) });
    }

    private List<T> FilterAssetsByType<T>(string[] guids, List<Type> targetTypes, string assetTypeName) where T : UnityEngine.Object
    {
        var relevantAssets = new List<T>();
        
        for (int i = 0; i < guids.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[i]);
            EditorUtility.DisplayProgressBar($"Filtering {assetTypeName}s", $"Checking {path}", (float)i / guids.Length);
            
            var asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset != null && targetTypes.Contains(asset.GetType()))
            {
                relevantAssets.Add(asset);
            }
        }
        
        EditorUtility.ClearProgressBar();
        Debug.Log($"Found {relevantAssets.Count} {assetTypeName.ToLower()}s matching selected types");
        
        return relevantAssets;
    }

    private bool PrefabContainsTargetTypes(GameObject prefab, List<Type> targetTypes)
    {
        var components = prefab.GetComponentsInChildren<MonoBehaviour>(true);
        return components.Any(component => component != null && targetTypes.Contains(component.GetType()));
    }

    private void ScanAssetsForBrokenReferences<T>(List<T> assets, Func<T, IBrokenAsset> createBrokenAsset) where T : UnityEngine.Object
    {
        for (int i = 0; i < assets.Count; i++)
        {
            var asset = assets[i];
            string path = AssetDatabase.GetAssetPath(asset);
            
            EditorUtility.DisplayProgressBar("Scanning YAML", $"Checking {asset.name}", (float)i / assets.Count);
            
            if (HasBrokenReferencesInYAML(path))
            {
                _brokenAssets.Add(createBrokenAsset(asset));
                Debug.Log($"Found broken references in: {asset.name}");
            }
        }
        
        EditorUtility.ClearProgressBar();
    }

    private bool HasBrokenReferencesInYAML(string assetPath)
    {
        try
        {
            string yamlContent = File.ReadAllText(assetPath);
            
            string[] oldAssemblyPatterns = {
                "asm: Assembly-CSharp}",
                "asm: Assembly-CSharp-firstpass}"
            };
            
            return oldAssemblyPatterns.Any(pattern => yamlContent.Contains(pattern));
        }
        catch (Exception e)
        {
            Debug.LogError($"Error reading YAML file {assetPath}: {e.Message}");
            return false;
        }
    }
    #endregion

    #region Fixing Logic
    private void FixAssetDirectly(IBrokenAsset brokenAsset)
    {
        try
        {
            string assetPath = brokenAsset.GetAssetPath();
            Debug.Log($"=== FIXING YAML: {brokenAsset.GetDisplayName()} ===");
            
            string yamlContent = File.ReadAllText(assetPath);
            string originalContent = yamlContent;
            
            EnsureTypeMappingCached();
            yamlContent = FixAssemblyReferencesInYAML(yamlContent, _typeToAssemblyMapping);
            
            if (yamlContent != originalContent)
            {
                CreateBackupAndSave(assetPath, originalContent, yamlContent);
                Debug.Log($"✓ Successfully fixed {brokenAsset.GetDisplayName()}");
            }
            else
            {
                Debug.Log($"No changes needed for {brokenAsset.GetDisplayName()}");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error fixing YAML for {brokenAsset.GetDisplayName()}: {e.Message}");
        }
    }

    private void FixAllAssets()
    {
        for (int i = 0; i < _brokenAssets.Count; i++)
        {
            EditorUtility.DisplayProgressBar("Fixing YAML Files", 
                $"Fixing {_brokenAssets[i].GetDisplayName()}", 
                (float)i / _brokenAssets.Count);
            
            FixAssetDirectly(_brokenAssets[i]);
        }
        
        EditorUtility.ClearProgressBar();
        AssetDatabase.Refresh();
        
        Debug.Log("=== ALL YAML FIXES COMPLETE ===");
        
        // Re-scan to verify fixes
        ScanSelectedTypes();
    }

    private void CreateBackupAndSave(string assetPath, string originalContent, string newContent)
    {
        string backupPath = assetPath + ".backup";
        File.WriteAllText(backupPath, originalContent);
        Debug.Log($"Created backup: {backupPath}");
        
        File.WriteAllText(assetPath, newContent);
        Debug.Log($"✓ Fixed YAML file: {assetPath}");
        
        AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
    }
    #endregion

    #region Type Mapping
    private void EnsureTypeMappingCached()
    {
        if (!_mappingCached)
        {
            _typeToAssemblyMapping = BuildTypeToAssemblyMapping();
            _mappingCached = true;
        }
    }

    private void InvalidateTypeMapping()
    {
        _mappingCached = false;
        _typeToAssemblyMapping = null;
    }

    private Dictionary<string, string> BuildTypeToAssemblyMapping()
    {
        var mapping = new Dictionary<string, string>();
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();
        
        foreach (var assembly in assemblies)
        {
            try
            {
                var types = assembly.GetTypes();
                string assemblyName = assembly.GetName().Name;
                
                foreach (var type in types)
                {
                    TryAddTypeMapping(mapping, type.Name, assemblyName);
                    if (!string.IsNullOrEmpty(type.FullName))
                    {
                        TryAddTypeMapping(mapping, type.FullName, assemblyName);
                    }
                }
            }
            catch (ReflectionTypeLoadException ex)
            {
                // Handle partial assembly loading
                string assemblyName = assembly.GetName().Name;
                foreach (var type in ex.Types.Where(t => t != null))
                {
                    TryAddTypeMapping(mapping, type.Name, assemblyName);
                    if (!string.IsNullOrEmpty(type.FullName))
                    {
                        TryAddTypeMapping(mapping, type.FullName, assemblyName);
                    }
                }
            }
            catch
            {
                continue;
            }
        }
        
        Debug.Log($"Built type-to-assembly mapping with {mapping.Count} entries");
        return mapping;
    }

    private void TryAddTypeMapping(Dictionary<string, string> mapping, string typeName, string assemblyName)
    {
        if (!mapping.ContainsKey(typeName))
        {
            mapping[typeName] = assemblyName;
        }
    }

    private string FixAssemblyReferencesInYAML(string yamlContent, Dictionary<string, string> typeToAssembly)
    {
        var lines = yamlContent.Split('\n');
        bool modified = false;
        
        for (int i = 0; i < lines.Length; i++)
        {
            string line = lines[i];
            
            if (line.Trim().StartsWith("type: {class:"))
            {
                string newLine = ProcessYAMLTypeLine(line, typeToAssembly);
                if (newLine != line)
                {
                    lines[i] = newLine;
                    modified = true;
                }
            }
        }
        
        return modified ? string.Join("\n", lines) : yamlContent;
    }

    private string ProcessYAMLTypeLine(string line, Dictionary<string, string> typeToAssembly)
    {
        int classStart = line.IndexOf("class: ") + 7;
        int classEnd = line.IndexOf(",", classStart);
        
        if (classStart <= 6 || classEnd <= classStart) return line;
        
        string className = line.Substring(classStart, classEnd - classStart).Trim();
        
        if (typeToAssembly.ContainsKey(className))
        {
            string correctAssembly = typeToAssembly[className];
            string pattern = "asm: [^}]+}";
            string replacement = $"asm: {correctAssembly}}}";
            
            string newLine = System.Text.RegularExpressions.Regex.Replace(line, pattern, replacement);
            
            if (newLine != line)
            {
                Debug.Log($"Fixed assembly for {className}: {correctAssembly}");
                return newLine;
            }
        }
        else
        {
            Debug.LogWarning($"Could not find assembly for type: {className}");
        }
        
        return line;
    }
    #endregion

    #region Asset Abstraction
    private interface IBrokenAsset
    {
        UnityEngine.Object GetUnityObject();
        Type GetAssetType();
        string GetAssetPath();
        string GetDisplayName();
        string GetDisplayInfo();
    }

    [System.Serializable]
    private class BrokenScriptableObject : IBrokenAsset
    {
        public ScriptableObject Asset;
        public string Path;

        public UnityEngine.Object GetUnityObject() => Asset;
        public Type GetAssetType() => typeof(ScriptableObject);
        public string GetAssetPath() => Path;
        public string GetDisplayName() => Asset.name;
        public string GetDisplayInfo() => Asset.GetType().Name;
    }

    [System.Serializable]
    private class BrokenPrefab : IBrokenAsset
    {
        public GameObject Prefab;
        public string Path;

        public UnityEngine.Object GetUnityObject() => Prefab;
        public Type GetAssetType() => typeof(GameObject);
        public string GetAssetPath() => Path;
        public string GetDisplayName() => Prefab.name;
        public string GetDisplayInfo() => "Prefab";
    }
    #endregion
}