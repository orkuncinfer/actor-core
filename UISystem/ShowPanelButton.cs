using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ShowPanelButton : MonoBehaviour, IPointerUpHandler
{
    [SerializeField] private string _panelId;
    public void OnPointerUp(PointerEventData eventData)
    {
        CanvasLayer.Instance.ShowPanel(_panelId);
    }
}
