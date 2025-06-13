using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.Utilities;
using UnityEngine;
[Serializable]
public class GameplayTagContainer 
{
    [SerializeField]
    private List<string> _tagHashes = new List<string>();

    public event Action OnTagChanged;
    public event Action<GameplayTag> OnTagAdded; 
    public event Action<GameplayTag> OnTagRemoved; 
    public int TagCount => _tagHashes.Count;

    public bool HasTag(GameplayTag tagToCheck)
    {
        //reverse for loop
        for (int i = _tagHashes.Count - 1; i >= 0; i--)
        {
            GameplayTag tag = GameplayTagManager.RequestTagHash(_tagHashes[i]);
            if (tag.FullTag.Contains(tagToCheck.FullTag))
            {
                return true;
            }
        }

        return false;
    }
    public bool HasTag(GameplayTag tagToCheck,out GameplayTag matchingTag)
    {
        //reverse for loop
        for (int i = _tagHashes.Count - 1; i >= 0; i--)
        {
            GameplayTag tag = GameplayTagManager.RequestTagHash(_tagHashes[i]);
            if (tag.FullTag.Contains(tagToCheck.FullTag))
            {
                matchingTag = tag;
                return true;
            }
        }
        matchingTag = default;
        return false;
    }
    
    public bool HasTagExact(GameplayTag tagToCheck)
    {
        //reverse for loop
        for (int i = _tagHashes.Count - 1; i >= 0; i--)
        {
            GameplayTag tag = GameplayTagManager.RequestTagHash(_tagHashes[i]);
            if (tag.FullTag == tagToCheck.FullTag)
            {
                return true;
            }
        }

        return false;
    }
    public bool HasTagExact(string tagToCheck)
    {
        //reverse for loop
        for (int i = _tagHashes.Count - 1; i >= 0; i--)
        {
            GameplayTag tag = GameplayTagManager.RequestTagHash(_tagHashes[i]);
            if (tag.FullTag == tagToCheck)
            {
                return true;
            }
        }

        return false;
    }
    public bool HasAny(GameplayTagContainer container)
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
    
    public bool HasAnyExact(GameplayTagContainer container)
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

    public bool HasAll(GameplayTagContainer container)
    {
        List<GameplayTag> tags = container.GetTags();
        for (int i = tags.Count - 1; i >= 0; i--)
        {
            if (!HasTag(tags[i]))
            {
                return false;
            }
        }

        return true;
    }
    
    public bool HasAllExact(GameplayTagContainer container)
    {
        List<GameplayTag> tags = container.GetTags();
        for (int i = tags.Count - 1; i >= 0; i--)
        {
            if (!HasTagExact(tags[i]))
            {
                return false;
            }
        }

        return true;
    }

    public void AddTag(string tagGuid)
    {
        _tagHashes.Add(tagGuid);
        OnTagChanged?.Invoke();
    }

    public void RemoveTag(string tagGuid)
    {
        _tagHashes.Remove(tagGuid);
        OnTagChanged?.Invoke();
    }

    public void AddTag(GameplayTag tag)
    {
        _tagHashes.Add(tag.HashCode);
        OnTagChanged?.Invoke();
        OnTagAdded?.Invoke(tag);
    }
    public void RemoveTag(GameplayTag tag)
    {
        _tagHashes.Remove(tag.HashCode);
        OnTagChanged?.Invoke();
        OnTagRemoved?.Invoke(tag);
    }
    
    public void AddTags(GameplayTagContainer container)
    {
        //add tags if not exist
        for (int i = container._tagHashes.Count - 1; i >= 0; i--)
        {
            if (!_tagHashes.Contains(container._tagHashes[i]))
            {
                _tagHashes.Add(container._tagHashes[i]);
                OnTagAdded?.Invoke(container.GetTags()[i]);
            }
        }
        OnTagChanged?.Invoke();
    }
    public void RemoveTags(GameplayTagContainer container)
    {
        for (int i = container._tagHashes.Count - 1; i >= 0; i--)
        {
            _tagHashes.Remove(container._tagHashes[i]);
            OnTagRemoved?.Invoke(container.GetTags()[i]);
        }
        OnTagChanged?.Invoke();
    }
    
    public List<GameplayTag> GetTags()
    {
        List<GameplayTag> tags = new List<GameplayTag>();
        for (int i = _tagHashes.Count - 1; i >= 0; i--)
        {
            tags.Add(GameplayTagManager.RequestTagHash(_tagHashes[i]));
        }

        return tags;
    }
}
