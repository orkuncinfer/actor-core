using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

namespace Heimdallr.Core
{
    [System.Serializable]
    public struct TriggerEventArgs2D
    {
        public BaseTriggerDetector2D Detector;
        public IColliderOwnerPointer ColliderOwnerPointer;
        public Collider2D OtherCollider;
        public bool HasCollidedMain;
        public Actor CollidedMain;
        public ITagContainer OwnerTagContainer;
        public GameObject CollidedGameObject;
    }
    
    public abstract class BaseTriggerDetector2D : MonoBehaviour
    {
        [ShowInInspector][ReadOnly][FoldoutGroup("Output")]
        protected HashSet<Collider2D> _ignoredColliders = new HashSet<Collider2D>();

        public HashSet<Collider2D> IgnoredColliders
        {
            get => _ignoredColliders;
            set => _ignoredColliders = value;
        }
        
        public abstract event UnityAction<Collider2D> onTriggerEnter;
        public abstract event UnityAction<Collider2D> onTriggerExit;
        public abstract event UnityAction<TriggerEventArgs2D> onArgTriggerEnter;
        public abstract event UnityAction<TriggerEventArgs2D> onArgTriggerExit;
        
        public abstract Collider2D Collider { get; }
        public abstract void Start();
    }
}