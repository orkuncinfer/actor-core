using RootMotion.FinalIK;
using UnityEngine;

public class AbilityAction_StartInteraction : AbilityAction
{
    public bool ToggleAiming;
    private AimIKWeightHandler _weightHandler;
    private InteractionSystem _interactionSystem;
    private AbilityController _abilityController;
    
    private bool _initialAimingState;
    public override AbilityAction Clone()
    {
        AbilityAction_StartInteraction clone = AbilityActionPool<AbilityAction_StartInteraction>.Shared.Get();
        clone._weightHandler = _weightHandler;
        clone.ToggleAiming = ToggleAiming;
        clone._initialAimingState = _initialAimingState;
        clone._interactionSystem = _interactionSystem;
        return clone;
    }

    public override void OnStart(Actor owner, ActiveAbility ability)
    {
        base.OnStart(owner, ability);
        _abilityController = owner.GetComponentInChildren<AbilityController>();
        _weightHandler = owner.GetComponentInChildren<AimIKWeightHandler>();
        _interactionSystem = owner.GetComponentInChildren<InteractionSystem>();

        _weightHandler.enabled = false;
        
        // If not paused, find the closest InteractionTrigger that the character is in contact with
        int closestTriggerIndex = _interactionSystem.GetClosestTriggerIndex();

        // ...if none found, do nothing
        if (closestTriggerIndex == -1) return;

        // ...if the effectors associated with the trigger are in interaction, do nothing
        if (!_interactionSystem.TriggerEffectorsReady(closestTriggerIndex)) return;

        // Its OK now to start the trigger

        _interactionSystem.TriggerInteraction(closestTriggerIndex, false);
        _interactionSystem.OnInteractionStop += OnInteractionStop;
    }

    private void OnInteractionStop(FullBodyBipedEffector effectortype, InteractionObject interactionobject)
    {
        Debug.Log("Interaction stopped " + interactionobject.name);
        _abilityController.CancelAbilityIfActive(Definition.name);
        _interactionSystem.OnInteractionStop -= OnInteractionStop;
    }
    public override void OnExit()
    {
        base.OnExit();
        _weightHandler.enabled = true;
    }
}