using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlwaysFaceCamera : MonoBehaviour
{
    private Camera _camera;

    private void Start()
    {
        _camera = Camera.main;
        transform.forward = _camera.transform.forward;
    }

    private void Update()
    {
        transform.forward = _camera.transform.forward;
    }
}
