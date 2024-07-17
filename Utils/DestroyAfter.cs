using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyAfter : MonoBehaviour
{
    public float TimeToDestroy = 1f;
    // Start is called before the first frame update
    void Start()
    {
        Destroy(gameObject, TimeToDestroy);
    }
}
