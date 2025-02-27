using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct EventArgs
{
    public ActorBase Sender;
    public string EventName;
}
