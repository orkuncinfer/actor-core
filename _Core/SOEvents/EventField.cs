using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Heimdallr.Core;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;


[System.Serializable]
[TopTitle(ShowGenericName = false,
    NameSuffix = "<color=#ff77ff55><b>↖EVENT</b></color>",
    NamePrefix = "<color=#ff77ff55><b>↖</b></color>",
    PerGenericArgString = ",", SetParentObject = true,
    BoldName = true)]
[GUIColor(1f, 0.6f, 1f)]
public struct EventField : ISerializationCallbackReceiver
{
    private UnityEngine.Object _parentObject;

    public UnityEngine.Object parentObject
    {
        get => _parentObject;
        set => _parentObject = value;
    }

    [DisableInPlayMode] [SerializeField] [HideLabel] [HorizontalGroup(GroupID = "install", Width = 0.2f)]
    private EventAddressType _addressType;

    [ValueDropdown("GetAllAppropriateKeys", DropdownWidth = 250, NumberOfItemsBeforeEnablingSearch = 2)]
    [ValidateInput("ValidateCurrentKey")]
    [OnValueChanged("OnEventKeyChanged")]
    [DisableInPlayMode]
    [SerializeField]
    [HideLabel]
    [HorizontalGroup(GroupID = "install")]
    private EventKey _eventKey;

    #region KeyCreation

    [ShowInInspector] [HideLabel] [ShowIf("ShowCreationOptions")] [HorizontalGroup(GroupID = "creation")]
    private string _keyName;

    [ShowInInspector]
    [HideLabel]
    [SuffixLabel("Should Be Global")]
    [ShowIf("ShowCreationOptions")]
    [HorizontalGroup(GroupID = "creation", MaxWidth = 0.15f)]
    private bool _createdKeyShouldBeGlobal;

    private bool ShowStartCreatingKey => _eventKey == null && !_isCreatingStarted;
    private bool _isCreatingStarted;
    private bool ShowCreationOptions => _eventKey == null && _isCreatingStarted;
#if UNITY_EDITOR
    [Button("Create")]
    [HorizontalGroup(GroupID = "install", Width = 0.2f)]
    [ShowIf("ShowStartCreatingKey")]
    private void StartCreatingKey()
    {
        _isCreatingStarted = true;
    }

