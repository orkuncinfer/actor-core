using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InventoryDisplayTrigger : MonoBehaviour
{
    [SerializeField] private GenericKey _inventoryKey;
    [SerializeField] private PanelActor _inventoryPanel;
    [SerializeField] private InputAction _inputAction;
    
    [SerializeField] private InputActionAsset _characterActionAsset;
    private InputActionMap _playerControlMap;

    private InventoryDefinition _inventoryDefinition;
    private UI_InventoryControl _inventoryControl;

    private PanelActor _panelInstance;
    
    private bool _showing;

    private void Start()
    {
        _inputAction.Enable();
        
        if(_characterActionAsset)_playerControlMap = _characterActionAsset.FindActionMap("Player Controls");
        
        _inputAction.performed += OnInventoryDisplay;
    }

    private void OnDestroy()
    {
        _inputAction.Disable();
        _inputAction.performed -= OnInventoryDisplay;
    }

    private void OnInventoryDisplay(InputAction.CallbackContext obj)
    {
        _showing = !_showing;

        if (_showing)
        {
            if(_characterActionAsset)_playerControlMap.Disable();
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            _inventoryDefinition = DefaultPlayerInventory.Instance.GetInventoryDefinition(_inventoryKey.ID);
            GameObject panelInstance = CanvasManager.Instance.ShowAdditive(_inventoryPanel);
            _panelInstance = panelInstance.GetComponent<PanelActor>();
            _inventoryControl = panelInstance.GetComponentInChildren<UI_InventoryControl>();
            _inventoryControl.SetInventoryDefinition(_inventoryDefinition);
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            if(_characterActionAsset)_playerControlMap.Enable();
            CanvasManager.Instance.HidePanel(_panelInstance.PanelId);
        }
        
    }
}
