using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Sirenix.OdinInspector;
using UnityEngine;

public enum GameplayEffectStackingExpirationPolicy
{
    NeverRefresh,
    RemoveSingleStackAndRefreshDuration
}
public enum GameplayEffectStackingDurationPolicy
{
    NeverRefresh,
    RefreshOnSuccessfulApplication
}
public enum GameplayEffectStackingPeriodPolicy
{
    NeverRefresh,
    ResetOnSuccessfulApplication
}

[EffectType(typeof(GameplayStackableEffect))]
[CreateAssetMenu(fileName = "GameplayStackableEffect", menuName = "AbilitySystem/Effect/GameplayStackableEffect")]
public class GameplayStackableEffectDefinition : GameplayPersistentEffectDefinition
{
    [BoxGroup("Stacking", ShowLabel = false)]
    [TitleGroup("Stacking/Overflow")]
    [SerializeField][ListDrawerSettings(ShowFoldout = true)] private protected List<GameplayEffectDefinition> _overflowEffects;
    public ReadOnlyCollection<GameplayEffectDefinition> OverflowEffects =>
        _overflowEffects.AsReadOnly();

    [BoxGroup("Stacking", ShowLabel = false)]
    [TitleGroup("Stacking/Overflow")]
    [SerializeField] private bool _denyOverflowApplication;
    public bool DenyOverflowApplication => _denyOverflowApplication;

    [BoxGroup("Stacking", ShowLabel = false)]
    [TitleGroup("Stacking/Stacking")]
    [SerializeField] private bool _clearStackOnOverflow;
    public bool ClearStackOnOverflow => _clearStackOnOverflow;

    [BoxGroup("Stacking", ShowLabel = false)]
    [TitleGroup("Stacking/Stacking")]
    [SerializeField] private int _stackLimitCount = 3;
    public int StackLimitCount => _stackLimitCount;

    [BoxGroup("Stacking", ShowLabel = false)]
    [TitleGroup("Stacking/Stacking")]
    [SerializeField] private GameplayEffectStackingDurationPolicy _stackDurationRefreshPolicy;
    public GameplayEffectStackingDurationPolicy StackDurationRefreshPolicy => _stackDurationRefreshPolicy;
    
    [BoxGroup("Stacking", ShowLabel = false)]
    [TitleGroup("Stacking/Stacking")]
    [SerializeField] private GameplayEffectStackingPeriodPolicy _stackPeriodResetPolicy;
    public GameplayEffectStackingPeriodPolicy StackPeriodResetPolicy => _stackPeriodResetPolicy;
    
    [BoxGroup("Stacking", ShowLabel = false)]
    [TitleGroup("Stacking/Stacking")]
    [SerializeField] private GameplayEffectStackingExpirationPolicy _stackingExpirationPolicy;
    public GameplayEffectStackingExpirationPolicy StackingExpirationPolicy => _stackingExpirationPolicy;
}
