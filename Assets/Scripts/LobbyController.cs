using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;


public class LobbyController : NetworkBehaviour
{
    [SerializeField] private Transform canvers;

    private Transform content;
    private GameObject cell;

    private Button startButton;
    private Toggle readyToggle;

    private TMP_InputField nickname;

    private Dictionary<ulong, PlayerListCellController> playerListCellDict;
    private Dictionary<ulong, PlayerInfoData> playerInfoDataDict;
    
    // 注意: 该方法会先于Start
    public override void OnNetworkSpawn() {
        if (IsServer) {
            NetworkManager.OnClientConnectedCallback += OnClientConnect;
        }
        
        playerInfoDataDict = new Dictionary<ulong, PlayerInfoData>();
        
        content = canvers.Find("PlayerList/Viewport/Content");
        cell = content.Find("Cell").gameObject;
        
        startButton = canvers.Find("StartButton").GetComponent<Button>();
        startButton.onClick.AddListener(OnStartButtonClick);
        
        readyToggle = canvers.Find("ReadyToggle").GetComponent<Toggle>();
        readyToggle.onValueChanged.AddListener(OnReadyToggleClick);

        nickname = canvers.Find("Nickname").GetComponent<TMP_InputField>();
        nickname.onEndEdit.AddListener(OnEditNicknameEnd);
        
        playerListCellDict = new Dictionary<ulong, PlayerListCellController>();
        
        // 将本地玩家添加到网络中
        // 玩家默认属性: nickname = "玩家"; ready = false; gender = "male"
        PlayerInfoData playerInfo = new PlayerInfoData(NetworkManager.LocalClientId);
        AddPlayer(playerInfo);
        
        // 添加监听选人toggle变化的事件
        Toggle maleToggle = canvers.Find("Gender/Male").GetComponent<Toggle>();
        maleToggle.onValueChanged.AddListener(OnMaleToggleChange);
        Toggle femaleToggle = canvers.Find("Gender/Female").GetComponent<Toggle>();
        femaleToggle.onValueChanged.AddListener(OnFemaleToggleChange);
        
        base.OnNetworkSpawn();
    }
    
    public void AddPlayer(PlayerInfoData playerInfo) {
        GameObject cloneCell = Instantiate(cell);
        cloneCell.SetActive(true);
        cloneCell.transform.SetParent(content, false);
        PlayerListCellController playerListCellController = cloneCell.GetComponent<PlayerListCellController>();
        playerListCellController.Init(playerInfo);
        playerListCellDict.Add(playerInfo.playerId, playerListCellController);
        playerInfoDataDict.Add(playerInfo.playerId, playerInfo);
    }
    
    // 当有新的客户端链接时会调用本方法
    private void OnClientConnect(ulong playerId) {
        PlayerInfoData playerInfo = new PlayerInfoData(playerId);
        AddPlayer(playerInfo);

        UpdatePlayerInfoAllClient();
    }
    
    // 远程: 服务端通知其他客户端添加玩家
    void UpdatePlayerInfoAllClient() {
        // 遍历通知所有的客户端把它们没有的玩家添加进去并更新UI
        foreach (PlayerInfoData playerInfo in playerInfoDataDict.Values) {
            UpdatePlayerInfoClientRpc(playerInfo);
        }
    }
    
