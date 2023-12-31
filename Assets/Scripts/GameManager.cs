using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class GameManager : NetworkBehaviour {
    
    public static GameManager Instance;

    public Dictionary<ulong, PlayerInfoData> playerInfoDataDict { get; private set;  }
    public UnityEvent OnStartGame;

    public override void OnNetworkSpawn() {
        base.OnNetworkSpawn();
        // NetworkManage加载场景后会有一个回调事件
        // 通过回调事件去判断当前是什么场景
        // OnLoadEventCompleted: 所有玩家加载完成事件后进行回调
        // OnLoadComplete: 一个玩家加载完成事件后进行回调
        NetworkManager.SceneManager.OnLoadEventCompleted += OnLoadEventCompleted;
    }

    void Start() {
        Instance = this;
        // 在切换场景时挂载该脚本的物体会保留
        DontDestroyOnLoad(gameObject);
        SceneManager.LoadScene("StartScene");
        // 切换场景时保留的数据
        playerInfoDataDict = new Dictionary<ulong, PlayerInfoData>();
    }

    private void OnLoadEventCompleted(string sceneName, LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimeOut) {
        if (sceneName == "GameScene") {
            OnStartGame.Invoke();
        }
    }
    
    public void LoadScene(string sceneName) {
        // 网络上加载场景
        NetworkManager.SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
    }

    public void StartGame(Dictionary<ulong, PlayerInfoData> playerInfoDataDict) {
        this.playerInfoDataDict = playerInfoDataDict;
        // 当服务端进入GameScene后需要更新所有客户端的GameManager中的playerInfoDataDict
        UpdatePlayerInfoAllClient();
    }
    
    // 远程: 服务端通知其他客户端更新玩家信息
    void UpdatePlayerInfoAllClient() {
        // 遍历通知所有的客户端把它们没有的玩家添加进去并更新UI
        foreach (PlayerInfoData playerInfo in playerInfoDataDict.Values) {
            UpdatePlayerInfoClientRpc(playerInfo);
        }
    }
    
    [ClientRpc]
    void UpdatePlayerInfoClientRpc(PlayerInfoData playerInfoData) {
        if (IsServer) {
            return;
        }
        if (playerInfoDataDict.ContainsKey(playerInfoData.playerId)) {
            playerInfoDataDict[playerInfoData.playerId] = playerInfoData;
        } else {
            playerInfoDataDict.Add(playerInfoData.playerId, playerInfoData);
        }
    }
}