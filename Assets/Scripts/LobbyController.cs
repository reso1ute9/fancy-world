using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

// 为了在network中进行传递, 储存数据的结构体需要继承自INetworkSerializable并自行实现序列化方法
public struct PlayerInfo : INetworkSerializable {
    public ulong playerId;
    public bool isReady;

    public PlayerInfo(ulong playerId, bool isReady) {
        this.playerId = playerId;
        this.isReady = isReady;
    }

    void Unity.Netcode.INetworkSerializable.NetworkSerialize<T>(BufferSerializer<T> serializer) {
        serializer.SerializeValue(ref playerId);
        serializer.SerializeValue(ref isReady);
    }
}

public class LobbyController : NetworkBehaviour
{
    [SerializeField] private Transform canvers;

    private Transform content;
    private GameObject cell;

    private Button startButton;
    private Toggle readyToggle;

    private List<PlayerListCellController> cellList;

    private Dictionary<ulong, PlayerInfo> playerInfoDict;
    
    public void AddPlayer(PlayerInfo playerInfo) {
        GameObject cloneCell = Instantiate(cell);
        cloneCell.SetActive(true);
        cloneCell.transform.SetParent(content, false);
        PlayerListCellController playerListCellController = cloneCell.GetComponent<PlayerListCellController>();
        playerListCellController.Init(playerInfo);
        cellList.Add(playerListCellController);
        
        playerInfoDict.Add(playerInfo.playerId, playerInfo);
    }
    
    // 注意: 该方法会先于Start
    public override void OnNetworkSpawn() {
        if (IsServer) {
            NetworkManager.OnClientConnectedCallback += OnClientConnect;
        }
        
        playerInfoDict = new Dictionary<ulong, PlayerInfo>();
        
        content = canvers.Find("PlayerList/Viewport/Content");
        cell = content.Find("Cell").gameObject;
        startButton = canvers.Find("StartButton").GetComponent<Button>();
        readyToggle = canvers.Find("ReadyToggle").GetComponent<Toggle>();
        
        startButton.onClick.AddListener(OnStartButtonClick);
        readyToggle.onValueChanged.AddListener(OnReadyToggleClick);

        cellList = new List<PlayerListCellController>();
        
        // 将本地玩家添加到网络中
        PlayerInfo playerInfo = new PlayerInfo(NetworkManager.LocalClientId, false);
        AddPlayer(playerInfo);
        base.OnNetworkSpawn();
    }
    
    // 当有新的客户端链接时会调用本方法
    private void OnClientConnect(ulong playerId) {
        PlayerInfo playerInfo = new PlayerInfo(playerId, false);
        AddPlayer(playerInfo);

        UpdateAllPlayerInfo();
    }
    
    // 服务端通知其他客户端添加玩家
    void UpdateAllPlayerInfo() {
        // 遍历所有的客户端把它们没有的玩家添加进去
        foreach (PlayerInfo playerInfo in playerInfoDict.Values) {
            UpdatePlayerInfoClientRpc(playerInfo);
        }
    }
    
    // 客户端更新玩家信息, 需要注意Host的情况也会调用本方法
    // Unity NetCode中分为三种情况:
    // 1. Host: 服务器+客户端合起来 
    // 2. Server: 服务端
    // 3. Client: 客户端
    [ClientRpc]
    void UpdatePlayerInfoClientRpc(PlayerInfo playerInfo) {
        // 仅当当前是Client时才更新客户端中的PlayerInfo
        if (!IsServer) {
            // 如果客户端中已经存储了playerId, 需要更新一下最新数据
            if (playerInfoDict.ContainsKey(playerInfo.playerId)) {
                playerInfoDict[playerInfo.playerId] = playerInfo;
            }
            else {
                AddPlayer(playerInfo);    
            }
        }
    }

    private void OnStartButtonClick() {
        
    }

    private void OnReadyToggleClick(bool args) {
        
    }
    
    void Update() {
        
    }
}
