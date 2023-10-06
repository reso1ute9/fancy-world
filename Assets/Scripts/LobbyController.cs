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

    private List<PlayerListCellController> cellList;
    
    public void AddPlayer(ulong playerId, bool isReady) {
        GameObject cloneCell = Instantiate(cell);
        cloneCell.SetActive(true);
        cloneCell.transform.SetParent(content, false);
        PlayerListCellController playerListCellController = cloneCell.GetComponent<PlayerListCellController>();
        playerListCellController.Init(playerId, isReady);
        cellList.Add(playerListCellController);
    }
    
    // 注意: 该方法会先于Start
    public override void OnNetworkSpawn() {
        content = canvers.Find("PlayerList/Viewport/Content");
        cell = content.Find("Cell").gameObject;
        startButton = canvers.Find("StartButton").GetComponent<Button>();
        readyToggle = canvers.Find("ReadyToggle").GetComponent<Toggle>();
        
        startButton.onClick.AddListener(OnStartButtonClick);
        readyToggle.onValueChanged.AddListener(OnReadyToggleClick);

        cellList = new List<PlayerListCellController>();
        
        // 将本地玩家添加到网络中
        AddPlayer(NetworkManager.LocalClientId, false);
        base.OnNetworkSpawn();
    }

    private void OnStartButtonClick() {
        
    }

    private void OnReadyToggleClick(bool args) {
        
    }

    void Update() {
        
    }
}
