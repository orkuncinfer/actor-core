using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;

using UnityEngine;

[RequireComponent(typeof(Canvas))]
public class CanvasLayer : MonoBehaviour
{
    [SerializeField] private GenericKey _layerTag;

    public List<PanelModel> Panels;

    [ShowInInspector] private List<PanelActor> _instanceList = new List<PanelActor>();

    private string _lastTriedShowPanelId;

    [ReadOnly] [SerializeField] private PanelActor _currentPanel;

    [SerializeField] private bool _registerAsDefaultLayer;
    
    private Canvas _canvas;

    private void Start()
    {
        _canvas = GetComponent<Canvas>();
        
        if (_registerAsDefaultLayer)
        {
            CanvasManager.Instance.RegisterLayer(_layerTag.ID, this,true);
        }
        else
        {
            CanvasManager.Instance.RegisterLayer(_layerTag.ID, this);
        }
      
    }

    public GameObject ShowPanel(string panelId)
    {
        if (_currentPanel != null)
        {
            if(panelId == _currentPanel.PanelId) return _currentPanel.gameObject;
        }
        
        _lastTriedShowPanelId = panelId;

        PanelModel panelModel = Panels.FirstOrDefault(panel => panel.PanelId == panelId);

        if (AnyPanelShowing())
        {
            _instanceList[_instanceList.Count - 1].onHideCompleted += OnLastHideCompleted;
            HideLastPanel();
            return null;
        }

        if (_instanceList.Count > 0)
        {
            if (_instanceList[_instanceList.Count - 1].PanelId == panelId) return _instanceList[_instanceList.Count - 1].gameObject;
        }

        if (panelModel != null)
        {
            GameObject newPanelInstance =
                PoolManager.SpawnObject(panelModel.PanelPrefab, Vector3.zero, Quaternion.identity, transform);
            newPanelInstance.SetActive(true);
            newPanelInstance.transform.localPosition = Vector3.zero;
            PanelActor instanceView = newPanelInstance.GetComponent<PanelActor>();
            instanceView.PanelId = panelId;
            instanceView.PanelInstance = newPanelInstance;
            instanceView.OwnerCanvas = _canvas;
            
            _instanceList.Add(instanceView);
            if (instanceView.transform.TryGetComponent(out PanelActor panelActor))
            {
                panelActor.StartIfNot();
            }
            instanceView.OpenPanel();
            _currentPanel = instanceView;
            return newPanelInstance;
        }
        else
        {
            Debug.LogWarning($"Trying to use panelId = {panelId}, but this is not found in Panels");
        }
        return null;
    }

    private void OnLastHideCompleted(PanelActor obj)
    {
        obj.onHideCompleted -= OnLastHideCompleted;
        ShowPanel(_lastTriedShowPanelId);
    }

    public void HideLastPanel()
    {
        if (AnyPanelShowing())
        {
            PanelActor lastPanel = _instanceList[_instanceList.Count - 1];
            _instanceList.Remove(lastPanel);

            lastPanel.ClosePanel();
        }
    }

    public bool AnyPanelShowing()
    {
        return GetAmountPanelsInList() > 0;
    }

    public int GetAmountPanelsInList()
    {
        return _instanceList.Count;
    }
    
    public PanelActor GetPanelInstance(string panelId)
    {
        return _instanceList.FirstOrDefault(panel => panel.PanelId == panelId);
    }


    #region Additive Panels

    public GameObject ShowAdditive(string panelId)
    {
        _lastTriedShowPanelId = panelId;

        PanelModel panelModel = Panels.FirstOrDefault(panel => panel.PanelId == panelId);

        foreach (PanelActor panelInstance in CanvasManager.Instance.PanelStack)
        {
            if (panelInstance.PanelId == panelId) return null;
        }

        GameObject newPanelInstance =
            PoolManager.SpawnObject(panelModel.PanelPrefab, Vector3.zero, Quaternion.identity, transform);
        newPanelInstance.SetActive(true);
        newPanelInstance.transform.localPosition = Vector3.zero;
        PanelActor instanceView = newPanelInstance.GetComponent<PanelActor>();
        instanceView.PanelId = panelId;
        instanceView.PanelInstance = newPanelInstance;
        instanceView.OwnerCanvas = _canvas;
        
        CanvasManager.Instance.PanelStack.Push(instanceView);
        if (instanceView.transform.TryGetComponent(out PanelActor panelActor))
        {
            panelActor.StartIfNot();
        }
        instanceView.OpenPanel();
        return newPanelInstance;
    }
    
    public GameObject ShowAdditive(PanelActor inPanelActor)
    {
        _lastTriedShowPanelId = inPanelActor.PanelId;
        
        
        foreach (PanelActor panelInstance in CanvasManager.Instance.PanelStack)
        {
            if (panelInstance.PanelId == inPanelActor.PanelId) return null;
        }
        GameObject newPanelInstance =
            PoolManager.SpawnObject(inPanelActor.gameObject, Vector3.zero, Quaternion.identity, transform);
        newPanelInstance.SetActive(true);
        newPanelInstance.transform.localPosition = Vector3.zero;
        PanelActor instanceView = newPanelInstance.GetComponent<PanelActor>();
        instanceView.PanelId = instanceView.name + ":" + newPanelInstance.gameObject.GetInstanceID();
        instanceView.PanelInstance = newPanelInstance;
        instanceView.OwnerCanvas = _canvas;
        
        CanvasManager.Instance.PanelStack.Push(instanceView);
        if (instanceView.transform.TryGetComponent(out PanelActor panelActorInstance))
        {
            panelActorInstance.StartIfNot();
        }
        instanceView.OpenPanel();
        
        if(Panels.FirstOrDefault(panel => panel.PanelId == instanceView.PanelId) == null)
        {
            PanelModel newPanelModel = new PanelModel();
            newPanelModel.PanelId = instanceView.PanelId;
            newPanelModel.PanelPrefab = inPanelActor.gameObject;
        
            Panels.Add(newPanelModel);
        }
        
        return newPanelInstance;
    }

    [Button]
    public void HidePanel(string panelId)
    {
        foreach (PanelActor panelInstance in CanvasManager.Instance.PanelStack)
        {
            if (panelInstance.PanelId == panelId)
            {
                CanvasManager.Instance.PanelStack.Pop();
                panelInstance.ClosePanel();
                return;
            }
        }

        foreach (PanelActor panelInstance in _instanceList)
        {
            if(panelInstance.PanelId == panelId)
            {
                panelInstance.ClosePanel();
                _instanceList.Remove(panelInstance);
                return;
            }
        }
    }
    [Button]
    public void HideLastAdditive()
    {
        if (CanvasManager.Instance.PanelStack.Count > 0)
        {
            PanelActor lastPanel = CanvasManager.Instance.PanelStack.Pop();
            lastPanel.ClosePanel();
        }
    }

    #endregion
}