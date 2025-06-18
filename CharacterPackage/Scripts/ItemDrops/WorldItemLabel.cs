using System;
using System.Collections;
using Sirenix.OdinInspector;
using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;

public class WorldItemLabel : MonoBehaviour,IPointerEnterHandler,IPointerExitHandler,IPointerClickHandler
{
    private Camera _camera;
    public RectTransform rectTransform;
    public TMP_Text _textComponent;
    [SerializeField] private RectTransform _normalBg;
    [SerializeField] private RectTransform _hoverBg;

    public float WidthSpacing = 0.1f;
    
    public bool IsMoving;
    private bool _initialized;
    public bool Initialized => _initialized;
    
    public ItemDropInstance ItemDropInstance;

    private Transform _labelParent;
    private void Start()
    {
        _camera = Camera.main;
        AdjustWidth();
    }

    public void ActivateLabel()
    {
        _hoverBg.gameObject.SetActive(false);
        _normalBg.gameObject.SetActive(true);
        _textComponent.gameObject.SetActive(true);
        IsMoving = false;
        _initialized = true;
        AdjustWidth();
        AdjustWidth();
        _normalBg.transform.SetParent(_labelParent);
        _hoverBg.transform.SetParent(_labelParent);
    }

    private void OnEnable()
    {
        PoolManager.onBeforeReturnToPool += OnBeforeReturnToPool;
        _textComponent.gameObject.SetActive(false);
        _hoverBg.gameObject.SetActive(false);
        _normalBg.gameObject.SetActive(false);
    }

    void OnBeforeReturnToPool()
    {
        IsMoving = false;
        _initialized = false;
        _textComponent.gameObject.SetActive(false);
        _hoverBg.gameObject.SetActive(false);
        _normalBg.gameObject.SetActive(false);
        PoolManager.onBeforeReturnToPool -= OnBeforeReturnToPool;
    }
    

    public void Initialize(Transform labelParent, Vector3 pos = default)
    {
        _labelParent = labelParent;
        IsMoving = true;
        _initialized = true;
     
        transform.position = pos;
        AdjustWidth();
        UpdateBgPositionAndRotation();
    }

    void Update()
    {
        AdjustWidth();
        UpdateBgPositionAndRotation();
        transform.forward = _camera.transform.forward;
    }
    [Button]
    public void UpdatePosition(RectTransform other)
    {
        //if (!_initialized || IsMoving) return;
        
        float pixelMove = CalculateHeightInPixels();
        Vector3 screenPos = _camera.WorldToScreenPoint(other.position);
        screenPos.y += pixelMove + 8;
        Vector3 worldPos = _camera.ScreenToWorldPoint(screenPos);
        rectTransform.position = new Vector3(rectTransform.position.x, worldPos.y, worldPos.z);
        UpdateBgPositionAndRotation();
    }
    
    public void NudgeUp(float pixels)
    {
        Vector3 screenPos = _camera.WorldToScreenPoint(transform.position);
        screenPos.y -= pixels;
        transform.position = _camera.ScreenToWorldPoint(screenPos);
    }

    
    void UpdateBgPositionAndRotation()
    {
        _normalBg.transform.position = transform.position;
        _hoverBg.transform.position = transform.position;
        _normalBg.transform.rotation = transform.rotation;
        _hoverBg.transform.rotation = transform.rotation;
    }

    float CalculateHeightInPixels()
    {
        Vector3[] worldCorners = new Vector3[4];
        rectTransform.GetWorldCorners(worldCorners);
        Vector3 screenCorner1 = _camera.WorldToScreenPoint(worldCorners[1]); // Top left
        Vector3 screenCorner3 = _camera.WorldToScreenPoint(worldCorners[3]); // Bottom left

        return Mathf.Abs(screenCorner1.y - screenCorner3.y);
    }

    void AdjustWidth()
    {
        if (_textComponent == null) return;
        _textComponent.ForceMeshUpdate();
        var textBounds = _textComponent.textBounds;
        rectTransform.sizeDelta = new Vector2(textBounds.size.x + WidthSpacing, rectTransform.sizeDelta.y);
    }
      
    public void SetHover(bool isHover)
    {
        _normalBg.gameObject.SetActive(!isHover);
        _hoverBg.gameObject.SetActive(isHover);
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        Debug.Log("Pointer Enter");
        SetHover(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        SetHover(false);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        ItemDropInstance.Collect();
    }
}
