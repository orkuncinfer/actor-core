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

    private Vector2 typeScrollPos;
    private Vector2 assetScrollPos;
    private List<BrokenAsset> brokenAssets = new List<BrokenAsset>();
    private bool scanCompleted = false;
    
    private List<Type> availableTypes = new List<Type>();
    private List<bool> selectedTypes = new List<bool>();
    private string[] typeNames;
    private bool showTypeSelection = true;
    private string searchFilter = "";
    private List<int> filteredIndices = new List<int>();

    private void OnEnable()
    {
        RefreshAvailableTypes();
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("SerializeReference Assembly Fixer", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        showTypeSelection = EditorGUILayout.Foldout(showTypeSelection, "Select ScriptableObject Types to Scan");
        
        if (showTypeSelection)
        {
            EditorGUILayout.BeginVertical("box");
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Search:", GUILayout.Width(50));
            string newSearchFilter = EditorGUILayout.TextField(searchFilter);
            if (newSearchFilter != searchFilter)
            {
                searchFilter = newSearchFilter;
                UpdateFilteredList();
            }
            if (GUILayout.Button("Clear", GUILayout.Width(50)))
            {
                searchFilter = "";
                UpdateFilteredList();
                GUI.FocusControl(null);
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space();
            
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Select All"))
            {
                for (int i = 0; i < selectedTypes.Count; i++)
                    selectedTypes[i] = true;
            }
            if (GUILayout.Button("Select None"))
            {
                for (int i = 0; i < selectedTypes.Count; i++)
                    selectedTypes[i] = false;
            }
            EditorGUILayout.EndHorizontal();
            
            if (GUILayout.Button("Refresh Types"))
            {
                RefreshAvailableTypes();
            }
            
            EditorGUILayout.Space();
            
            EditorGUILayout.LabelField($"Showing {filteredIndices.Count} of {availableTypes.Count} types");
            
            typeScrollPos = EditorGUILayout.BeginScrollView(typeScrollPos, GUILayout.Height(200));
            
            foreach (int i in filteredIndices)
            {
                EditorGUILayout.BeginHorizontal();
                selectedTypes[i] = EditorGUILayout.Toggle(selectedTypes[i], GUILayout.Width(20));
                
                string typeName = typeNames[i];
                string assemblyName = availableTypes[i].Assembly.GetName().Name;
                
                if (!string.IsNullOrEmpty(searchFilter))
                {
                    var style = new GUIStyle(EditorStyles.label);
                    if (typeName.ToLower().Contains(searchFilter.ToLower()))
                    {
                        style.normal.textColor = Color.yellow;
                    }
                    EditorGUILayout.LabelField(typeName, style);
                }
                else
                {
                    EditorGUILayout.LabelField(typeName);
                }
                
                EditorGUILayout.LabelField($"({assemblyName})", EditorStyles.miniLabel);
                EditorGUILayout.EndHorizontal();
            }
            
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        EditorGUILayout.Space();

        int selectedCount = selectedTypes.Count(x => x);
        EditorGUILayout.LabelField($"Selected Types: {selectedCount}");

        if (selectedCount == 0)
        {
            EditorGUILayout.HelpBox("Please select at least one ScriptableObject type to scan.", MessageType.Warning);
            return;
        }

        if (GUILayout.Button("Scan Selected Types for Broken References"))
        {
            ScanSelectedTypes();
        }

        EditorGUILayout.Space();

        if (scanCompleted)
        {
            EditorGUILayout.LabelField($"Found {brokenAssets.Count} assets with broken references:");
            
            if (brokenAssets.Count > 0)
            {
                assetScrollPos = EditorGUILayout.BeginScrollView(assetScrollPos, GUILayout.MinHeight(100), GUILayout.MaxHeight(300));
                
                foreach (var asset in brokenAssets)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.ObjectField(asset.Asset, typeof(ScriptableObject), false);
                    EditorGUILayout.LabelField($"({asset.Asset.GetType().Name})", EditorStyles.miniLabel, GUILayout.Width(150));
                    if (GUILayout.Button("Fix", GUILayout.Width(70)))
                    {
                        FixYAMLDirectly(asset);
                    }
                    EditorGUILayout.EndHorizontal();
                }
                
                EditorGUILayout.EndScrollView();
            }
            else
            {
                EditorGUILayout.HelpBox("No broken references found!", MessageType.Info);
            }
        }

        if (scanCompleted && brokenAssets.Count > 0)
        {
            EditorGUILayout.Space();
            if (GUILayout.Button("Fix All", GUILayout.Height(30)))
            {
                FixAllYAMLFiles();
            }
        }
    }

    private void RefreshAvailableTypes()
    {
        availableTypes.Clear();
        selectedTypes.Clear();
        
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();
        var scriptableObjectTypes = new List<Type>();
        
        foreach (var assembly in assemblies)
        {
            try
            {
                var types = assembly.GetTypes()
                    .Where(t => typeof(ScriptableObject).IsAssignableFrom(t))
                    .Where(t => !t.IsAbstract)
                    .Where(t => t != typeof(ScriptableObject))
                    .OrderBy(t => t.Name);
                
                scriptableObjectTypes.AddRange(types);
            }
            catch
            {
                continue;
            }
        }
        
        availableTypes = scriptableObjectTypes.OrderBy(t => t.Name).ToList();
        selectedTypes = Enumerable.Repeat(false, availableTypes.Count).ToList();
        typeNames = availableTypes.Select(t => t.Name).ToArray();
        
        UpdateFilteredList();
        
        Debug.Log($"Found {availableTypes.Count} ScriptableObject types");
    }

    private void UpdateFilteredList()
    {
        filteredIndices.Clear();
        
        if (string.IsNullOrEmpty(searchFilter))
        {
            for (int i = 0; i < availableTypes.Count; i++)
            {
                filteredIndices.Add(i);
            }
        }
        else
        {
            string lowerFilter = searchFilter.ToLower();
            for (int i = 0; i < availableTypes.Count; i++)
            {
                string typeName = typeNames[i].ToLower();
                string assemblyName = availableTypes[i].Assembly.GetName().Name.ToLower();
                string fullName = availableTypes[i].FullName?.ToLower() ?? "";
                
                if (typeName.Contains(lowerFilter) || 
                    assemblyName.Contains(lowerFilter) || 
                    fullName.Contains(lowerFilter))
                {
                    filteredIndices.Add(i);
                }
            }
        }
    }

    private void ScanSelectedTypes()
    {
        brokenAssets.Clear();
        
        var selectedTypesList = new List<Type>();
        for (int i = 0; i < availableTypes.Count; i++)
        {
            if (selectedTypes[i])
                selectedTypesList.Add(availableTypes[i]);
        }
        
        Debug.Log($"=== SCANNING {selectedTypesList.Count} SELECTED TYPES ===");
        
        string[] allGuids = AssetDatabase.FindAssets("t:ScriptableObject");
        var relevantAssets = new List<ScriptableObject>();
        
        for (int i = 0; i < allGuids.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(allGuids[i]);
            EditorUtility.DisplayProgressBar("Filtering Assets", $"Checking {path}", (float)i / allGuids.Length);
            
            var asset = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);
            if (asset != null && selectedTypesList.Contains(asset.GetType()))
            {
                relevantAssets.Add(asset);
            }
        }
        
        EditorUtility.ClearProgressBar();
        
        Debug.Log($"Found {relevantAssets.Count} assets matching selected types");
        
        for (int i = 0; i < relevantAssets.Count; i++)
        {
            var asset = relevantAssets[i];
            string path = AssetDatabase.GetAssetPath(asset);
            
            EditorUtility.DisplayProgressBar("Scanning YAML", $"Checking {asset.name}", (float)i / relevantAssets.Count);
            
            if (HasBrokenReferencesInYAML(path))
            {
                brokenAssets.Add(new BrokenAsset { Asset = asset, Path = path });
                Debug.Log($"Found broken references in: {asset.name}");
            }
        }
        
        EditorUtility.ClearProgressBar();
        scanCompleted = true;
        
        Debug.Log($"=== SCAN COMPLETED ===");
        Debug.Log($"Assets with broken assembly references: {brokenAssets.Count}");
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
            
            foreach (string pattern in oldAssemblyPatterns)
            {
                if (yamlContent.Contains(pattern))
                {
                    return true;
                }
            }
            
            return false;
        }
        catch (Exception e)
        {
            Debug.LogError($"Error reading YAML file {assetPath}: {e.Message}");
            return false;
        }
    }

    private void FixYAMLDirectly(BrokenAsset brokenAsset)
    {
        try
        {
            string assetPath = brokenAsset.Path;
            Debug.Log($"=== FIXING YAML: {brokenAsset.Asset.name} ===");
            
            string yamlContent = File.ReadAllText(assetPath);
            string originalContent = yamlContent;
            
            var typeToAssembly = BuildTypeToAssemblyMapping();
            
            yamlContent = FixAssemblyReferencesInYAML(yamlContent, typeToAssembly);
            
            if (yamlContent != originalContent)
            {
                string backupPath = assetPath + ".backup";
                File.WriteAllText(backupPath, originalContent);
                Debug.Log($"Created backup: {backupPath}");
                
                File.WriteAllText(assetPath, yamlContent);
                Debug.Log($"✓ Fixed YAML file: {assetPath}");
                
                AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
                
                Debug.Log($"✓ Successfully fixed {brokenAsset.Asset.name}");
            }
            else
            {
                Debug.Log($"No changes needed for {brokenAsset.Asset.name}");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error fixing YAML for {brokenAsset.Asset.name}: {e.Message}");
        }
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
                    if (!mapping.ContainsKey(type.Name))
                    {
                        mapping[type.Name] = assemblyName;
                    }
                    
                    if (!string.IsNullOrEmpty(type.FullName) && !mapping.ContainsKey(type.FullName))
                    {
                        mapping[type.FullName] = assemblyName;
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

    private string FixAssemblyReferencesInYAML(string yamlContent, Dictionary<string, string> typeToAssembly)
    {
        var lines = yamlContent.Split('\n');
        bool modified = false;
        
        for (int i = 0; i < lines.Length; i++)
        {
            string line = lines[i];
            
            if (line.Trim().StartsWith("type: {class:"))
            {
                int classStart = line.IndexOf("class: ") + 7;
                int classEnd = line.IndexOf(",", classStart);
                
                if (classStart > 6 && classEnd > classStart)
                {
                    string className = line.Substring(classStart, classEnd - classStart).Trim();
                    
                    if (typeToAssembly.ContainsKey(className))
                    {
                        string correctAssembly = typeToAssembly[className];
                        
                        string pattern = "asm: [^}]+}";
                        string replacement = $"asm: {correctAssembly}}}";
                        
                        string newLine = System.Text.RegularExpressions.Regex.Replace(line, pattern, replacement);
                        
                        if (newLine != line)
                        {
                            lines[i] = newLine;
                            modified = true;
                            Debug.Log($"Fixed assembly for {className}: {correctAssembly}");
                        }
                    }
                    else
                    {
                        Debug.LogWarning($"Could not find assembly for type: {className}");
                    }
                }
            }
        }
        
        if (modified)
        {
            return string.Join("\n", lines);
        }
        
        return yamlContent;
    }

    private void FixAllYAMLFiles()
    {
        for (int i = 0; i < brokenAssets.Count; i++)
        {
            EditorUtility.DisplayProgressBar("Fixing YAML Files", 
                $"Fixing {brokenAssets[i].Asset.name}", 
                (float)i / brokenAssets.Count);
            
            FixYAMLDirectly(brokenAssets[i]);
        }
        
        EditorUtility.ClearProgressBar();
        AssetDatabase.Refresh();
        
        Debug.Log("=== ALL YAML FIXES COMPLETE ===");
        
        ScanSelectedTypes();
    }

    [System.Serializable]
    private class BrokenAsset
    {
        public ScriptableObject Asset;
        public string Path;
    }
}