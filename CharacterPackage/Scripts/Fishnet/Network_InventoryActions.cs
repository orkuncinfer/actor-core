using System.Collections.Generic;
using FishNet.Object;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

public class Network_InventoryActions : NetworkBehaviour
{
    [SerializeField] private EventField<InventoryActionArgs> _inventoryActionEvent;

    [SerializeField] private GenericKey _equipmentInventoryKey;
    [SerializeField] private GenericKey _playerInventoryKey;

    [ShowInInspector,ReadOnly] private GameObject _hoveredItemObject;
    

    private Actor _actorBase;

    #region ACTION DETECTION FIELDS

    private PointerEventData pointerEventData;
    private EventSystem eventSystem;
    private List<RaycastResult> _raycastResults = new List<RaycastResult>();
    private bool _pointerDown;
    private Vector2 _pointerDownPosition;
    public bool _draggingItem;
    [SerializeField] private GameObject _ghostUIInventoryItemElementPrefab;
    [ReadOnly] public UI_InventoryItemElement _draggedUIInventoryItem;
    [ReadOnly] public UI_InventoryItemElement _hoveredItemElement;
    private GameObject _draggingGhostItemInstance;

    #endregion

    public override void OnStartNetwork()
    {
        base.OnStartNetwork();
        _actorBase = GetComponent<Actor>();
        _inventoryActionEvent.Register(_actorBase, OnInventoryAction);
    }

    private void OnInventoryAction(EventArgs arg1, InventoryActionArgs arg2)
    {
        RpcSwap(arg2);
    }

    [ServerRpc]
    private void RpcSwap(InventoryActionArgs actionArgs)
    {
        if (!IsServer) return;
        InventoryDefinition fromInventory = _actorBase.GetInventoryDefinition(actionArgs.FromInventoryId);
        InventoryDefinition targetInventory = null;
        ItemData itemData = fromInventory.InventoryData.InventorySlots[actionArgs.FromInventorySlotIndex].ItemData;
        if (itemData == null) return;
        ItemDefinition itemDefinition = InventoryUtils.FindItemDefinitionWithId(itemData.ItemID);
        if (itemDefinition == null) return;

        if (!string.IsNullOrEmpty(actionArgs.ToInventoryId))
        {
            targetInventory = _actorBase.GetInventoryDefinition(actionArgs.ToInventoryId);
        }
        
        Debug.Log("interact item 0");

        #region INTERACT

        if (actionArgs.InteractItem) // right click or use
        {
            #region INTERACT EQUIPMENT INVENTORY

            if (actionArgs.FromInventoryId == _equipmentInventoryKey.ID) // right click on an item in equipments.
            {
                InventoryDefinition playerInventory =
                    fromInventory.Owner.GetInventoryDefinition(_playerInventoryKey.ID);
                if (playerInventory.HaveSpace(itemData, out int slotIndex1)) // UNeqyip directly
                {
                    playerInventory.InventoryData.InventorySlots[slotIndex1].ItemData = itemData;
                    fromInventory.InventoryData.InventorySlots[actionArgs.FromInventorySlotIndex].ItemData = null;
                    playerInventory.TriggerChanged();
                    fromInventory.TriggerChanged();
                    return;
                }

                return;
            }else if (actionArgs.FromInventoryId == _playerInventoryKey.ID)
            {
                InventoryDefinition equipmentInventory =
                    fromInventory.Owner.GetInventoryDefinition(_equipmentInventoryKey.ID);
                if (equipmentInventory.HaveSpace(itemData, out int slotIndex1)) // UNeqyip directly
                {
                    equipmentInventory.InventoryData.InventorySlots[slotIndex1].ItemData = itemData;
                    fromInventory.InventoryData.InventorySlots[actionArgs.FromInventorySlotIndex].ItemData = null;
                    equipmentInventory.TriggerChanged();
                    fromInventory.TriggerChanged();
                    return;
                }
            }

            #endregion

            if (itemDefinition.TryGetData(out Data_Equippable dataEquippable)) // try equip
            {
                InventoryDefinition equipmentInventory =
                    fromInventory.Owner.GetInventoryDefinition(_equipmentInventoryKey.ID);
                if (equipmentInventory == null) return;
                Debug.Log("interact item 1");
                if (equipmentInventory.HaveSpace(itemData, out int slotIndex1)) // eqyip directly
                {
                    equipmentInventory.InventoryData.InventorySlots[slotIndex1].ItemData = itemData;
                    fromInventory.InventoryData.InventorySlots[actionArgs.FromInventorySlotIndex].ItemData = null;
                    equipmentInventory.TriggerChanged();
                    fromInventory.TriggerChanged();
                    return;
                }
                else
                {
                    Debug.Log("interact item 2");
                    if (fromInventory.HaveSpace(equipmentInventory.InventoryData.InventorySlots[slotIndex1].ItemData,
                            out int slotIndex2))
                    {
                        fromInventory.SwapItems(fromInventory.InventoryData.InventorySlots[slotIndex2],
                            equipmentInventory.InventoryData.InventorySlots[slotIndex1]);
                        equipmentInventory.TriggerChanged();
                        fromInventory.TriggerChanged();
                    }
                }
            }
        }

        #endregion

        #region DRAGGED

        else // dragged action
        {
            Debug.Log("dragged action 0 : " + actionArgs.FromInventoryId + " - " + actionArgs.ToInventoryId);
            if (string.IsNullOrEmpty(actionArgs.ToInventoryId)) // drop item
            {
                Debug.Log("dragged action 1");
                fromInventory.RemoveEntireSlot(
                    fromInventory.InventoryData.InventorySlots[actionArgs.FromInventorySlotIndex]);
            }
            else
            {
                if (actionArgs.FromInventoryId == actionArgs.ToInventoryId) // swap in same inventory
                {
                    fromInventory.SwapOrMergeItems(actionArgs.FromInventorySlotIndex, actionArgs.ToInventorySlotIndex);
                }
                else
                {
                    if (targetInventory != null)
                    {
                        Debug.Log("To another inventory");
                        fromInventory.SwapOrMergeItems(actionArgs.FromInventorySlotIndex, actionArgs.ToInventorySlotIndex, targetInventory);
                        targetInventory.TriggerChanged();
                    }
                }
            }
        }

        #endregion
    }

