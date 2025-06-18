using System.Collections;
using System.Collections.Generic;
using ECM2.Examples.Networking.FishNet;
using FishNet.Object;
using Pathfinding;
using UnityEngine;

[RequireComponent(typeof(FollowerEntity))]
public class Network_Movement : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private FollowerEntity _follower;
    
    [Header("Movement Settings")]
    [SerializeField] private float _stoppingDistance = 0.5f;
    
    // Component References
    private DS_MovingActor _movingActor;
    private RPG_PlayerController _controller;
    private ActorBase _actor;
    
    // State
    private List<Vector3> _remainingPath = new List<Vector3>();
    
    public bool IsMoving => _follower.hasPath;
    public Vector3 CurrentDestination => _follower.destination;
    
    public override void OnStartClient()
    {
        base.OnStartClient();
        
        if (!IsOwner) return;
        
        // Get references
        _actor = GetComponent<ActorBase>();
        _movingActor = _actor.GetData<DS_MovingActor>();
        _controller = GetComponent<RPG_PlayerController>();
        
        // Set initial state
        _controller.CameraRelative = true;
        _follower.enabled = false;
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        if (IsServerOnly)
        {
            _follower.enabled = false;
        }
    }

    #region Movement Coroutines
    
    public IEnumerator MoveTo(Vector3 destination)
    {
        _follower.enabled = true;
        _controller.CameraRelative = false;
        _follower.destination = destination;
       
        
        yield return FollowPathUntilReached(_stoppingDistance);
        
        _follower.destination = transform.position;
        _controller.CameraRelative = true;
        _movingActor.MoveInput = Vector2.zero;
        _movingActor.MoveDirection = Vector3.zero;
        _follower.enabled = false;
    }
    
    public IEnumerator MoveToTarget(GameObject target, float stoppingDistance)
    {
        _follower.enabled = true;
        _controller.CameraRelative = false;
        
        while (target != null && target.activeInHierarchy)
        {
            _follower.destination = target.transform.position;
            
            // Check if we're close enough
            float distanceToTarget = Vector3.Distance(transform.position, _follower.destination);
            if (distanceToTarget <= stoppingDistance)
            {
                break;
            }
            
            yield return FollowPathStep();
        }

        _follower.destination = transform.position;
        _controller.CameraRelative = true;
        _movingActor.MoveInput = Vector2.zero;
        _movingActor.MoveDirection = Vector3.zero;
        _follower.enabled = false;
    }
    
    public IEnumerator MoveToFollowable(PathfindingFollowable followable)
    {
        _follower.enabled = true;
        _controller.CameraRelative = false;
        
        while (followable != null && followable.gameObject.activeInHierarchy)
        {
            _follower.destination = followable.transform.position;
            
            if (_follower.reachedDestination)
            {
                followable.TargetReached(NetworkObject);
                break;
            }
            
            yield return FollowPathStep();
        }
        
        _controller.CameraRelative = true;
        _movingActor.MoveInput = Vector2.zero;
        _follower.enabled = false;
    }
    
    public IEnumerator FaceTarget(GameObject target)
    {
        if (target == null || !IsOwner)
            yield break;
    
        Vector3 direction = (target.transform.position - transform.position).normalized;
        _movingActor.MoveDirection = direction;
    
        if (direction != Vector3.zero)
        {
            float targetYaw = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
            
            if (targetYaw < 0)
                targetYaw += 360f;
            
            while (true)
            {
                float currentYaw = transform.eulerAngles.y;
                float angleDifference = Mathf.DeltaAngle(currentYaw, targetYaw);
                
                //Debug.Log($"Current: {currentYaw:F1}°, Target: {targetYaw:F1}°, Difference: {angleDifference:F1}°");
                if (Mathf.Abs(angleDifference) < 0.5f)
                    break;
                
                yield return null;
            }
            _movingActor.MoveDirection = Vector3.zero;
        }
        else
        {
            _movingActor.MoveDirection = Vector3.zero;
        }
    }
    
    #endregion
    
    #region Helper Methods
    
    private IEnumerator FollowPathUntilReached(float stoppingDistance)
    {
        while (!_follower.reachedDestination)
        {
            float distanceToDestination = Vector3.Distance(transform.position, _follower.destination);
            
            if (distanceToDestination <= stoppingDistance)
            {
                break;
            }
            
            yield return FollowPathStep();
        }
    }
    
    private IEnumerator FollowPathStep()
    {
        _follower.GetRemainingPath(_remainingPath, out bool _);
        
        // Debug visualization
        for (int i = 0; i < _remainingPath.Count - 1; i++)
        {
            Debug.DrawLine(_remainingPath[i], _remainingPath[i + 1], Color.red);
        }
        
        // Calculate movement direction
        if (_remainingPath.Count >= 2)
        {
            Vector3 direction = (_remainingPath[1] - _remainingPath[0]).normalized;
            _movingActor.MoveInput = new Vector2(direction.x, direction.z);
        }
        
        yield return null;
    }
    
    public void StopMovement()
    {
        _movingActor.MoveInput = Vector2.zero;
        _movingActor.MoveDirection = Vector3.zero;
        _controller.CameraRelative = true;
        if (_follower.enabled)
        {
            _follower.enabled = false;
        }
    }
    
    #endregion
    
    #region Public API
    
    public float GetDistanceToDestination()
    {
        return Vector3.Distance(transform.position, _follower.destination);
    }
    
    public float GetDistanceToTarget(GameObject target)
    {
        if (target == null) return float.MaxValue;
        return Vector3.Distance(transform.position, target.transform.position);
    }
    
    public bool IsInRange(GameObject target, float range)
    {
        return GetDistanceToTarget(target) <= range;
    }
    
    public void SetCameraRelative(bool enabled)
    {
        _controller.CameraRelative = enabled;
    }
    
    #endregion
}