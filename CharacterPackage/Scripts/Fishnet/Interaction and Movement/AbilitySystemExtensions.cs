using FishNet.Object;
using UnityEngine;

// Extension methods for Service_GAS to integrate with command system
public static class AbilitySystemExtensions
{
    public static bool TryActivateAbility(this Service_GAS abilitySystem, string abilityName, GameObject target)
    {
        // Set target if provided
        if (target != null)
        {
            abilitySystem.AbilityController.Target = target;
        }
        
        // Try to activate ability by name
        // This is a placeholder - replace with your actual ability activation logic
        // You might have different ways to activate abilities in your system
        
        // Example implementation (adjust to your actual ability system):
        /*
        var ability = abilitySystem.GetAbilityByName(abilityName);
        if (ability != null && ability.CanUse())
        {
            ability.Activate();
            return true;
        }
        */
        
        return false;
    }
    
    public static bool IsAbilityOnCooldown(this Service_GAS abilitySystem, string abilityName)
    {
        // Placeholder - implement based on your ability system
        return false;
    }
    
    public static float GetAbilityRange(this Service_GAS abilitySystem, string abilityName)
    {
        // Placeholder - implement based on your ability system
        return 3f; // Default range
    }
}

// Interface for attackable objects
public interface IAttackable
{
    bool CanBeAttacked { get; }
    void OnAttacked(GameObject attacker);
}

// Interface for interactive objects
public interface IInteractable
{
    string InteractionPrompt { get; }
    bool CanInteract { get; }
    void Interact(GameObject interactor);
}

// Example implementation for an attackable enemy
public class AttackableEnemy : MonoBehaviour, IAttackable
{
    [Header("Combat Settings")]
    [SerializeField] private float _health = 100f;
    [SerializeField] private bool _isAlive = true;
    
    public bool CanBeAttacked => _isAlive && _health > 0;
    
    public void OnAttacked(GameObject attacker)
    {
        // Handle being attacked
        // This is where you'd apply damage, trigger reactions, etc.
        Debug.Log($"{gameObject.name} was attacked by {attacker.name}!");
    }
    
    // Example damage method
    public void TakeDamage(float damage)
    {
        _health -= damage;
        if (_health <= 0)
        {
            _isAlive = false;
            // Handle death
        }
    }
}

// Example context for special interaction abilities
public class AbilityContext : InteractionContext
{
    private readonly Service_GAS _abilitySystem;
    private readonly Network_Movement _movement;
    private readonly string _abilityName;
    private readonly float _range;
    
    public AbilityContext(Service_GAS abilitySystem, Network_Movement movement, string abilityName, float range)
    {
        _abilitySystem = abilitySystem;
        _movement = movement;
        _abilityName = abilityName;
        _range = range;
    }
    
    public override int Priority => 8; // Between attack and collect
    
    public override ICommand CreateCommand(Vector3 position, GameObject target, NetworkBehaviour source)
    {
        return new AbilityCommand(_abilitySystem, _movement, target, _abilityName, _range);
    }
    
    public override bool CanHandle(GameObject target)
    {
        // Check if target has the required component for this ability
        return target != null && target.GetComponent<IInteractable>() != null;
    }
}

// Command for special abilities
public class AbilityCommand : ICommand
{
    private readonly Service_GAS _abilitySystem;
    private readonly Network_Movement _movement;
    private readonly GameObject _target;
    private readonly string _abilityName;
    private readonly float _range;
    private Coroutine _abilityCoroutine;
    
    public bool IsComplete { get; private set; }
    public float Priority => 8;
    
    public AbilityCommand(Service_GAS abilitySystem, Network_Movement movement, GameObject target, string abilityName, float range)
    {
        _abilitySystem = abilitySystem;
        _movement = movement;
        _target = target;
        _abilityName = abilityName;
        _range = range;
    }
    
    public void Execute()
    {
        _abilityCoroutine = _movement.StartCoroutine(ExecuteAbility());
        IsComplete = false;
    }
    
    public void Cancel()
    {
        if (_abilityCoroutine != null)
        {
            _movement.StopCoroutine(_abilityCoroutine);
            _movement.StopMovement();
        }
        IsComplete = true;
    }
    
    private System.Collections.IEnumerator ExecuteAbility()
    {
        // Move to target
        yield return _movement.MoveToTarget(_target, _range);
        
        // Check if target is still valid
        if (_target == null || !_target.activeInHierarchy)
        {
            IsComplete = true;
            yield break;
        }
        
        // Face the target
        yield return _movement.FaceTarget(_target);
        
        // Activate the ability
        if (_abilitySystem.TryActivateAbility(_abilityName, _target))
        {
            // Wait for ability animation/execution
            yield return new WaitForSeconds(1f); // Adjust based on ability duration
        }
        
        IsComplete = true;
    }
}