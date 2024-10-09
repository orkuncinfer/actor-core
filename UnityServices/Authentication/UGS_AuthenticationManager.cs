using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
#if UNITY_ANDROID
using GooglePlayGames;
using GooglePlayGames.BasicApi;
#endif
using TMPro;
using UnityEngine;

public class UGS_AuthenticationManager : MonoBehaviour
{
   /* public string Token;
    public string Error;
    
    public TextMeshProUGUI _errorText;
    public TextMeshProUGUI _tokenText;

    public EventSignal OnAuthCompleted;

    
    private  void Awake()
    {
        //Initialize PlayGamesPlatform
        PlayGamesPlatform.Activate();
        LoginGooglePlayGames();
        //SaveData();
    }

    public void LoginGooglePlayGames()
    {
        PlayGamesPlatform.Instance.Authenticate((success) =>
        {
            if (success == SignInStatus.Success)
            {
                Debug.Log("Login with Google Play games successful.");
             UnityServices.InitializeAsync().ContinueWith(initialize =>
                {
                    if (initialize.IsCanceled)
                    {
                        Debug.LogError("UnityServices.InitializeAsync was canceled.");
                        return;
                    }

                    if (initialize.IsFaulted)
                    {
                        Debug.LogError("UnityServices.InitializeAsync encountered an error: " + initialize.Exception);
                        return;
                    }

                    Debug.Log("UnityServices.InitializeAsync completed successfully.");
                });   

                
                PlayGamesPlatform.Instance.RequestServerSideAccess(true, code =>
                {
                    
                    Debug.Log("Authorization code: " + code);
                    Token = code;
                    _tokenText.text = Token;
                    AuthenticationService.Instance.SignInWithGooglePlayGamesAsync(code).ContinueWith(task => {
                        if (task.IsCanceled) {
                            Debug.LogError("SignInWithGooglePlayGamesAsync was canceled.");
                            return;
                        }
                        if (task.IsFaulted) {
                            Debug.LogError("SignInWithGooglePlayGamesAsync encountered an error: " + task.Exception);
                            return;
                        }
                        if (task.IsCompletedSuccessfully)
                        {
                            Debug.Log("SignInWithGooglePlayGamesAsync completed successfully.");
                            OnAuthCompleted.Raise();
                        }
                        
                       // SaveData();
                    });

                });
            }
            else
            {
                Error = "Failed to retrieve Google play games authorization code";
                _errorText.text = Error;
                Debug.Log("Login Unsuccessful");
            }

        });

    }

    
    public void SaveMyData()
    {
        SaveData();
    }
    public async void  SaveData()
    {
        var playerData = new Dictionary<string, object>{
            {"firstKeyName", "a text value"},
            {"secondKeyName", 123}
        };
        Debug.Log("Player Username : " + AuthenticationService.Instance.PlayerInfo.Username);
        Debug.Log("Player ID : " + AuthenticationService.Instance.PlayerInfo.Id);
        Debug.Log(PlayGamesPlatform.Instance.GetUserDisplayName());
        /*try
        {
            var playerName = AuthenticationService.Instance.GetPlayerNameAsync(); // Attempt to access PlayerName
            Debug.Log("Player Name : " + playerName.Result);
        }
        catch (Exception ex)
        {
            Debug.LogError("Error when getting Player Name: " + ex.ToString());
            return; // Exit if we cannot even fetch the player name
        }
        
        try
        {
            await CloudSaveService.Instance.Data.Player.SaveAsync(playerData);
            Debug.Log($"Data saved successfully: {string.Join(", ", playerData.Select(kv => $"{kv.Key}={kv.Value}"))}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to save data: {ex.Message}");
        }
        Debug.Log($"Saved data {string.Join(',', playerData)}");
    }*/
}