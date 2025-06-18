using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterRigHandler : ActorBehaviour
{
    [SerializeField] private GameObject RigPrefab;
    private GameObject _instance;
    protected override void OnStart()
    {
        base.OnStart();
        
        if (RigPrefab)
        {
            _instance = Instantiate(RigPrefab, transform);
        }
    }
}
