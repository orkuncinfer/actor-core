using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class UI_InventoryControl : MonoBehaviour
{
    [SerializeField] private InventoryDefinition _inventoryDefinition;
    [SerializeField] private GridLayoutGroup _inventoryLayout;
    [FormerlySerializedAs("_uıItemElementPrefab")] [FormerlySerializedAs("ItemElementPrefab")] [SerializeField] private UI_InventoryItemElement _uıInventoryItemElementPrefab;
    [FormerlySerializedAs("_ghostUIItemElementPrefab")] [FormerlySerializedAs("_ghostItemElementPrefab")] [SerializeField] private UI_InventoryItemElement _ghostUIInventoryItemElementPrefab;
    [SerializeField] private GameObject _garbageDragArea;

    [SerializeField] private EventSignal _onAuthCompleted;

    [ReadOnly] [SerializeField] private GameObject _hoveredItemInfo;

    private PointerEventData pointerEventData;
    private EventSystem eventSystem;
    private List<RaycastResult> _raycastResults = new List<RaycastResult>();

    private bool _pointerDown;
    private Vector2 _pointerDownPosition;
    public bool _draggingItem;
    [FormerlySerializedAs("_draggedUIItem")] [FormerlySerializedAs("_draggedItem")] public UI_InventoryItemElement _draggedUIInventoryItem;
    private GameObject _draggingGhostItemInstance;

    private Dictionary<int, UI_InventoryItemElement> _itemElements = new Dictionary<int, UI_InventoryItemElement>();
    

    private void Awake()
    {
        eventSystem = EventSystem.current;
        _onAuthCompleted.Register(OnAuthCompleted);
    }

    private void OnAuthCompleted()
    {
    }

    private void OnInventoryChanged()
    {
        foreach (var item in _itemElements)
        {
            item.Value.ClearItemData();
        }

        for (int i = 0; i < _inventoryDefinition.InventoryData.InventorySlots.Count; i++)
        {
            if (_inventoryDefinition.InventoryData.InventorySlots[i].ItemID.IsNullOrWhitespace())
            {
                continue;
            }
            
            if (!_inventoryDefinition.InventoryData.InventorySlots[i].ItemID.IsNullOrWhitespace())
            {
                _itemElements[i].SetItemData(_inventoryDefinition.InventoryData.InventorySlots[i].ItemID,
                    _inventoryDefinition.InventoryData.InventorySlots[i]);
            }
        }
    }

    private void Start()
    {
        for (int i = 0; i < _inventoryDefinition.InventoryData.InventorySlots.Count; i++)
        {
            UI_InventoryItemElement uıInventoryItemElement = Instantiate(_uıInventoryItemElementPrefab, _inventoryLayout.transform);
            _itemElements.Add(i, uıInventoryItemElement);
            if (!_inventoryDefinition.InventoryData.InventorySlots[i].ItemID.IsNullOrWhitespace())
            {
                uıInventoryItemElement.SetItemData(_inventoryDefinition.InventoryData.InventorySlots[i].ItemID,
                    _inventoryDefinition.InventoryData.InventorySlots[i]);
            }
            else
            {
                uıInventoryItemElement.ItemDefinition = null;
            }

            uıInventoryItemElement.SlotIndex = i;
        }

        _inventoryDefinition.onInventoryChanged += OnInventoryChanged;
    }

    void Update()
    {
        // Check what UI object is under the mouse
        GameObject uiObject = GetUIObjectUnderMouse();
        if (uiObject != null)
        {
            _hoveredItemInfo = uiObject;
        }

        if (Input.GetMouseButtonDown(0))
        {
            _pointerDown = true;
            _pointerDownPosition = Input.mousePosition;
            if (_hoveredItemInfo == null) return;
            if (_hoveredItemInfo.transform.TryGetComponent(out UI_InventoryItemElement itemElement))
            {
                if (itemElement.ItemDefinition != null)
                {
                    _draggedUIInventoryItem = itemElement;
                }
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            if (_draggedUIInventoryItem != null)
            {
                if (_hoveredItemInfo.transform.TryGetComponent(out UI_InventoryItemElement hoveredItemElement))
                {
                    InventorySlot draggedSlot =
                        _inventoryDefinition.InventoryData.InventorySlots[_draggedUIInventoryItem.SlotIndex];
                    InventorySlot hoveredSlot =
                        _inventoryDefinition.InventoryData.InventorySlots[hoveredItemElement.SlotIndex];
                    
                    if (hoveredItemElement.ItemDefinition == null) // hovered is empty
                    {
                        hoveredItemElement.SetItemData(draggedSlot.ItemID, draggedSlot);
                        _draggedUIInventoryItem.ClearItemData();
                        _inventoryDefinition.SwapOrMergeItems(_draggedUIInventoryItem.SlotIndex, hoveredItemElement.SlotIndex);
                    }
                    else
                    {
                        ItemDefinition temp = hoveredItemElement.ItemDefinition;
                        hoveredItemElement.SetItemData(draggedSlot.ItemID, draggedSlot);
                        _draggedUIInventoryItem.SetItemData(hoveredSlot.ItemID, hoveredSlot);
                        _inventoryDefinition.SwapOrMergeItems(_draggedUIInventoryItem.SlotIndex, hoveredItemElement.SlotIndex);
                    }
                }
            }

            if (_draggingGhostItemInstance != null)
            {
                Destroy(_draggingGhostItemInstance);

                if (_hoveredItemInfo == _garbageDragArea)
                {
                    _draggedUIInventoryItem.ClearItemData();
                    _inventoryDefinition.RemoveEntireSlot(
                        _inventoryDefinition.InventoryData.InventorySlots[_draggedUIInventoryItem.SlotIndex]);
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

                _draggingGhostItemInstance = Instantiate(_ghostUIInventoryItemElementPrefab.gameObject, transform);
                //ItemDefinition itemDefinition = InventoryUtils.FindItemWithId(_draggedItem.ItemData.ItemID);
                InventorySlot draggedSlot = _inventoryDefinition.InventoryData.InventorySlots[_draggedUIInventoryItem.SlotIndex];
                _draggingGhostItemInstance.GetComponent<UI_InventoryItemElement>().SetItemData(draggedSlot.ItemID, draggedSlot);
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
    }

    GameObject GetUIObjectUnderMouse()
    {
        if(pointerEventData == null) pointerEventData = new PointerEventData(eventSystem);
        pointerEventData.position = Input.mousePosition;

        _raycastResults.Clear();
        eventSystem.RaycastAll(pointerEventData, _raycastResults);

        if (_raycastResults.Count > 0)
        {
            return _raycastResults[0].gameObject; // Return the topmost UI element
        }

        return null; // Return null if no UI element was found under the mouse
    }
}