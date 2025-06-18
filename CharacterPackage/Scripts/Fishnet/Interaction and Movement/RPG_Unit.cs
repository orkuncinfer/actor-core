using UnityEngine;

public class RPG_Unit : MonoBehaviour,IInteractable,IAttackable
{
    public string InteractionPrompt { get; }
    public bool CanInteract { get; }
    
    public void Interact(GameObject interactor)
    {
        
    }

    public bool CanBeAttacked { get; }
    public void OnAttacked(GameObject attacker)
    {
        
    }
}
