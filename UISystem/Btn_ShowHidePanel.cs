using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public enum ShowHidePanelButtonMode
{
    Show,
    Hide,
}
public class Btn_ShowHidePanel : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private ShowHidePanelButtonMode _mode;
    [SerializeField] private string _panelId;
    [SerializeField] private bool _showAdditive;
    [SerializeField] private bool _parentToFirstActor;

    private void HidePanel()
    {
        CanvasManager.Instance.HidePanel(_panelId);
    }

    private void ShowPanel()
    {
        GameObject instance = null;
        if (_showAdditive)
        {
            instance = CanvasManager.Instance.ShowAdditive(_panelId);
        }
        else
        {
            instance = CanvasManager.Instance.ShowPanel(_panelId);
        }
        Debug.Log("instance is " + instance);
        if(_parentToFirstActor)
        {
            ActorBase actor = ActorUtilities.FindFirstActorInParents(transform);
            Debug.Log("parented0 " + actor);
            if (instance != null && actor != null)
            {
                Debug.Log("parented " + instance);
                instance.transform.SetParent(actor.transform);
            }
        }
    }

    public void OnPointerClick(PointerEventData eventData)
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
}
