using System;
using UnityEngine;

public class LateFollower : MonoBehaviour
{
    public Transform Follow;
    public Vector3 Offset;
    private void LateUpdate()
    {
        transform.position = Follow.transform.position + Offset;
    }
}
