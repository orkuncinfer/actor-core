using System;
using System.Collections;
using System.Collections.Generic;
using Pathfinding;
using UnityEngine;

public class PathDestination : MonoBehaviour
{
    [SerializeField]private Vector3 _destination;

    private AIPath _aiPath;
    public Vector3 Destination
    {
        get => _destination;
        set
        {
            if (_destination == value) return;
            _destination = value;
            isChanged = true;
        }
    }
    private bool isChanged;

    private void Start()
    {
        _aiPath = GetComponent<AIPath>();
    }

    private void Update()
    {
        if (!isChanged)
        {
            _destination = transform.position;
        }

        _aiPath.destination = Destination;
    }
}
