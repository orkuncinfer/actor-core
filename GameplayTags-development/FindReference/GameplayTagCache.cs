using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GameplayTagCache", menuName = "GameplayTags/Tag Cache", order = 1)]
public class GameplayTagCache : ScriptableObject
{
    public List<CacheEntry> Entries = new List<CacheEntry>();

    [System.Serializable]
    public class CacheEntry
    {
        public string Tag;
        public List<string> References;
    }
}