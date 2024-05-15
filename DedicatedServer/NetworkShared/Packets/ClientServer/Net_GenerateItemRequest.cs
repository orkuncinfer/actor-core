using LiteNetLib.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkShared.Packets.ClientServer
{
    public struct Net_GenerateItemRequest : INetPacket
    {
        public PacketType Type => PacketType.GenerateItemRequest;

        public string OwnerId;
        public string MonsterId;
        public string RequestId;

        public void Deserialize(NetDataReader reader)
        {
            OwnerId = reader.GetString();
            MonsterId = reader.GetString();
            RequestId = reader.GetString();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put((byte)Type);
            writer.Put(OwnerId);
            writer.Put(MonsterId);
            writer.Put(RequestId);
        }

    }
}