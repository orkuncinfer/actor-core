using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DefaultInventoryDebugTrigger : MonoBehaviour
{
    [SerializeField] private GameObject _itemAddedDisplayerPrefab;
    public GameObject DebuggerPrefab;
    private GameObject _spawnedPrefab;
    public float clickThreshold = 1.0f; // Time in seconds within which the clicks must occur
    private int clickCount = 0;
    private float lastClickTime = 0f;

    private DefaultPlayerInventory _inventory;
    private bool _isRegistered;
    

    private void Start()
    {
        if (!_isRegistered)
        {
            DefaultPlayerInventory.Instance.onItemAdded += OnItemAdded;
        }
    }
    private void OnDestroy()
    {
        if(_isRegistered)
            DefaultPlayerInventory.Instance.onItemAdded -= OnItemAdded;
    }
    private void TriggerDebugger()
    {
        if (_spawnedPrefab == null)
        {
            _spawnedPrefab = Instantiate(DebuggerPrefab);
        }
        else
        {
            _spawnedPrefab.SetActive(true);
        }
    }
    private void OnItemAdded(string arg1, int arg2, int arg3)
    {
        if(_itemAddedDisplayerPrefab)
        {
            ItemBaseDefinition item = InventoryUtils.FindItemWithId(arg1);
            var itemAddedDisplayer = Instantiate(_itemAddedDisplayerPrefab).GetComponentInChildren<BasicItemDisplayer>();
            itemAddedDisplayer.ItemDefinition = item;
            itemAddedDisplayer.SetItemCount(arg3);
            itemAddedDisplayer.DisplayItem(item);
            Destroy(itemAddedDisplayer.transform.parent.gameObject, 1.5f);
        }
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(1)) 
        {
            float currentTime = Time.time;

            if (currentTime - lastClickTime <= clickThreshold)
            {
                clickCount++;
            }
            else
            {
                clickCount = 1; 
            }

            lastClickTime = currentTime;

            if (clickCount == 3)
            {
                TriggerDebugger();
                clickCount = 0; 
            }
        }
    }

   
}
