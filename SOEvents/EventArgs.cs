using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct EventArgs
{
    public Actor Sender;
    public string EventName;
}
