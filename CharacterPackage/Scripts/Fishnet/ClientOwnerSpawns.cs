using System.Collections.Generic;
using FishNet.Object;
using UnityEngine;

public class ClientOwnerSpawns : NetworkBehaviour
{
    public Actor OwnerActor;
    public List<GameObject> Prefabs;

    public override void OnStartClient()
    {
        base.OnStartClient();
        if (base.IsOwner)
        {
            foreach (var prefab in Prefabs)
            {
                GameObject instance = Instantiate(prefab,transform);
                instance.transform.position = new Vector3(0, 0, 0);
                instance.transform.rotation = Quaternion.identity;
                
                if(instance.TryGetComponent(out OwnerActorPointer pointer))
                {
                    pointer.SetPointedActor(OwnerActor);
                }
            }
        }
    }
}
