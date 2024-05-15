using System.Collections;
using System.Collections.Generic;
using Firebase.Auth;
using NetworkShared.Packets.ClientServer;
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
        NetworkClient.Instance.TestConnectWithTestId();
        NetworkClient.Instance.TestAuth();
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
        NetworkClient.Instance.Connect();
        while (!NetworkClient.Instance.IsConnected)
        {
            yield return null;
        }
        
        var authRequest = new Net_AuthRequest
        {
            Username = FirebaseAuth.DefaultInstance.CurrentUser.UserId,
            Password = ""
        };
        NetworkClient.Instance.SendServer(authRequest);
        
        while (!GooglePlayServicesInitialization.Instance.DedicatedServerSignedIn)
        {
            yield return null;
        }
        CurrentState = AuthState.ServerSignedIn;
        CurrentState = AuthState.Authed;
        _authText.text = "Authed";
        CheckoutExit();
    }
}
