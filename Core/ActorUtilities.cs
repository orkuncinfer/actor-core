using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ActorUtilities
{
    public static ActorBase FindFirstActorInParents(Transform currentParent)
    {
        if (currentParent == null)
        {
            return null; 
        }
        ActorBase actor = currentParent.GetComponent<ActorBase>();

        if (actor != null)
        {
            return actor; 
        }
        return FindFirstActorInParents(currentParent.parent);
    }
}
