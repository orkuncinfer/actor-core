using UnityEngine;

public class SM_CharacterBody : ActorStateMachine
{
    [SerializeField] private MonoState _grounded;
    [SerializeField] private MonoState _airborne;
    [SerializeField] private MonoState _ability;
    
    [SerializeField] private GameplayTag AbilityStateTag;
    
    protected override MonoState _initialState => _grounded;
    
    TagController _tagController;
    private Service_GAS _gas;
    private bool _shouldUseAbility;

    protected override void OnEnter()
    {
        base.OnEnter();
        _gas = Owner.GetService<Service_GAS>();
        Owner.GameplayTags.OnTagChanged += OnAnyTagCountChange;
    }

    protected override void OnExit()
    {
        base.OnExit();
        Owner.GameplayTags.OnTagChanged -= OnAnyTagCountChange;
    }

    private void OnAnyTagCountChange()
    {
        //if(AbilityStateTag == GameplayTag.None) return;
        if (Owner.GameplayTags.HasTagExact(AbilityStateTag))
        {
            _shouldUseAbility = true;
        }else
        {
            _shouldUseAbility = false; // no match
        }
    }

    protected override void OnRequireAddTransitions()
    {
        AddTransition(_grounded,_ability, GroundedToAbility);
        AddTransition(_ability,_grounded, AbilityToGrounded);
    }

    private bool AbilityToGrounded()
    {
        return !_shouldUseAbility;
    }

    private bool GroundedToAbility()
    {
        return _shouldUseAbility;
    }
}