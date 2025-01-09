using LiteNetLib.Utils;
using System;


namespace NetworkShared.Packets.ServerClient
{
    public struct Net_OnAuthFail : INetPacket
    {
        public PacketType Type => PacketType.OnAuthFail;

        public string test;

        public void Deserialize(NetDataReader reader)
        {
            test = reader.GetString();   
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put((byte)Type);
            writer.Put(test);
        }
    }
}
