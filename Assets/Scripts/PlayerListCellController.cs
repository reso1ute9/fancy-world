using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

// 存放player数据并获取content下的组件
public class PlayerListCellController : MonoBehaviour 
{
    private TMP_Text playerName;
    private TMP_Text ready;
    private TMP_Text gender;
    
    // public PlayerInfo playerInfo { get; private set; }
    
    public void Init(PlayerInfo playerInfo) {
        // this.playerInfo = playerInfo;
        
        playerName = transform.Find("Name").GetComponent<TMP_Text>();
        playerName.text = "玩家:" + playerInfo.playerId.ToString();
        
        ready = transform.Find("Ready").GetComponent<TMP_Text>();
        SetReady(playerInfo.isReady);
        
        gender = transform.Find("Gender").GetComponent<TMP_Text>();
        SetGender(playerInfo.gender);
    }

    public void UpdatePlayerCellInfo(PlayerInfo playerInfo) {
        SetReady(playerInfo.isReady);
        SetGender(playerInfo.gender);
    }
    
    public void SetReady(bool isReady) {
        ready.text = isReady ? "准备" : "未准备";
    }

    public void SetGender(GENDER playerGender) {
        gender.text = playerGender == GENDER.Male ? "男" : "女";
    }
}
