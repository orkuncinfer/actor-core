using System;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

public class PanelActor : ActorBase
{
    [BoxGroup("PanelSettings")][SerializeField] private MonoState _openedState;
    [BoxGroup("PanelSettings")][SerializeField] private Transform _viewTransform;
    
    [BoxGroup("PanelSettings")][ReadOnly]public string PanelId;
    [BoxGroup("PanelSettings")][ReadOnly]public GameObject PanelInstance;
    private CanvasGroup _canvasGroup;
    public event Action<PanelInstanceView> onHideCompleted;
    public event Action<PanelInstanceView> onShowCompleted;
    
    [BoxGroup("PanelSettings")]public bool FadeOutOnHide = true;
    [BoxGroup("PanelSettings")]public bool FadeInOnShow = true;

    protected override void OnActorStart()
    {
        base.OnActorStart();
        _viewTransform.gameObject.SetActive(false);
    }

    public void OpenPanel()
    {
        _viewTransform.gameObject.SetActive(true);
        if (_openedState)
        {
            _openedState.CheckoutEnter(this);
        }
    }

    public void ClosePanel()
    {
        _viewTransform.gameObject.SetActive(false);
        if (_openedState)
        {
            _openedState.CheckoutExit();
        }
    }

    protected override void OnActorStop()
    {
        base.OnActorStop();
        if (_openedState)
        {
            if (_openedState.IsRunning)
            {
                _openedState.CheckoutExit();
            }
        }

        PoolManager.ReleaseObject(gameObject);
    }
}