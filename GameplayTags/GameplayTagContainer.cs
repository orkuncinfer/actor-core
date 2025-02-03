using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.Utilities;
using UnityEngine;
[Serializable]
public class GameplayTagContainer2 
{
    [SerializeField]
    private List<string> _tagHashes;
    


    public bool HasTag(GameplayTag tagToCheck)
    {
        //reverse for loop
        for (int i = _tagHashes.Count - 1; i >= 0; i--)
        {
            GameplayTag tag = GameplayTagManger2.RequestTagHash(_tagHashes[i]);
            if (tag.FullTag.Contains(tagToCheck.FullTag))
            {
                return true;
            }
        }

        return false;
    }
    
    public bool HasTagExact(GameplayTag tagToCheck)
    {
        //reverse for loop
        for (int i = _tagHashes.Count - 1; i >= 0; i--)
        {
            GameplayTag tag = GameplayTagManger2.RequestTagHash(_tagHashes[i]);
            if (tag.FullTag == tagToCheck.FullTag)
            {
                return true;
            }
        }

        return false;
    }

    public bool HasAny(GameplayTagContainer2 container)
    {
        List<GameplayTag> tags = container.GetTags();
        for (int i = tags.Count - 1; i >= 0; i--)
        {
            if (HasTag(tags[i]))
            {
                return true;
            }
        }

        return false;
    }
    
    public bool HasAnyExact(GameplayTagContainer2 container)
    {
        List<GameplayTag> tags = container.GetTags();
        for (int i = tags.Count - 1; i >= 0; i--)
        {
            if (HasTagExact(tags[i]))
            {
                return true;
            }
        }

        return false;
    }

    public void AddTag(string tagGuid)
    {
        _tagHashes.Add(tagGuid);
    }

    public void RemoveTag(string tagGuid)
    {
        _tagHashes.Remove(tagGuid);
    }

    public void AddTag(GameplayTag tag)
    {
        _tagHashes.Add(tag.HashCode);
    }
    public void RemoveTag(GameplayTag tag)
    {
        _tagHashes.Remove(tag.HashCode);
    }
    
    public void AddTags(GameplayTagContainer2 container)
    {
        //add tags if not exist
        for (int i = container._tagHashes.Count - 1; i >= 0; i--)
        {
            if (!_tagHashes.Contains(container._tagHashes[i]))
            {
                _tagHashes.Add(container._tagHashes[i]);
            }
        }
    }
    public void RemoveTags(GameplayTagContainer2 container)
    {
        for (int i = container._tagHashes.Count - 1; i >= 0; i--)
        {
            _tagHashes.Remove(container._tagHashes[i]);
        }
    }
    
    public List<GameplayTag> GetTags()
    {
        List<GameplayTag> tags = new List<GameplayTag>();
        for (int i = _tagHashes.Count - 1; i >= 0; i--)
        {
            tags.Add(GameplayTagManger2.RequestTagHash(_tagHashes[i]));
        }

        return tags;
    }
}
