using Cinemachine;
using UnityEngine;

public class CinemachineSetFollowOwnerPointer : MonoCore
{
    protected override void OnGameReady()
    {
        base.OnGameReady();
        transform.GetComponent<CinemachineVirtualCamera>().Follow = transform.GetComponent<OwnerActorPointer>().PointedActor.transform;
    }
}