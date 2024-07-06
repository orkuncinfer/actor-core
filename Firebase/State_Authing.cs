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

    protected override void OnEnter()
    {
        base.OnEnter();
#if GOOGLE_PLAY_ENABLED
        StartCoroutine(WaitForGooglePlaySignIn());
#endif
        

#if UNITY_EDITOR && SERVER_ENABLED
        NetworkClient.Instance.TestConnectWithTestId();
        NetworkClient.Instance.TestAuth();
        CheckoutExit();
#elif UNITY_EDITOR || !GOOGLE_PLAY_ENABLED
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
        StartCoroutine(WaitForFireBaseSignIn());
    }
    
    IEnumerator WaitForFireBaseSignIn()
    {
        while (!GooglePlayServicesInitialization.Instance.FirebaseSignedIn)
        {
            yield return null;
        }
        CurrentState = AuthState.FirebaseSignedIn;

#if SERVER_ENABLED
        StartCoroutine(WaitForServerSignIn());
#else
        CheckoutExit();
#endif
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
        CheckoutExit();
    }
}
