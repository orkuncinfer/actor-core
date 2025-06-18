
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ActorCharacterExtensions 
{
    public static GameObject GetEquippedInstance(this ActorBase actor)
    {
        return actor.GetData<DS_EquipmentUser>().EquipmentInstance;
    }
}
