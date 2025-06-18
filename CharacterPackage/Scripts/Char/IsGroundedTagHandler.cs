using System;
using ECM2;
using FishNet.Object;
using UnityEngine;

public class IsGroundedTagHandler : NetworkBehaviour
{
    private CharacterMovement _characterMovement;
    private Actor _actor;

    [SerializeField] private GameplayTag _groundedTag;

    private void Start()
    {
        _characterMovement = GetComponent<CharacterMovement>();
        _actor = GetComponent<Actor>();
    }

    private void Update()
    {
        
    }
}
