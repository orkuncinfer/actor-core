using System;
using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Sirenix.OdinInspector;

#if UNITY_EDITOR

using UnityEditor;
#endif

[CreateAssetMenu(fileName = "GameplayTags", menuName = "Gameplay/Tags", order = 1)]
public class GameplayTagsAsset : ScriptableObject
{
    public bool ShowInfo;
    [Searchable]public List<GameplayTagInfo> TagsCache = new List<GameplayTagInfo>();

    GameplayTagsAsset GetGameplayTagsAsset()
    {
        // Return your tags asset
        return this;
    }
    [Button]public void SetAssets()
    {
        foreach (var tag in TagsCache)
        {
            tag._asset = this;
        }
    }
    [Button]public void RefreshTagManager()
    {
        GameplayTagManager.Refresh();
    }
    

#if UNITY_EDITOR
    [Button]
    private void CreateTag(string fullTag)
    {
        bool alreadyExists = TagsCache.Exists(tagInfo => tagInfo.Tag == fullTag);
        if(alreadyExists) return;
        var tagsToCheck = fullTag.Split('.');
        var currentTag = "";

        foreach (var part in tagsToCheck)
        {
            currentTag = string.IsNullOrEmpty(currentTag) ? part : $"{currentTag}.{part}";

            // Check if the tag already exists in the TagsCache
            if (!TagsCache.Exists(tagInfo => tagInfo.Tag == currentTag))
            {
                // Create a new GameplayTagInfo if the tag doesn't exist
                var newTag = new GameplayTagInfo
                {
                    Tag = currentTag,
                    Description = $"Generated tag for {currentTag}"
                };
                newTag.GenerateNewHashCode(this);

                // Add the new tag to the TagsCache
                TagsCache.Add(newTag);
                Debug.Log($"Created new tag: {currentTag}");
            }
        }

        // Mark the asset as dirty so Unity saves the changes
        UnityEditor.EditorUtility.SetDirty(this);
        UnityEditor.AssetDatabase.SaveAssets();
    }
#endif
    
}

[Serializable]
public class GameplayTagInfo
{
    public string Tag;
    [ShowIf("ShowInfo")]public string HashCode;
    [ShowIf("ShowInfo")]public string Description;
    [HideInInspector]public GameplayTagsAsset _asset;

    private bool ShowInfo()
    {
        if (_asset == null) return true;
        return _asset.ShowInfo;
    }
    
#if UNITY_EDITOR
    public void GenerateNewHashCode(GameplayTagsAsset asset)
    {
        if (string.IsNullOrEmpty(HashCode))
        {
            HashCode = Guid.NewGuid().ToString();
            DDebug.Log("newHashCode set: " + HashCode);
            _asset = asset;
            EditorUtility.SetDirty(asset);
            AssetDatabase.SaveAssets();
        }
    }
#endif
   
}
