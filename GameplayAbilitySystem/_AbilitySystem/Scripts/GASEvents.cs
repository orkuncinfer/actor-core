using System;
using Opsive.Shared.Events;
using StatSystem;
using UnityEngine;

public static class GASEvents
{
    public static event Action<StatModifier> OnModifierApplied;

    public static void InvokeOnModifierApplied(StatModifier modifier)
    {
        Debug.Log("Modifier event applied");
        OnModifierApplied?.Invoke(modifier);
    }
}
