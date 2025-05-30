using TMPro;
using UnityEngine;

[RequireComponent(typeof(TextMeshProUGUI))]
public class ResizeToFitText : MonoBehaviour
{
    [SerializeField] private Vector2 _padding = Vector2.zero;
    [SerializeField] private bool _resizeWidth = true;
    [SerializeField] private bool _resizeHeight = true;

    private TextMeshProUGUI _text;
    private RectTransform _rectTransform;

    private void Awake()
    {
        _text = GetComponent<TextMeshProUGUI>();
        _rectTransform = GetComponent<RectTransform>();
        TMPro_EventManager.TEXT_CHANGED_EVENT.Add(OnTextChanged);
    }

    private void OnTextChanged(Object obj)
    {
        if (obj == _text)
        {
            UpdateSize();
        }
    }

    private void OnEnable()
    {
        UpdateSize();
    }


    /// <summary>
    /// Resizes the RectTransform based on the preferred size of the TextMeshProUGUI content.
    /// </summary>
    public void UpdateSize()
    {
        if (_text == null || _rectTransform == null)
            return;

        Vector2 preferredSize = _text.GetPreferredValues();

        Vector2 newSize = _rectTransform.sizeDelta;

        if (_resizeWidth)
            newSize.x = preferredSize.x + _padding.x;

        if (_resizeHeight)
            newSize.y = preferredSize.y + _padding.y;

        _rectTransform.sizeDelta = newSize;
    }

    /// <summary>
    /// Public method to trigger manual update from other scripts.
    /// </summary>
    public void Refresh()
    {
        UpdateSize();
    }
}