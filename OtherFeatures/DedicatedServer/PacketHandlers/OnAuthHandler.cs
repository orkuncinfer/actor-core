using NetworkShared;
using NetworkShared.Attributes;
using NetworkShared.Packets.ClientServer;
using NetworkShared.Packets.ServerClient;
using UnityEngine;
#if UNITY_ANDROID
[HandlerRegister(PacketType.OnAuth)]
public class OnAuthHandler : IPacketHandler
{ 
    public void Handle(INetPacket packet, int connectionId)
    {
        var msg = (Net_OnAuth) packet;
        Debug.Log("OnAuthHandler triggered!");
        Debug.Log(msg.test);
        //GooglePlayServicesInitialization.Instance.DedicatedServerSignedIn = true;
    }
}
#endif