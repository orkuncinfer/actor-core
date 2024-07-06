using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public enum ShowHidePanelButtonMode
{
    Show,
    Hide,
}
public class ShowHidePanelButton : MonoBehaviour, IPointerUpHandler
{
    [SerializeField] private ShowHidePanelButtonMode _mode;
    [SerializeField] private string _panelId;
    [SerializeField] private bool _showAdditive;
    public void OnPointerUp(PointerEventData eventData)
    {
        if (_mode == ShowHidePanelButtonMode.Show)
        {
            ShowPanel();
        }
        else
        {
            HidePanel();
        }
    }

    private void HidePanel()
    {
        CanvasManager.Instance.HidePanel(_panelId);
    }

    private void ShowPanel()
    {
        if (_showAdditive)
        {
            CanvasManager.Instance.ShowAdditive(_panelId);
        }
        else
        {
            CanvasManager.Instance.ShowPanel(_panelId);
        }
    }
}
