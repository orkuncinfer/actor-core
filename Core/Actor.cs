using UnityEngine;


public class Actor : ActorBase
{
    [SerializeField] private MonoState _initialState;
    [SerializeField] private GOPoolMember _poolMember;
    protected override void OnActorStart()
    {
        base.OnActorStart();
        if(_initialState)
            _initialState.CheckoutEnter(this);
    }

    protected override void OnActorStop()
    {
        base.OnActorStop();
        if (_initialState)
        {
            if (_initialState.IsRunning)
            {
                _initialState.CheckoutExit();
            }
        }
        if (_poolMember)
        {
            _poolMember.ReturnToPool();
        }
    }
}