using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

public class MNF_PlayerGlobal : DataManifest
{
    [SerializeField] private DS_ItemUpgrades _itemUpgrades;
    [SerializeField] private DS_PlayerPersistent _playerPersistent;
    
    protected override Data[] InstallData()
    {
        return new Data[] { _itemUpgrades, _playerPersistent };
    }
}
[System.Serializable]
public class DS_PlayerPersistent : Data
{
    public Dictionary<string,int> ActiveMissions = new Dictionary<string, int>();
    public Dictionary<string,int> Inventory = new Dictionary<string, int>();


    public event Action<int, int> onCurrentLevelChanged; 
    [SerializeField]private int _currentLevelIndex; 
    public int CurrentLevelIndex 
    {
        get => _currentLevelIndex;
        set
        {
            int oldValue = _currentLevelIndex;
            bool isChanged = _currentLevelIndex != value;
            _currentLevelIndex = value;
            if (isChanged)
            {
                onCurrentLevelChanged?.Invoke(oldValue, value);
                CurrentLevelIndexSO.Value = value;
            }
        }
    }
    public event Action<int, int> onMaxReachedLevelChanged; 
    [SerializeField]private int _maxReachedLevel; 
    public int MaxReachedLevel 
    {
        get => _maxReachedLevel;
        set
        {
            int oldValue = _maxReachedLevel;
            bool isChanged = _maxReachedLevel != value;
            _maxReachedLevel = value;
            if (isChanged)
            {
                onMaxReachedLevelChanged?.Invoke(oldValue, value);
                //CurrentLevelIndexSO.Value = value;
            }
        }
    }
    
    [SerializeField]
    private float _attackSpeed; 
    public float AttackSpeed 
    {
        get => _attackSpeed;
        set => _attackSpeed = value;
    }
    
    [SerializeField]
    private int _maxHealth; 
    public int MaxHealth 
    {
        get => _maxHealth;
        set => _maxHealth = value;
    }
    [SerializeField]
    private string _lastTimeRewardCollected; 
    public string LastTimeRewardCollected 
    {
        get => _lastTimeRewardCollected;
        set => _lastTimeRewardCollected = value;
    }
    
    [ES3NonSerializable]public IntVar CurrentLevelIndexSO;

    public override void OnInstalled()
    {
        base.OnInstalled();
        CurrentLevelIndexSO.Value = CurrentLevelIndex;
    }
}