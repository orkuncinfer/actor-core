using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages item drops and their associated labels
/// </summary>
public class ItemDropManager : MonoBehaviour
{
    [Header("References")]
    [Tooltip("The label prefab to instantiate")]
    [SerializeField] private GameObject _labelPrefab;
    
    [Tooltip("The UI canvas where labels will be parented")]
    [SerializeField] private Canvas _labelCanvas;
    
    [Tooltip("The container where labels will be placed")]
    [SerializeField] private RectTransform _labelContainer;
    
    [Header("Settings")]
    [Tooltip("Maximum distance at which labels are visible")]
    [SerializeField] private float _maxLabelDistance = 30f;
    
    // Dictionary to track active drops and their labels
    private Dictionary<ItemDrop, LootLabel> _dropLabels = new Dictionary<ItemDrop, LootLabel>();

    [SerializeField] private Transform _labelsBgParent;
    
    // Camera reference for distance calculations
    private Camera _mainCamera;
    
    private void Awake()
    {
        _mainCamera = Camera.main;
        
        // Ensure we have a label container
        if (_labelContainer == null && _labelCanvas != null)
        {
            _labelContainer = _labelCanvas.GetComponent<RectTransform>();
        }
    }
    
    private void Update()
    {
        // Update visibility of labels based on distance
        UpdateLabelVisibility();
    }
    
    /// <summary>
    /// Registers a new item drop and creates a label for it
    /// </summary>
    public void RegisterItemDrop(ItemDrop itemDrop)
    {
        // Skip if already registered
        if (_dropLabels.ContainsKey(itemDrop))
        {
            return;
        }
        
        // Create a new label
        GameObject labelObj = Instantiate(_labelPrefab, _labelContainer);
        LootLabel label = labelObj.GetComponent<LootLabel>();
        label.SetBgParent(_labelsBgParent);
        
        if (label != null)
        {
            // Setup the label
            label.TargetItemDrop = itemDrop;
            label.SetTarget(itemDrop.transform);
            label.SetText(itemDrop.GetItemName());
            label.SetPriority(itemDrop.GetItemRarity());
            label.SetVisibility(true);
            // Store the reference
            _dropLabels.Add(itemDrop, label);
        }
    }
    
    /// <summary>
    /// Unregisters an item drop and removes its label
    /// </summary>
    public void UnregisterItemDrop(ItemDrop itemDrop)
    {
        if (_dropLabels.TryGetValue(itemDrop, out LootLabel label))
        {
            // Destroy the label
            if (label != null)
            {
                Destroy(label.gameObject);
            }
            
            // Remove from dictionary
            _dropLabels.Remove(itemDrop);
        }
    }
    
    /// <summary>
    /// Updates the visibility of labels based on distance
    /// </summary>
    private void UpdateLabelVisibility()
    {
        if (_mainCamera == null) return;
        
        Vector3 cameraPos = _mainCamera.transform.position;
        
        // Check each drop and update its label visibility
        foreach (var pair in _dropLabels)
        {
            ItemDrop drop = pair.Key;
            LootLabel label = pair.Value;
            
            if (drop == null || label == null) continue;
            
            // Calculate distance to camera
            float distance = Vector3.Distance(cameraPos, drop.transform.position);
            
            // Check if item is in view
            bool inView = IsItemInView(drop.transform.position);
            
            // Update label visibility based on distance and view
            bool isVisible = distance <= _maxLabelDistance && inView;
            label.gameObject.SetActive(isVisible);
        }
    }
    
    /// <summary>
    /// Checks if an item is in the camera's view
    /// </summary>
    private bool IsItemInView(Vector3 worldPos)
    {
        if (_mainCamera == null) return false;
        
        // Convert world position to viewport position
        Vector3 viewportPos = _mainCamera.WorldToViewportPoint(worldPos);
        
        // Check if position is within viewport bounds (with small margin)
        return viewportPos.z > 0 && 
               viewportPos.x > -0.1f && viewportPos.x < 1.1f && 
               viewportPos.y > -0.1f && viewportPos.y < 1.1f;
    }
    
    /// <summary>
    /// Gets all active drop labels
    /// </summary>
    public Dictionary<ItemDrop, LootLabel> GetActiveLabels()
    {
        return _dropLabels;
    }
}