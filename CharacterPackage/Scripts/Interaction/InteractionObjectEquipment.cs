using System.Collections;
using System.Collections.Generic;
using RootMotion.FinalIK;
using UnityEngine;
using UnityEngine.Serialization;

public class InteractionObjectEquipment : MonoBehaviour
{
    [SerializeField] private  InteractionObject _interactionObject;
    private Equippable _equippable;

    public void PickUp()
    {
        Actor actor =
            ActorUtilities.FindFirstActorInParents(_interactionObject.lastUsedInteractionSystem.transform);
        _equippable.EquipThisInstance(actor);
    }
}
