using Sirenix.OdinInspector;
using UnityEngine;

public class OwnerActorPointer : MonoBehaviour
{
    [ReadOnly]public Actor PointedActor;
    
    public void SetPointedActor(Actor actor)
    {
        PointedActor = actor;
    }
}
