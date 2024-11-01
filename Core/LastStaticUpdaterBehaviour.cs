using UnityEngine;

public class LastStaticUpdaterBehaviour : MonoBehaviour
{
    void Update()
    {
        LastStaticUpdater.Update();
    }
    
    void LateUpdate()
    {
        LastStaticUpdater.LateUpdate();
    }
    
    void FixedUpdate()
    {
        LastStaticUpdater.FixedUpdate();
    }
}