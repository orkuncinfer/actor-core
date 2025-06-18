using System;
using System.Collections;
using System.Collections.Generic;
using Oddworm.Framework;
using RootMotion.FinalIK;
using UnityEngine;

public class MeleeWeapon : Equippable, ICastable
{
    public LayerMask LayerMask;
    public BoxCollider Collider;

    public event Action<Collider> onHit;

    private void Awake()
    {
        ItemData.Attributes.Add("ItemAttribute_AttackSpeed","1,2");//test
    }

    public void Cast()
    {
        Vector3 worldCenter = Collider.transform.TransformPoint(Collider.center);
        Vector3 worldHalfExtents = Collider.size * 0.5f; // only necessary when collider is scaled by non-uniform transform
        float maxDistance = 10f; // Max distance for the cast
        RaycastHit hitInfo;

        // Perform the BoxCast
        
        Collider[] hitColliders = new Collider[10];  // Adjust the size as necessary
        int numColliders = Physics.OverlapBoxNonAlloc(worldCenter, worldHalfExtents, hitColliders, Collider.transform.rotation, LayerMask);
        
        Color debugColor = Color.red;
        debugColor.a = 0.4f;
        DbgDraw.WireCube(worldCenter, Collider.transform.rotation, worldHalfExtents * 2, debugColor, 5);
        
        for (int i = 0; i < numColliders; i++)
        {
            if(hitColliders[i].transform == Owner.transform) continue;
            //DDebug.Log("Hit : " + hitColliders[i].name + i);
            if(hitColliders[i].transform.TryGetComponent(out Actor actor))
            {
                onHit?.Invoke(hitColliders[i]);
            }
        }
        // ExtDebug.DrawBox(center,halfExtents,transform.rotation,Color.red);
    }

    public override void OnEquip(Actor owner)
    {
        base.OnEquip(owner);
    }
}
