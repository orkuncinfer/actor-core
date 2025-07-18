using UnityEngine;

public class ClearTrailRenderer : MonoBehaviour
{
    TrailRenderer _trailRenderer;
    private void Awake()
    {
        _trailRenderer = GetComponent<TrailRenderer>();
    }
    private void OnEnable()
    {
        _trailRenderer.Clear();
    }
}
