using System;
using System.Collections;
using System.Collections.Generic;
using FishNet.Object;
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
    private UI_InventoryView _ınventoryView;

    private PanelActor _panelInstance;


#if USING_FISHNET
    private NetworkObject _networkObject;
    private bool isOwner => _networkObject.IsOwner;
#endif
    
    private bool _showing;

    private void Start()
    {
#if USING_FISHNET
        _networkObject = transform.parent.GetComponent<NetworkObject>();
#endif
        
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
#if USING_FISHNET
        if(!isOwner) return;
#endif
        _showing = !_showing;
        
        Debug.Log($"{transform.name} is now showing : {_showing}");

        if (_showing)
        {
            if(_characterActionAsset)_playerControlMap.Disable();
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            _inventoryDefinition = ActorUtilities.FindFirstActorInParents(transform).GetInventoryDefinition(_inventoryKey.ID);
            GameObject panelInstance = CanvasManager.Instance.ShowAdditive(_inventoryPanel);
            _panelInstance = panelInstance.GetComponent<PanelActor>();
            _ınventoryView = panelInstance.GetComponentInChildren<UI_InventoryView>();
            _ınventoryView.SetInventoryDefinition(_inventoryDefinition);
        }
        else
        {
            if(_characterActionAsset)_playerControlMap.Enable();
            CanvasManager.Instance.HidePanel(_panelInstance.PanelId);
        }
        
    }
}
