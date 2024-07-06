using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

public class DataContainer : MonoBehaviour
{
    public string ContainerName;
    [SerializeReference]
    [OdinSerialize]public Data Data1;
    public bool showData1;
    [SerializeReference]
    [OdinSerialize]public Data Data2;
    public bool showData2;
}
