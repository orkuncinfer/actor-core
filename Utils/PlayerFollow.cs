using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class PlayerFollow : MonoBehaviour
{
    public GameObjectVariable PlayerFollowerVar;
    public GameObject Player;
    
    private void Awake()
    {
        Vector3 playerPos = Player.transform.position;
 
        PlayerFollowerVar.Value = transform.gameObject;
        
        transform.SetParent(null);
    }

    private void LateUpdate()
    {
        Vector3 playerPos = Player.transform.position;
        transform.position = new Vector3(playerPos.x, transform.position.y, playerPos.z);
    }
}
