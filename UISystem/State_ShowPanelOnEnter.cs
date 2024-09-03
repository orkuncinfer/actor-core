using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class State_ShowPanelOnEnter : MonoState
{
    [SerializeField] private string _panelId;
    [SerializeField] private bool _continueWithoutMenu;

    protected override void OnEnter()
    {
        base.OnEnter();
        if (_continueWithoutMenu)
        {
            Owner.GetData<DS_GameModeRuntime>()._currentGameMode = GameMode.Playing;
        }
        else
        {
            CanvasManager.Instance.ShowPanel(_panelId);
        }
    }
}