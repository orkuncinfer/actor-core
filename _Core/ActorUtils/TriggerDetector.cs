using System;
using System.Collections.Generic;
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
        
        [SerializeField]
        private LayerMask _allowedLayers = ~0;
        
        [SerializeField] private GameplayTagContainer _targetMustHaveTags;
        
        [SerializeField] private GameplayTagContainer _targetMustNotHaveTags;
        
        [SerializeField] private List<Actor> _detectedActors = new List<Actor>();
        public List<Actor>  DetectedActors => _detectedActors;

        [SerializeField] private Actor _closestTarget;
        public Actor ClosestTarget => _closestTarget;

        public event Action<Actor> onClosestTargetChanged;

        
        private Collider _collider;
        
        private readonly Dictionary<int, Actor> _actorCache = new();


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

            bool metRequirements = MetRequirements(other, out var detectedActor);

            if (metRequirements)
            {
                _detectedActors.Add(detectedActor);
                detectedActor.GameplayTags.OnTagChanged += OnTagChangedOnTarget;
            }
            
            if (_useDefault && metRequirements)
            {
                onTriggerEnter?.Invoke(other);
            }
            if (_useArgEvent && metRequirements)
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

        private void OnTagChangedOnTarget()
        {
            //negative for loop
            for (int i = _detectedActors.Count - 1; i >= 0; i--)
            {
                Actor actor = _detectedActors[i];
                if (actor == null || !actor.isActiveAndEnabled)
                {
                    _detectedActors.RemoveAt(i);
                    continue;
                }

                if (!actor.GameplayTags.HasAllExact(_targetMustHaveTags) ||
                    actor.GameplayTags.HasAny(_targetMustNotHaveTags))
                {
                    OnTriggerExit(actor.GetComponent<Collider>());
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (_ignoredColliders.Contains(other))
                return;
            
            int id = other.transform.GetInstanceID();
            if (_actorCache.TryGetValue(id, out Actor actor))
            {
                if (_detectedActors.Contains(actor))
                {
                    _detectedActors.Remove(actor);
                    actor.GameplayTags.OnTagChanged -= OnTagChangedOnTarget; 
                }
            }
            
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

        private bool MetRequirements(Collider other,out Actor detectedActor)
        {
            detectedActor = null;
            
            if ((_allowedLayers.value & (1 << other.gameObject.layer)) == 0)
                return false;

            int id = other.transform.GetInstanceID();

            if (!_actorCache.TryGetValue(id, out Actor actor))
            {
                if (other.transform.TryGetComponent(out actor))
                {
                    _actorCache[id] = actor;
                }
                else
                {
                    _actorCache[id] = null; // Cache the miss
                    return false;
                }
            }
            detectedActor = actor;
            if (actor == null)
                return false;

            return actor.GameplayTags.HasAllExact(_targetMustHaveTags) &&
                   !actor.GameplayTags.HasAny(_targetMustNotHaveTags);
        }
        private void Update()
        {
            if (_detectedActors.Count > 0)
            {
                UpdateClosestTarget();
            }
            else
            {
                if (_closestTarget != null)
                {
                    _closestTarget = null;
                    onClosestTargetChanged?.Invoke(null);
                }
            }
        }

        private void UpdateClosestTarget()
        {
            if (_detectedActors.Count == 0)
            {
                if (_closestTarget != null)
                {
                    _closestTarget = null;
                    onClosestTargetChanged?.Invoke(null);
                }
                return;
            }

            float minDistance = float.MaxValue;
            Actor closest = null;
            Vector3 origin = transform.position;

            for (int i = 0; i < _detectedActors.Count; i++)
            {
                Actor actor = _detectedActors[i];
                if (actor == null || !actor.isActiveAndEnabled)
                    continue;

                float distance = Vector3.SqrMagnitude(actor.transform.position - origin);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closest = actor;
                }
            }

            if (_closestTarget != closest)
            {
                _closestTarget = closest;
                onClosestTargetChanged?.Invoke(_closestTarget);
            }
        }
    }
}