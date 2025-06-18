using FishNet.Connection;
using FishNet.Managing.Logging;
using FishNet.Object;
using System;
using FirstGearGames.FPSLand.Managers.Gameplay;
using Sirenix.OdinInspector;
using UnityEngine;

namespace FirstGearGames.FPSLand.Clients
{
    public class PlayerSpawner : NetworkBehaviour
    {
        #region Types.
        public class PlayerData
        {
            public NetworkObject NetworkObject;
        }
        #endregion

        #region Public.

        public EventField _requestRespawnPlayer;
        /// <summary>
        /// Dispatched when the character is updated.
        /// </summary>
        public static event Action<GameObject> OnCharacterUpdated;
        /// <summary>
        /// Data about the currently spawned player.
        /// </summary>
        public PlayerData SpawnedCharacterData { get; private set; } = new PlayerData();
        #endregion

        #region Serialized.
        /// <summary>
        /// Character prefab to spawn.
        /// </summary>
        [Tooltip("Character prefab to spawn.")]
        [SerializeField]
        private GameObject _characterPrefab;
        #endregion

        /// <summary>
        /// Tries to respawn the player.
        /// </summary>
        [Client(Logging = LoggingType.Off)][Button]
        public void TryRespawn()
        {
            CmdRespawn();
        }

        /// <summary>
        /// Sets up SpawnedCharacterData using a gameObject.
        /// </summary>
        /// <param name="go"></param>
        private void SetupSpawnedCharacterData(GameObject go)
        {
            SpawnedCharacterData.NetworkObject = go.GetComponent<NetworkObject>();
        }

        public override void OnStartNetwork()
        {
            base.OnStartNetwork();
            _requestRespawnPlayer.Register(null,OnRequestRespawn);
        }

        public override void OnStopNetwork()
        {
            base.OnStopNetwork();
            _requestRespawnPlayer.Unregister(null,OnRequestRespawn);
        }

        private void OnRequestRespawn(EventArgs obj)
        {
            TryRespawn();
        }

        /// <summary>
        /// Request a respawn from the server.
        /// </summary>
        [ServerRpc]
        private void CmdRespawn()
        {
            Transform spawn = OfflineGameplayDependencies.SpawnManager.ReturnSpawnPoint();
            if (spawn == null)
            {
                Debug.LogError("All spawns are occupied.");
            }
            else
            {
                //If the character is not spawned yet.
                if (SpawnedCharacterData.NetworkObject == null)
                {
                    GameObject r = Instantiate(_characterPrefab, spawn.position, Quaternion.Euler(0f, spawn.eulerAngles.y, 0f));
                    base.Spawn(r, base.Owner);

                    SetupSpawnedCharacterData(r);
                    TargetCharacterSpawned(base.Owner, SpawnedCharacterData.NetworkObject);
                }
                //Character is already spawned.
                else
                {
                    base.Despawn(SpawnedCharacterData.NetworkObject.gameObject);
                    
                    GameObject r = Instantiate(_characterPrefab, spawn.position, Quaternion.Euler(0f, spawn.eulerAngles.y, 0f));
                    base.Spawn(r, base.Owner);

                    SetupSpawnedCharacterData(r);
                    TargetCharacterSpawned(base.Owner, SpawnedCharacterData.NetworkObject);
                    return;
                    SpawnedCharacterData.NetworkObject.transform.position = spawn.position;
                    SpawnedCharacterData.NetworkObject.transform.rotation = Quaternion.Euler(0f, spawn.eulerAngles.y, 0f);
                    Physics.SyncTransforms();
                    //Restore health and set respawned.
                    //SpawnedCharacterData.Health.RestoreHealth();
                    //SpawnedCharacterData.Health.Respawned();
                }

            }
        }

        /// <summary>
        /// Received when the server has spawned the character.
        /// </summary>
        /// <param name="character"></param>
        [TargetRpc]
        private void TargetCharacterSpawned(NetworkConnection conn, NetworkObject character)
        {
            GameObject playerObj = (character == null) ? null : playerObj = character.gameObject;
            OnCharacterUpdated?.Invoke(playerObj);

            //If player was spawned.
            if (playerObj != null)
                SetupSpawnedCharacterData(character.gameObject);
        }

    }


}