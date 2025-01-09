using LiteNetLib.Utils;
using NetworkShared.Packets.ServerClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Firebase.Firestore;

namespace NetworkShared.Packets.ServerClient
{
    [FirestoreData][Serializable]
    public struct FbGeneratedItemResult
    {
        [FirestoreProperty]
        public string OwnerId { get; set; }
        [FirestoreProperty]
        public string ItemId { get; set; }
        [FirestoreProperty]
        public string UniqueItemId { get; set; }
        [FirestoreProperty]
        public int Rarity { get; set; }
        [FirestoreProperty]
        public int UpgradeLevel { get; set; }
        [FirestoreProperty]
        public ItemModifierResult[] Modifiers { get; set; }
    }
}

