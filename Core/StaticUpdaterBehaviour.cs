using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StaticUpdaterBehaviour : MonoBehaviour
{
    void Update()
    {
        StaticUpdater.Update();
    }
    
    void LateUpdate()
    {
        StaticUpdater.LateUpdate();
    }
    
    void FixedUpdate()
    {
        StaticUpdater.FixedUpdate();
    }
}
