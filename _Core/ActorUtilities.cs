using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ActorUtilities
{
    public static ActorBase FindFirstActorInParents(Transform currentParent)
    {
        while (currentParent != null)
        {
            ActorBase actor = currentParent.GetComponent<ActorBase>();
            if (actor != null)
            {
                return actor;
            }
            currentParent = currentParent.parent;
        }
        return null;
    }
}
