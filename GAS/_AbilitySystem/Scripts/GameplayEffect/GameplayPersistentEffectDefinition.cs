using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Core.Editor;
using JetBrains.Annotations;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

[EffectType(typeof(GameplayPersistentEffect))]
[CreateAssetMenu(fileName = "GameplayPersistentEffect", menuName = "AbilitySystem/Effect/GameplayPersistentEffect")]
public class GameplayPersistentEffectDefinition : GameplayEffectDefinition
{
    [BoxGroup("Persistence", ShowLabel = false)]
    [TitleGroup("Persistence/Persistence")]
    [SerializeField] protected bool _isInfinite;
    public bool IsInfinite => _isInfinite;

    [BoxGroup("Persistence", ShowLabel = false)]
    [TitleGroup("Persistence/Persistence")]
    [HideIf("_isInfinite")][SerializeField] protected FormulaField _durationFormula;
    public FormulaField DurationFormula => _durationFormula;
    
    [BoxGroup("Persistence", ShowLabel = false)]
    [TitleGroup("Persistence/Persistence")]
    [SerializeField] private bool _isPeriodic;
    public bool IsPeriodic => _isPeriodic;
    
    [BoxGroup("Persistence", ShowLabel = false)]
    [TitleGroup("Persistence/Persistence")][ShowIf("_isPeriodic")]
    [SerializeField] private float _period;
    public float Period => _period;
    
    [BoxGroup("Persistence", ShowLabel = false)]
    [TitleGroup("Persistence/Persistence")][ShowIf("_isPeriodic")]
    [SerializeField] private bool _executePeriodicEffectOnApplication;
    public bool ExecutePeriodicEffectOnApplication => _executePeriodicEffectOnApplication;


    
    [BoxGroup("Persistence", ShowLabel = false)]
    [TitleGroup("Persistence/Persistence")]
    [SerializeField] private SpecialEffectDefinition _specialPersistentEffectDefinition;
    public SpecialEffectDefinition SpecialPersistentEffectDefinition => _specialPersistentEffectDefinition;
    

    
   
    

}

