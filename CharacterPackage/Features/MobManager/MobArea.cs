using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FishNet.Managing;
using FishNet.Managing.Server;
using FishNet.Object;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class MobArea : NetworkBehaviour
{
    [System.Serializable]
    public class MobGroup
    {
    public MobGroupDefinition MobGroupDefinition;
        public List<Transform> SpawnPoints;
        public List<GameObject> activeEnemies = new List<GameObject>();
        [HideInInspector] public float respawnTimer = 0f;
    }

    [Header("Mob Area Settings")]
    [SerializeField] private List<MobGroup> possibleMobGroups = new List<MobGroup>();
    [SerializeField] private int maxTotalMobCount = 15;
    [SerializeField] private float respawnTime = 30f;
    [SerializeField] private float respawnVariance = 10f;
    
    [Header("Debug")]
    [SerializeField] private bool showGizmos = true;
    [SerializeField] private Color gizmoColor = new Color(1f, 0.5f, 0.2f, 0.3f);
    
    private List<MobGroup> activeMobGroups = new List<MobGroup>();
    private List<MobGroup> pendingRespawnGroups = new List<MobGroup>();
    private Collider areaCollider;
    private int currentMobCount = 0;

    [SerializeField] private EventField _onActorDiedEvent;
    
    private void Awake()
    {
        areaCollider = GetComponent<Collider>();
        if (areaCollider == null)
        {
            Debug.LogError("MobArea requires a Collider component (Box or Sphere)");
            enabled = false;
            return;
        }
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        if(base.IsServer == false)Destroy(gameObject);
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        if(base.IsServer == false) Destroy(gameObject);
        SpawnInitialMobs();
    }


    private void Update()
    {
        if(base.IsServer == false) return;
        // Process mob group deaths and respawns
        CheckForDeadMobGroups();
        ProcessRespawnTimers();
    }
    
    private void SpawnInitialMobs()
    {
        int groupsToSpawn = Mathf.Min(possibleMobGroups.Count, maxTotalMobCount / 5);
        
        for (int i = 0; i < groupsToSpawn; i++)
        {
            if (currentMobCount >= maxTotalMobCount)
                break;
                
            MobGroup groupToSpawn = possibleMobGroups[Random.Range(0, possibleMobGroups.Count)];
            SpawnMobGroup(groupToSpawn);
        }
    }
    
    private void SpawnMobGroup(MobGroup groupTemplate)
    {
        // Create a new instance of the mob group to track separately
        MobGroup newGroup = new MobGroup
        {
            MobGroupDefinition = groupTemplate.MobGroupDefinition,
            SpawnPoints = groupTemplate.SpawnPoints,
            activeEnemies = new List<GameObject>()
        };
        
        // Calculate how many mobs we can actually spawn
        int mobsToSpawn = newGroup.MobGroupDefinition.Mobs.Count;
        
        if (mobsToSpawn <= 0)
            return;
            
        // Parent object for this group
        GameObject groupContainer = new GameObject($"MobGroup_{newGroup.MobGroupDefinition.GroupName}_{activeMobGroups.Count}");
        groupContainer.transform.SetParent(transform);
        
        // Spawn position for the group (used if no spawn points are specified)
        Vector3 groupPosition = GetRandomPositionInBounds();
        groupContainer.transform.position = groupPosition;
        
        // Spawn the actual mobs
        for (int i = 0; i < newGroup.MobGroupDefinition.Mobs.Count; i++)
        {
            // Get spawn position
            Vector3 spawnPosition;
            
            if (newGroup.SpawnPoints != null && newGroup.SpawnPoints.Count > 0)
            {
                // Use a random spawn point
                Transform spawnPoint = newGroup.SpawnPoints[Random.Range(0, newGroup.SpawnPoints.Count)];
                spawnPosition = spawnPoint.position;
            }
            else
            {
                // Spawn at random position near the group position
                spawnPosition = groupPosition + new Vector3(
                    Random.Range(-5f, 5f),
                    0f,
                    Random.Range(-5f, 5f)
                );
                
                // Make sure it's within bounds
                if (!IsPositionInBounds(spawnPosition))
                {
                    spawnPosition = GetRandomPositionInBounds();
                }
            }
            
            // Select a random enemy prefab
            GameObject enemyPrefab = newGroup.MobGroupDefinition.Mobs[i].MobPrefab;
            
            // Instantiate the enemy
            GameObject enemy = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
            base.Spawn(enemy);
            //enemy.transform.SetParent(groupContainer.transform);
            
            // Register the enemy with our tracking
            newGroup.activeEnemies.Add(enemy);
            currentMobCount++;
            EventArgs newArgs = default;
            ActorBase actor = enemy.GetComponent<ActorBase>();
            actor.GetData<Data_Mob>().MobGroup = newGroup;
            _onActorDiedEvent.Register(actor, OnActorDied);
            
        }
        
        // Add to active groups
        activeMobGroups.Add(newGroup);
    }
    
    private void OnActorDied(EventArgs obj)
    {
        _onActorDiedEvent.Unregister(obj.Sender,OnActorDied);

        Data_Mob mobData = obj.Sender.GetData<Data_Mob>();
        
        OnEnemyDeath(obj.Sender.gameObject,mobData.MobGroup);
    }

    private void OnEnemyDeath(GameObject enemy, MobGroup group)
    {
        if (group.activeEnemies.Contains(enemy))
        {
            group.activeEnemies.Remove(enemy);
            currentMobCount--;
        }
        
        // Destroy the enemy object after a delay (for death animation, etc.)
        Destroy(enemy, 3f);
    }
    
    private void CheckForDeadMobGroups()
    {
        for (int i = activeMobGroups.Count - 1; i >= 0; i--)
        {
            MobGroup group = activeMobGroups[i];
            
            // Check if all enemies in this group are dead
            if (group.activeEnemies.Count == 0)
            {
                // Calculate respawn time
                group.respawnTimer = respawnTime + Random.Range(-respawnVariance, respawnVariance);
                
                // Move to pending respawn
                pendingRespawnGroups.Add(group);
                activeMobGroups.RemoveAt(i);
                
                // Destroy group container
                Transform groupTransform = transform.Find($"MobGroup_{group.MobGroupDefinition.GroupName}_{i}");
                if (groupTransform != null)
                {
                    Destroy(groupTransform.gameObject);
                }
            }
        }
    }
    
    private void ProcessRespawnTimers()
    {
        for (int i = pendingRespawnGroups.Count - 1; i >= 0; i--)
        {
            MobGroup group = pendingRespawnGroups[i];
            
            // Update timer
            group.respawnTimer -= Time.deltaTime;
            
            // Check if it's time to respawn
            if (group.respawnTimer <= 0f)
            {
                // Choose a random mob group from possible groups
                MobGroup groupToSpawn = possibleMobGroups[Random.Range(0, possibleMobGroups.Count)];
                
                // Spawn it
                SpawnMobGroup(groupToSpawn);
                
                // Remove from pending
                pendingRespawnGroups.RemoveAt(i);
            }
        }
    }
    
    private Vector3 GetRandomPositionInBounds()
    {
        Vector3 position = Vector3.zero;
        
        if (areaCollider is BoxCollider)
        {
            BoxCollider boxCollider = areaCollider as BoxCollider;
            
            // Get a random point inside the box
            Vector3 extents = boxCollider.size / 2f;
            position = new Vector3(
                Random.Range(-extents.x, extents.x),
                Random.Range(-extents.y, extents.y),
                Random.Range(-extents.z, extents.z)
            );
            
            // Transform to world space
            position = transform.TransformPoint(position + boxCollider.center);
            
            // Ensure Y position is valid (optional)
            RaycastHit hit;
            if (Physics.Raycast(position + Vector3.up * 50f, Vector3.down, out hit, 100f, LayerMask.GetMask("Ground")))
            {
                position.y = hit.point.y;
            }
        }
        else if (areaCollider is SphereCollider)
        {
            SphereCollider sphereCollider = areaCollider as SphereCollider;
            
            // Get a random point inside the sphere
            Vector3 randomDirection = Random.insideUnitSphere * sphereCollider.radius;
            position = transform.TransformPoint(randomDirection + sphereCollider.center);
            
            // Ensure Y position is valid (optional)
            RaycastHit hit;
            if (Physics.Raycast(position + Vector3.up * 50f, Vector3.down, out hit, 100f, LayerMask.GetMask("Ground")))
            {
                position.y = hit.point.y;
            }
        }
        
        return position;
    }
    
    private bool IsPositionInBounds(Vector3 position)
    {
        return areaCollider.bounds.Contains(position);
    }
    
    #if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (!showGizmos) return;
        
        Collider gizmoCollider = GetComponent<Collider>();
        if (gizmoCollider == null) return;
        
        // Store original color and matrix
        Color originalColor = Gizmos.color;
        Matrix4x4 originalMatrix = Gizmos.matrix;
        
        // Set new color and matrix
        Gizmos.color = gizmoColor;
        Gizmos.matrix = transform.localToWorldMatrix;
        
        if (gizmoCollider is BoxCollider)
        {
            BoxCollider box = gizmoCollider as BoxCollider;
            Gizmos.DrawCube(box.center, box.size);
        }
        else if (gizmoCollider is SphereCollider)
        {
            SphereCollider sphere = gizmoCollider as SphereCollider;
            Gizmos.DrawSphere(sphere.center, sphere.radius);
        }
        
        // Restore original color and matrix
        Gizmos.color = originalColor;
        Gizmos.matrix = originalMatrix;
    }
    #endif
    
    // Public API for external management
    
    public int GetCurrentMobCount()
    {
        return currentMobCount;
    }
    
    public int GetActiveMobGroupCount()
    {
        return activeMobGroups.Count;
    }
    
    public bool CanSpawnMoreMobs()
    {
        return currentMobCount < maxTotalMobCount;
    }
    
    // Optional: Method to force spawn a specific mob group
    public void ForceSpawnMobGroup(int groupIndex)
    {
        if (groupIndex < 0 || groupIndex >= possibleMobGroups.Count)
            return;
            
        SpawnMobGroup(possibleMobGroups[groupIndex]);
    }
}