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
    [BoxGroup("Basic Info")][VerticalGroup("Basic Info/info/row2")][LabelWidth(100)]
    public string IdemID;
    [BoxGroup("Basic Info")][VerticalGroup("Basic Info/info/row2")][LabelWidth(100)]
    public string ItemName;
    [BoxGroup("Basic Info")][VerticalGroup("Basic Info/info/row2")][TextArea(3,9)][LabelWidth(200)]
    public string Description;
    [BoxGroup("Basic Info")][PreviewField(90)][HideLabel][HorizontalGroup("Basic Info/info",Width = 100)]
    public Sprite Icon;

}
