using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manages the positioning and stacking of loot drop labels
/// </summary>
public class LootLabelManager : MonoBehaviour
{
    private static LootLabelManager _instance;
    public static LootLabelManager Instance => _instance;

    [Header("Label Settings")]
    [Tooltip("Vertical offset for labels from their target")]
    [SerializeField] private float _labelVerticalOffset = 50f;
    
    [Tooltip("Minimum spacing between labels")]
    [SerializeField] private float _minSpacingBetweenLabels = 5f;
    
    [Tooltip("The maximum stacking depth (how many labels can stack vertically)")]
    [SerializeField] private int _maxStackDepth = 10;

    [SerializeField] private float _attractionSpeed = 10;
    
    [Tooltip("Should labels try to return to original positions when space becomes available")]
    [SerializeField] private bool _returnToOriginalPositions = true;

    // Internal tracking of all active labels
    private readonly List<LootLabel> _activeLabels = new List<LootLabel>();
    
    // Track original positions
    private Dictionary<LootLabel, Vector2> _originalPositions = new Dictionary<LootLabel, Vector2>();
    
    // Rectangle-based collision detection for precise overlap avoidance
    private Dictionary<Rect, LootLabel> _occupiedCells = new Dictionary<Rect, LootLabel>();
    
    // Performance tracking variables
    private float _lastFrameTime = 0f;
    private float _averageFrameTime = 0f;
    private float _maxFrameTime = 0f;
    private int _frameCount = 0;
    private const int FRAME_SAMPLE_SIZE = 60; // Calculate average over this many frames
    
    // Camera reference for world to screen conversion
    private Camera _mainCamera;
    
    // Screen boundaries cached each frame
    private float _screenWidth;
    private float _screenHeight;

    private void Awake()
    {
        // Singleton pattern
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        _instance = this;
        _mainCamera = Camera.main;
    }

    private void Start()
    {
        // Cache screen dimensions
        UpdateScreenDimensions();
    }

    public GameObject LootDrop;
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
        {
            Vector3 randomPos = Random.insideUnitSphere;
            randomPos.y = 0;
            GameObject lootDrop = Instantiate(LootDrop, randomPos, Quaternion.identity);
            ItemDrop item = lootDrop.GetComponent<ItemDrop>();
            
            string[] randomItemNames = { "Sword", "Shield", "Potion", "Gold" };
            
            if (item != null)
            {
                // Set random properties
                string itemName = randomItemNames[Random.Range(0, randomItemNames.Length)] + " +" + Random.Range(0, 12);
                ItemDrop.Rarity rarity = (ItemDrop.Rarity)Random.Range(0, 5);
                int quantity = Random.Range(1, 100);
            
                // Apply properties
                item.SetItemName(itemName);
                item.SetItemRarity(rarity);
                item.SetQuantity(quantity);
            }
        }
        
       
        // Update screen dimensions if changed
        UpdateScreenDimensions();
        
        // Clear occupied cells map for fresh positioning
        _occupiedCells.Clear();
        
        // Sort labels by priority (with older labels having higher priority)
        SortLabelsByPriority();
        
        // Position each label
        PositionAllLabels();
        
        // Try to return labels to their original positions if space is available
        // Only do this every few frames to avoid constant repositioning
        if (Time.frameCount % 15 == 0)
        {
            //AttemptRepositionToOriginal();
        }
        
