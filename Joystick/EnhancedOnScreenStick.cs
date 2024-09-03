using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.OnScreen;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace EnhancedOnScreenControls
{
    public enum StickType
    {
        Fixed = 0,
        Floating = 1,
        Dynamic = 2
    }

    [AddComponentMenu("Input/Enhanced On-Screen Stick")]
    [RequireComponent(typeof(RectTransform))]
    public class EnhancedOnScreenStick : OnScreenControl, IPointerDownHandler, IPointerUpHandler, IDragHandler
    {
        [InputControl(layout = "Vector2")] [FormerlySerializedAs("controlPath")] [SerializeField]
        string internalControlPath;

        [SerializeField] StickType stickType;
        [SerializeField] float movementRange = 50f;
        [SerializeField, Range(0f, 1f)] float deadZone = 0f;
        [SerializeField] bool showOnlyWhenPressed;

        [SerializeField] RectTransform background;
        [SerializeField] RectTransform handle;
        [SerializeField] private PanelActor _panelActor;
        private Canvas canvas => _panelActor.OwnerCanvas;
        public Vector3 _initBackgroundPosition;

        public event Action onPointerDown;
        public event Action onPointerUp;
        public event Action<Vector2> onPointerDrag;

        private Image _backgroundImage;
        private Vector2 _initialMousePosition;

        public bool IsButton;
        public bool IsAimable;
        public bool IsDisabled;

        protected override string controlPathInternal
        {
            get => internalControlPath;
            set => internalControlPath = value;
        }

        public StickType StickType
        {
            get => stickType;
            set => stickType = value;
        }

        public float MovementRange
        {
            get => movementRange;
            set => movementRange = value;
        }

        public float DeadZone
        {
            get => deadZone;
            set => deadZone = value;
        }


        protected void Awake()
        {
            _backgroundImage = background.GetComponent<Image>();
            if (showOnlyWhenPressed) background.gameObject.SetActive(false);

            if (IsButton) _backgroundImage.enabled = false;
        }

        private void Start()
        {
            _initBackgroundPosition = background.localPosition;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (IsDisabled) return;
            onPointerDown?.Invoke();
            var camera = canvas.worldCamera;
            var startPositionInWorld = RectTransformUtility.WorldToScreenPoint(camera, background.position);
            _initialMousePosition = eventData.position - startPositionInWorld;

            if (IsAimable)
            {
                _backgroundImage.enabled = true;
            }


            if (stickType != StickType.Fixed)
            {
                RectTransform canvasRectTransform = canvas.GetComponent<RectTransform>();

                // Convert screen point to local point in canvas
                Vector2 localPoint;
                if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                        background.transform.parent.GetComponent<RectTransform>(), eventData.position,
                        eventData.pressEventCamera, out localPoint))
                {
                    // Set the local position of the background to the converted point
                    background.localPosition = localPoint;
                }
            }

            if (IsAimable)
                handle.anchoredPosition = Vector2.zero;
            OnDrag(eventData);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (IsDisabled) return;
            onPointerUp?.Invoke();
            SentDefaultValueToControl();

            handle.anchoredPosition = Vector2.zero;

            if (showOnlyWhenPressed) background.gameObject.SetActive(false);
            if (IsAimable)
            {
                _backgroundImage.enabled = false;
            }

            RectTransform canvasRectTransform = canvas.GetComponent<RectTransform>();

            // Convert screen point to local point in canvas
            background.localPosition = _initBackgroundPosition;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (IsDisabled) return;
            if (IsButton && !IsAimable) return;
            var camera = canvas.worldCamera;
            var position = RectTransformUtility.WorldToScreenPoint(camera, background.position);

            if (!IsAimable)
            {
                _initialMousePosition = Vector2.zero;
            }

            var input = ((eventData.position - position) - _initialMousePosition) /
                        (movementRange * canvas.scaleFactor);
            var rawMagnitude = input.magnitude;
            var normalized = input.normalized;

            if (rawMagnitude < deadZone) input = Vector2.zero;
            else if (rawMagnitude > 1f) input = input.normalized;

            SendValueToControl(input);

            if (stickType == StickType.Dynamic && rawMagnitude > 1f)
            {
                var difference = movementRange * (rawMagnitude - 1f) * normalized;
                background.anchoredPosition += difference;
            }

            handle.anchoredPosition = input * movementRange;

            onPointerDrag?.Invoke(input);
        }
    }
}