using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

public class CanvasLayer : Singleton<CanvasLayer>
{
    [SerializeField] private GenericKey _layerTag;

    public List<PanelModel> Panels;

    [ShowInInspector] private List<PanelInstanceModel> _instanceList = new List<PanelInstanceModel>();

    private string _lastTriedShowPanelId;

    [ShowInInspector] private Stack<PanelInstanceModel> _panelStack = new Stack<PanelInstanceModel>();

    [ReadOnly] [SerializeField] private PanelInstanceModel _currentPanel;

    public void ShowPanel(string panelId)
    {
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
            newPanelInstance.transform.localPosition = Vector3.zero;
            PanelInstanceModel instanceModel = newPanelInstance.GetComponent<PanelInstanceModel>();
            instanceModel.PanelId = panelId;
            instanceModel.PanelInstance = newPanelInstance;

            _instanceList.Add(instanceModel);

            instanceModel.ShowPanel();
            _currentPanel = instanceModel;
        }
        else
        {
            Debug.LogWarning($"Trying to use panelId = {panelId}, but this is not found in Panels");
        }
    }

    private void OnLastHideCompleted(PanelInstanceModel obj)
    {
        obj.onHideCompleted -= OnLastHideCompleted;
        ShowPanel(_lastTriedShowPanelId);
    }

    public void HideLastPanel()
    {
        if (AnyPanelShowing())
        {
            PanelInstanceModel lastPanel = _instanceList[_instanceList.Count - 1];
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

        foreach (PanelInstanceModel panelInstance in _panelStack)
        {
            if (panelInstance.PanelId == panelId) return;
        }

        GameObject newPanelInstance =
            GOPoolProvider.Retrieve(panelModel.PanelPrefab, Vector3.zero, Quaternion.identity, transform);
        newPanelInstance.transform.localPosition = Vector3.zero;
        PanelInstanceModel instanceModel = newPanelInstance.GetComponent<PanelInstanceModel>();
        instanceModel.PanelId = panelId;
        instanceModel.PanelInstance = newPanelInstance;

        _panelStack.Push(instanceModel);

        instanceModel.ShowPanel();
    }

    [Button]
    public void HidePanel(string panelId)
    {
        foreach (PanelInstanceModel panelInstance in _panelStack)
        {
            if (panelInstance.PanelId == panelId)
            {
                _panelStack.Pop();
                panelInstance.HidePanel();
                return;
            }
        }

        foreach (PanelInstanceModel panelInstance in _instanceList)
        {
            if(panelInstance.PanelId == panelId)
            {
                panelInstance.HidePanel();
                _instanceList.Remove(panelInstance);
                return;
            }
        }
    }

    #endregion
}