using System;
using System.Collections;
using System.Collections.Generic;
using ECM2.Examples.Networking.FishNet;
using FishNet.Object;
using Pathfinding;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems; // Added for UI detection

public class Network_Pathfinding : NetworkBehaviour
{
    [SerializeField] private FollowerEntity _follower;
    [SerializeField] private LayerMask Mask;

    private Camera _camera;
    private DS_MovingActor _movingActor;
    private ABPath _path;
    private RPG_PlayerController _controller;
    [SerializeField] private List<Vector3> _remainingPath = new List<Vector3>();
    [SerializeField] private InputActionReference _movementInputAction;

    private IEnumerator _coroutine;
    private PathfindingFollowable _followingTarget;
    
    public override void OnStartClient()
    {
        base.OnStartClient();
        _movingActor = GetComponent<ActorBase>().GetData<DS_MovingActor>();
        _controller = GetComponent<RPG_PlayerController>();
        _camera = Camera.main;
        _controller.CameraRelative = true;
    }

    private void Update()
    {
        if(_camera == null || _movingActor == null || _controller == null) return;
        if(!base.IsOwner) return;
        
        Vector3 newPosition = Vector3.zero;
        bool positionFound = false;
        Transform hitObject = null;

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            // Check if mouse is over UI element
            if (!IsPointerOverUIElement())
            {
                if (_camera.pixelRect.Contains(Input.mousePosition) && Physics.Raycast(_camera.ScreenPointToRay(Input.mousePosition), out var hit, Mathf.Infinity, Mask)) 
                {
                    newPosition = hit.point;
                    hitObject = hit.transform;
                    positionFound = true;
                    if (hit.collider.transform.TryGetComponent(out PathfindingFollowable followable))
                    {
                        GoToDestination(followable);
                        return;
                    }
                }

                if (positionFound)
                {
                   Debug.Log("Ground clicked");
                    GoToDestination(newPosition);
                }
            }
        }

        if (_movementInputAction.action.ReadValue<Vector2>() != Vector2.zero && _follower.hasPath)
        {
            // pathfinding interrupted
            _follower.destination = transform.position;
            _controller.CameraRelative = true;
            _followingTarget = null;
            StopCoroutine(_coroutine);
        }
    }

    // Check if pointer is over a UI element
    private bool IsPointerOverUIElement()
    {
        if (EventSystem.current == null)
            return false;
            
        // Check for UI elements under pointer
        PointerEventData eventData = new PointerEventData(EventSystem.current);
        eventData.position = Input.mousePosition;
        
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        if (results.Count > 0)
        {
            if(results[0].gameObject.TryGetComponent(out LootLabel lootLabel))
            {
                GoToDestination(lootLabel.TargetTransform.position);
            }
        }
        
        return results.Count > 0;
    }

    private void GoToDestination(Vector3 destination)
    {
        if(_coroutine != null)StopCoroutine(_coroutine);
        _coroutine = GoToDest(destination);
        StartCoroutine(_coroutine);
    }
    private void GoToDestination(PathfindingFollowable followable)
    {
        _coroutine = GoToTransform(followable);
        _followingTarget = followable;
        StartCoroutine(_coroutine);
    }

    IEnumerator GoToDest(Vector3 dest)
    {
        
        _controller.CameraRelative = false;
        _follower.destination = dest;
        yield return null;
        while (!_follower.reachedDestination)
        {
            
            _follower.GetRemainingPath(_remainingPath, out bool sl);

            for (int i = 0; i < _remainingPath.Count; i++)
            {
                if(i >= _remainingPath.Count - 1) continue;
                Debug.DrawLine(_remainingPath[i], _remainingPath[i+1], Color.red);
            }

            if (_remainingPath.Count >= 2)
            {
                Vector3 direction = _remainingPath[1] - _remainingPath[0];
                direction.Normalize();
                _movingActor.MoveInput = new Vector2(direction.x, direction.z);
            }
                
            yield return null;
        }

        if (_followingTarget != null)
        {
            _followingTarget.TargetReached(NetworkObject);
        }

        _controller.CameraRelative = true;
        _movingActor.MoveInput = Vector2.zero;
    }
    
    IEnumerator GoToTransform(PathfindingFollowable followable)
    {
        _controller.CameraRelative = false;
        _follower.destination = followable.transform.position;
        yield return null;
        while (!_follower.reachedDestination)
        {
            if (followable.gameObject.activeInHierarchy == false)
            {
                _controller.CameraRelative = true;
                _movingActor.MoveInput = Vector2.zero;
                yield break;
            }
            _follower.destination = followable.transform.position;
            _follower.GetRemainingPath(_remainingPath, out bool sl);

            for (int i = 0; i < _remainingPath.Count; i++)
            {
                if(i >= _remainingPath.Count - 1) continue;
                Debug.DrawLine(_remainingPath[i], _remainingPath[i+1], Color.red);
            }

            if (_remainingPath.Count >= 2)
            {
                Vector3 direction = _remainingPath[1] - _remainingPath[0];
                direction.Normalize();
                _movingActor.MoveInput = new Vector2(direction.x, direction.z);
            }
                
            yield return null;
        }

        _controller.CameraRelative = true;
        _movingActor.MoveInput = Vector2.zero;
    }
}