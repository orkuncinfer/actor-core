using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

namespace Heimdallr.Core
{
    [System.Serializable]
    public struct TriggerEventArgs
    {
        public BaseTriggerDetector Detector;
        public IColliderOwnerPointer ColliderOwnerPointer;
        public Collider OtherCollider;
        public bool HasCollidedMain;
        public Actor CollidedMain;
        public ITagContainer OwnerTagContainer;
        public GameObject CollidedGameObject;
    }
    
    public class ParticleTriggerDetector : BaseTriggerDetector
    {
        public override event UnityAction<Collider> onTriggerEnter;
        public override event UnityAction<Collider> onTriggerExit;
        public override event UnityAction<TriggerEventArgs> onArgTriggerEnter;
        public override event UnityAction<TriggerEventArgs> onArgTriggerExit;
        public override Collider Collider { get; }

        [SerializeField] private ParticleSystem _particleSystem;
        
        public override void Start()
        {
            
        }

        private void OnParticleTrigger()
        {
            
        }
    }
    
    public abstract class BaseTriggerDetector : MonoBehaviour
    {
        [ShowInInspector][ReadOnly][FoldoutGroup("Output")]
        protected HashSet<Collider> _ignoredColliders = new HashSet<Collider>();

        public HashSet<Collider> IgnoredColliders
        {
            get => _ignoredColliders;
            set => _ignoredColliders = value;
        }
        
        public abstract event UnityAction<Collider> onTriggerEnter;
        public abstract event UnityAction<Collider> onTriggerExit;
        public abstract event UnityAction<TriggerEventArgs> onArgTriggerEnter;
        public abstract event UnityAction<TriggerEventArgs> onArgTriggerExit;
        
        public abstract Collider Collider { get; }
        public abstract void Start();

        [SerializeField] private bool _showWireGizmos;
        [SerializeField] private bool _showClickableGizmos;

        private void OnDrawGizmos()
        {
            Collider col = GetComponent<Collider>();

            if (_showWireGizmos)
            {
                if (col is BoxCollider boxCollider)
                {
                    Gizmos.DrawWireCube(boxCollider.bounds.center,boxCollider.bounds.size);
                }

                if (col is SphereCollider sphereCollider)
                {
                    Gizmos.DrawWireSphere(sphereCollider.bounds.center,sphereCollider.radius);
                }
            
                if (col is CapsuleCollider capsuleCollider)
                {
                    Gizmos.DrawWireCube(capsuleCollider.bounds.center
                        ,
                        new Vector3
                        ( capsuleCollider.radius,
                            capsuleCollider.height,
                            capsuleCollider.radius
                        ));
                }
            }

            if (_showClickableGizmos)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawCube(col.transform.position,Vector3.one);
            }
        }
    }
}