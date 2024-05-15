using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using LiteNetLib.Utils;
using NetworkShared;
using NetworkShared.Attributes;
using NetworkShared.Packets.ServerClient;
using UnityEngine;

[HandlerRegister(PacketType.GeneratedItemResult)]
public class OnItemGenerateResultHandler : IPacketHandler
{
    public void Handle(INetPacket packet, int connectionId)
    {
        var msg = (Net_GeneratedItemResult)packet;

        // Complete the pending request
        NetworkClient.Instance.CompleteRequest(msg.RequestId,(INetPacket)msg);

        NetworkClient.Instance.ItemResult = msg;

        if (msg.Modifiers.Length > 0)
        {
            foreach (var modifier in msg.Modifiers)
            {
                Debug.Log("Modifier: " + modifier.ModifierId + " Value: " + modifier.ModifierValue);
            }
        }
        else
        {
            Debug.Log("No modifiers found");
        }
    }
    
}