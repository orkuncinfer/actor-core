#if UNITY_EDITOR
using System;
using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Sirenix.OdinInspector;
using UnityEditor;

[CreateAssetMenu(fileName = "GameplayTags", menuName = "Gameplay/Tags", order = 1)]
public class GameplayTagsAsset : ScriptableObject
{
    public List<GameplayTagFetcher> TagsCache = new List<GameplayTagFetcher>();
    [Button]
    void FetchGameplayTagsForAllAssets()
    {
        // Process all MonoBehaviour instances in the scene
        ProcessMonobehaviours();

        // Process all prefabs
        ProcessPrefabs();

        // Process all ScriptableObjects
        ProcessScriptableObjects();

        // Save changes to assets
        AssetDatabase.SaveAssets();
    }

    void ProcessMonobehaviours()
{
    MonoBehaviour[] allMonos = Resources.FindObjectsOfTypeAll<MonoBehaviour>();
    foreach (var mono in allMonos)
    {
        if (PrefabUtility.IsPartOfPrefabInstance(mono))
            continue;  // Skip prefab instances that are part of the scene
        ProcessObject(mono);
    }
}

void ProcessPrefabs()
{
    string[] allAssetPaths = AssetDatabase.GetAllAssetPaths();
    var prefabPaths = allAssetPaths.Where(path => path.EndsWith(".prefab"));
    foreach (var path in prefabPaths)
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (prefab != null)
        {
            MonoBehaviour[] monoBehaviours = prefab.GetComponentsInChildren<MonoBehaviour>(true);
            foreach (var mono in monoBehaviours)
            {
                ProcessObject(mono);
            }
        }
    }
}

void ProcessScriptableObjects()
{
    string[] allAssetPaths = AssetDatabase.GetAllAssetPaths();
    var scriptableObjectPaths = allAssetPaths.Where(path => path.EndsWith(".asset"));
    foreach (var path in scriptableObjectPaths)
    {
        ScriptableObject scriptableObject = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);
        if (scriptableObject != null)
        {
            ProcessObject(scriptableObject);
        }
    }
}

void ProcessType(object obj, Type type, string path, HashSet<object> visited)
{
    // Check for cyclic references by examining if we've already visited this object
    if (visited.Contains(obj))
    {
        DDebug.Log("Cyclic reference detected, skipping object: " + path);
        return;
    }
    visited.Add(obj);

    foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
    {
        var fieldType = field.FieldType;
        var fieldValue = field.GetValue(obj);

        // Direct usage of GameplayTag
        if (fieldType == typeof(GameplayTag))
        {
            ProcessGameplayTag((GameplayTag)fieldValue);
        }
        // Handle list of GameplayTags
        else if (fieldType.IsGenericType && fieldType.GetGenericTypeDefinition() == typeof(List<>))
        {
            var listType = fieldType.GetGenericArguments()[0];
            if (listType == typeof(GameplayTag))
            {
                ProcessGameplayTagList(fieldValue as List<GameplayTag>);
            }
            else
            {
                // Recursive call for List items
                if (fieldValue != null)
                {
                    foreach (var item in (IEnumerable)fieldValue)
                    {
                        if (item != null && !visited.Contains(item))
                            ProcessType(item, item.GetType(), path + field.Name + ".", visited);
                    }
                }
            }
        }
        // Recursive process for custom objects which are not primitive, enum, or string, and not already visited
        else if (!fieldType.IsPrimitive && !fieldType.IsEnum && fieldType != typeof(string))
        {
            if (fieldValue != null && !visited.Contains(fieldValue))
                ProcessType(fieldValue, fieldType, path + field.Name + ".", visited);
        }
    }
}

void ProcessObject(UnityEngine.Object obj)
{
    var type = obj.GetType();
    HashSet<object> visited = new HashSet<object>();  // To track visited objects and avoid cycles
    ProcessType(obj, type, obj.name + " -> ", visited);
}


void ProcessGameplayTag(GameplayTag tag)
{
    if (tag.FullTag != null)
    {
        GameplayTagsAsset tagsAsset = GetGameplayTagsAsset();
        tag.Fetch(tagsAsset);

        DDebug.Log($"Fetched GameplayTag: {tag.FullTag}");
    }
}

void ProcessGameplayTagList(List<GameplayTag> tags)
{
    if (tags != null)
    {
        foreach (var tag in tags)
        {
            ProcessGameplayTag(tag);
        }
    }
}

GameplayTagsAsset GetGameplayTagsAsset()
{
    // Return your tags asset
    return this;
}
}

[Serializable]
public class GameplayTagFetcher
{
    public string Tag;
    public string HashCode;

    public void GenerateNewHashCode(GameplayTagsAsset asset)
    {
        if (string.IsNullOrEmpty(HashCode))
        {
            HashCode = Guid.NewGuid().ToString();
            DDebug.Log("newHashCode set: " + HashCode);
            EditorUtility.SetDirty(asset); 
            AssetDatabase.SaveAssets();
        }
    }
}
#endif