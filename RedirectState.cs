using UnityEngine;

public class RedirectState : MonoState
{
    [SerializeField] private MonoState _redirectTo;
    protected override void OnEnter()
    {
        base.OnEnter();
       _redirectTo.CheckoutEnter(Owner);
    }

    protected override void OnExit()
    {
        base.OnExit();
       _redirectTo.CheckoutExit();
    }
}