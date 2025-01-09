using LiteNetLib.Utils;

namespace NetworkShared.Packets.ServerClient
{
    public struct Net_OnAuth : INetPacket
    {
        public PacketType Type => PacketType.OnAuth;

        public string test { get; set; }



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