    [Button("Create")]
    [ShowIf("ShowCreationOptions")]
    [HorizontalGroup(GroupID = "creation")]
    private void CreateKey()
    {
        ScriptableObject obj = ScriptableObject.CreateInstance(typeof(EventKey));
        EventKey eventKey = obj as EventKey;
        eventKey.SetupKey(_keyName, _createdKeyShouldBeGlobal);
        EnsurePathExistence("Assets/_Project/Script Assets/EventKeys");
        string uniquePath =
            AssetDatabase.GenerateUniqueAssetPath("Assets/_Project/Script Assets/EventKeys" + "/" + eventKey.name +
                                                  ".asset");
        AssetDatabase.CreateAsset(eventKey, uniquePath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Undo.RecordObject(parentObject, "CreatedKeyAndSet");
        _eventKey = eventKey;
        _isCreatingStarted = false;
        EditorUtility.SetDirty(parentObject);
    }

    [Button("Cancel")]
    [ShowIf("ShowCreationOptions")]
    [HorizontalGroup(GroupID = "creation")]
    private void CancelCreate()
    {
        _isCreatingStarted = false;
    }

    /// <summary>
    /// If directory path doesn't exist it will create, if it exist it will do nothing.
    /// </summary>
    /// <param name="path"></param>
    public void EnsurePathExistence(string path)
    {
        string[] splitFoldersArray = path.Split('/');
        List<string> splitFolders = splitFoldersArray.ToList();
        splitFolders.RemoveAt(0); //Removing Assets directory it's special.

        //Ensure path exists.
        string directory = "Assets";
        foreach (string folder in splitFolders)
        {
            if (!AssetDatabase.IsValidFolder(directory + "/" + folder))
                AssetDatabase.CreateFolder(directory, folder);

            directory += "/" + folder;
        }
    }
    [Button("*",ButtonStyle.Box)][HorizontalGroup(GroupID = "install",Width = 0.03f)][ShowIf("@_eventKey!=null")]
    private void FocusAsset()
    {
        EditorGUIUtility.PingObject(_eventKey);
    }
    
#endif

    #endregion


    public void Register(ActorBase selfMain, Action<EventArgs> action)
    {
        if (_eventKey == null) return;
        RegisterForEach(selfMain, action);
    }

    public void Unregister(ActorBase selfMain, Action<EventArgs> action)
    {
        if (_eventKey == null) return;
        UnregisterForEach(selfMain, action);
    }

    public void Raise(ActorBase selfMain = null)
    {
        RaiseForEach(selfMain);
    }

    private void RegisterForEach(ActorBase selfMain, Action<EventArgs> action)
    {
        if (_eventKey == null) return;


#if UNITY_EDITOR
        if (!_eventKey.Listeners.Contains(action.Target))
            _eventKey.Listeners.Add(action.Target);
#endif


        if (_addressType == EventAddressType.Owner)
        {
            EventRegistry.Register(selfMain,_eventKey.ID, action);
        }

        if (_addressType == EventAddressType.Global)
        {
            EventRegistry.Register(_eventKey.ID, action);
        }
    }

    private void UnregisterForEach(ActorBase selfMain, Action<EventArgs> action)
    {
        if (_eventKey == null) return;

#if UNITY_EDITOR
        if (_eventKey.Listeners.Contains(action.Target))
            _eventKey.Listeners.Remove(action.Target);
#endif


        if (_addressType == EventAddressType.Owner)
        {
            EventRegistry.Unregister(selfMain,_eventKey.ID, action);
        }

        if (_addressType == EventAddressType.Global)
        {
            EventRegistry.Unregister(_eventKey.ID, action);
        }
    }

    private void RaiseForEach(ActorBase selfMain)
    {
        if (_eventKey == null) return;
        if (_addressType == EventAddressType.Owner)
        {
            EventRegistry.Raise(selfMain,_eventKey.ID);
        }

        if (_addressType == EventAddressType.Global)
        {
            EventRegistry.Raise(_eventKey.ID);
        }
    }


    public void OnBeforeSerialize()
    {
        if (_eventKey == null) return;
        if (_eventKey.MustBeGlobal)
        {
            _addressType = EventAddressType.Global;
        }
    }

    public void OnAfterDeserialize()
    {
        if (_eventKey == null) return;
        if (_eventKey.MustBeGlobal)
        {
            _addressType = EventAddressType.Global;
        }
    }


    private void OnEventKeyChanged()
    {
        if (_eventKey == null) return;
    }

    private bool ValidateCurrentKey()
    {
        if (_eventKey == null) return true;
        if (_eventKey.GetType() != typeof(EventKey)) return false;

        return string.IsNullOrEmpty(_eventKey.Arg1Type) && string.IsNullOrEmpty(_eventKey.Arg2Type) &&
               string.IsNullOrEmpty(_eventKey.ReturnType);
    }

#if UNITY_EDITOR


    private List<ValueDropdownItem<EventKey>> GetAllAppropriateKeys()
    {
        var allKeys = Resources.FindObjectsOfTypeAll<EventKey>();
        var dropdownItems = new List<ValueDropdownItem<EventKey>>();
        foreach (var key in allKeys)
        {
            if (_addressType == EventAddressType.Global && key.MustBeGlobal && key.Arg1Type == "")
            {
                dropdownItems.Add(new ValueDropdownItem<EventKey>(key.name, key));
            }

            if (_addressType != EventAddressType.Global && !key.MustBeGlobal && key.Arg1Type == "")
            {
                dropdownItems.Add(new ValueDropdownItem<EventKey>(key.name, key));
            }
        }

        return dropdownItems;
    }
#endif
}