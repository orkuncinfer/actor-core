using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Firebase.Auth;
using Firebase.Firestore;
using Newtonsoft.Json;
using NUnit.Framework;
using UnityEngine;
public class FirestoreInventorySynchronizer : MonoBehaviour
{
#if UNITY_ANDROID
    [SerializeField] private InventoryDefinition _inventory;
    private ListenerRegistration _listener;

    private bool _registered;

    void Start()
    {
        _inventory.Initialize();
        
        if (GooglePlayServicesInitialization.Instance.FirebaseSignedIn)
        {
            RegisterListeners();
        }
        else
        {
            GooglePlayServicesInitialization.Instance.onFirebaseSignIn += RegisterListeners;
        }
    }
    public void RegisterListeners()
    {
        if (_registered) return;
        var firestore = FirebaseFirestore.DefaultInstance;
        
        _listener = firestore.Collection("player-data").Document(FirebaseAuth.DefaultInstance.CurrentUser.UserId)
            .Collection("Inventories").Document(_inventory.InventoryId).Listen(snapshot =>
            {
                if (!snapshot.Exists)
                {
                    Debug.Log("Snapshot does not exist.");
                    return;
                }

                InventoryData inventoryData = snapshot.ConvertTo<InventoryData>();
                _inventory.SetInventoryData(inventoryData);
            });
        _registered = true;
    }

    private void OnDestroy()
    {
        if (_listener != null)
        {
            _listener.Stop();
        }
        GooglePlayServicesInitialization.Instance.onFirebaseSignIn -= RegisterListeners;
    }
#endif
}
