using Pathfinding;
using UnityEngine;

public class State_AIMoveInput : MonoState
{
    public float EndReachDistance;
    public float LookSpeed;
    public float LookThreshold = 0.95f; // How close the AI should be facing the target before moving

    private DS_MovingActor _movingActor;
    private AIPath _aiPath;
    private Seeker _seeker;
    private Vector3 _desiredMoveDirection = Vector3.zero; // Stores the intended movement direction

    private Transform _movementTransform => Owner.transform;

    protected override void OnEnter()
    {
        base.OnEnter();
        _movingActor = Owner.GetData<DS_MovingActor>();

        _seeker = Owner.GetComponentInChildren<Seeker>();
        _aiPath = Owner.GetComponentInChildren<AIPath>();
        
        _seeker.pathCallback += PathCallback;
    }

    protected override void OnExit()
    {
        base.OnExit();
        _seeker.pathCallback -= PathCallback;
    }

    protected override void OnUpdate()
    {
        base.OnUpdate();

        if (_desiredMoveDirection.sqrMagnitude > 0.01f) // Ensure there's a target direction to look at
        {
            // Smoothly interpolate LookDirection toward the desired movement direction using Slerp
            _movingActor.LookDirection = Vector3.Slerp(
                _movingActor.LookDirection, 
                _desiredMoveDirection, 
                LookSpeed * Time.deltaTime
            );

            // Check if the AI is facing the correct direction before moving
            float alignment = Vector3.Dot(_movingActor.LookDirection.normalized, _desiredMoveDirection.normalized);

            if (alignment >= LookThreshold) // Only move if facing the correct direction
            {
                _movingActor.MoveInput = new Vector2(_desiredMoveDirection.x, _desiredMoveDirection.z);
            }
            else
            {
                _movingActor.MoveInput = Vector2.zero; // Stop moving until facing the correct way
            }
        }
        else
        {
            _movingActor.MoveInput = Vector2.zero; // Stop moving if no path is set
        }
    }

    private void PathCallback(Path p)
    {
        float distance = Vector3.Distance(_movementTransform.position, _aiPath.destination);

        if (p.vectorPath.Count > 0 && distance > EndReachDistance && p.vectorPath.Count > 1)
        {
            // Calculate the next movement direction
            Vector3 nextPoint = (p.vectorPath[1] - _movementTransform.position).normalized;

            _desiredMoveDirection = new Vector3(nextPoint.x, 0, nextPoint.z); // Store for aiming first
        }
        else
        {
            _desiredMoveDirection = Vector3.zero;
        }
    }
}
