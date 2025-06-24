using System;
using System.Collections.Generic;
using Cinemachine;
using FishNet.Object;
using UnityEngine;

public class Network_CameraSpawner : NetworkBehaviour
{
    public Actor OwnerActor;
    public GameObject CameraPrefab;
    public GameObject FollowerPrefab;
    public GameObject LootLabelCamera;
    public Transform CameraFollowTransform;

    private PlayerFollow _playerFollow;

    private GameObject _cameraInstance;
    private GameObject _followerIstance;

#if USING_FISHNET
    public override void OnStartClient()
    {
        base.OnStartClient();
        if (base.IsOwner)
        {
            GameObject followerInstance = Instantiate(FollowerPrefab,transform);
            followerInstance.transform.position = new Vector3(0, 0, 0);
            followerInstance.transform.rotation = Quaternion.identity;
            _playerFollow = followerInstance.GetComponent<PlayerFollow>();
            _playerFollow.Player = CameraFollowTransform.gameObject;
            
            GameObject cameraInstance = Instantiate(CameraPrefab,transform);
            cameraInstance.transform.position = new Vector3(0, 0, 0);
            cameraInstance.transform.rotation = Quaternion.identity;
            
            cameraInstance.GetComponent<CinemachineVirtualCamera>().Follow = followerInstance.transform;
            
            _cameraInstance = cameraInstance;
            _followerIstance = followerInstance;
        }
        TimeManager.OnPostTick += OnPostTick;
        
    }

    private void OnDestroy()
    {
        if(_cameraInstance)Destroy(_cameraInstance);
        if(_followerIstance)Destroy(_followerIstance);
    }

    private void OnPostTick()
    {
       // if(_playerFollow)_playerFollow.OnTick();
    }

    public override void OnStopClient()
    {
        base.OnStopClient();
        TimeManager.OnPostTick -= OnPostTick;
    }
    #endif
}