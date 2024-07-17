using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class ItemBaseDefinition : ScriptableObject, IItemIdOwner
{
    public string ItemId
    {
        get => IdemID;
        set => IdemID = value;
    }
    [BoxGroup("Basic Info")][VerticalGroup("Basic Info/info/row2")][LabelWidth(100)][InlineButton("SetId", "Generate")]
    public string IdemID;
    [BoxGroup("Basic Info")][VerticalGroup("Basic Info/info/row2")][LabelWidth(100)][InlineButton("SetName", "Generate")]
    public string ItemName;
    [BoxGroup("Basic Info")][VerticalGroup("Basic Info/info/row2")][TextArea(3,9)][LabelWidth(200)]
    public string Description;
    [BoxGroup("Basic Info")][PreviewField(90)][HideLabel][HorizontalGroup("Basic Info/info",Width = 100)]
    public Sprite Icon;


    public void SetName()
    {
        string input = name;
        string prefix = "ItemAsset_";
        if (input.StartsWith(prefix))
        {
            ItemName = input.Substring(prefix.Length);
        }
        else
        {
            ItemName = input;
        }
    }
    public void SetId()
    {
        string input = name;
        string prefix = "ItemAsset_";
        if (input.StartsWith(prefix))
        {
            IdemID = input.Substring(prefix.Length);
        }
        else
        {
            IdemID = input;
        }
    }
}
