using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerSync : NetworkBehaviour {
    private NetworkVariable<Vector3> syncPosition = new NetworkVariable<Vector3>();
    private NetworkVariable<Quaternion> syncRotation = new NetworkVariable<Quaternion>();

    private Transform syncTransform;

    // 切换男女玩家预制体
    public void SetTarget(GENDER gender) {
        if (gender == GENDER.Male) {
            syncTransform = transform.Find("Male").transform;
        }
        else {
            syncTransform = transform.Find("Female").transform;
        }
    }
    
    private void Update() {
        // 本地玩家需要不断上传自己的位置和旋转信息
        if (IsLocalPlayer) {
            UploadTransform();
        }
    }
    
    // 防止帧不同导致的数据无法同步
    // FixedUpdate是按照固定时间间隔去更新
    private void FixedUpdate() {
        if (!IsLocalPlayer) {
            SyncTransform();
        }
    }

    // 上传玩家位置和旋转信息
    private void UploadTransform() {
        if (IsServer) {
            syncPosition.Value = syncTransform.position;
            syncRotation.Value = syncTransform.rotation;
        }
        else {
            UploadTransformServerRpc(syncTransform.position, syncTransform.rotation);
        }
    }

    [ServerRpc]
    private void UploadTransformServerRpc(Vector3 position, Quaternion rotation) {
        syncPosition.Value = position;
        syncRotation.Value = rotation;
    }

    // 下载位置和旋转信息
    private void SyncTransform() {
        syncTransform.position = syncPosition.Value;
        syncTransform.rotation = syncRotation.Value;
    }
}
