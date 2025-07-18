using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileStandardCollisionComponent : ProjectileCollisionComponent
{
    [Header("Projectile Collision Settings")]
    [SerializeField] int maxCollisions = 1;
    [SerializeField] bool canBounce;
    int collisionCount = 0;
    

    void OnCollisionEnter(Collision collision){
        
        if(collisionCount == maxCollisions) return;
        
        if (canBounce && CheckIfGroundCollision(collision)) return; // if can bounce and hit the ground, no action needed

        if (hasEffect){
            InitialiseEffect(collision.contacts[0].point ,collision);
        }
        damageComponent.ApplyDamage(collision.gameObject,collision);
        collisionCount++;

        if (collisionCount >= maxCollisions)
        {
            collisionCount = 0;
            PoolManager.ReleaseObject(gameObject,true);
        }
    }
}
