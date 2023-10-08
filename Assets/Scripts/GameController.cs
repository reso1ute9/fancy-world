using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine.UI;
using UnityEngine;
using Cinemachine;

public class GameController : NetworkBehaviour {
    public static GameController Instance;
    
    [SerializeField] private Transform canvers;

    [SerializeField] private CinemachineVirtualCamera cameraController;

    private TMP_InputField inputMessage;
    private RectTransform DialogContent;
    private GameObject DialogContentCell;
    private Button sendButton;

    public override void OnNetworkSpawn() {
        Instance = this;
        // 输入框组件: 待发送的输入文本
        inputMessage = canvers.Find("Dialog/InputMessage").GetComponent<TMP_InputField>();
        // 聊天框Content
        DialogContent = canvers.Find("Dialog/DialogPanel/Viewport/Content") as RectTransform;
        // 聊天框
        DialogContentCell = DialogContent.Find("Cell").gameObject;
        // 聊天信息发送按钮
        sendButton = canvers.Find("Dialog/SendButton").GetComponent<Button>();
        sendButton.onClick.AddListener(OnSendButtonClick);
        
        base.OnNetworkSpawn();
    }
    
    private void OnSendButtonClick() {
        // 更新聊天框显示内容
        if (string.IsNullOrEmpty(inputMessage.text)) {
            return;
        }
        ulong playerId = NetworkManager.Singleton.LocalClientId;
        PlayerInfoData playerInfoData = GameManager.Instance.playerInfoDataDict[playerId];
        AddDialogCell(playerInfoData.playerName, inputMessage.text);

        if (IsServer) {
            // 将消息同步到所有客户端
            SendMessageToAllClientRpc(playerInfoData, inputMessage.text);
        }
        else {
            // 将消息上传到服务端然后由服务端同步到所有客户端
            SendMessageToServerRpc(playerInfoData, inputMessage.text);
        }
    }
    
    // 发送给客户端的Rpc: 服务端 -> 客户端, 将方法委托给远端进行调用
    [ClientRpc]
    void SendMessageToAllClientRpc(PlayerInfoData playerInfoData, string content) {
        // 避免将消息发给自己
        if (IsServer || NetworkManager.LocalClientId == playerInfoData.playerId) {
            return;
        }
        // 本地显示聊天框内容
        AddDialogCell(playerInfoData.playerName, content);
    }
    
    // 发送给服务端的Rpc: 客户端 -> 服务端, 将方法委托给远端进行调用
    [ServerRpc(RequireOwnership = false)]
    void SendMessageToServerRpc(PlayerInfoData playerInfoData, string content) {
        // 本地显示聊天框内容
        AddDialogCell(playerInfoData.playerName, content);
        SendMessageToAllClientRpc(playerInfoData, content);
    }

    private void AddDialogCell(string playerName, string content) {
        // 先克隆出一个cell并将其挂到聊天框上
        GameObject cell = Instantiate(DialogContentCell);
        cell.transform.SetParent(DialogContent, false);
        // 初始化控制UI脚本展示聊天框结果
        cell.GetComponent<DialogListCellController>().Initial(playerName, content);
        cell.gameObject.SetActive(true);
    }
    
    // 设置相机跟随目标
    public void SetFollowTarget(Transform target) {
        cameraController.Follow = target;
    }
    
    // 获得玩家的出生坐标
    public Vector3 GetPlayerSpawnPosition() {
        // 生成前方和右方方向向量的一个随机结果
        Vector3 offset = transform.forward * Random.Range(-10, 10) + transform.right * Random.Range(-10, 10);
        return transform.position + offset;
    }
}
