using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataSetKey : GenericKey
{
    [SerializeField] [TextArea] private string _dataSetType;
    public string DataSetType => _dataSetType;
}
