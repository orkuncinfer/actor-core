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
        return new Data[] { _playerPersistent, _itemUpgrades };
    }
}
[System.Serializable]
public class DS_PlayerPersistent : Data
{
    [ShowInInspector]protected Dictionary<string,int> _activeMissions = new Dictionary<string, int>();
    public Dictionary<string,int> ActiveMissions => _activeMissions;
    
    [ShowInInspector]protected Dictionary<string,int> _inventory = new Dictionary<string, int>();
    public Dictionary<string,int> Inventory => _inventory;
    
    [SerializeField]
    private string _lastTimeRewardCollected; 
    public string LastTimeRewardCollected 
    {
        get => _lastTimeRewardCollected;
        set => _lastTimeRewardCollected = value;
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
}