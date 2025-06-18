using System;
using ECM2;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.InputSystem;


    public class CharacterInput : MonoBehaviour
    {
        public Character Character;
        public InputActionAsset ActionAsset;
        public bool BlockInput;
        protected InputAction movementInputAction { get; set; }
        public Vector2 MoveInput;
        [SerializeField] private bool _isCameraRelative = true;
        private Camera _mainCam;
        
        private void Awake()
        {
            Character = GetComponent<Character>();
            SetupPlayerInput();
            _mainCam = Camera.main;
        }
        
        public void SetCustomRotationMode(bool isCustom)
        {
            Character.rotationMode = isCustom ? Character.RotationMode.Custom : Character.RotationMode.OrientRotationToMovement;
        }

        private void OnEnable()
        {
            movementInputAction?.Enable();
        }

        private void OnDisable()
        {
            movementInputAction?.Disable();
        }
        protected virtual void SetupPlayerInput()
        {
            if (ActionAsset == null)
                return;
            movementInputAction = ActionAsset.FindAction("Movement");
        }

        private void OnPerformed(InputAction.CallbackContext obj)
        {
            Debug.Log("performed");
            
        }

        public virtual Vector2 GetMovementInput()
        {
            if (BlockInput) return Vector2.zero;

            if (movementInputAction != null && ActionAsset != null)
            {
                return movementInputAction.ReadValue<Vector2>();
            }
            return MoveInput;
        }
        private void Update()
        {
            if (movementInputAction != null)
            {
                if (!movementInputAction.enabled) movementInputAction?.Enable();
            }
           
            Vector2 inputMove = GetMovementInput();
            Vector3 movementDirection = Vector3.zero;
            if (_isCameraRelative)
            {
                Vector3 cameraForward = _mainCam.transform.forward;
                Vector3 cameraRight = _mainCam.transform.right;
                cameraForward.y = 0;
                cameraRight.y = 0;
                cameraForward.Normalize();
                cameraRight.Normalize();

                // Calculate movement direction relative to the camera's orientation
                movementDirection = cameraForward * inputMove.y + cameraRight * inputMove.x;
            }
            else
            {
                movementDirection =  Vector3.zero;
                movementDirection += Vector3.right * inputMove.x;
                movementDirection += Vector3.forward * inputMove.y;
            }
            // Obtain the camera's forward and right vectors, but ignore the y component to keep movement horizontal.
            

            Character.SetMovementDirection(movementDirection);
            
            if (Input.GetKeyDown(KeyCode.LeftControl) || Input.GetKeyDown(KeyCode.C))
                Character.Crouch();
            else if (Input.GetKeyUp(KeyCode.LeftControl) || Input.GetKeyUp(KeyCode.C))
                Character.UnCrouch();
            
            if (Input.GetButtonDown("Jump"))
                Character.Jump();
            else if (Input.GetButtonUp("Jump"))
                Character.StopJumping();
        }
    }

