using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Profiling;

namespace Heimdallr.Core
{
    public class TriggerDetector2D : BaseTriggerDetector2D
    {
        public override event UnityAction<Collider2D> onTriggerEnter;
        public override event UnityAction<Collider2D> onTriggerExit;
        public override event UnityAction<TriggerEventArgs2D> onArgTriggerEnter;
        public override event UnityAction<TriggerEventArgs2D> onArgTriggerExit;

        [SerializeField]
        private bool _useArgEvent;
        [SerializeField]
        private bool _useDefault = true;

        private Collider2D _collider;

        public override Collider2D Collider => _collider;

        public override void Start()
        {
            _collider = GetComponent<Collider2D>();
        }

        private bool _deleteLater;
        
        private void OnTriggerEnter2D(Collider2D other)
        {
            Profiler.BeginSample("TriggerEnter");
            _deleteLater = true;
            Profiler.EndSample();
            
            if (_ignoredColliders.Contains(other))
                return;
            if(_useDefault)
                onTriggerEnter?.Invoke(other);
            if (_useArgEvent)
            {
                TriggerEventArgs2D args = new TriggerEventArgs2D();
                args.OtherCollider = other;
                args.Detector = this;

                if (other.TryGetComponent(out IColliderOwnerPointer ownerPointer))
                {
                    args.ColliderOwnerPointer = ownerPointer;
                    args.CollidedMain = ownerPointer.GetFinalOwner();
                    args.HasCollidedMain = args.CollidedMain != null;
                    args.CollidedGameObject = ownerPointer.GetFinalGameObjectOwner();
                    args.OwnerTagContainer = args.HasCollidedMain
                        ? args.CollidedMain
                        : args.CollidedGameObject.GetComponent<ITagContainer>();
                }
                
                onArgTriggerEnter?.Invoke(args);
            }
        }
        
        private void OnTriggerExit2D(Collider2D other)
        {
            if (_ignoredColliders.Contains(other))
                return;
            if(_useDefault)
                onTriggerExit?.Invoke(other);
            if (_useArgEvent)
            {
                TriggerEventArgs2D args = new TriggerEventArgs2D();
                args.OtherCollider = other;
                args.Detector = this;

                if (other.TryGetComponent(out IColliderOwnerPointer ownerPointer))
                {
                    args.ColliderOwnerPointer = ownerPointer;
                    args.CollidedMain = ownerPointer.GetFinalOwner();
                    args.HasCollidedMain = args.CollidedMain != null;
                    args.CollidedGameObject = ownerPointer.GetFinalGameObjectOwner();
                    args.OwnerTagContainer = args.HasCollidedMain
                        ? args.CollidedMain
                        : args.CollidedGameObject.GetComponent<ITagContainer>();
                }
                
                onArgTriggerExit?.Invoke(args);
            }
        }
    }
}