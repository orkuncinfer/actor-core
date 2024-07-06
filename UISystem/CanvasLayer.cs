using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;

using UnityEngine;

public class CanvasLayer : MonoBehaviour
{
    [SerializeField] private GenericKey _layerTag;

    public List<PanelModel> Panels;

    [ShowInInspector] private List<PanelInstanceView> _instanceList = new List<PanelInstanceView>();

    private string _lastTriedShowPanelId;

    [ReadOnly] [SerializeField] private PanelInstanceView _currentPanel;

    [SerializeField] private bool _registerAsDefaultLayer;

    private void Start()
    {
        if (_registerAsDefaultLayer)
        {
            CanvasManager.Instance.RegisterLayer(_layerTag.ID, this,true);
        }
        else
        {
            CanvasManager.Instance.RegisterLayer(_layerTag.ID, this);
        }
      
    }

    public void ShowPanel(string panelId)
    {
        if (_currentPanel != null)
        {
            if(panelId == _currentPanel.PanelId) return;
        }
        
        _lastTriedShowPanelId = panelId;

        PanelModel panelModel = Panels.FirstOrDefault(panel => panel.PanelId == panelId);

        if (AnyPanelShowing())
        {
            _instanceList[_instanceList.Count - 1].onHideCompleted += OnLastHideCompleted;
            HideLastPanel();
            return;
        }

        if (_instanceList.Count > 0)
        {
            if (_instanceList[_instanceList.Count - 1].PanelId == panelId) return;
        }

        if (panelModel != null)
        {
            GameObject newPanelInstance =
                GOPoolProvider.Retrieve(panelModel.PanelPrefab, Vector3.zero, Quaternion.identity, transform);
            newPanelInstance.SetActive(true);
            newPanelInstance.transform.localPosition = Vector3.zero;
            PanelInstanceView instanceView = newPanelInstance.GetComponent<PanelInstanceView>();
            instanceView.PanelId = panelId;
            instanceView.PanelInstance = newPanelInstance;

            _instanceList.Add(instanceView);

            instanceView.ShowPanel();
            _currentPanel = instanceView;
        }
        else
        {
            Debug.LogWarning($"Trying to use panelId = {panelId}, but this is not found in Panels");
        }
    }

    private void OnLastHideCompleted(PanelInstanceView obj)
    {
        obj.onHideCompleted -= OnLastHideCompleted;
        ShowPanel(_lastTriedShowPanelId);
    }

    public void HideLastPanel()
    {
        if (AnyPanelShowing())
        {
            PanelInstanceView lastPanel = _instanceList[_instanceList.Count - 1];
            _instanceList.Remove(lastPanel);

            lastPanel.HidePanel();
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


    #region Additive Panels

    public void ShowAdditive(string panelId)
    {
        _lastTriedShowPanelId = panelId;

        PanelModel panelModel = Panels.FirstOrDefault(panel => panel.PanelId == panelId);

        foreach (PanelInstanceView panelInstance in CanvasManager.Instance.PanelStack)
        {
            if (panelInstance.PanelId == panelId) return;
        }

        GameObject newPanelInstance =
            GOPoolProvider.Retrieve(panelModel.PanelPrefab, Vector3.zero, Quaternion.identity, transform);
        newPanelInstance.SetActive(true);
        newPanelInstance.transform.localPosition = Vector3.zero;
        PanelInstanceView instanceView = newPanelInstance.GetComponent<PanelInstanceView>();
        instanceView.PanelId = panelId;
        instanceView.PanelInstance = newPanelInstance;

        CanvasManager.Instance.PanelStack.Push(instanceView);

        instanceView.ShowPanel();
    }

    [Button]
    public void HidePanel(string panelId)
    {
        foreach (PanelInstanceView panelInstance in CanvasManager.Instance.PanelStack)
        {
            if (panelInstance.PanelId == panelId)
            {
                CanvasManager.Instance.PanelStack.Pop();
                panelInstance.HidePanel();
                return;
            }
        }

        foreach (PanelInstanceView panelInstance in _instanceList)
        {
            if(panelInstance.PanelId == panelId)
            {
                panelInstance.HidePanel();
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
            PanelInstanceView lastPanel = CanvasManager.Instance.PanelStack.Pop();
            lastPanel.HidePanel();
        }
    }

    #endregion
}