using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttachToLayerPanel : MonoBehaviour
{
    [SerializeField] private string _panelId;
    void Start()
    {
        Transform parent = CanvasManager.Instance.GetDesiredLayer("Default").GetPanelInstance(_panelId).transform;
        transform.SetParent(parent);
        transform.localPosition = Vector3.zero;
    }
}
