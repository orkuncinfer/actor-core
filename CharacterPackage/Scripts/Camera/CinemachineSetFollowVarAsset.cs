using System;
using Cinemachine;
using UnityEngine;

public class CinemachineSetFollowVarAsset : MonoBehaviour
{
    [SerializeField] private GameObjectVariable _playerFollower;

    private void Start()
    {
        if (_playerFollower.Value != null)
        {
            transform.GetComponent<CinemachineVirtualCamera>().Follow = _playerFollower.Value.transform;
        }
        _playerFollower.OnChange += OnPlayerValueChange;
    }

    private void OnDestroy()
    {
        _playerFollower.OnChange -= OnPlayerValueChange;
    }

    private void OnPlayerValueChange(GameObject obj)
    {
        transform.GetComponent<CinemachineVirtualCamera>().Follow = _playerFollower.Value.transform;
    }
}