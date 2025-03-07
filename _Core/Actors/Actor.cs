using UnityEngine;

public class Actor : ActorBase
{
    [SerializeField] private MonoState _initialState;
    
    [SerializeField] private bool _returnToPoolOnStop = false;
    protected override void OnActorStart()
    {
        base.OnActorStart();
        if(_initialState)
            _initialState.CheckoutEnter(this);
        
        ActorRegistry.RegisterActor(this);
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
        if (_returnToPoolOnStop)
        {
            PoolManager.ReleaseObject(gameObject);
        }
    }
}