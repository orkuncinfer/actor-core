using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[Serializable]
public class DS_GameModePersistent : Data
{
    public event Action<int> OnCurrentLevelChanged;
    [SerializeField]
    private int _currentLevelIndex; 
    public int CurrentLevelIndex 
    {
        get => _currentLevelIndex;
        set
        {
            if (_currentLevelIndex != value)
            {
                _currentLevelIndex = value;
                OnCurrentLevelChanged?.Invoke((int)_currentLevelIndex);
            }
        }
    }
    
    [SerializeField]
    private int _maxReachedLevelIndex; 
    public int MaxReachedLevelIndex 
    {
        get => _maxReachedLevelIndex;
        set => _maxReachedLevelIndex = value;
    }
}
