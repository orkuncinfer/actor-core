using System;
using System.Collections.Generic;
using UnityEngine.Events;

public interface ITagContainer
{
    bool ContainsTag(string tag);
    void AddTag(string tag);
    void RemoveTag(string tag);
    void ModifyFlagTag(string t, bool flagValue);
    event Action<ITagContainer, string> onTagAdded;
    event Action<ITagContainer, string> onTagRemoved;
    event Action<ITagContainer, string> onTagsChanged;
}