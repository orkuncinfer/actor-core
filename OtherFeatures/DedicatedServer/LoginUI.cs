using System;
using System.Collections;
using System.Collections.Generic;
using NetworkShared.Packets.ClientServer;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoginUI : MonoBehaviour
{
    [SerializeField] private TMP_InputField _username;
    [SerializeField] private TMP_InputField _password;

    [SerializeField] private TextMeshProUGUI _loginStatus;
    private bool _isConnected;
    private void Start()
    {
        NetworkClient.Instance.onServerConnected += OnServerConnected;
        NetworkClient.Instance.onServerDisconnected += OnServerDisconnected;
    }

    private void OnServerDisconnected()
    {
        _isConnected = false;
        _loginStatus.text = "Disconnected from server!";
    }

    private void OnServerConnected()
    {
        _isConnected = true;
        _loginStatus.text = "Connected to server!";
    }

    public void Login()
    {
        StartCoroutine(LoginRoutine());
    }

    public void GenerateItem()
    {
        var generateItemRequest = new Net_GenerateItemRequest
        {
            MonsterId = "mns_golem",
        };
        NetworkClient.Instance.SendServer(generateItemRequest);
    }
    
    IEnumerator LoginRoutine()
    {
        NetworkClient.Instance.Connect();
        while (!_isConnected)
        {
            _loginStatus.text = "Waiting for connection...";
            yield return null;
        }

        var authRequest = new Net_AuthRequest
        {
            Username = _username.text,
            Password = _password.text
        };
        NetworkClient.Instance.SendServer(authRequest);
    }
}
