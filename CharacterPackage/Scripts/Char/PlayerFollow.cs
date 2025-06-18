using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using FishNet.Managing.Timing;
using FishNet.Object;
using Sirenix.OdinInspector;
using UnityEngine;

public class PlayerFollow : MonoBehaviour
{
    public GameObjectVariable PlayerFollowerVar;
    public GameObject Player;
    
    [Header("Follow Settings")]
    [Tooltip("Higher values = smoother camera, but more lag")]
    [Range(0.01f, 0.5f)]
    public float SmoothTime = 0.1f;
    
    [Tooltip("Set to true if you want to update in FixedUpdate instead")]
    public bool UseFixedUpdate = false;
    
    // Private variables
    private CinemachineBrain _brain;
    private Vector3 _currentVelocity;
    private Vector3 _targetPosition;
    
    private void Start()
    {
        _brain = Camera.main.GetComponent<CinemachineBrain>();
        if (Player == null)
        {
            if (TryGetComponent(out OwnerActorPointer actorPointer))
            {
                if(actorPointer.PointedActor)
                    Player = actorPointer.PointedActor.gameObject;
            }
        }
        
        if (Player != null)
        {
            _targetPosition = new Vector3(
                Player.transform.position.x, 
                transform.position.y, 
                Player.transform.position.z);
        }
        
        PlayerFollowerVar.Value = gameObject;
        transform.SetParent(null);

#if USING_FISHNET
        if (UseFixedUpdate)
        {
            LastStaticUpdater.onFixedUpdate += OnFixedUpdate;
        }
        else
        {
            LastStaticUpdater.onLateUpdate += OnLateUpdate;
        }
#endif
    }

    private void OnDestroy()
    {
#if USING_FISHNET
        if (UseFixedUpdate)
        {
            LastStaticUpdater.onFixedUpdate -= OnFixedUpdate;
        }
        else
        {
            LastStaticUpdater.onLateUpdate -= OnLateUpdate;
        }
#endif
    }

#if USING_FISHNET
    // Use this if you want to tie camera movement to physics updates
    public void OnFixedUpdate()
    {
        if(Player == null || _brain == null) return;
        UpdateFollowPosition(Time.fixedDeltaTime);
        _brain.ManualUpdate();
    }

    public void OnLateUpdate()
    {
        if(Player == null || _brain == null) return;
        UpdateFollowPosition(Time.deltaTime);
        _brain.ManualUpdate();
    }
#endif
    
#if !USING_FISHNET
    private void FixedUpdate()
    {
        if (UseFixedUpdate && Player != null)
        {
            UpdateFollowPosition(Time.fixedDeltaTime);
        }
    }

    private void LateUpdate()
    {
        if (!UseFixedUpdate && Player != null)
        {
            UpdateFollowPosition(Time.deltaTime);
        }
    }
#endif

    private void UpdateFollowPosition(float deltaTime)
    {
        // Update target position based on player's current position
        _targetPosition = new Vector3(
            Player.transform.position.x, 
            transform.position.y, 
            Player.transform.position.z);
        
        // Smoothly move the follow target position
        transform.position = Vector3.SmoothDamp(
            transform.position, 
            _targetPosition, 
            ref _currentVelocity, 
            SmoothTime,
            Mathf.Infinity,
            deltaTime);
    }
}