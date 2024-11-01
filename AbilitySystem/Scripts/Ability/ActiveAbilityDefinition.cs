using System;
using System.Collections.Generic;
using System.Linq;
using Animancer;
using BandoWare.GameplayTags;
using NUnit.Framework;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

public abstract class ActiveAbilityDefinition : AbilityDefinition
{
    public bool IsBasicAttack => _isBasicAttack;
    [SerializeField] private bool _isBasicAttack;
    
    public int AbilityLayer => _abilityLayer;
    [SerializeField][Tooltip("The ability can only be activated if there is no other ability active on the same layer")]
    private int _abilityLayer;
    
    public bool OverrideAnimSpeed => _overrideAnimSpeed;
    [HideIf("_isBasicAttack")][FoldoutGroup("AnimConfig")]
    [SerializeField] private bool _overrideAnimSpeed;

    public float AnimationSpeed => _animationSpeed;
    [SerializeField][ShowIf("_overrideAnimSpeed")][FoldoutGroup("AnimConfig")] private float _animationSpeed;
    [FoldoutGroup("AnimConfig")]
    [SerializeField] private ClipTransition _clipTransition;
    
    public ClipTransition ClipTransition => _clipTransition;
    
    public AnimationClip AnimationClip => _clipTransition.Clip;
    
    public float EndTime => _endTime;
    [FoldoutGroup("AnimConfig")][PropertyRange(0,1)][Tooltip("At 1 the animation will play until the end, at 0 it will play for 0 seconds")]
    [SerializeField] private float _endTime = 1f;
    
    public AvatarMask AvatarMask => _avatarMask;
    [FoldoutGroup("AnimConfig")]
    [SerializeField] private AvatarMask _avatarMask;

    public int AnimationLayer => _animationLayer;
    [FoldoutGroup("AnimConfig")]
    [SerializeField]
    private int _animationLayer = 0;
    
    public bool IsLoopingAbility => _isLoopingAbility;
    [SerializeField] private bool _isLoopingAbility;
       
    public float Duration => _duration;
    [SerializeField][ShowIf("_isLoopingAbility")] private float _duration;

     public GameplayEffectDefinition Cost;
     
     public GameplayPersistentEffectDefinition Cooldown;
     public GameplayTagContainer AbilityTags => _abilityTags; // IMPLEMENTED // TESTED
     [BoxGroup("Tags", ShowLabel = false)][ListDrawerSettings(ShowFoldout = true)]
     [TitleGroup("Tags/Tags")]
     [SerializeField][Tooltip("The ability has these tags for use of other operations")]
     private GameplayTagContainer _abilityTags;
     public GameplayTagContainer GrantedTagsDuringAbility => _grantedTagsDuringAbility;
     [BoxGroup("Tags", ShowLabel = false)][ListDrawerSettings(ShowFoldout = true)]
     [TitleGroup("Tags/Tags")]
     [SerializeField][Tooltip("These effects are applied to the user of the ability and are removed after the ability is done")]
     private GameplayTagContainer _grantedTagsDuringAbility;
     
     public GameplayTagContainer ActivationRequiredTags => _activationRequiredTags; // NOT IMPLEMENTED // NOT TESTED
     [BoxGroup("Tags", ShowLabel = false)][ListDrawerSettings(ShowFoldout = true)]
     [TitleGroup("Tags/Tags")]
     [SerializeField][Tooltip("This ability can only be activated if the user has these tags")]
     private GameplayTagContainer _activationRequiredTags;
     public GameplayTagContainer ActivationBlockedTags => _activationBlockedTags; // NOT IMPLEMENTED // NOT TESTED
     [BoxGroup("Tags", ShowLabel = false)][ListDrawerSettings(ShowFoldout = true)]
     [TitleGroup("Tags/Tags")]
     [SerializeField][Tooltip("This ability is blocked if the user has any of these tags")]
     private GameplayTagContainer _activationBlockedTags;
     
     public GameplayTagContainer CancelAbilitiesWithTag => _cancelAbilitiesWithTag; // NOT IMPLEMENTED // NOT TESTED
     [BoxGroup("Tags", ShowLabel = false)][ListDrawerSettings(ShowFoldout = true)]
     [TitleGroup("Tags/Tags")]
     [SerializeField][Tooltip("Abilities with these tags will be canceled when this ability is activated")]
     private GameplayTagContainer _cancelAbilitiesWithTag;
     
     [Space(5)]
     public List<GameplayEffectDefinition> GrantedEffectsDuringAbility;
     
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
     private void Awake()
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