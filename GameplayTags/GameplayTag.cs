using System;
using System.Collections.Generic;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor.ShortcutManagement;
#endif

using UnityEngine;
[Serializable]
public struct GameplayTag
{
    [SerializeField]
    private string fullTag;
    public string FullTag => GameplayTagManger2.GetFullTag(hashCode);
    [SerializeField]private List<string> tagHierarchy;

    [SerializeField]
    private string hashCode;
    public string HashCode
    {
        get => hashCode;
        set => hashCode = value;
    }

    public string Description;
    
    private GameplayTag(string tag, string hashCode)
    {
        this.hashCode = hashCode;
        fullTag = tag;
        tagHierarchy = new List<string>();
        Description = "";
        ParseTagHierarchy();
    }


    public void SetTag(string newtag, string hashCode)
    {
        HashCode = hashCode;
        Debug.Log("settag" + newtag + "hascode :" + this.hashCode);
       
        fullTag = newtag;
        
        ParseTagHierarchy();

        foreach (string VARIABLE in tagHierarchy)
        {
            DDebug.Log("parsed "+VARIABLE);
        }
    }
#if UNITY_EDITOR
    public void Fetch(GameplayTagsAsset tagsAsset)//not called in runtime
    {
        Debug.Log("fetchingtag" + fullTag +"hascode :" + hashCode);
        foreach (GameplayTagInfo tagFetcher in tagsAsset.TagsCache)
        {
            if (tagFetcher.HashCode == hashCode)
            {
                DDebug.Log("fetchedtag" + fullTag +"hascode :" + hashCode);
                fullTag = tagFetcher.Tag;
                ParseTagHierarchy();
                return;
            }
        }

        fullTag = "";
    }
#endif
  

    private void ParseTagHierarchy()
    {
        tagHierarchy = new List<string>();
        var splits = fullTag.Split('.');

        string current = "";
        foreach (var split in splits)
        {
            current = string.IsNullOrEmpty(current) ? split : $"{current}.{split}";
            tagHierarchy.Add(current);
        }
    }

    public bool Matches(GameplayTag other)
    {
        foreach (string otherTag in other.tagHierarchy)
        {
            
            foreach (string thisTag in tagHierarchy)
            {
                if (thisTag == otherTag)
                {
                    return true;
                }
            }
        }
        return false;
    }

    public bool MatchesExact(GameplayTag other)
    {
        return this.fullTag.Equals(other.fullTag, StringComparison.OrdinalIgnoreCase);
    }

    public bool ContainsAny(GameplayTag other)
    {
        // Split the fullTag of both GameplayTag objects by dots to get the individual tags.
        string[] tags1 = this.fullTag.Split('.');
        string[] tags2 = other.fullTag.Split('.');

        // Check if any tag from tags1 matches any tag from tags2.
        foreach (string tag1 in tags1)
        {
            foreach (string tag2 in tags2)
            {
                if (tag1 == tag2)
                {
                    // Return true immediately if a match is found.
                    return true;
                }
            }
        }
        // Return false if no matches were found.
        return false;
    }
    public IEnumerable<string> GetHierarchy()
    {
        return tagHierarchy;
    }
}