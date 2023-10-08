using Unity.Netcode;

public enum GENDER {
    Male,
    Female
};

// 为了在network中进行传递, 储存数据的结构体需要继承自INetworkSerializable并自行实现序列化方法
public struct PlayerInfoData : INetworkSerializable {
    public ulong playerId;
    public string playerName;
    public bool isReady;
    public GENDER gender;
    
    public PlayerInfoData(ulong playerId) {
        this.playerId = playerId;
        this.playerName = "玩家:" + playerId;
        this.isReady = false;
        this.gender = GENDER.Male;
    }
    
    public PlayerInfoData(ulong playerId, string playerName, bool isReady, GENDER gender) {
        this.playerId = playerId;
        this.playerName = playerName;
        this.isReady = isReady;
        this.gender = gender;
    }

    void Unity.Netcode.INetworkSerializable.NetworkSerialize<T>(BufferSerializer<T> serializer) {
        serializer.SerializeValue(ref playerId);
        serializer.SerializeValue(ref playerName);
        serializer.SerializeValue(ref isReady);
        serializer.SerializeValue(ref gender);
    }
}
