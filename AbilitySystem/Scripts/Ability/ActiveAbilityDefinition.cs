using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

public abstract class ActiveAbilityDefinition : AbilityDefinition
{
    [SerializeField] private bool _isBasicAttack;
    public bool IsBasicAttack => _isBasicAttack;

    [HideIf("_isBasicAttack")]
    [SerializeField] private bool _overrideAnimSpeed;
    public bool OverrideAnimSpeed => _overrideAnimSpeed;

    [SerializeField][ShowIf("_overrideAnimSpeed")] private float _animationSpeed;
    public float AnimationSpeed => _animationSpeed;
    [HorizontalGroup]
    [SerializeField] private AnimationClip _animationClip;
    public AnimationClip AnimationClip => _animationClip;
    
    [HorizontalGroup][PropertyRange(0,1)][Tooltip("At 1 the animation will play until the end, at 0 it will play for 0 seconds")]
    [SerializeField] private float _playTime = 1f;
    public float PlayTime => _playTime;
    
    [SerializeField] private bool _isLoopingAbility;
    public bool IsLoopingAbility => _isLoopingAbility;
       
    [SerializeField][ShowIf("_isLoopingAbility")] private float _duration;
    public float Duration => _duration;

     public GameplayEffectDefinition Cost;
     
     public GameplayPersistentEffectDefinition Cooldown;

     public List<GameplayTag> GrantedTagsDuringAbility;
     
     [SerializeReference][TypeFilter("GetFilteredTypeList")] [ListDrawerSettings(ShowFoldout = true)]
     public List<AbilityAction> AbilityActions = new List<AbilityAction>();
     
     public IEnumerable<Type> GetFilteredTypeList()
     {
         var baseType = typeof(AbilityAction);
         var assemblies = AppDomain.CurrentDomain.GetAssemblies();
         var q = assemblies.SelectMany(assembly => assembly.GetTypes())
             .Where(x => !x.IsAbstract)
             .Where(x => !x.IsGenericTypeDefinition)
             .Where(x => baseType.IsAssignableFrom(x) && x != baseType); // Exclude the base class itself
         return q;
     }
#if UNITY_EDITOR
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
     
#endif
}