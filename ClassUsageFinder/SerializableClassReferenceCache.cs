using System.Collections.Generic;
using UnityEngine;
[SOCreatable]
public class SerializableClassReferenceCache : ScriptableObject
{
    public List<CacheEntry> Entries = new List<CacheEntry>();

    [System.Serializable]
    public class CacheEntry
    {
        public string Tag;
        public List<string> References;
    }

    public string LastSearchText;
}
