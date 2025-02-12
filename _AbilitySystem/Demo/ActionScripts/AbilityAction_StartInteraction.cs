using RootMotion.FinalIK;
using UnityEngine;

public class AbilityAction_StartInteraction : AbilityAction
{
    public bool CancelAfterInteract;
    private AimIKWeightHandler _weightHandler;
    private InteractionSystem _interactionSystem;
    
    private bool _initialAimingState;
    public override AbilityAction Clone()
    {
        AbilityAction_StartInteraction clone = AbilityActionPool<AbilityAction_StartInteraction>.Shared.Get();
        clone._weightHandler = _weightHandler;
        clone._initialAimingState = _initialAimingState;
        clone._interactionSystem = _interactionSystem;
        clone.CancelAfterInteract = CancelAfterInteract;
        return clone;
    }

    public override void OnStart(Actor owner, ActiveAbility ability)
    {
        base.OnStart(owner, ability);
        _weightHandler = owner.GetComponentInChildren<AimIKWeightHandler>();
        _interactionSystem = owner.GetComponentInChildren<InteractionSystem>();

        _weightHandler.enabled = false;
        
        // If not paused, find the closest InteractionTrigger that the character is in contact with
        int closestTriggerIndex = _interactionSystem.GetClosestTriggerIndex();

        // ...if none found, do nothing
        if (closestTriggerIndex == -1)
        {
            RequestEndAbility();
            return;
        }

        // ...if the effectors associated with the trigger are in interaction, do nothing
        if (!_interactionSystem.TriggerEffectorsReady(closestTriggerIndex))
        {
            RequestEndAbility();
            return;
        }

        // Its OK now to start the trigger

        if (!_interactionSystem.TriggerInteraction(closestTriggerIndex, false))
        {
            RequestEndAbility();
            return;
        }

        if (CancelAfterInteract)
        {
            RequestEndAbility();
            return;
        }
        _interactionSystem.OnInteractionStop += OnInteractionStop;
    }

    private void OnInteractionStop(FullBodyBipedEffector effectortype, InteractionObject interactionobject)
    {
        Debug.Log("Interaction stopped " + interactionobject.name);
        RequestEndAbility();
        _interactionSystem.OnInteractionStop -= OnInteractionStop;
    }
    public override void OnExit()
    {
        base.OnExit();
        _weightHandler.enabled = true;
    }
}