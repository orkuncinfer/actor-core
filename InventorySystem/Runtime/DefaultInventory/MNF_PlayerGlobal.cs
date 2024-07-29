using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;
using WolarGames.Variables;

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
    public Dictionary<string, int> Inventory = new Dictionary<string, int>();
    
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

    public override void OnInstalled()
    {
        base.OnInstalled();
    }
}