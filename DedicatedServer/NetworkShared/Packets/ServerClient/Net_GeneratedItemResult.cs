using LiteNetLib.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Firebase.Firestore;

namespace NetworkShared.Packets.ServerClient
{
    [Serializable]
    public struct Net_GeneratedItemResult : INetPacket
    {
        public PacketType Type => PacketType.GeneratedItemResult;

        public string OwnerId;
        public string ItemId;
        public string UniqueItemId;
        public int Rarity;
        public int UpgradeLevel;
        public string RequestId;

        public ItemModifierResult[] Modifiers { get; set; }


        public void Deserialize(NetDataReader reader)
        {
            OwnerId = reader.GetString();
            ItemId = reader.GetString();
            UniqueItemId = reader.GetString();
            Rarity = reader.GetInt();
            UpgradeLevel = reader.GetInt();
            RequestId = reader.GetString();
            var modifiersLength = reader.GetUShort();
            Modifiers = new ItemModifierResult[modifiersLength];
            for (int i = 0; i < modifiersLength; i++)
            {
                Modifiers[i] = reader.Get<ItemModifierResult>();
            }
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put((byte)Type);
            writer.Put(OwnerId);
            writer.Put(ItemId);
            writer.Put(UniqueItemId);
            writer.Put(Rarity);
            writer.Put(UpgradeLevel);
            writer.Put(RequestId);
            writer.Put((ushort)Modifiers.Length);
            for (int i = 0; i < Modifiers.Length; i++)
            {
                writer.Put(Modifiers[i]);
            }
        }


    }
    [FirestoreData]
    public struct ItemModifierResult : INetSerializable
    {
        [FirestoreProperty]
        public string ModifierId { get; set; }
        [FirestoreProperty]
        public int ModifierValue { get; set; }

        public void Deserialize(NetDataReader reader)
        {
            ModifierId = reader.GetString();
            ModifierValue = reader.GetInt();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(ModifierId);
            writer.Put(ModifierValue);
        }
    }
}
