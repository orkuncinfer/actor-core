using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Profiling;

namespace Heimdallr.Core
{

    public class TriggerDetector : BaseTriggerDetector
    {
        public override event UnityAction<Collider> onTriggerEnter;
        public override event UnityAction<Collider> onTriggerExit;
        public override event UnityAction<TriggerEventArgs> onArgTriggerEnter;
        public override event UnityAction<TriggerEventArgs> onArgTriggerExit;

        [SerializeField]
        private bool _useArgEvent;
        [SerializeField]
        private bool _useDefault = true;

        private Collider _collider;

        public override Collider Collider => _collider;

        public override void Start()
        {
            _collider = GetComponent<Collider>();
        }

        private bool _deleteLater;
        private void OnTriggerEnter(Collider other)
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
                TriggerEventArgs args = new TriggerEventArgs();
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
        private void OnTriggerExit(Collider other)
        {
            if (_ignoredColliders.Contains(other))
                return;
            if(_useDefault)
                onTriggerExit?.Invoke(other);
            if (_useArgEvent)
            {
                TriggerEventArgs args = new TriggerEventArgs();
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