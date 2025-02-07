using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class GameplayTagManager
{
    public static List<GameplayTagsAsset> TagAssets = new List<GameplayTagsAsset>();
    private static GameplayTag[] s_Tags;
    private static bool s_IsInitialized;
    
    private static Dictionary<string,GameplayTag> _tagHashDictionary = new Dictionary<string, GameplayTag>();
    private static Dictionary<string,GameplayTag> _tagDictionary = new Dictionary<string, GameplayTag>();
    private static Dictionary<string,string> _tagHashToTagDictionary = new Dictionary<string, string>();
    
    public static void InitializeIfNeeded()
    {
        if (s_IsInitialized)
        {
            return;
        }

        LoadAllGameplayTagsAssets();
        s_IsInitialized = true;
    }

    public static GameplayTag RequestTagHash(string hasCode)
    {
        //Debug.Log($"Requested tag with hash code: {hasCode}");
        InitializeIfNeeded();
        FillDictionary();
        return _tagHashDictionary[hasCode];
    }
    public static GameplayTag RequestTag(string fullTag)
    {
        //Debug.Log($"Requested tag with hash code: {fullTag}");
        InitializeIfNeeded();
        FillDictionary();
        return _tagDictionary[fullTag];
    }
    
    private static void FillDictionary(bool force = false)
    {
        int tagsCount = 0;
        foreach (var tagAsset in TagAssets)
        {
            tagsCount += tagAsset.TagsCache.Count;
        }

        if (!force)
        {
            if(tagsCount == _tagHashDictionary.Count) return;
        }
        foreach (var tagAsset in TagAssets)
        {
            foreach (var tag in tagAsset.TagsCache)
            {
                GameplayTag returnTag = new GameplayTag();
                returnTag.Description = tag.Description;
                returnTag.HashCode = tag.HashCode;
                _tagHashDictionary[tag.HashCode] = returnTag;
                _tagDictionary[tag.Tag] = returnTag;
                _tagHashToTagDictionary[tag.HashCode] = tag.Tag;
                //_tagDictionary.Add(tag.Tag, returnTag);
                //_tagHashDictionary.Add(tag.HashCode, returnTag);
            }
        }
    }

    [Button]
    public static void Refresh()
    {
        FillDictionary(true);
    }
    
    private static void LoadAllGameplayTagsAssets()
    {
        GameplayTagsAsset assets = Resources.Load<GameplayTagsAsset>("GameplayTagList");

        if (assets != null )
        {
            TagAssets.Add(assets);
            Debug.Log($"Loaded {assets} GameplayTagsAsset(s) from Resources.");
        }
        else
        {
            Debug.LogWarning("No GameplayTagsAsset found in Resources.");
        }
    }

    public static string GetFullTag(string tagHash)
    {
        return _tagHashToTagDictionary[tagHash];
    }
#if UNITY_EDITOR
    private static void CreateTag(string fullTag)
    {
        InitializeIfNeeded();

        var defaultTagAsset = TagAssets[0];
        var tagsToCheck = fullTag.Split('.');
        var currentTag = "";

        foreach (var part in tagsToCheck)
        {
            currentTag = string.IsNullOrEmpty(currentTag) ? part : $"{currentTag}.{part}";

            // Check if the tag already exists in the TagsCache
            if (!defaultTagAsset.TagsCache.Exists(tagInfo => tagInfo.Tag == currentTag))
            {
                // Create a new GameplayTagInfo if the tag doesn't exist
                var newTag = new GameplayTagInfo
                {
                    Tag = currentTag,
                    Description = $"Generated tag for {currentTag}"
                };
                newTag.GenerateNewHashCode(defaultTagAsset);

                // Add the new tag to the TagsCache
                defaultTagAsset.TagsCache.Add(newTag);
                Debug.Log($"Created new tag: {currentTag}");
            }
        }
        FillDictionary();
        // Mark the asset as dirty so Unity saves the changes
        UnityEditor.EditorUtility.SetDirty(defaultTagAsset);
        UnityEditor.AssetDatabase.SaveAssets();
    }
#endif
    
}
