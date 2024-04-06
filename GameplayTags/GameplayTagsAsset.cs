using System;
using UnityEngine;
using System.Collections.Generic;
using UnityEditor;

[CreateAssetMenu(fileName = "GameplayTags", menuName = "Gameplay/Tags", order = 1)]
public class GameplayTagsAsset : ScriptableObject
{
    public List<GameplayTagFetcher> TagsCache = new List<GameplayTagFetcher>();
}

[Serializable]
public class GameplayTagFetcher
{
    public string Tag;
    [HideInInspector]public string HashCode;

    public void GenerateNewHashCode(GameplayTagsAsset asset)
    {
        if (string.IsNullOrEmpty(HashCode))
        {
            HashCode = Guid.NewGuid().ToString();
            Debug.Log("newHashCode set: " + HashCode);
            EditorUtility.SetDirty(asset); 
            AssetDatabase.SaveAssets();
        }
    }
}