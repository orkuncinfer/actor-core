using System;
using Sirenix.OdinInspector;
using UnityEngine;

public interface IDataInstaller
{
    void InstallFor(ActorBase context);

    void OnDestroy();
}

[System.Serializable][GUIColor(0.9f,1f,1f)][PropertySpace(5f)]

public class DataInstaller<TData>: IDataInstaller where TData : class, IData
{
    #region Private Variables

    [SerializeField][HideInInspector] private bool _isVisible;

    [SerializeField][HideInInspector] private string _editorID;

    [HideInPlayMode][SerializeField][HideLabel][HorizontalGroup(GroupID = "install",Width = 0.2f)] private DataInstallerType _installType;

    [SerializeField][HorizontalGroup(GroupID = "persistence")] private bool _isPersistent;

    [SerializeField][HorizontalGroup(GroupID = "persistence")] private bool _installGlobal;

    [SerializeField][HorizontalGroup(GroupID = "persistence2")][ShowIf("_isPersistent")] private bool _loadAtInitialize;

    [SerializeField][HorizontalGroup(GroupID = "persistence2")][ShowIf("_isPersistent")] private bool _saveAtRemove;

    [SerializeField][HorizontalGroup(GroupID = "persistence3")][ShowIf("_isPersistent")]
    [Tooltip("Usually used for overriding save file name. \"Default\" is the default save file.")]
    private bool _overrideGlobalCategory;

    [SerializeField][HorizontalGroup(GroupID = "persistence3",Width = .7f)]
    [ShowIf("@_isPersistent && _overrideGlobalCategory"),HideLabel]
    [Tooltip("Save file name.")]
    private string _globalDataCategory;

    private bool _isInstalledGlobal;

    private bool ShouldShowCreateDataSetKey => IsDictionary && Key == null;

    #endregion

    #region Public Variables

    public bool IsVisible
    {
        get => _isVisible;
        set => _isVisible = value;
    }

    public string EditorID => _editorID;


    //[ValueDropdown("GetAllAppropriateKeys")]
    [HideInPlayMode][SerializeField][HideLabel][HorizontalGroup(GroupID = "install")][ShowIf("IsDictionary")]  public GenericKey Key;

    public bool OverrideGlobalCategory
    {
        get => _overrideGlobalCategory;
        set => _overrideGlobalCategory = value;
    }

    public string GlobalDataCategory
    {
        get => _globalDataCategory;
        set => _globalDataCategory = value;
    }

    [SerializeField][HideLabel] public TData DataSet;

    public bool IsDirect => _installType == DataInstallerType.Single;

    public bool IsDictionary => _installType == DataInstallerType.Key;
    

    public DataInstallerType InstallType => _installType;

    public IData InstalledData => DataSet;

    public GenericKey DictionaryKey => Key;

    public Type DataSetType => typeof(TData);

    public bool IsInstalledGlobal => _isInstalledGlobal;

    public string InstallID
    {
        get
        {
            string typeName = typeof(TData).Name + ",";
            string installType;
            if (_installType == DataInstallerType.Key)
            {
                if (Key != null)
                {
                    installType = "Key=" + Key.ID;
                }
                else
                {
                    installType = "Key=" + "null";
                }
            }
            else
            {
                installType = "Single";
            }
            return typeName + installType;
        }
    }

    #endregion

    public void InstallFor(ActorBase owner)
    {
       // DataSet.IsPersistent = _isPersistent;
        
        if (DataSet is Data data)
        {
            if (true)
            {
                var loadedData = data.LoadData();
                if (loadedData != null)
                {
                    Type loadedType = loadedData.GetType();
                    //DataSet = (TData)loadedData;
                    
                    data = DataSet as Data;
                }
            }

            //DataSet = data as TData;
        }
    }

    public void OnDestroy()
    {
        if(!_isPersistent)return;
        if (DataSet is Data data)
        {
            data.SaveData();
            Debug.Log("3404 : Installer saved data");
        }
       // DataSet.SaveData();
    }

    
}

public enum DataInstallerType
{
    Single,
    Key,
}
