using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using FishNet.Object;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Network_Movement))]
public class Network_InteractionController : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private Camera _camera;
    [SerializeField] private LayerMask _interactionMask = -1;
    [SerializeField] private InputActionReference _movementInputAction;
    
    [Header("Combat Settings")]
    [SerializeField] private float _attackRange = 3f;
    [SerializeField] private string _attackAbilityName = "BasicAttack";
    [SerializeField] private GameplayTag _attackAbilityTag;
    
    [Header("Visual Effects")]
    [SerializeField] private GameObject _hoverEffect;
    [SerializeField] private GameObject _selectedEffect;
    
    // Component References
    private Network_Movement _movement;
    private ActorBase _actor;
    private Service_GAS _abilitySystem;
    
    // Command Management
    private ICommand _currentCommand;
    private readonly List<InteractionContext> _contexts = new List<InteractionContext>();
    
    // Hover/Selection State
    private GameObject _hoveredObject;
    private GameObject _selectedObject;
    private GameObject _hoverEffectInstance;
    private GameObject _selectedEffectInstance;
    
    public override void OnStartClient()
    {
        base.OnStartClient();
        
        if (!IsOwner) return;
        
        // Initialize components
        _movement = GetComponent<Network_Movement>();
        _actor = GetComponent<ActorBase>();
        _abilitySystem = _actor.GetService<Service_GAS>();
        
        if (_camera == null)
            _camera = Camera.main;
        
        // Setup contexts in priority order
        InitializeContexts();
        
        // Setup visual effects
        SetupVisualEffects();
    }
    
    private void InitializeContexts()
    {
        _contexts.Add(new AttackContext(_abilitySystem, _movement, _attackRange,_attackAbilityTag));
        _contexts.Add(new InteractionContext_Collectible(_movement));
        _contexts.Add(new MovementContext(_movement));
        
        // Sort by priority (highest first)
        _contexts.Sort((a, b) => b.Priority.CompareTo(a.Priority));
    }
    
    private void SetupVisualEffects()
    {
        _hoverEffectInstance = Instantiate(_hoverEffect);
        _hoverEffectInstance.SetActive(false);
        
        _selectedEffectInstance = Instantiate(_selectedEffect);
        _selectedEffectInstance.SetActive(false);
    }
    
    void Update()
    {
        if (!IsOwner) return;
        
        // Check for input interruption
        if (_movementInputAction.action.ReadValue<Vector2>() != Vector2.zero)
        {
            CancelCurrentCommand();
        }
        
        // Don't process clicks if over UI
        
        
        // Update hover effect
        UpdateHoverState();
        
        // Handle mouse clicks
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            HandleLeftClick();
        }
        else if (Mouse.current.rightButton.wasPressedThisFrame)
        {
            HandleRightClick();
        }
    }
    
    private void UpdateHoverState()
    {
        if (_camera.pixelRect.Contains(Input.mousePosition) && 
            Physics.Raycast(_camera.ScreenPointToRay(Input.mousePosition), out var hit, Mathf.Infinity, _interactionMask))
        {
            var hoveredObject = hit.collider.gameObject;
            
            if (_hoveredObject != hoveredObject)
            {
                SetHoveredObject(hoveredObject);
            }
        }
        else
        {
            SetHoveredObject(null);
        }
    }
    
    private void HandleLeftClick()
    {
        if (!_camera.pixelRect.Contains(Input.mousePosition)) return;
        
        HandleUIInteractions();
        if (UIRaycastHelper.IsPointerOverUI()) return;
        
        if (Physics.Raycast(_camera.ScreenPointToRay(Input.mousePosition), out var hit, Mathf.Infinity, _interactionMask))
        {
            // Try to find appropriate context
            var hitObject = hit.collider.gameObject;
            var context = FindContextForTarget(hitObject);
            
            if (context != null)
            {
                // Set selection
                SetSelectedObject(hitObject);
                
                // Create and execute command
                ExecuteCommand(context.CreateCommand(hit.point, hitObject, this));
            }
        }
        else
        {
            // Clicked on empty space, just move there
            var groundContext = _contexts.FirstOrDefault(c => c is MovementContext);
            if (groundContext != null)
            {
                ExecuteCommand(groundContext.CreateCommand(hit.point, null, this));
            }
        }
    }
    
    private void HandleRightClick()
    {
        // Right-click behavior (can be customized)
        // For now, force attack if possible
        if (!_camera.pixelRect.Contains(Input.mousePosition)) return;
        
        HandleUIInteractions();
        if (UIRaycastHelper.IsPointerOverUI()) return;
        
        if (Physics.Raycast(_camera.ScreenPointToRay(Input.mousePosition), out var hit, Mathf.Infinity, _interactionMask))
        {
            var attackContext = _contexts.FirstOrDefault(c => c is AttackContext) as AttackContext;
            if (attackContext != null && attackContext.CanHandle(hit.collider.gameObject))
            {
                SetSelectedObject(hit.collider.gameObject);
                //ExecuteCommand(attackContext.CreateCommand(hit.point, hit.collider.gameObject, this));
            }
        }
    }
    
    
    private void HandleUIInteractions()
    {
        if (EventSystem.current == null)
            return;
        
        PointerEventData eventData = new PointerEventData(EventSystem.current);
        eventData.position = Input.mousePosition;
        
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        if (results.Count > 0)
        {
            var hitObject = results[0].gameObject;
            var context = FindContextForTarget(hitObject);
            
            Vector3 pos = hitObject.transform.position;
            
            if (context != null)
            {
                ExecuteCommand(context.CreateCommand(pos, hitObject, this));
            }
        }
    }
    
    private InteractionContext FindContextForTarget(GameObject target)
    {
        // Find highest priority context that can handle this target
        return _contexts.FirstOrDefault(context => context.CanHandle(target));
    }
    
    private void ExecuteCommand(ICommand command)
    {
        // Cancel current command if any
        CancelCurrentCommand();
        
        // Execute new command
        _currentCommand = command;
        _currentCommand.Execute();
    }
    
    private void CancelCurrentCommand()
    {
        if (_currentCommand != null && !_currentCommand.IsComplete)
        {
            _currentCommand.Cancel();
        }
        _currentCommand = null;
    }
    
    private void SetHoveredObject(GameObject obj)
    {
        _hoveredObject = obj;
        
        if (obj != null && obj != _selectedObject)
        {
            var actorBase = obj.GetComponent<ActorBase>();
            if (actorBase != null)
            {
                var socket = actorBase.GetSocket("root");
                if (socket != null)
                {
                    ShowHoverEffect(socket);
                    return;
                }
            }
        }
        
        ShowHoverEffect(null);
    }
    
    private void SetSelectedObject(GameObject obj)
    {
        _selectedObject = obj;
       
        
        if (obj != null)
        {
            var actorBase = obj.GetComponent<ActorBase>();
            if (actorBase != null)
            {
                _actor.GetService<Service_GAS>().AbilityController.Target = obj;
                var socket = actorBase.GetSocket("root");
                if (socket != null)
                {
                    ShowSelectionEffect(socket);
                    return;
                }
            }
        
        }
        _actor.GetService<Service_GAS>().AbilityController.Target = null;
        ShowSelectionEffect(null);
    }
    
    private void ShowHoverEffect(Transform target)
    {
        if (_hoverEffectInstance == null)
        {
            _hoverEffectInstance = Instantiate(_hoverEffect);
        }
        
        if (target != null)
        {
            _hoverEffectInstance.transform.SetParent(target);
            _hoverEffectInstance.transform.localPosition = Vector3.zero;
            _hoverEffectInstance.SetActive(true);
        }
        else
        {
            _hoverEffectInstance.transform.SetParent(null);
            _hoverEffectInstance.SetActive(false);
        }
    }
    
    private void ShowSelectionEffect(Transform target)
    {
        if (_selectedEffectInstance == null)
        {
            _selectedEffectInstance = Instantiate(_selectedEffect);
        }
        
        // Hide hover effect when selecting
        if (target != null)
        {
            _hoverEffectInstance.SetActive(false);
        }
        
        if (target != null)
        {
            _selectedEffectInstance.transform.SetParent(target);
            _selectedEffectInstance.transform.localPosition = Vector3.zero;
            _selectedEffectInstance.SetActive(true);
        }
        else
        {
            _selectedEffectInstance.transform.SetParent(null);
            _selectedEffectInstance.SetActive(false);
        }
    }
    
    // Public API for other systems
    public void SetTarget(GameObject target)
    {
        SetSelectedObject(target);
        
        // If targeting an attackable, create attack command
        var attackContext = _contexts.FirstOrDefault(c => c is AttackContext) as AttackContext;
        if (attackContext != null && attackContext.CanHandle(target))
        {
            ExecuteCommand(attackContext.CreateCommand(target.transform.position, target, this));
        }
    }
    
    public GameObject GetCurrentTarget()
    {
        return _selectedObject;
    }
    
    public bool IsExecutingCommand()
    {
        return _currentCommand != null && !_currentCommand.IsComplete;
    }
    
    // Cleanup
    private void OnDestroy()
    {
        if (_hoverEffectInstance != null)
            Destroy(_hoverEffectInstance);
            
        if (_selectedEffectInstance != null)
            Destroy(_selectedEffectInstance);
    }
}