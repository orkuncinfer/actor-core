using Cinemachine;
using UnityEngine;

public class CinemachineSetFollowVarAsset : MonoCore
{
    [SerializeField] private GameObjectVariable _playerFollower;

    protected override void OnGameReady()
    {
        base.OnGameReady();
        transform.GetComponent<CinemachineVirtualCamera>().Follow = _playerFollower.Value.transform;
    }
}