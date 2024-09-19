using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DefaultInventoryDebugTrigger : MonoBehaviour
{
    public GameObject DebuggerPrefab;
    private GameObject _spawnedPrefab;
    public float clickThreshold = 1.0f; // Time in seconds within which the clicks must occur
    private int clickCount = 0;
    private float lastClickTime = 0f;

    void Update()
    {
        if (Input.GetMouseButtonDown(1)) // Right mouse button
        {
            float currentTime = Time.time;

            if (currentTime - lastClickTime <= clickThreshold)
            {
                clickCount++;
            }
            else
            {
                clickCount = 1; // Reset count if time threshold is exceeded
            }

            lastClickTime = currentTime;

            if (clickCount == 3)
            {
                TriggerDebugger();
                clickCount = 0; // Reset count after triggering
            }
        }
    }

    private void TriggerDebugger()
    {
        if (_spawnedPrefab != null) return;
        
        _spawnedPrefab = Instantiate(DebuggerPrefab);
    }
}
