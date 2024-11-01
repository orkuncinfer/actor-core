using Sirenix.Utilities;
using UnityEngine;
using UnityEngine.InputSystem;

public class State_WeaponShootHandling : MonoState
{
    public InputActionAsset ActionAsset;

    [SerializeField] private string[] EquipActionNames;
    
    private int _lastTriedSlotIndex;
        

}