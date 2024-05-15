using LiteNetLib.Utils;

public enum PacketType : byte
{
    //Client-Server
    Invalid = 0,
    AuthRequest = 1,
    GenerateItemRequest = 2,
    
    //Server-Client
    OnAuth = 100,
    OnAuthFail = 101,
    OnServerStatus = 102,
    GeneratedItemResult = 103
}

public interface INetPacket : INetSerializable
{
    PacketType Type { get; }
}