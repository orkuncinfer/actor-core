using LiteNetLib.Utils;

namespace NetworkShared.Packets.ClientServer
{
    public struct Net_AuthRequest : INetPacket
    {
        public PacketType Type => PacketType.AuthRequest;

        public string Username;
        public string Password;

        public void Deserialize(NetDataReader reader)
        {
            //receiving
            Username = reader.GetString();
            Password = reader.GetString();
        }

        public void Serialize(NetDataWriter writer)
        {
            //sending
            writer.Put((byte)Type);
            writer.Put(Username);
            writer.Put(Password);
        }
    }
}
