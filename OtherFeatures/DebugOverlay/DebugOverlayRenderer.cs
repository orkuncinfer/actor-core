using HexCore.DebugOverlay;
using Sirenix.OdinInspector;
using UnityEngine;

public class DebugOverlayRenderer : MonoBehaviour
{
    [SerializeField] private float _showDuration = 1;
    [SerializeField] private bool _enabled;
    private void Update()
    {
        if(!_enabled)return;
        DebugOverlay.Instance.Update(Time.deltaTime);
    }

    private void OnGUI()
    {
        if(!_enabled)return;
        DebugOverlay.Instance.Draw();
    }
    [Button]
    public void DebugTest(string message)
    {
        DebugOverlay.Instance.Log(message,_showDuration);
    }
}