    // 远程: 客户端更新玩家信息, 需要注意Host的情况也会调用本方法
    // Unity NetCode中分为三种情况:
    // 1. Host: 服务器+客户端合起来 
    // 2. Server: 服务端
    // 3. Client: 客户端
    [ClientRpc]
    void UpdatePlayerInfoClientRpc(PlayerInfoData playerInfo) {
        // 仅当当前是Client时才更新客户端中的PlayerInfo
        if (!IsServer) {
            // 如果客户端中已经存储了playerId, 需要更新一下最新数据
            if (playerInfoDataDict.ContainsKey(playerInfo.playerId)) {
                playerInfoDataDict[playerInfo.playerId] = playerInfo;
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
    void UpdatePlayerInfoServerRpc(PlayerInfoData playerInfo) {
        // 需要先修改当前player的信息再通知客户端进行修改
        playerInfoDataDict[playerInfo.playerId] = playerInfo;
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
            UpdatePlayerInfoServerRpc(playerInfoDataDict[NetworkManager.LocalClientId]);
        }
    }

    private void OnStartButtonClick() {
        
    }

    private void OnReadyToggleClick(bool args) {
        ulong playerId = NetworkManager.LocalClientId;
        // 将UI界面设置为Ready
        playerListCellDict[playerId].SetReady(args);
        // 更新玩家信息数据
        UpdatePlayerInfo(playerId, playerInfoDataDict[playerId].playerName, args, playerInfoDataDict[playerId].gender);
        // 更新玩家信息
        UpdatePlayerInfoRemote();
    }

    private void UpdatePlayerInfo(ulong playerId, string playerName, bool isReady, GENDER gender) {
        PlayerInfoData playerInfo = playerInfoDataDict[playerId];
        playerInfo.playerName = playerName;
        playerInfo.isReady = isReady;
        playerInfo.gender = gender;
        playerInfoDataDict[playerId] = playerInfo;
        // 更新UI信息
        UpdatePlayerCellInfo(playerInfo);
    }
    
    // UI: 根据playerInfo修改准备Toggle UI界面
    private void UpdatePlayerCellInfo(PlayerInfoData playerInfo) {
        playerListCellDict[playerInfo.playerId].UpdatePlayerCellInfo(playerInfo);
    }
    
    // UI: 根据playerInfo修改准备Toggle UI界面
    private void UpdatePlayerCellsInfo() {
        foreach (var item in playerInfoDataDict) {
            // 更新玩家UI界面准备状态和性别
            UpdatePlayerCellInfo(item.Value);
        }
    }

    private void OnMaleToggleChange(bool isMale) {
        ulong playerId = NetworkManager.LocalClientId;
        // 更新本地数据和UI
        if (isMale) {
            PlayerSelectController.Instance.SwitchGender(GENDER.Male);
            UpdatePlayerInfo(playerId, playerInfoDataDict[playerId].playerName, playerInfoDataDict[playerId].isReady, GENDER.Male);
        }
        else {
            PlayerSelectController.Instance.SwitchGender(GENDER.Female);
            UpdatePlayerInfo(playerId, playerInfoDataDict[playerId].playerName, playerInfoDataDict[playerId].isReady, GENDER.Female);
        }
        // 更新玩家信息
        UpdatePlayerInfoRemote();
    }

    private void OnFemaleToggleChange(bool isFemale) {
        ulong playerId = NetworkManager.LocalClientId;
        // 更新本地数据和UI
        if (isFemale) {
            PlayerSelectController.Instance.SwitchGender(GENDER.Female);
            UpdatePlayerInfo(playerId, playerInfoDataDict[playerId].playerName, playerInfoDataDict[playerId].isReady, GENDER.Female);
        }
        else {
            PlayerSelectController.Instance.SwitchGender(GENDER.Male);
            UpdatePlayerInfo(playerId, playerInfoDataDict[playerId].playerName, playerInfoDataDict[playerId].isReady, GENDER.Male);
        }
        // 更新远程玩家数据和UI
        UpdatePlayerInfoRemote();
    }

    private void OnEditNicknameEnd(string name) {
        if (string.IsNullOrEmpty(name)) {
            return;
        }
        ulong playerId = NetworkManager.LocalClientId;
        // 更新本地数据和UI
        UpdatePlayerInfo(playerId, name, playerInfoDataDict[playerId].isReady, playerInfoDataDict[playerId].gender);
        // 更新远程玩家数据和UI
        UpdatePlayerInfoRemote();
    }
}
