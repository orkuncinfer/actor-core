using System;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "GenericKey",menuName = "Keys/Generic Key")]
public class GenericKey : ScriptableObject
{
    [SerializeField][HideIf("HideIDString")][OnValueChanged("OnIDChanged")] private string _id;
    [SerializeField] private bool _useAssetNameAsID = false;

    protected virtual bool HideIDString => _useAssetNameAsID;

    public string IgnorePrefix;
    public static event Action<GenericKey> onCreate;
    public static event Action<GenericKey> onDestroy;


    public virtual string ID
    {
        get => _useAssetNameAsID ? name : _id;
        set
        {
            if (_useAssetNameAsID)
            {
                return;
            }
            _id = value;
        }
    }

    [Button]
    private void SetId()
    {
        // generate id with ignore prefix of asset name
        string assetName = name;
        if (IgnorePrefix != "")
        {
            assetName = assetName.Replace(IgnorePrefix, "");
        }
        _id = assetName;
    }

    protected virtual void OnIDChanged(string value)
    {
            
    }

    protected virtual void Awake()
    {
        onCreate?.Invoke(this);
    }

    protected virtual void OnDestroy()
    {
        onDestroy?.Invoke(this);
    }

    protected virtual void OnEnable()
    {
            
    }

    protected virtual void OnDisable()
    {
            
    }
}