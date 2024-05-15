using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PathFollower : MonoBehaviour
{
    private float elapsedTime = 0f;
    private float totalTime;
    private Vector3[] _pathArray;
    public bool IsMoving;

    private Vector3 _endPosition;
    private bool _initialized;
    public bool Initialized => _initialized;
    
    public UnityEvent OnPathEnd;
    public UnityEvent OnPathStart;
    
    public void Initialize(Vector3[] pathArray, float totalTime, Vector3 pos = default)
    {
        this.totalTime = totalTime;
        _pathArray = pathArray;
        IsMoving = true;
        _initialized = true;
        transform.position = pathArray[0];
        _endPosition = pos;
    }

    private void Update()
    {
        if (_initialized && IsMoving)
        {
            MoveAlongPath();
        }
    }
    
    void MoveAlongPath()
    {
        elapsedTime += Time.deltaTime;
        float t = elapsedTime / totalTime;

        if (t > 1f)
        {
            End();
            return;
        }

        int positionIndex = Mathf.FloorToInt(t * (_pathArray.Length - 1));
        Vector3 currentPosition = _pathArray[positionIndex];
        Vector3 nextPosition = _pathArray[Mathf.Min(positionIndex + 1, _pathArray.Length - 1)];
        Vector3 newPosition = Vector3.Lerp(currentPosition, nextPosition, (t * (_pathArray.Length - 1)) % 1);
        transform.position = newPosition;
        transform.LookAt(nextPosition);

        if (t >= 1f)
        {
            End();
        }
    }
    
    private void End()
    {
        IsMoving = false;
        OnPathEnd?.Invoke();
        transform.eulerAngles = Vector3.zero;
        transform.position = _endPosition;
    }

}
