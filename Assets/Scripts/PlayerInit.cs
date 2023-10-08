using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

// 根据玩家一开始选择的性别初始化玩家信息
public class PlayerInit : NetworkBehaviour
{
    public override void OnNetworkSpawn() {
        GameManager.Instance.OnStartGame.AddListener(OnStartGame);
        base.OnNetworkSpawn();
    }

    // 正式进入Game场景
    private void OnStartGame() {
        // 注意OwnerClientId与LocalClientId的区别
        // LocalClientId: 本地玩家Id, 唯一值
        // OwnerClientId: 代表当前物体是谁拥有的, 例如PlayerInit脚本挂到Player预制体上
        // 需要查找该预制体对应的具体玩家Id时需要用OwnerClientId
        PlayerInfoData playerInfoData = GameManager.Instance.playerInfoDataDict[OwnerClientId];
        Transform body;
        if (playerInfoData.gender == GENDER.Male) {
            body = transform.Find("Male");
        } else {
            body = transform.Find("Female");
        }
        body.gameObject.SetActive(true);
        // 设置摄像机
        if (IsLocalPlayer) {
            GameController.Instance.SetFollowTarget(body);
        }
        // 设置玩家随机出生点
        transform.position = GameController.Instance.GetPlayerSpawnPosition();
    }
}
