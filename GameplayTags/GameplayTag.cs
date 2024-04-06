using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.ShortcutManagement;
using UnityEngine;
[Serializable]
public class GameplayTag
{
    [SerializeField]
    private string fullTag;
    public string FullTag
    {
        get => fullTag;
        set
        {
            fullTag = value;
            ParseTagHierarchy();
        }
    }
    [SerializeField]private List<string> tagHierarchy;

    public string HashCode;
    
    private GameplayTag(string tag)
    {
        this.fullTag = tag;
        ParseTagHierarchy();
    }   

    public void SetTag(string newtag, string hashCode)
    {
        HashCode = hashCode;
       
        fullTag = newtag;
        
        ParseTagHierarchy();
    }

    public void Fetch(GameplayTagsAsset tagsAsset)//not called in runtime
    {
        Debug.Log("fetched");
        foreach (GameplayTagFetcher tagFetcher in tagsAsset.TagsCache)
        {
            if (tagFetcher.HashCode == HashCode)
            {
                fullTag = tagFetcher.Tag;
                return;
            }
        }

        fullTag = "";
    }

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
        return other.tagHierarchy.Any(tag => tagHierarchy.Contains(tag));
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
    public static implicit operator GameplayTag(string tag)
    {
        return new GameplayTag(tag);
    }
}