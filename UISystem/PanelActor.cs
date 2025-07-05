using System;
using System.Collections;
using System.Collections.Generic;
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

    [Tooltip("Set this if you want this panel to be parented on show")]
    public string ParentPanelKey;

    [ShowInInspector]public bool IsShowing { get; set; }
    public Canvas OwnerCanvas
    {
        get;
        set;
    }

    protected override void OnActorStart()
    {
        base.OnActorStart();
        if(_viewTransform)_viewTransform.gameObject.SetActive(false);

        if (string.IsNullOrEmpty(ParentPanelKey) == false)
        {
            CanvasManager.Instance.RegisterPanelRelation(this,ParentPanelKey);
        }
        
        if (CanvasManager.Instance.TryGetRelatedPanels(PanelId, out List<PanelActor> childPanels))
        {
            foreach (var panel in childPanels)
            {
                panel.transform.SetParent(transform,false);
                panel.OwnerCanvas = OwnerCanvas;
                panel.StartIfNot();
            }
        }
    }

    public void OpenPanel()
    {
        if(IsShowing) return;
        
        if(_viewTransform)_viewTransform.gameObject.SetActive(true);
        gameObject.SetActive(true);
        if (_openedState)
        {
            _openedState.CheckoutEnter(this);
        }

        if (CanvasManager.Instance.TryGetRelatedPanels(PanelId, out List<PanelActor> childPanels))
        {
            foreach (var panel in childPanels)
            {
                panel.OpenPanel();
            }
        }
        
        onShowCompleted?.Invoke(this);
        onShowComplete?.Invoke();
        IsShowing = true;
    }

    public void ClosePanel()
    {
        if(!IsShowing) return;
        
        onHideCompleted?.Invoke(this);
        onHideComplete?.Invoke();
        if(_viewTransform)_viewTransform.gameObject.SetActive(false);
        if (_openedState)
        {
            _openedState.CheckoutExit();
        }
        
        if (CanvasManager.Instance.TryGetRelatedPanels(PanelId, out List<PanelActor> childPanels))
        {
            foreach (var panel in childPanels)
            {
                panel.ClosePanel();
            }
        }

        IsShowing = false;
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
        
        if (CanvasManager.Instance.TryGetRelatedPanels(PanelId, out List<PanelActor> childPanels))
        {
            foreach (var panel in childPanels)
            {
                panel.StopIfNot();
            }
        }
        
        PoolManager.ReleaseObject(gameObject);
    }

    private void OnDestroy()
    {
        if(!Application.isPlaying) return;
        if (string.IsNullOrEmpty(ParentPanelKey) == false)
        {
            CanvasManager.Instance.UnregisterPanelRelation(this,ParentPanelKey);
        }
    }

    public void SetParentOf(PanelActor panelActor)
    {
        /*if (_childPanels.Contains(panelActor) == false)
        {
            _childPanels.Add(panelActor);
            panelActor.transform.SetParent(transform,false);
        }*/
    }
}