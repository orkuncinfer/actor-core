using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

public abstract class ActiveAbilityDefinition : AbilityDefinition
{
    [SerializeField] protected string _animationName;
    public string AnimationName => _animationName;
    
    [SerializeField] private bool _isBasicAttack;
    public bool IsBasicAttack => _isBasicAttack;

    [HideIf("_isBasicAttack")]
    [SerializeField] private bool _overrideAnimSpeed;
    public bool OverrideAnimSpeed => _overrideAnimSpeed;

    [SerializeField][ShowIf("_overrideAnimSpeed")] private float _animationSpeed;
    public float AnimationSpeed => _animationSpeed;
    
    [SerializeField] private AnimationClip _animationClip;
    public AnimationClip AnimationClip => _animationClip;
    
    [SerializeField] private bool _isLoopingAbility;
    public bool IsLoopingAbility => _isLoopingAbility;
       
    [SerializeField][ShowIf("_isLoopingAbility")] private float _duration;
    public float Duration => _duration;

     public GameplayEffectDefinition Cost;
     
     public GameplayPersistentEffectDefinition Cooldown;

     public List<GameplayTag> GrantedTagsDuringAbility;
     
     public List<AbilityWindowAction> AbilityWindowActions;
     
     private void Reset()
     {
         if(Cost != null) return;
         if(Cooldown != null) return;
         if (!AssetDatabase.Contains(this))
         {
             return;
         }
     
         GameplayEffectDefinition costItem = ScriptableObject.CreateInstance<GameplayEffectDefinition>();
         Cost = costItem;
         costItem.name = "Cost";
         Cost = costItem;
        
         GameplayPersistentEffectDefinition cooldownItem = ScriptableObject.CreateInstance<GameplayPersistentEffectDefinition>();
         Cooldown = cooldownItem;
         cooldownItem.name = "Cooldown";
         Cooldown = cooldownItem;
        
         AssetDatabase.AddObjectToAsset(costItem, this);
         AssetDatabase.AddObjectToAsset(cooldownItem, this);
         AssetDatabase.SaveAssets();
         AssetDatabase.Refresh();
     }
}