using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleReturnPoolOnStop : MonoBehaviour
{
    public void OnParticleSystemStopped()
    {
        PoolManager.ReleaseObject(gameObject);
    }
}
