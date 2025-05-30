using HexCore.DebugOverlay;
using Sirenix.OdinInspector;
using UnityEngine;

public class DebugOverlayRenderer : MonoBehaviour
{
    [SerializeField] private float _showDuration = 1;
    private void Update()
    {
        DebugOverlay.Instance.Update(Time.deltaTime);
    }

    private void OnGUI()
    {
        DebugOverlay.Instance.Draw();
    }
    [Button]
    public void DebugTest(string message)
    {
        DebugOverlay.Instance.Log(message,_showDuration);
    }
}