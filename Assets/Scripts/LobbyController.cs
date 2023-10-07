using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public enum GENDER {
    Male,
    Female
};

// 为了在network中进行传递, 储存数据的结构体需要继承自INetworkSerializable并自行实现序列化方法
public struct PlayerInfo : INetworkSerializable {
    public ulong playerId;
    public bool isReady;
    public GENDER gender;

    public PlayerInfo(ulong playerId, bool isReady, GENDER gender) {
        this.playerId = playerId;
        this.isReady = isReady;
        this.gender = gender;
    }

    void Unity.Netcode.INetworkSerializable.NetworkSerialize<T>(BufferSerializer<T> serializer) {
        serializer.SerializeValue(ref playerId);
        serializer.SerializeValue(ref isReady);
        serializer.SerializeValue(ref gender);
    }
}

public class LobbyController : NetworkBehaviour
{
    [SerializeField] private Transform canvers;

    private Transform content;
    private GameObject cell;

    private Button startButton;
    private Toggle readyToggle;

    private Dictionary<ulong, PlayerListCellController> playerListCellDict;
    private Dictionary<ulong, PlayerInfo> playerInfoDict;
    
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

        playerListCellDict = new Dictionary<ulong, PlayerListCellController>();
        
        // 将本地玩家添加到网络中
        PlayerInfo playerInfo = new PlayerInfo(NetworkManager.LocalClientId, false, GENDER.Male);
        AddPlayer(playerInfo);
        
        // 添加监听选人toggle变化的事件
        Toggle maleToggle = canvers.Find("Gender/Male").GetComponent<Toggle>();
        maleToggle.onValueChanged.AddListener(OnMaleToggleChange);
        Toggle femaleToggle = canvers.Find("Gender/Female").GetComponent<Toggle>();
        femaleToggle.onValueChanged.AddListener(OnFemaleToggleChange);
        
        base.OnNetworkSpawn();
    }
    
    public void AddPlayer(PlayerInfo playerInfo) {
        GameObject cloneCell = Instantiate(cell);
        cloneCell.SetActive(true);
        cloneCell.transform.SetParent(content, false);
        PlayerListCellController playerListCellController = cloneCell.GetComponent<PlayerListCellController>();
        playerListCellController.Init(playerInfo);
        playerListCellDict.Add(playerInfo.playerId, playerListCellController);
        playerInfoDict.Add(playerInfo.playerId, playerInfo);
    }
    
    // 当有新的客户端链接时会调用本方法
    private void OnClientConnect(ulong playerId) {
        PlayerInfo playerInfo = new PlayerInfo(playerId, false, GENDER.Male);
        AddPlayer(playerInfo);

        UpdatePlayerInfoAllClient();
    }
    
    // 远程: 服务端通知其他客户端添加玩家
    void UpdatePlayerInfoAllClient() {
        // 遍历通知所有的客户端把它们没有的玩家添加进去并更新UI
        foreach (PlayerInfo playerInfo in playerInfoDict.Values) {
            UpdatePlayerInfoClientRpc(playerInfo);
        }
    }
    
    // 远程: 客户端更新玩家信息, 需要注意Host的情况也会调用本方法
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
            // 更新所有玩家信息UI
            UpdatePlayerCellsInfo();
        }
    }
    
    // 远程: 服务端更新玩家信息
    [ServerRpc(RequireOwnership = false)]
    void UpdatePlayerInfoServerRpc(PlayerInfo playerInfo) {
        // 需要先修改当前player的信息再通知客户端进行修改
        playerInfoDict[playerInfo.playerId] = playerInfo;
        // 更新玩家UI界面准备状态和性别
        playerListCellDict[playerInfo.playerId].UpdatePlayerCellInfo(playerInfo);
        UpdatePlayerInfoAllClient();
    }
    
    // 本地: 触发服务端/客户端更新玩家信息
    void UpdatePlayerInfoRemote() {
        if (IsServer) {
            // 直接通知所有客户端更新数据
            UpdatePlayerInfoAllClient();
        }
        else {
            // 通过告知服务端来更新所有客户端数据
            // 1. 客户端向服务端发通知, 通知服务端执行UpdateAllPlayerInfo函数
            // 2. 服务端执行UpdateAllPlayerInfo后会遍历通知所有的客户端把它们没有的玩家添加进去
            UpdatePlayerInfoServerRpc(playerInfoDict[NetworkManager.LocalClientId]);
        }
    }

    private void OnStartButtonClick() {
        
    }

    private void OnReadyToggleClick(bool args) {
        ulong playerId = NetworkManager.LocalClientId;
        // 将UI界面设置为Ready
        playerListCellDict[playerId].SetReady(args);
        // 更新玩家信息数据
        UpdatePlayerInfo(playerId, args, playerInfoDict[playerId].gender);
        // 更新玩家信息
        UpdatePlayerInfoRemote();
    }

    private void UpdatePlayerInfo(ulong playerId, bool isReady, GENDER gender) {
        PlayerInfo playerInfo = playerInfoDict[playerId];
        playerInfo.isReady = isReady;
        playerInfo.gender = gender;
        playerInfoDict[playerId] = playerInfo;
        // 更新UI信息
        UpdatePlayerCellInfo(playerInfo);
    }
    
    // UI: 根据playerInfo修改准备Toggle UI界面
    private void UpdatePlayerCellInfo(PlayerInfo playerInfo) {
        playerListCellDict[playerInfo.playerId].UpdatePlayerCellInfo(playerInfo);
    }
    
    // UI: 根据playerInfo修改准备Toggle UI界面
    private void UpdatePlayerCellsInfo() {
        foreach (var item in playerInfoDict) {
            // 更新玩家UI界面准备状态和性别
            UpdatePlayerCellInfo(item.Value);
        }
    }

    private void OnMaleToggleChange(bool isMale) {
        ulong playerId = NetworkManager.LocalClientId;
        if (isMale) {
            PlayerSelectController.Instance.SwitchGender(GENDER.Male);
            UpdatePlayerInfo(playerId, playerInfoDict[playerId].isReady, GENDER.Male);
        }
        else {
            PlayerSelectController.Instance.SwitchGender(GENDER.Female);
            UpdatePlayerInfo(playerId, playerInfoDict[playerId].isReady, GENDER.Female);
        }
        // 更新玩家信息
        UpdatePlayerInfoRemote();
    }

    private void OnFemaleToggleChange(bool isFemale) {
        ulong playerId = NetworkManager.LocalClientId;
        if (isFemale) {
            PlayerSelectController.Instance.SwitchGender(GENDER.Female);
            UpdatePlayerInfo(playerId, playerInfoDict[playerId].isReady, GENDER.Female);
        }
        else {
            PlayerSelectController.Instance.SwitchGender(GENDER.Male);
            UpdatePlayerInfo(playerId, playerInfoDict[playerId].isReady, GENDER.Male);
        }
        // 更新玩家信息
        UpdatePlayerInfoRemote();
    }
}
