using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

public abstract class AbilityDefinition : ScriptableObject
{
    [BoxGroup("AbilityAttributes")]
    [SerializeField] private string _title;
    public string Title => _title;
    
    [BoxGroup("AbilityAttributes")]
    [SerializeField] private string _description;
    public string Description => _description;
    
    [BoxGroup("AbilityAttributes")]
    [SerializeField] private int _maxLevel = 20;
    public int MaxLevel => _maxLevel;
    
    [BoxGroup("AbilityAttributes")][PreviewField(Alignment = ObjectFieldAlignment.Left)]
    [SerializeField] private Sprite _icon;
    public Sprite Icon  => _icon;
    
    [SerializeField] private List<GameplayEffectDefinition> _gameplayEffectDefinitions;
    public List<GameplayEffectDefinition> GameplayEffectDefinitions => _gameplayEffectDefinitions;
    

  
}
