using System;
using System.Collections;
using System.Collections.Generic;
using Firebase.Auth;
using NetworkShared.Packets.ClientServer;
using NetworkShared.Packets.ServerClient;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class ItemDropManager : MonoBehaviour
{
    public static ItemDropManager Instance;
    
    public MonsterListDefinition MonsterListDefinition;
    public float MonsterListFetchFrequency = 10f;
    public GameObject DropPrefab;
    public GameObject LabelPrefab;
    public LayerMask GroundLayer;
    public Canvas WorldCanvas;
    public Transform LabelParent;
    
    public List<WorldItemLabel> itemLabels = new List<WorldItemLabel>();
    public List<WorldItemLabel> _pickedUpLabels = new List<WorldItemLabel>();
    
    [SerializeField] private LineRenderer lineRenderer;
    private Vector3 _itemGroundPos;
    
    [SerializeField] private EventField<float> onMobKillesd2;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        
        onMobKillesd2.Register(null,OnMob);
    }

    private void OnMob(EventArgs obj, float arg1)
    {
        Debug.Log("logfired" + arg1);
    }
    [Button]
    public void Raise()
    {
        onMobKillesd2.Raise(4);
    }
[Button]
    public void Unregister()
    {
        onMobKillesd2.Unregister(null,OnMob);
    }

    private void Start()
    {
        StartCoroutine(FetchMonsterList());
        Events.onExampleEvent.AddListener(OnMobKilled);
    }

    private void OnMobKilled(MobKilledEventArgs obj)
    {
        KilledMonster(obj.MobId, obj.Position);
        Debug.Log("Killed");
    }

    private void Update()
    {
        for (int i = 0; i < _pickedUpLabels.Count; i++)
        {
            PoolManager.ReleaseObject(_pickedUpLabels[i].gameObject);
            itemLabels.Remove(_pickedUpLabels[i]);
        }
        _pickedUpLabels.Clear();
        
        Iterate();

        if (Input.GetKeyDown(KeyCode.Y))
        {
            Vector3 randomPosInCircle = Random.insideUnitCircle * 2;
            KilledMonster("mns_golem", randomPosInCircle);
        }
        if (Input.GetKeyDown(KeyCode.U))
        {
            Iterate();
        }
    }
    [Button]
    public void Iterate()
    {
        for (int i = 0; i < itemLabels.Count; i++)
        {
            for (int j = i + 1; j < itemLabels.Count; j++)
            {
                if (RectTransformExtensions.WorldRectOverlaps(itemLabels[i].rectTransform, itemLabels[j].rectTransform))
                {
                    if(!itemLabels[i].Initialized || !itemLabels[j].Initialized) continue;
                    if(itemLabels[i].IsMoving || itemLabels[j].IsMoving) continue;
                    itemLabels[i].UpdatePosition(itemLabels[j].rectTransform);
                }
            }
        }
    }
    
    private IEnumerator FetchMonsterList()
    {
        while (true)
        {
            MonsterListDefinition.FetchData();
            yield return new WaitForSeconds(MonsterListFetchFrequency);
        }
    }
    [Button]
    public async void KilledMonster(string monsterID, Vector3 monsterCenterPos)
    {
        Vector3 groundPos = monsterCenterPos;
        Net_GeneratedItemResult itemResultPacket = new Net_GeneratedItemResult();
        string ownerId = FirebaseAuth.DefaultInstance.CurrentUser.UserId;

#if UNITY_EDITOR
        ownerId = "JUL1vonO6EWSM2uDdl63FJuUZwP2";
#endif
        var generatedItem = new Net_GenerateItemRequest()
        {
            
            OwnerId = ownerId,
            MonsterId = monsterID,
            RequestId = NetworkClient.Instance.GenerateUniqueRequestId()
        };
        try
        {
            var result = await NetworkClient.Instance.SendServerAsync<Net_GenerateItemRequest, Net_GeneratedItemResult>(generatedItem);
            if (result is Net_GeneratedItemResult itemResult)
            {
                itemResultPacket = itemResult;
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Failed to receive response: " + e.Message);
            return;
        }
        
        if(Physics.Raycast(monsterCenterPos + Vector3.up*10, Vector3.down, out RaycastHit hit, 500, GroundLayer)) 
        {
            if (hit.collider != null)
            {
                groundPos = hit.point;
                Debug.Log("Hit ground");
            }
        }
        GameObject itemMeshInstance = Instantiate(DropPrefab, groundPos, Quaternion.identity);
        PathFollower pathFollower = itemMeshInstance.GetComponent<PathFollower>();
        ItemDropInstance itemDropInstance = itemMeshInstance.GetComponent<ItemDropInstance>();

        itemDropInstance.GeneratedItemResult = new FbGeneratedItemResult();
        itemDropInstance.GeneratedItemResult.OwnerId = itemResultPacket.OwnerId;
        itemDropInstance.GeneratedItemResult.ItemId = itemResultPacket.ItemId;
        itemDropInstance.GeneratedItemResult.UniqueItemId = itemResultPacket.UniqueItemId;
        itemDropInstance.GeneratedItemResult.Modifiers = itemResultPacket.Modifiers;
        itemDropInstance.GeneratedItemResult.Rarity = itemResultPacket.Rarity;
        itemDropInstance.GeneratedItemResult.UpgradeLevel = itemResultPacket.UpgradeLevel;
        
        
        GameObject labelInstance = PoolManager.SpawnObject(LabelPrefab, groundPos, Quaternion.identity);
        WorldItemLabel itemLabel = labelInstance.GetComponent<WorldItemLabel>();
        
        itemDropInstance.LabelInstance = itemLabel;
        itemLabel.ItemDropInstance = itemDropInstance;
        Vector3 endPos = groundPos + Vector3.forward * -2;
        DrawArc(monsterCenterPos, endPos);
        Vector3[] pathArray = new Vector3[lineRenderer.positionCount];
        lineRenderer.GetPositions(pathArray);
        pathFollower.Initialize(pathArray,.4f,endPos);
        
        labelInstance.transform.SetParent(WorldCanvas.transform);
        labelInstance.transform.position = groundPos;
        
        itemLabel.Initialize(LabelParent,endPos );
        itemLabels.Add(itemLabel);
    }

    [Button]
    public void DebugSpawnPathFollower()
    {
        Vector3 groundPos = transform.position;
        
        if(Physics.Raycast(transform.position + Vector3.up*10, Vector3.down, out RaycastHit hit, 500, GroundLayer)) 
        {
            if (hit.collider != null)
            {
                groundPos = hit.point;
                Debug.Log("Hit ground");
            }
        }
        
        GameObject itemMeshInstance = Instantiate(DropPrefab, groundPos, Quaternion.identity);
        PathFollower pathFollower = itemMeshInstance.GetComponent<PathFollower>();
        ItemDropInstance itemDropInstance = itemMeshInstance.GetComponent<ItemDropInstance>();
        
        
        
        GameObject labelInstance = PoolManager.SpawnObject(LabelPrefab, groundPos, Quaternion.identity);
        WorldItemLabel itemLabel = labelInstance.GetComponent<WorldItemLabel>();
        
        itemDropInstance.LabelInstance = itemLabel;
        itemLabel.ItemDropInstance = itemDropInstance;
        Vector3 endPos = groundPos + Vector3.forward * -2;
        DrawArc(transform.position, endPos);
        Vector3[] pathArray = new Vector3[lineRenderer.positionCount];
        lineRenderer.GetPositions(pathArray);
        pathFollower.Initialize(pathArray,.4f,endPos);
        
        labelInstance.transform.SetParent(WorldCanvas.transform);
        labelInstance.transform.position = groundPos;
        
        itemLabel.Initialize(LabelParent,endPos );
        itemLabels.Add(itemLabel);
    }

 
    
    void DrawArc(Vector3 startPoint, Vector3 endPoint)
    {
        int pointsDensity = 50;

        Vector3[] arcPoints = new Vector3[pointsDensity];
        lineRenderer.positionCount = pointsDensity;

        for (int i = 0; i < pointsDensity; i++)
        {
            float t = i / (pointsDensity - 1f);
            arcPoints[i] = CalculateArcPoint(t, startPoint, endPoint, 1);
        }

        lineRenderer.SetPositions(arcPoints);
    }
    Vector3 CalculateArcPoint(float t, Vector3 start, Vector3 end, float height)
    {
        // Linearly interpolate between start and end points
        Vector3 linearPoint = Vector3.Lerp(start, end, t);
        // Add the arc height (parabola formula)
        Vector3 arcPoint = linearPoint + Vector3.up * (height * Mathf.Sin(Mathf.PI * t));
        return arcPoint;
    }

    public void PickedUp(WorldItemLabel label)
    {
        _pickedUpLabels.Add(label);
    }
}

[Serializable]
public class GeneratedItemResult
{
    public string itemId;
    public string uniqueItemId;
    public List
        <ItemModifierResult> modifiers;
    public int rarity;
}

[Serializable]
public class ItemModifierResult
{
    public string modifierId;
    public int rndValue;
}
