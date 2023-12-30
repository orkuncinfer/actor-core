using Sirenix.OdinInspector;
using UnityEngine;


    public class Actor : ActorBase
    {
        [SerializeField] private MonoState _initialState;
        [SerializeField] private GOPoolMember _poolMember;
    
        protected override void OnActorStart()
        {
            base.OnActorStart();
            _initialState.CheckoutEnter(this);
        }

        protected override void OnActorStop()
        {
            base.OnActorStop();
            _initialState.CheckoutExit();
            if(_poolMember) _poolMember.ReturnToPool();
        }

        
    }

