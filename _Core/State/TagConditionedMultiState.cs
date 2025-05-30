using System;
using System.Collections.Generic;
using UnityEngine;

public class TagConditionedMultiState : MultiStateComponent
{
    public GameplayTagContainer RequiredTags;
    
    private void Awake()
    {
        Owner = ActorUtilities.FindFirstActorInParents(transform);
        Owner.GameplayTags.OnTagChanged += OnTagChanged;
    }

    private void OnDestroy()
    {
        if (Owner != null)
        {
            Owner.GameplayTags.OnTagChanged -= OnTagChanged;
        }
    }

    private void OnTagChanged()
    {
        if (Owner.GameplayTags.HasAll(RequiredTags))
        {
            CheckoutEnter(Owner);
        }
        else
        {
            CheckoutExit();
        }
    }
}
