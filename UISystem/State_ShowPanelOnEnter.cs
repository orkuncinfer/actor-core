using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class State_ShowPanelOnEnter : MonoState
{
    [SerializeField] private string _panelId;

    protected override void OnEnter()
    {
        base.OnEnter();
        CanvasLayer.Instance.ShowPanel(_panelId);
    }
}