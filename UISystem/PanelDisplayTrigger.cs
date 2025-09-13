using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PanelDisplayTrigger : MonoBehaviour
{
    [SerializeField] private InputAction _showAction;
    [SerializeField] private InputAction _hideAction;

    [SerializeField] private string panelKey;
    private void Start()
    {
        _showAction.Enable();
        _hideAction.Enable();
        
        _showAction.performed += OnShowPerformed;
        _hideAction.performed += OnHidePerformed;
    }

    private void OnShowPerformed(InputAction.CallbackContext obj)
    {
        CanvasManager.Instance.ShowPanel(panelKey);
    }

    private void OnHidePerformed(InputAction.CallbackContext obj)
    {
        CanvasManager.Instance.HidePanel(panelKey);
    }
}
