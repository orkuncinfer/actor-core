using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActorUtilities
{
    public static Actor FindFirstActorInParents(Transform currentParent)
    {
        while (currentParent != null)
        {
            Actor actor = currentParent.GetComponent<Actor>();
            if (actor != null)
            {
                return actor;
            }
            currentParent = currentParent.parent;
        }
        return null;
    }
}
