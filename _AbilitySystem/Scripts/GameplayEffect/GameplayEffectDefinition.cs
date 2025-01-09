using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using BandoWare.GameplayTags;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;

[EffectType(typeof(GameplayEffect))]
[CreateAssetMenu(fileName = "GameplayEffect", menuName = "AbilitySystem/Effect/GameplayEffect")]
public class GameplayEffectDefinition : ScriptableObject
{
    [BoxGroup("General", ShowLabel = false)]
    [TitleGroup("General/General")]
    [SerializeReference][TypeFilter("GetFilteredTypeList")] [ListDrawerSettings(ShowFoldout = true)]
    public List<AbstractGameplayEffectStatModifier> ModifierDefinitions = new List<AbstractGameplayEffectStatModifier>();
    
    [BoxGroup("General", ShowLabel = false)]
    [TitleGroup("General/General")]
    [SerializeField][ShowInInspector] private SpecialEffectDefinition m_SpecialEffectDefinition;
    public SpecialEffectDefinition specialEffectDefinition => m_SpecialEffectDefinition;
    
    [BoxGroup("General", ShowLabel = false)]
    [TitleGroup("General/General")]
    [SerializeField] private List<GameplayEffectDefinition> _conditionalEffects;
    public List<GameplayEffectDefinition>  ConditionalEffects => _conditionalEffects;
    
    [BoxGroup("General", ShowLabel = false)]
    [TitleGroup("General/General")]
    [SerializeField] private string _description;
    public string Description => _description;
    
    [Tooltip("These tags are applied to the actor I am applied to")]
    [BoxGroup("Tags", ShowLabel = false)][ListDrawerSettings(ShowFoldout = true)]
    [TitleGroup("Tags/Tags")]
    [SerializeField] protected GameplayTagContainer _grantedTags;
    public GameplayTagContainer GrantedTags => _grantedTags;
    
    [Tooltip("If target have these tags, this effect will not be applied")]
    [BoxGroup("Tags", ShowLabel = false)][ListDrawerSettings(ShowFoldout = true)]
    [TitleGroup("Tags/Tags")]
    [SerializeField] protected GameplayTagContainer _applicationBlockerTags;
    public GameplayTagContainer ApplicationBlockerTags => _applicationBlockerTags;

    [BoxGroup("Tags", ShowLabel = false)]
    [TitleGroup("Tags/Tags")][ListDrawerSettings(ShowFoldout = true)]
    [SerializeField] protected GameplayTagContainer _removeEffectsWithTags;
    public GameplayTagContainer RemoveEffectsWithTags => _removeEffectsWithTags;
    
    
    public IEnumerable<Type> GetFilteredTypeList()
    {
        var baseType = typeof(AbstractGameplayEffectStatModifier);
        var q = baseType.Assembly.GetTypes()
            .Where(x => !x.IsAbstract)
            .Where(x => !x.IsGenericTypeDefinition)
            .Where(x => baseType.IsAssignableFrom(x) && x != baseType); // Exclude the base class itself
        return q;
    }
}

