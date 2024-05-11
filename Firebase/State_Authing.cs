using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class State_Authing : MonoState
{
    public enum AuthState
    {
        None,
        GooglePlaySignedIn,
        FirebaseSignedIn,
        ServerSignedIn,
        Authed,
        AuthFailed
    }
    [SerializeField] private TextMeshProUGUI _authText;

    protected override void OnEnter()
    {
        base.OnEnter();
        _authText.text = "Authenticating...";
        StartCoroutine(WaitForGooglePlaySignIn());

#if UNITY_EDITOR
        CheckoutExit();
#endif
    }

    public AuthState CurrentState = AuthState.None;
    
    IEnumerator WaitForGooglePlaySignIn()
    {
        while (!GooglePlayServicesInitialization.Instance.GooglePlaySignedIn)
        {
            yield return null;
        }
        CurrentState = AuthState.GooglePlaySignedIn;
        _authText.text = "Google Play Signed In";
        StartCoroutine(WaitForFireBaseSignIn());
    }
    
    IEnumerator WaitForFireBaseSignIn()
    {
        while (!GooglePlayServicesInitialization.Instance.FirebaseSignedIn)
        {
            yield return null;
        }
        CurrentState = AuthState.FirebaseSignedIn;
        _authText.text = "Firebase Signed In";
        StartCoroutine(WaitForServerSignIn());
    }
    
    IEnumerator WaitForServerSignIn()
    {
        while (!GooglePlayServicesInitialization.Instance.FirebaseSignedIn)
        {
            yield return null;
        }
        CurrentState = AuthState.ServerSignedIn;
        CurrentState = AuthState.Authed;
        _authText.text = "Authed";
        CheckoutExit();
    }
}
