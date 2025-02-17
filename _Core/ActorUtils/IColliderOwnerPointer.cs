using UnityEngine;

namespace Heimdallr.Core
{
    public interface IColliderOwnerPointer
    {
        void SetInitialOwner(Actor owner);
        void SetOwner(Actor main);
        void SetOwner(GameObject ownerGameObject);
        Actor Owner { get; }
        GameObject OwnerGameObject { get; }
        Actor GetFinalOwner();
        GameObject GetFinalGameObjectOwner();
        
        bool OwnerIsMainBehaviour { get; }
    }
}