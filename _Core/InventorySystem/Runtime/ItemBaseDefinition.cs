using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

public class ItemBaseDefinition : ScriptableObject, IItemIdOwner
{
    public string ItemId
    {
        get => ItemID;
        set => ItemID = value;
    }
    [LockableField("_itemIDLocked")][BoxGroup("Basic Info")][VerticalGroup("Basic Info/info/row2")][LabelWidth(100)][InlineButton("SetId", "Generate")]
    public string ItemID;
    [SerializeField][HideInInspector]private bool _itemIDLocked;
    [LockableField("_itemNameLocked")][BoxGroup("Basic Info")][VerticalGroup("Basic Info/info/row2")][LabelWidth(100)][InlineButton("SetName", "Generate")]
    public string ItemName;
    [SerializeField][HideInInspector]private bool _itemNameLocked;
    [BoxGroup("Basic Info")][VerticalGroup("Basic Info/info/row2")][TextArea(3,9)][LabelWidth(200)]
    public string Description;
    [BoxGroup("Basic Info")][PreviewField(90)][HideLabel][HorizontalGroup("Basic Info/info",Width = 100)]
    public Sprite Icon;
    
    [SerializeReference]
    [ListDrawerSettings(ShowFoldout = true, DraggableItems = true)][Searchable(FuzzySearch = true, Recursive = true)]
    public List<Data> DataList = new List<Data>();


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
#if UNITY_EDITOR
    public void SetId()
    {
        _itemIDLocked = true;
        string input = name;
        string prefix = "ItemAsset_";
        if (input.StartsWith(prefix))
        {
            ItemID = input.Substring(prefix.Length);
        }
        else
        {
            ItemID = input;
        }
        EditorUtility.SetDirty(this);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
#endif
    
    public T GetData<T>(string key = "") where T : Data
    {
        if (key.IsNullOrWhitespace())
        {
            return DataList.Find(x => x.GetType() == typeof(T)) as T;
        }
        else
        {
            return DataList.Find(x => x.GetType() == typeof(T) && x.DataKey == key) as T;
        }
    }
    public bool TryGetData<T>(out T data) where T : Data
    {
        data = DataList.Find(x => x.GetType() == typeof(T)) as T;
        return data != null;
    }
}
