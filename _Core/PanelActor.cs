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
    
    [SerializeField][HideInEditorMode]
    private List<PanelActor> _childPanels = new List<PanelActor>();

    public Canvas OwnerCanvas
    {
        get;
        set;
    }

    protected override void OnActorStart()
    {
        base.OnActorStart();
        if(_viewTransform)_viewTransform.gameObject.SetActive(false);

        foreach (var childPanel in _childPanels)
        {
            childPanel.StartIfNot();
        }
        
        if (string.IsNullOrEmpty(ParentPanelKey) == false)
        {
            CanvasManager.Instance.SetParent(this,ParentPanelKey);
        }
    }

    public void OpenPanel()
    {
        if(_viewTransform)_viewTransform.gameObject.SetActive(true);
        gameObject.SetActive(true);
        if (_openedState)
        {
            _openedState.CheckoutEnter(this);
        }

        foreach (var childPanel in _childPanels)
        {
            childPanel.OpenPanel();
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
        
        foreach (var childPanel in _childPanels)
        {
            childPanel.ClosePanel();
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
        
        foreach (var childPanel in _childPanels)
        {
            childPanel.StopIfNot();
        }
        PoolManager.ReleaseObject(gameObject);
    }

    public void SetParentOf(PanelActor panelActor)
    {
        if (_childPanels.Contains(panelActor) == false)
        {
            _childPanels.Add(panelActor);
            panelActor.transform.SetParent(transform,false);
        }
    }
}