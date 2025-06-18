using System.Collections.Generic;
using FishNet;
using FishNet.Object;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;



public class Network_SetTarget : NetworkBehaviour
{
    private Camera _camera;
    private ActorBase _actor;
    private Service_GAS _abilitySystem;
    
    public LayerMask TargetsMask;

    public GameObject HoverFx;
    public GameObject SelectedFx;

    private GameObject _hoverFxInstance;
    private GameObject _selectedFxInstance;

    private GameObject _hoveringObject;
    private ActorBase _selectedActor;    
    public override void OnStartNetwork()
    {
        base.OnStartNetwork();
        _camera = Camera.main;
        _actor = GetComponent<ActorBase>();
        _abilitySystem = _actor.GetService<Service_GAS>();
        
        _hoverFxInstance = Instantiate(HoverFx);
        _hoverFxInstance.SetActive(false);
        _selectedFxInstance = Instantiate(SelectedFx);
        _selectedFxInstance.SetActive(false);
    }

    void Update()
    {
        if(!IsOwner) return;
        
        // Use the efficient UI check
        if(UIRaycastHelper.IsPointerOverUI()) return;
        
        if (_camera.pixelRect.Contains(Input.mousePosition) && 
            Physics.Raycast(_camera.ScreenPointToRay(Input.mousePosition), out var hit, Mathf.Infinity, TargetsMask)) 
        {
            if (_hoveringObject != hit.collider.gameObject)
            {
                Transform rootSocket = hit.collider.gameObject.GetComponent<ActorBase>().GetSocket("root");
                if(rootSocket == null) return;
                HoverOver(rootSocket);
            }
            if (Mouse.current.leftButton.wasPressedThisFrame || Mouse.current.rightButton.wasPressedThisFrame)
            {
                if (hit.collider.transform.TryGetComponent(out NetworkObject networkObject))
                {
                    if (_abilitySystem != null)
                    {
                        _abilitySystem.AbilityController.Target = networkObject.gameObject;
                    }
                    Transform rootSocket = hit.collider.gameObject.GetComponent<ActorBase>().GetSocket("root");
                    if(rootSocket == null) return;
                    SelectOver(rootSocket);
                    SetTarget(networkObject.ObjectId);
                   
                }
                else
                {
                    SelectOver(null);
                    SetTarget(0);
                }
            }
        }
        else
        {
            HoverOver(null);
        }
    }

    private void HoverOver(Transform target)
    {
        if (_hoverFxInstance == null) _hoverFxInstance = Instantiate(HoverFx);
        if (target != null)
        {
            _hoveringObject = target.gameObject;
            _hoverFxInstance.transform.SetParent(target);
            _hoverFxInstance.transform.localPosition= Vector3.zero;
            _hoverFxInstance.SetActive(true);
        }
        else
        {
            _hoveringObject = null;
            _hoverFxInstance.transform.SetParent(null);
            _hoverFxInstance.SetActive(false);
        }
    }
    private void SelectOver(Transform target)
    {
        if (_selectedFxInstance == null) _selectedFxInstance = Instantiate(SelectedFx);
        if (target != null)
        {
            _hoverFxInstance.transform.SetParent(null);
            _hoverFxInstance.SetActive(false);
            _selectedFxInstance.transform.SetParent(target);
            _selectedFxInstance.transform.localPosition = Vector3.zero;
            _selectedFxInstance.SetActive(true);
        }
        else
        {
            _selectedFxInstance.transform.SetParent(null);
            _selectedFxInstance.SetActive(false);
        }
    }
  

    [ServerRpc]
    private void SetTarget(int connectionId)
    {
        if (connectionId == 0)
        {
            _abilitySystem.AbilityController.Target = null;
        }
        if (InstanceFinder.ServerManager.Objects.Spawned.TryGetValue(connectionId, out NetworkObject networkObject))
        {
            if (_abilitySystem != null)
            {
                _abilitySystem.AbilityController.Target = networkObject.gameObject;
                Debug.Log($"Selected target is {networkObject.gameObject.name} for player: {gameObject.name}");
            }
        }
    }
}
// Static helper class to avoid allocations
public static class UIRaycastHelper
{
    private static readonly PointerEventData _pointerEventData = new PointerEventData(EventSystem.current);
    private static readonly List<RaycastResult> _raycastResults = new List<RaycastResult>(10);
    
    public static bool IsPointerOverUI()
    {
        if (EventSystem.current == null)
            return false;
            
        _pointerEventData.position = Input.mousePosition;
        
        // Clear previous results without deallocating the list
        _raycastResults.Clear();
        
        EventSystem.current.RaycastAll(_pointerEventData, _raycastResults);
        
        bool isOverUI = _raycastResults.Count > 0;
        
        // Optional: Clear results to avoid holding references
        _raycastResults.Clear();
        
        return isOverUI;
    }
}