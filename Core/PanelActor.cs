using System;
using System.Collections;
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
    public event Action<PanelActor> onHideCompleted;
    public event Action<PanelActor> onShowCompleted;
    
    public UnityEvent onShowComplete;
    public UnityEvent onHideComplete;
    
    [BoxGroup("PanelSettings")]public bool FadeOutOnHide = true;
    [BoxGroup("PanelSettings")]public bool FadeInOnShow = true;
    

    protected override void OnActorStart()
    {
        base.OnActorStart();
        if(_viewTransform)_viewTransform.gameObject.SetActive(false);
    }

    public void OpenPanel()
    {
        if(_viewTransform)_viewTransform.gameObject.SetActive(true);
        gameObject.SetActive(true);
        if (_openedState)
        {
            Debug.Log("31-  "+ gameObject.activeInHierarchy);
            _openedState.CheckoutEnter(this);
        }
        onShowCompleted?.Invoke(this);
        onShowComplete?.Invoke();
    }

    public void ClosePanel()
    {
        onHideCompleted?.Invoke(this);
        onHideComplete?.Invoke();
        if(_viewTransform)_viewTransform.gameObject.SetActive(false);
        if (_openedState)
        {
            _openedState.CheckoutExit();
        }
        
        PoolManager.ReleaseObject(this.gameObject,false);
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