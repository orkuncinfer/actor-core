using LiteNetLib.Utils;

namespace NetworkShared.Packets.ServerClient
{
    public struct Net_OnServerStatus : INetPacket
    {
        public PacketType Type => PacketType.OnServerStatus;

        public void Deserialize(NetDataReader reader)
        {

        }

        public void Serialize(NetDataWriter writer)
        {
            // todo: implement
            writer.Put((byte)Type);
        }
    }
}
