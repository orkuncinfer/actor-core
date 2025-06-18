using System;
using Animancer;
using UnityEngine;

[Serializable]
public class Data_Gun : Data
{
    public AbilityDefinition ShootAbility => _shootAbility;
    [SerializeField]private AbilityDefinition _shootAbility;
    
    public ClipTransition ReloadClip => _reloadClip;
    [SerializeField]private ClipTransition _reloadClip;
}