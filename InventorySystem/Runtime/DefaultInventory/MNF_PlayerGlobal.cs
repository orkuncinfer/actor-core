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
    [SerializeField] private DS_GameSettings _gameSettings;
    
    protected override Data[] InstallData()
    {
        return new Data[] { _itemUpgrades, _playerPersistent,_gameSettings};
    }
}
[System.Serializable]
public class DS_PlayerPersistent : Data
{
    [ShowInInspector]public Dictionary<int,float> LevelCompletionProgress = new Dictionary<int, float>(); // level index - level complete health percentage 0-1
    public Dictionary<string,int> ActiveMissions = new Dictionary<string, int>();
    public Dictionary<string, int> Inventory = new Dictionary<string, int>();
    
    [ShowInInspector]public Dictionary<string,int> RuntimeBuffs = new Dictionary<string, int>();
    
    
     [SerializeField]
    private float _attackSpeedPersistent; 
    public float AttackSpeedPersistent 
    {
        get => _attackSpeedPersistent;
        set => _attackSpeedPersistent = value;
    }
    [SerializeField]
    private float _maxAttackSpeed; 
    public float MaxAttackSpeed 
    {
        get => _maxAttackSpeed;
        set => _maxAttackSpeed = value;
    }
    
    [SerializeField]
    private int _maxHealth; 
    public int MaxHealth 
    {
        get => _maxHealth;
        set => _maxHealth = value;
    }
    
    [SerializeField]
    private int _playerInLevelExp; 
    public int PlayerInLevelExp 
    {
        get => _playerInLevelExp;
        set => _playerInLevelExp = value;
    }
    
    [SerializeField]
    private string _lastTimeRewardCollected; 
    public string LastTimeRewardCollected 
    {
        get => _lastTimeRewardCollected;
        set => _lastTimeRewardCollected = value;
    }
    
    [SerializeField]
    private bool _hasOngoingSession; 
    public bool HasOngoingSession 
    {
        get => _hasOngoingSession;
        set => _hasOngoingSession = value;
    }

    public override void OnInstalled()
    {
        base.OnInstalled();

        if (!HasOngoingSession)
        {
            RuntimeBuffs.Clear();
        }
    }
}

[System.Serializable]
public class DS_GameSettings : Data
{
    [SerializeField]
    private float _balloonSpeedFactor; 
    public float BalloonSpeedFactor 
    {
        get => _balloonSpeedFactor;
        set => _balloonSpeedFactor = value;
    }
}