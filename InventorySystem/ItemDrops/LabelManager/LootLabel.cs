using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

/// <summary>
/// Represents a single loot label that follows a transform
/// </summary>
public class LootLabel : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("References")] [Tooltip("The transform this label will follow")] [SerializeField]
    private Transform _targetTransform;

    [Tooltip("The text component for displaying the label content")] [SerializeField]
    private TMP_Text _labelText;

    [Header("Settings")] [Tooltip("Priority of this label (higher values are positioned first)")] [SerializeField]
    private int _priority = 1;

    [Tooltip("Should this label auto-register with the manager on start?")] [SerializeField]
    private bool _autoRegister = true;

    [Tooltip("Should this label auto-resize based on text content?")] [SerializeField]
    private bool _autoResize = true;

    [Tooltip("Padding to add when auto-resizing")] [SerializeField]
    private Vector2 _textPadding = new Vector2(10f, 5f);

    public ItemDrop TargetItemDrop { get; set; }
    public Transform TargetTransform => _targetTransform;

    public Vector2 ScreenOffset;

    public RectTransform RectTransform;

    public int Priority => _priority;

    private RectTransform _rectTransform;

    // Timestamp when this label was created (for age-based sorting)
    private float _creationTime;

    [SerializeField] private RectTransform _normalBg;
    [SerializeField] private RectTransform _hoverBg;

    private Transform _labelParent;

    private void Awake()
    {
        // Record creation time for age sorting
        _creationTime = Time.time;

        // Cache components
        _rectTransform = GetComponent<RectTransform>();

        // Ensure we have the required components
        if (_labelText == null)
        {
            _labelText = GetComponentInChildren<TMP_Text>();
        }
    }

    public float GetCreationTime()
    {
        return _creationTime;
    }

    private void Start()
    {
        if (_autoRegister && LootLabelManager.Instance != null)
        {
            LootLabelManager.Instance.RegisterLabel(this);
        }

        // Resize if needed
        if (_autoResize)
        {
            ResizeToFitText();
        }
    }

    public void SetBgParent(Transform parent)
    {
        _labelParent = parent;
        
        _normalBg.SetParent(_labelParent);
        _hoverBg.SetParent(_labelParent);
    }
    private void OnEnable()
    {
        if (LootLabelManager.Instance != null)
        {
            LootLabelManager.Instance.RegisterLabel(this);
        }

        _hoverBg.gameObject.SetActive(false);
        _normalBg.gameObject.SetActive(true);
        _normalBg.transform.SetParent(_labelParent);
        _hoverBg.transform.SetParent(_labelParent);
    }


    private void OnDisable()
    {
        if (LootLabelManager.Instance != null)
        {
            LootLabelManager.Instance.UnregisterLabel(this);
        }
        _hoverBg.gameObject.SetActive(false);
        _normalBg.gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        if (LootLabelManager.Instance != null)
        {
            LootLabelManager.Instance.UnregisterLabel(this);
        }

        Destroy(_normalBg.gameObject);
        Destroy(_hoverBg.gameObject);
    }

    public void SetTarget(Transform target)
    {
        _targetTransform = target;
    }

    public void SetText(string text)
    {
        if (_labelText != null)
        {
            _labelText.text = text;

            // Resize if needed
            if (_autoResize)
            {
                ResizeToFitText();
            }
        }
    }

    public void SetPriority(int priority)
    {
        _priority = priority;
    }

    public void SetVisibility(bool visible)
    {
        if (_labelText != null)
        {
            _labelText.enabled = visible;
        }
    }

    public void ResizeToFitText()
    {
        if (_labelText != null && _rectTransform != null)
        {
            // Get the preferred size of the text
            Vector2 preferredSize = _labelText.GetPreferredValues();

            // Add padding
            preferredSize += _textPadding * 2;

            // Set the size of the rect transform
            _rectTransform.sizeDelta = preferredSize;
        }
    }

    public Vector2 GetSize()
    {
        if (_rectTransform != null)
        {
            return _rectTransform.rect.size;
        }

        return Vector2.zero;
    }

    void UpdateBgPositionAndRotation()
    {
        _normalBg.transform.position = transform.position;
        _hoverBg.transform.position = transform.position;
        _normalBg.transform.rotation = transform.rotation;
        _hoverBg.transform.rotation = transform.rotation;
        _normalBg.sizeDelta = _rectTransform.sizeDelta;
        _hoverBg.sizeDelta = _rectTransform.sizeDelta;
    }

    public void SetHover(bool isHover)
    {
        _normalBg.gameObject.SetActive(!isHover);
        _hoverBg.gameObject.SetActive(isHover);
    }

    void Update()
    {
        UpdateBgPositionAndRotation();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        SetHover(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        SetHover(false);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
       
    }

    public void Collect()
    {
        LootLabelManager.Instance.UnregisterLabel(this);
        Destroy(TargetTransform.gameObject);
        Destroy(transform.gameObject);
    }
}