        // End timing and record performance metrics
    }

    /// <summary>
    /// Registers a new loot label
    /// </summary>
    public void RegisterLabel(LootLabel label)
    {
        if (!_activeLabels.Contains(label))
        {
            _activeLabels.Add(label);
        }
    }

    /// <summary>
    /// Unregisters a loot label
    /// </summary>
    public void UnregisterLabel(LootLabel label)
    {
        _activeLabels.Remove(label);
    }

    /// <summary>
    /// Updates cached screen dimensions
    /// </summary>
    private void UpdateScreenDimensions()
    {
        _screenWidth = Screen.width;
        _screenHeight = Screen.height;
    }

    /// <summary>
    /// Sorts labels by priority for consistent positioning
    /// Older labels have priority to push newer labels
    /// </summary>
    private void SortLabelsByPriority()
    {
        // Sort labels by age (oldest labels first)
        // This ensures older labels can push newer ones downward
        /*_activeLabels.Sort((a, b) => {
            // Sort by creation time (oldest first)
            // Since we don't have a creation timestamp, use index in active labels list as proxy
            // Older labels will have lower indices, so reverse the comparison
            return b.GetInstanceID().CompareTo(a.GetInstanceID());
        });*/
        
        // Store original desired positions for potential repositioning
        if (_returnToOriginalPositions)
        {
            foreach (var label in _activeLabels)
            {
                if (label.TargetTransform == null) continue;
                
                // Calculate and store original position
                Vector3 targetWorldPos = label.TargetTransform.position;
                Vector2 targetScreenPos = _mainCamera.WorldToScreenPoint(targetWorldPos);
                Vector2 originalPos = new Vector2(targetScreenPos.x, targetScreenPos.y + _labelVerticalOffset);
                
                _originalPositions[label] = originalPos;
            }
        }
    }

    /// <summary>
    /// Positions all labels to avoid overlap
    /// </summary>
    private void PositionAllLabels()
    {
        // Clear occupied positions
        _occupiedCells.Clear();
        
        // Process labels in order (newer labels are positioned first, so they can be pushed down by older labels)
        for (int i = 0; i < _activeLabels.Count; i++)
        {
            LootLabel label = _activeLabels[i];
            if (!label.gameObject.activeInHierarchy) continue;
            
            // Calculate desired position based on target
            Vector3 targetWorldPos = label.TargetTransform.position;
            Vector2 targetScreenPos = _mainCamera.WorldToScreenPoint(targetWorldPos);
            
            // Apply vertical offset
            Vector2 desiredPos = new Vector2(targetScreenPos.x, targetScreenPos.y + _labelVerticalOffset);
            
            // Check if the label would be entirely off-screen
            RectTransform rectTransform = label.RectTransform;
            Vector2 labelSize = rectTransform.sizeDelta;
            
            // Skip positioning if all four corners would be off-screen
            if (IsCompletelyOffScreen(desiredPos, labelSize))
            {
                // Hide the label if completely off screen
                label.SetVisibility(false);
                continue;
            }
            
            // Make the label visible
            label.SetVisibility(true);
            
            // Find a non-overlapping position for this label

            AdjustLabelOffsets(label);
            
            Vector2 finalPos = FindNonOverlappingPosition(desiredPos + label.ScreenOffset, labelSize, label);
            //Vector2 finalPos = desiredPos;
            if (Vector2.Distance(rectTransform.position,finalPos) > 20f)
            {
                rectTransform.position = Vector2.Lerp(rectTransform.position, finalPos, Time.deltaTime * _attractionSpeed);   
            }
            else
            {
                rectTransform.position = finalPos;
            }
            // Set the position immediately without lerping
            
            // Mark cells as occupied with exact label size
            MarkCellsAsOccupied(finalPos, labelSize,label);
        }
    }

    private void AdjustLabelOffsets(LootLabel label)
    {
        // Get current screen offset and rect transform
        Vector2 currentOffset = label.ScreenOffset;
        RectTransform rectTransform = label.RectTransform;
        
        // Skip adjustment if offset isn't negative (label isn't pushed down)
        if (currentOffset.y >= 0)
            return;
        
        Vector2 rectSize = rectTransform.rect.size;
        Vector2 rectPosition = rectTransform.position;
        
        // Calculate top corners in screen space
        Vector2 topLeft = new Vector2(rectPosition.x - rectSize.x / 2, rectPosition.y + rectSize.y / 2);
        Vector2 topRight = new Vector2(rectPosition.x + rectSize.x / 2, rectPosition.y + rectSize.y / 2);
        
        // Raycast length - we want to check how far up we can move
        float rayLength = Mathf.Infinity;
        
        // Reusable array for raycast results to avoid allocation
        RaycastHit2D[] leftResults = new RaycastHit2D[8];
        RaycastHit2D[] rightResults = new RaycastHit2D[8];
        
        // Set up ray direction (screen space is Y-up)
        Vector2 rayDirection = Vector2.up;
        
        int leftHitCount = Physics2D.RaycastNonAlloc(topLeft, rayDirection, leftResults, rayLength);
        int rightHitCount = Physics2D.RaycastNonAlloc(topRight, rayDirection, rightResults, rayLength);

        Color color = leftHitCount > 1 ? Color.green : Color.red;
        Debug.DrawLine(topLeft,topLeft + rayDirection*rayLength,color);
        color = rightHitCount > 1 ? Color.green : Color.red;
        Debug.DrawLine(topRight,topRight + rayDirection*rayLength,color);
        
        float availableSpace = rayLength; // Default to full ray length
        
        // Process left corner results
        for (int i = 0; i < leftHitCount; i++)
        {
            RaycastHit2D hit = leftResults[i];
            if (hit.collider != null && hit.distance < availableSpace)
            {
                // Ignore collisions with the label itself or its children
                if (!IsPartOfLabel(hit.collider, label))
                {
                    availableSpace = Mathf.Min(availableSpace, hit.distance);
                }
            }
        }
        
        // Process right corner results
        for (int i = 0; i < rightHitCount; i++)
        {
            RaycastHit2D hit = rightResults[i];
            if (hit.collider != null && hit.distance < availableSpace)
            {
                // Ignore collisions with the label itself or its children
                if (!IsPartOfLabel(hit.collider, label))
                {
                    availableSpace = Mathf.Min(availableSpace, hit.distance);
                }
            }
        }
        
        // If we found available space, reduce the offset
        if (availableSpace > 0)
        {
            // Apply a buffer for safety (to prevent exact touching)
            float safetyBuffer = _minSpacingBetweenLabels;
            float adjustedSpace = availableSpace - safetyBuffer;
            
            // Only adjust if there's meaningful space available
            if (adjustedSpace > 0)
            {
                // Gradually move back toward original position (lerp for smoothness)
                float lerpFactor = Time.deltaTime * _attractionSpeed; // Adjust speed as needed
                //float newYOffset = Mathf.Lerp(currentOffset.y, currentOffset.y + adjustedSpace, lerpFactor);
                float newYOffset = currentOffset.y + adjustedSpace;
                if (newYOffset > 0) newYOffset = 0;
                // Update screen offset
                label.ScreenOffset = new Vector2(currentOffset.x, newYOffset);
            }
        }
    }
    private bool IsPartOfLabel(Collider2D collider, LootLabel label)
    {
        // Check if the collider is on the label itself
        if (collider.gameObject == label.gameObject)
            return true;
        
        // Check if the collider is a child of the label
        Transform parent = collider.transform.parent;
        while (parent != null)
        {
            if (parent.gameObject == label.gameObject)
                return true;
            
            parent = parent.parent;
        }
        
        return false;
    }
    /// <summary>
    /// Finds a non-overlapping position for a label with strong preference for vertical stacking
    /// </summary>
    private Vector2 FindNonOverlappingPosition(Vector2 desiredPos, Vector2 labelSize, LootLabel label)
    {
        // First, try the desired position
        if (!IsPositionOccupied(desiredPos, labelSize, label, out Rect occupyRect))
        {
            return desiredPos;
        }
        else
        {
            Rect labelRect = new Rect(desiredPos - labelSize / 2, labelSize);
            Debug.DrawLine(labelRect.center, occupyRect.center, Color.yellow, 2);

            // Calculate initial offset
            float yOffset = (labelRect.height / 2 + occupyRect.height / 2) - Mathf.Abs(labelRect.y - occupyRect.y) + _minSpacingBetweenLabels;

            // Ensure we only move down (negative Y)
            yOffset = -Mathf.Abs(yOffset);

            // Initialize accumulated offset
            float totalYOffset = yOffset;
        
            // Calculate new position with offset
            Vector2 newPos = desiredPos + new Vector2(0, totalYOffset);

            int iteration = 0;
            while (IsPositionOccupied(newPos, labelSize, label, out Rect newOccupy))
            {
                iteration++;
                if (iteration > 60)
                {
                    Debug.LogError("Reached max iteration " + label.transform.name);
                    break;
                }
            
                // Calculate label rect at current position
                labelRect = new Rect(newPos - labelSize / 2, labelSize);
            
                // Calculate additional offset needed
                yOffset = (labelRect.height / 2 + newOccupy.height / 2) - Mathf.Abs(labelRect.y - newOccupy.y) + _minSpacingBetweenLabels;
            
                // Ensure we only move down (negative Y)
                yOffset = -Mathf.Abs(yOffset);
            
                // Accumulate the offset
                totalYOffset += yOffset;
            
                // Calculate new position with accumulated offset
                newPos = desiredPos + new Vector2(0, totalYOffset);
            }

            label.ScreenOffset += new Vector2(0, totalYOffset);
            Debug.DrawLine(labelRect.center, newPos, Color.green, 2);
            return newPos;
        }
    }
    private bool IsPositionOccupied(Vector2 position, Vector2 size,LootLabel label, out Rect occupyRect)
    {
        // Define the rectangle of the current label
        Rect labelRect = new Rect(position - size / 2, size);
        
        // Expand the rect slightly to ensure no pixel-perfect collisions
        labelRect.x -= _minSpacingBetweenLabels / 2;
        labelRect.y -= _minSpacingBetweenLabels / 2;
        labelRect.width += _minSpacingBetweenLabels;
        labelRect.height += _minSpacingBetweenLabels;
        
        // Check against all occupied rectangles
        foreach (var pair in _occupiedCells)
        {
            if(pair.Value == label) continue;
            Rect occupiedRect = pair.Key;
            if (labelRect.Overlaps(occupiedRect))
            {
                occupyRect = occupiedRect;
                return true;
            }
        }

        occupyRect = default;
        return false;
    }
    /// <summary>
    /// Marks a rectangle as occupied after positioning a label
    /// </summary>
    private void MarkCellsAsOccupied(Vector2 position, Vector2 size, LootLabel lootLabel)
    {
        // Create a rectangle representing this label's exact position and size
        Rect labelRect = new Rect(position - size / 2, size);
        
        // Store this rectangle as occupied
        _occupiedCells[labelRect] = lootLabel;
    }
    
 

    /// <summary>
    /// Checks if a label would be completely off-screen
    /// </summary>
    private bool IsCompletelyOffScreen(Vector2 position, Vector2 size)
    {
        float halfWidth = size.x / 2;
        float halfHeight = size.y / 2;
        
        // Check if all four corners are outside the screen
        bool topLeftOutside = (position.x - halfWidth < 0 && position.y + halfHeight > _screenHeight) ||
                              (position.x - halfWidth > _screenWidth) || 
                              (position.y + halfHeight < 0);
                              
        bool topRightOutside = (position.x + halfWidth < 0) || 
                               (position.x + halfWidth > _screenWidth && position.y + halfHeight > _screenHeight) ||
                               (position.y + halfHeight < 0);
                               
        bool bottomLeftOutside = (position.x - halfWidth < 0 && position.y - halfHeight < 0) ||
                                 (position.x - halfWidth > _screenWidth) || 
                                 (position.y - halfHeight > _screenHeight);
                                 
        bool bottomRightOutside = (position.x + halfWidth < 0) || 
                                  (position.x + halfWidth > _screenWidth && position.y - halfHeight < 0) ||
                                  (position.y - halfHeight > _screenHeight);
        
        return topLeftOutside && topRightOutside && bottomLeftOutside && bottomRightOutside;
    }
    
    /// <summary>
    /// Removes a label's occupation from the occupied cells
    /// </summary>
    private void RemoveLabelOccupation(Vector2 position, Vector2 size)
    {
        Rect labelRect = new Rect(position - size / 2, size);
        
        // Find and remove the rectangle
        List<Rect> toRemove = new List<Rect>();
        foreach (var pair in _occupiedCells)
        {
            if (pair.Key.Overlaps(labelRect) && 
                Mathf.Approximately((float)pair.Key.width, (float)size.x) && 
                Mathf.Approximately((float)pair.Key.height, (float)size.y))
            {
                toRemove.Add(pair.Key);
                break;
            }
        }
        
        foreach (var rect in toRemove)
        {
            _occupiedCells.Remove(rect);
        }
    }

    // Optional debug visualization
    private void OnDrawGizmos()
    {
        // Draw occupied rects for debugging
        if (_occupiedCells == null) return;
        
        Gizmos.color = new Color(1, 0, 0, 0.2f);
        foreach (var cell in _occupiedCells)
        {
            if (cell.Value)
            {
                Rect rect = cell.Key;
                Vector3 center = new Vector3(rect.center.x, rect.center.y, 0);
                Vector3 size = new Vector3(rect.width, rect.height, 0.1f);
                Gizmos.DrawCube(center, size);
            }
        }
    }
}