    #region ACTION DETECTION

    private void Awake()
    {
        eventSystem = EventSystem.current;
    }

    #region READ INPUTS

    void Update()
    {
        if (!base.IsOwner) return;
        // Check what UI object is under the mouse
        GameObject objectUnderMouse = GetUIObjectUnderMouse();
        if (objectUnderMouse != _hoveredItemObject) // hovering object changed
        {
            if (_hoveredItemElement != null)
            {
                _hoveredItemElement.SetHovered(false);
                _hoveredItemElement.SetActionBlocked(false);
            }

            if (objectUnderMouse != null)
            {
                if (objectUnderMouse.transform.TryGetComponent(out UI_InventoryItemElement itemElement))
                {
                    _hoveredItemElement = itemElement;
                }
                else
                {
                    _hoveredItemElement = null;
                }
            }
        }
        _hoveredItemObject = objectUnderMouse;

        if (Input.GetMouseButtonDown(0))
        {
            _pointerDown = true;
            _pointerDownPosition = Input.mousePosition;
            if (_hoveredItemObject == null) return;
            if (_hoveredItemObject.transform.TryGetComponent(out UI_InventoryItemElement itemElement))
            {
                if (itemElement.ItemDefinition != null)
                {
                    _draggedUIInventoryItem = itemElement;
                }
            }
        }

        if (_draggedUIInventoryItem != null && _pointerDown && _hoveredItemElement != null && _draggedUIInventoryItem != _hoveredItemElement)
        {
            if (_draggedUIInventoryItem.OwnerInventory.CanSwapOrMergeItems(_draggedUIInventoryItem.SlotIndex,_hoveredItemElement.SlotIndex,_hoveredItemElement.OwnerInventory))
            {
                _hoveredItemElement.SetHovered(true);
            }
            else
            {
                _hoveredItemElement.SetActionBlocked(true);
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            if (_hoveredItemElement != null)
            {
                _hoveredItemElement.SetHovered(false);
                _hoveredItemElement.SetActionBlocked(false);
            }
            
            if (_draggedUIInventoryItem != null && _hoveredItemObject != null)
            {
                if (_hoveredItemObject != null)
                {
                    if (_hoveredItemObject.transform.TryGetComponent(out UI_InventoryItemElement hoveredItemElement))
                    {
                        InventorySlot draggedSlot =
                            _draggedUIInventoryItem.OwnerInventory.InventoryData.InventorySlots[
                                _draggedUIInventoryItem.SlotIndex];
                        InventorySlot hoveredSlot =
                            hoveredItemElement.OwnerInventory.InventoryData
                                .InventorySlots[hoveredItemElement.SlotIndex];

                        if (hoveredItemElement.ItemDefinition == null) // hovered is empty
                        {
                            //_inventoryDefinition.SwapOrMergeItems(_draggedUIInventoryItem.SlotIndex, hoveredItemElement.SlotIndex);
                            InventoryActionArgs inventoryAcitonArgs = new InventoryActionArgs
                            {
                                FromInventoryId = _draggedUIInventoryItem.OwnerInventory.InventoryId.ID,
                                FromInventorySlotIndex = _draggedUIInventoryItem.SlotIndex,
                                ToInventoryId = hoveredItemElement.OwnerInventory.InventoryId.ID,
                                ToInventorySlotIndex = hoveredItemElement.SlotIndex,
                            };
                            _inventoryActionEvent.Raise(inventoryAcitonArgs,
                                _draggedUIInventoryItem.OwnerInventory.Owner);
                        }
                        else
                        {
                            //_inventoryDefinition.SwapOrMergeItems(_draggedUIInventoryItem.SlotIndex, hoveredItemElement.SlotIndex);
                            InventoryActionArgs inventoryAcitonArgs = new InventoryActionArgs
                            {
                                FromInventoryId = _draggedUIInventoryItem.OwnerInventory.InventoryId.ID,
                                FromInventorySlotIndex = _draggedUIInventoryItem.SlotIndex,
                                ToInventoryId = hoveredItemElement.OwnerInventory.InventoryId.ID,
                                ToInventorySlotIndex = hoveredItemElement.SlotIndex,
                            };
                            _inventoryActionEvent.Raise(inventoryAcitonArgs,
                                _draggedUIInventoryItem.OwnerInventory.Owner);
                        }
                    }
                }
                else // dropped on empty space
                {
                    InventoryActionArgs inventoryAcitonArgs = new InventoryActionArgs
                    {
                        FromInventoryId = _draggedUIInventoryItem.OwnerInventory.InventoryId.ID,
                        FromInventorySlotIndex = _draggedUIInventoryItem.SlotIndex,
                    };
                    _inventoryActionEvent.Raise(inventoryAcitonArgs, _draggedUIInventoryItem.OwnerInventory.Owner);
                }
            }

            if (_draggingGhostItemInstance != null)
            {
                Destroy(_draggingGhostItemInstance);

                if (_hoveredItemObject == null)
                {
                    InventoryActionArgs inventoryAcitonArgs = new InventoryActionArgs
                    {
                        FromInventoryId = _draggedUIInventoryItem.OwnerInventory.InventoryId.ID,
                        FromInventorySlotIndex = _draggedUIInventoryItem.SlotIndex,
                    };
                    _inventoryActionEvent.Raise(inventoryAcitonArgs, _draggedUIInventoryItem.OwnerInventory.Owner);
                    _draggedUIInventoryItem.ClearItemData();
                }
            }


            _pointerDown = false;
            _draggedUIInventoryItem = null;
        }

        if (Input.GetMouseButton(0))
        {
            if (Vector2.Distance(_pointerDownPosition, Input.mousePosition) > 10f && !_draggingItem &&
                _draggedUIInventoryItem != null)
            {
                _draggingItem = true;

                _draggingGhostItemInstance = Instantiate(_ghostUIInventoryItemElementPrefab.gameObject,
                    _draggedUIInventoryItem.transform.position, Quaternion.identity);
                _draggingGhostItemInstance.transform.parent = _draggedUIInventoryItem.transform.parent.parent;
                _draggingGhostItemInstance.transform.localPosition = _draggedUIInventoryItem.transform.localPosition;
                //ItemDefinition itemDefinition = InventoryUtils.FindItemWithId(_draggedItem.ItemData.ItemID);
                InventorySlot draggedSlot =
                    _draggedUIInventoryItem.OwnerInventory.InventoryData.InventorySlots[
                        _draggedUIInventoryItem.SlotIndex];
                if (draggedSlot.ItemData == null) return;
                _draggingGhostItemInstance.GetComponentInChildren<UI_InventoryItemElement>()
                    .SetItemData(draggedSlot.ItemID, draggedSlot);
            }

            if (_draggingGhostItemInstance != null)
            {
                _draggingGhostItemInstance.transform.position = Input.mousePosition;
            }
        }
        else
        {
            _draggingItem = false;
        }

        if (Input.GetMouseButtonDown(1))
        {
            if (_hoveredItemObject == null) return;
            if (_hoveredItemObject.transform.TryGetComponent(out UI_InventoryItemElement itemElement))
            {
                if (itemElement.ItemDefinition != null)
                {
                    InventoryActionArgs inventoryAcitonArgs = new InventoryActionArgs
                    {
                        FromInventoryId = itemElement.OwnerInventory.InventoryId.ID,
                        FromInventorySlotIndex = itemElement.SlotIndex,
                        InteractItem = true
                    };
                    _inventoryActionEvent.Raise(inventoryAcitonArgs, itemElement.OwnerInventory.Owner);
                }
            }
        }
    }

    #endregion

    GameObject GetUIObjectUnderMouse()
    {
        if (pointerEventData == null) pointerEventData = new PointerEventData(eventSystem);
        pointerEventData.position = Input.mousePosition;

        _raycastResults.Clear();
        eventSystem.RaycastAll(pointerEventData, _raycastResults);

        if (_raycastResults.Count > 0)
        {
            return _raycastResults[0].gameObject; // Return the topmost UI element
        }

        return null; // Return null if no UI element was found under the mouse
    }

    #endregion
}