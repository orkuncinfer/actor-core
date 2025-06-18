using System;
using System.Collections.Generic;
using FishNet.Object;
using FishNet.Object.Prediction;
using FishNet.Transporting;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ECM2.Examples.Networking.FishNet
{
    public class RPG_PlayerController : NetworkBehaviour
    {
        public struct ReplicateData : IReplicateData
        {
            public bool fromOwner;
            public float horizontal;
            public float vertical;
            public float cameraYaw;
            public float targetYaw;
            public bool jump;
            
            public bool forceSnapRotation;
            public float snapYaw;
            public bool cameraRelative;
            public Vector3 knockback;
            
            // Enhanced movement blocking data
            public bool isMovementBlocked;
            public MovementBlockType blockType;

            private uint _tick;

            public void Dispose()
            {
                fromOwner = false;
            }

            public uint GetTick() => _tick;
            public void SetTick(uint value) => _tick = value;
        }

        public struct ReconcileData : IReconcileData
        {
            public Vector3 position;
            public Quaternion rotation;
            public Vector3 velocity;
            public bool constrainedToGround;
            public float unconstrainedTime;
            public bool hitGround;
            public bool isWalkable;
            
            // Store blocked state for reconciliation
            public Vector3 preservedVelocity;
            public bool wasMovementBlocked;
            
            private uint _tick;

            public void Dispose()
            {
            }

            public uint GetTick() => _tick;
            public void SetTick(uint value) => _tick = value;
        }

        [System.Serializable]
        public enum MovementBlockType
        {
            None = 0,
            SoftStop = 1,      // Gradual deceleration (current behavior)
            HardStop = 2,      // Immediate velocity zeroing
            Freeze = 3,        // Complete movement and rotation lock
            PreserveMomentum = 4 // Stop but preserve velocity for restoration
        }

        [Header("Movement Parameters")]
        public float rotationRate = 540.0f;
        public float maxSpeed = 5;
        public float acceleration = 20.0f;
        public float deceleration = 20.0f;
        public float groundFriction = 8.0f;
        public float airFriction = 0.5f;
        public float jumpImpulse = 6.5f;
        [Range(0.0f, 1.0f)] public float airControl = 0.3f;
        public Vector3 gravity = Vector3.down * 9.81f;

        [Header("Movement Blocking")]
        [SerializeField] private MovementBlockType _defaultBlockType = MovementBlockType.HardStop;
        [SerializeField] private float _blockTransitionSpeed = 10.0f;
        [SerializeField] private bool _allowRotationWhenBlocked = false;
        
        public bool CameraRelative { get; set; }
        public GameplayTagContainer MovementBlockers;
        public List<string> _movementBlockers;
        
        // Private fields
        private Camera _camera;
        private DS_MovingActor _movingData;
        private Actor _actor;
        private bool _jump;
        private CharacterMovement characterMovement { get; set; }
        
        // Movement blocking state
        private Vector3 _preservedVelocity = Vector3.zero;
        private bool _wasBlockedLastFrame = false;
        private Vector3 _pendingKnockback = Vector3.zero;
        private bool _forceSnapRotation;
        private float _snapYaw;

        // Network prediction variables
        private ReplicateData _lastTickedReplicateData = default;
        private uint _lastAppliedKnockbackTick = 0;
        private Quaternion _lastReconcileRotation = Quaternion.identity;
        private bool _initialRotationSet = false;

        #region Movement Blocking Logic

        /// <summary>
        /// Determines the current movement block state based on active blockers
        /// </summary>
        private MovementBlockType GetCurrentBlockType()
        {
            if (_movementBlockers.Count == 0)
                return MovementBlockType.None;

            // You can implement tag-specific block types here
            // For now, using the default type
            return _defaultBlockType;
        }

        /// <summary>
        /// Applies movement blocking based on the specified block type
        /// </summary>
        private Vector3 ApplyMovementBlocking(Vector3 desiredVelocity, MovementBlockType blockType, float deltaTime)
        {
            switch (blockType)
            {
                case MovementBlockType.None:
                    // Restore preserved velocity if transitioning from blocked state
                    if (_wasBlockedLastFrame && _preservedVelocity.sqrMagnitude > 0.01f)
                    {
                        characterMovement.velocity = Vector3.Lerp(characterMovement.velocity, _preservedVelocity, _blockTransitionSpeed * deltaTime);
                        _preservedVelocity = Vector3.zero;
                    }
                    _wasBlockedLastFrame = false;
                    return desiredVelocity;

                case MovementBlockType.SoftStop:
                    _wasBlockedLastFrame = true;
                    return Vector3.zero; // Your current approach - gradual deceleration

                case MovementBlockType.HardStop:
                    _wasBlockedLastFrame = true;
                    // Immediately zero horizontal velocity, preserve vertical for gravity/jumping
                    characterMovement.velocity = new Vector3(0f, characterMovement.velocity.y, 0f);
                    return Vector3.zero;

                case MovementBlockType.Freeze:
                    _wasBlockedLastFrame = true;
                    // Complete velocity lock
                    characterMovement.velocity = Vector3.zero;
                    return Vector3.zero;

                case MovementBlockType.PreserveMomentum:
                    if (!_wasBlockedLastFrame)
                    {
                        // Store current velocity when first blocked
                        _preservedVelocity = characterMovement.velocity;
                    }
                    _wasBlockedLastFrame = true;
                    characterMovement.velocity = new Vector3(0f, characterMovement.velocity.y, 0f);
                    return Vector3.zero;

                default:
                    return desiredVelocity;
            }
        }

        /// <summary>
        /// Determines if rotation should be blocked based on current block type
        /// </summary>
        private bool ShouldBlockRotation(MovementBlockType blockType)
        {
            if (_allowRotationWhenBlocked)
                return false;

            return blockType == MovementBlockType.Freeze;
        }

        #endregion

        private ReplicateData CreateReplicateData()
        {
            if (!IsOwner) return default;
            if (_movingData == null) return default;

            float horizontal = _movingData.MoveInput.x;
            float vertical = _movingData.MoveInput.y;
            
            float cameraYaw = _camera.transform.eulerAngles.y;
            float targetYaw = CalculateTargetYaw(horizontal, vertical, cameraYaw);

            MovementBlockType currentBlockType = GetCurrentBlockType();

            ReplicateData replicateData = new()
            {
                horizontal = horizontal,
                vertical = vertical,
                jump = _jump,
                cameraYaw = cameraYaw,
                targetYaw = targetYaw,
                knockback = _pendingKnockback,
                forceSnapRotation = _forceSnapRotation,
                snapYaw = _snapYaw,
                cameraRelative = CameraRelative,
                isMovementBlocked = _movementBlockers.Count > 0,
                blockType = currentBlockType,
                fromOwner = true
            };

            // Reset one-time actions
            _pendingKnockback = Vector3.zero;
            _jump = false;
            _forceSnapRotation = false;
            _snapYaw = 0f;

            return replicateData;
        }

        /// <summary>
        /// Calculates target yaw based on input and camera orientation
        /// </summary>
        private float CalculateTargetYaw(float horizontal, float vertical, float cameraYaw)
        {
            Vector3 inputDir = new Vector3(horizontal, 0f, vertical);
            
            if (inputDir.sqrMagnitude > 0.01f)
            {
                if (CameraRelative)
                {
                    Quaternion camRot = Quaternion.Euler(0f, cameraYaw, 0f);
                    Vector3 worldDir = camRot * inputDir;
                    return Mathf.Atan2(worldDir.x, worldDir.z) * Mathf.Rad2Deg;
                }
                else
                {
                    return Mathf.Atan2(inputDir.x, inputDir.z) * Mathf.Rad2Deg;
                }
            }
            else if (_movingData.MoveDirection.sqrMagnitude > 0.01f)
            {
                Vector3 worldDir = _movingData.MoveDirection;
                return Mathf.Atan2(worldDir.x, worldDir.z) * Mathf.Rad2Deg;
            }
            
            return characterMovement.transform.eulerAngles.y;
        }

        [Replicate]
        private void RunInputsReplicate(ReplicateData md, ReplicateState state = ReplicateState.Invalid,
            Channel channel = Channel.Unreliable)
        {
            float delta = (float)TimeManager.TickDelta;
            
            // Handle spectator prediction
            if (!IsServerStarted && !IsOwner)
            {
                if (state.ContainsCreated())
                {
                    _lastTickedReplicateData = md;
                }
                else if (state.IsFuture())
                {
                    return; // Prevent over-prediction
                }
            }

            // Prevent future prediction on client
            if (!IsServerOnly && state.IsFuture())
            {
                if (md.GetTick() - _lastTickedReplicateData.GetTick() < PredictionManager.StateInterpolation)
                {
                    md = _lastTickedReplicateData;
                }
            }

            // ====== Jump Logic ======
            if (md.jump && characterMovement.isGrounded && !md.isMovementBlocked)
            {
                characterMovement.PauseGroundConstraint();
                characterMovement.velocity.y = Mathf.Max(characterMovement.velocity.y, jumpImpulse);
            }

            // ====== Knockback Logic ======
            if (md.knockback.sqrMagnitude > 0.001f)
            {
                // Knockback can override movement blocking for gameplay reasons
                characterMovement.velocity = md.knockback;
                _lastAppliedKnockbackTick = md.GetTick();
            }

            // ====== Movement Direction Calculation ======
            Vector3 moveDirection = CalculateMoveDirection(md);
            Vector3 desiredVelocity = moveDirection * maxSpeed;

            // ====== Apply Movement Blocking ======
            desiredVelocity = ApplyMovementBlocking(desiredVelocity, md.blockType, delta);

            // ====== Rotation Logic ======
            bool shouldApplyRotation = md.fromOwner && !ShouldBlockRotation(md.blockType);
            
            if (md.forceSnapRotation)
            {
                characterMovement.rotation = Quaternion.Euler(0f, md.snapYaw, 0f);
            }
            else if (shouldApplyRotation)
            {
                ApplyDeterministicYawRotation(md.targetYaw, delta);
            }

            // ====== Apply Movement ======
            float actualAcceleration = characterMovement.isGrounded ? acceleration : acceleration * airControl;
            float actualDeceleration = characterMovement.isGrounded ? deceleration : 0f;
            float actualFriction = characterMovement.isGrounded ? groundFriction : airFriction;

            // Increase friction when movement is blocked for better stopping
            if (md.isMovementBlocked && md.blockType == MovementBlockType.SoftStop)
            {
                actualFriction *= 3.0f; // Increase friction for faster stopping
            }

            characterMovement.SimpleMove(
                desiredVelocity,
                maxSpeed,
                actualAcceleration,
                actualDeceleration,
                actualFriction,
                actualFriction,
                gravity,
                true,
                delta
            );
        }

        /// <summary>
        /// Calculates movement direction based on input and camera settings
        /// </summary>
        private Vector3 CalculateMoveDirection(ReplicateData md)
        {
            Vector3 moveDirection;
            
            if (md.cameraRelative)
            {
                Quaternion yawRotation = Quaternion.Euler(0f, md.cameraYaw, 0f);
                Vector3 forward = yawRotation * Vector3.forward;
                Vector3 right = yawRotation * Vector3.right;
                moveDirection = right * md.horizontal + forward * md.vertical;
            }
            else
            {
                moveDirection = new Vector3(md.horizontal, 0, md.vertical);
            }
            
            return Vector3.ClampMagnitude(moveDirection, 1f);
        }

        private void ApplyDeterministicYawRotation(float targetYaw, float delta)
        {
            float currentYaw = characterMovement.rotation.eulerAngles.y;
            float maxDelta = rotationRate * delta;
            float newYaw = Mathf.MoveTowardsAngle(currentYaw, targetYaw, maxDelta);
            
            characterMovement.rotation = Quaternion.Euler(0f, newYaw, 0f);
        }

        public override void CreateReconcile()
        {
            ReconcileData reconcileData = new ReconcileData
            {
                position = characterMovement.position,
                rotation = characterMovement.rotation,
                velocity = characterMovement.velocity,
                constrainedToGround = characterMovement.constrainToGround,
                unconstrainedTime = characterMovement.unconstrainedTimer,
                hitGround = characterMovement.currentGround.hitGround,
                isWalkable = characterMovement.currentGround.isWalkable,
                preservedVelocity = _preservedVelocity,
                wasMovementBlocked = _wasBlockedLastFrame
            };

            ReconcileState(reconcileData);
        }

        [Reconcile]
        private void ReconcileState(ReconcileData rd, Channel channel = Channel.Unreliable)
        {
            _lastReconcileRotation = rd.rotation;
            _preservedVelocity = rd.preservedVelocity;
            _wasBlockedLastFrame = rd.wasMovementBlocked;
            
            characterMovement.SetState(
                rd.position,
                rd.rotation,
                rd.velocity,
                rd.constrainedToGround,
                rd.unconstrainedTime,
                rd.hitGround,
                rd.isWalkable);
        }

        #region Public API

        [Button]
        public void AddForce(Vector3 dir)
        {
            characterMovement.AddForce(dir, ForceMode.Impulse);
        }

        [Button]
        public void ApplyKnockback(Vector3 force)
        {
            if (IsOwner)
                _pendingKnockback += force;
        }
        
        public void SnapToYaw(float yaw)
        {
            if (IsOwner)
            {
                _forceSnapRotation = true;
                _snapYaw = yaw;
            }
        }

        /// <summary>
        /// Temporarily override the movement block type for specific gameplay scenarios
        /// </summary>
        public void SetTemporaryBlockType(MovementBlockType blockType)
        {
            _defaultBlockType = blockType;
        }

        #endregion

        #region Unity Lifecycle & Network Events

        private void OnTick()
        {
            ReplicateData replicateData = CreateReplicateData();
            RunInputsReplicate(replicateData);
            CreateReconcile();
        }

        private void OnPostTick()
        {
            // Reserved for post-tick logic
        }

        public override void OnStartNetwork()
        {
            _movingData = GetComponent<ActorBase>().GetData<DS_MovingActor>();
            TimeManager.OnTick += OnTick;
            TimeManager.OnPostTick += OnPostTick;
        }

        public override void OnStopNetwork()
        {
            if (TimeManager == null)
                return;

            TimeManager.OnTick -= OnTick;
            TimeManager.OnPostTick -= OnPostTick;
        }

        private void Awake()
        {
            characterMovement = GetComponent<CharacterMovement>();
            _actor = GetComponent<Actor>();
            _actor.GameplayTags.OnTagAdded += OnTagAdded;
            _actor.GameplayTags.OnTagRemoved += OnTagRemoved;
            _camera = Camera.main;
            
            // Initialize collections
            _movementBlockers = new List<string>();
        }

        private void OnDestroy()
        {
            if (_actor?.GameplayTags != null)
            {
                _actor.GameplayTags.OnTagAdded -= OnTagAdded;
                _actor.GameplayTags.OnTagRemoved -= OnTagRemoved;
            }
        }

        private void OnTagRemoved(GameplayTag obj)
        {
            if (!MovementBlockers.HasTag(obj)) return;
            
            _movementBlockers.Remove(obj.FullTag);
        }

        private void OnTagAdded(GameplayTag obj)
        {
            if (!MovementBlockers.HasTag(obj)) return;
            
            if (!_movementBlockers.Contains(obj.FullTag))
            {
                _movementBlockers.Add(obj.FullTag);
            }
        }

        private void Update()
        {
            if (!IsOwner) return;

            HandleDebugInput();
        }

        private void HandleDebugInput()
        {
            if (Input.GetKeyDown(KeyCode.Space))
                _jump = true;

            if (Input.GetKeyDown(KeyCode.N))
                ApplyKnockback(Vector3.forward * 15);
                
            if (Input.GetKeyDown(KeyCode.M))
                AddForce(Vector3.forward * 15);
                
            if (Input.GetKeyDown(KeyCode.O))
                SnapToYaw(characterMovement.transform.eulerAngles.y + 45);
                
            if (Input.GetKeyDown(KeyCode.L))
                SnapToYaw(characterMovement.transform.eulerAngles.y + 180);
        }

        #endregion
    }
}