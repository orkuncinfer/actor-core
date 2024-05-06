using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using Firebase.Auth;
using UnityEngine.SocialPlatforms;
using System.Threading.Tasks;
using Firebase;
using Firebase.Analytics;
using Firebase.Database;
using Firebase.Extensions;
using GooglePlayGames.OurUtils;
using JetBrains.Annotations;
using Newtonsoft.Json;

public class GooglePlayServicesInitialization : MonoBehaviour
{
    public static GooglePlayServicesInitialization Instance;
    
    private FirebaseDatabase _database;
    public FirebaseUser ThisUser;
    
    public event Action onFirebaseSignIn;
    
    public bool FirebaseSignedIn {get; set;}

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public IEnumerator Start() {

        PlayGamesPlatform.Instance.Authenticate(Social.localUser, async (success, authCode) =>
        {
            if (success)
            {
                PlayGamesPlatform.Instance.RequestServerSideAccess(
                    false,
                    (string token) => FireBaseSignIn(token)
                );
            }
            else
            {
                Debug.Log("Could not connect to Google Play Services.");
                 /*await Firebase.Auth.FirebaseAuth.DefaultInstance.SignInAnonymouslyAsync().ContinueWith(task => {
                    if (task.IsCanceled) {
                        Debug.LogError("SignInAnonymouslyAsync was canceled.");
                        return;
                    }
                    if (task.IsFaulted) {
                        Debug.LogError("SignInAnonymouslyAsync encountered an error: " + task.Exception);
                        return;
                    }

                    Firebase.Auth.AuthResult result = task.Result;
                    Debug.LogFormat("User signed in successfully: {0} ({1})",
                        result.User.DisplayName, result.User.UserId);
                });*/
            }
        });

        yield return new WaitForSeconds(5);
        _database = FirebaseDatabase.GetInstance("https://auto-chain-130cd-default-rtdb.firebaseio.com/");
    }

    internal void ProcessAuthentication(SignInStatus status) {
        if (status == SignInStatus.Success) {
            // Continue with Play Games Services
            Debug.Log("GooglePlay Sign In Succeded  : " + PlayGamesPlatform.Instance.GetUserDisplayName());
        } else
        {
            Debug.Log("GooglePlay Sign In Failed  : " + status);
            // Disable your integration with Play Games Services or show a login button
            // to ask users to sign-in. Clicking it should call
            // PlayGamesPlatform.Instance.ManuallyAuthenticate(ProcessAuthentication).
        }
        
        Debug.Log("GooglePlay Status : " + status);
    }

    public void TrySignIn()
    {
        PlayGamesPlatform.Instance.ManuallyAuthenticate(ProcessAuthentication);
    }

    public void InitializeFirebase()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            if (task.Exception != null)
            {
               Debug.LogError($"Failed to initialize Firebase with {task.Exception}"); 
               return;
            }
            
            Debug.Log("Firebaze Initialize Succesful");
        });
    }

    void FireBaseSignIn(string authCode)
    {
        Debug.Log("AUTHCODE: "+ authCode);
        Firebase.Auth.FirebaseAuth auth = Firebase.Auth.FirebaseAuth.DefaultInstance;
        Firebase.Auth.Credential credential =
            Firebase.Auth.PlayGamesAuthProvider.GetCredential(authCode);
        auth.SignInWithCredentialAsync(credential).ContinueWith(task => {
            if (task.IsCanceled) {
                Debug.LogError("SignInWithCredentialAsync was canceled.");
                return;
            }
            if (task.IsFaulted) {
                Debug.LogError("SignInWithCredentialAsync encountered an error: " + task.Exception);
                return;
            }

            Firebase.Auth.FirebaseUser newUser = task.Result;
            ThisUser = newUser;
            Debug.Log("User signed in successfully: {0} ({1})"+
                newUser.DisplayName + newUser.UserId);
            Debug.LogFormat("User signed in successfully: {0} ({1})",
                newUser.DisplayName, newUser.UserId);

            Firebase.Auth.FirebaseUser user = auth.CurrentUser;
            Debug.Log(user.DisplayName + user.UserId);
            onFirebaseSignIn?.Invoke();
            FirebaseSignedIn = true;
            //SaveLoadManager.Instance.LoadPlayer();
        });
    }

    void InitFireBaseAnalytics()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            var app = FirebaseApp.DefaultInstance;
            FirebaseAnalytics.LogEvent("died",new Parameter("type","spikey_doom"));
        });
    }

   
    
    
}
