using UnityEngine;
using UnityEngine.EventSystems;

public class HideLastPanelButton : MonoBehaviour, IPointerUpHandler
{
    public void OnPointerUp(PointerEventData eventData)
    {
        CanvasManager.Instance.HideLastPanel();
    }
}