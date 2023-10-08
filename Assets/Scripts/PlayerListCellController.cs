using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

// UI: 存放player数据并获取content下的组件
public class PlayerListCellController : MonoBehaviour 
{
    private TMP_Text playerName;
    private TMP_Text ready;
    private TMP_Text gender;
    
    public void Init(PlayerInfoData playerInfoData) {
        playerName = transform.Find("Name").GetComponent<TMP_Text>();
        playerName.text = playerInfoData.playerName.ToString();
        
        ready = transform.Find("Ready").GetComponent<TMP_Text>();
        SetReady(playerInfoData.isReady);
        
        gender = transform.Find("Gender").GetComponent<TMP_Text>();
        SetGender(playerInfoData.gender);
    }

    public void UpdatePlayerCellInfo(PlayerInfoData playerInfo) {
        SetName(playerInfo.playerName);
        SetReady(playerInfo.isReady);
        SetGender(playerInfo.gender);
    }
    
    public void SetName(string name) {
        playerName.text = name;
    }
    
    public void SetReady(bool isReady) {
        ready.text = isReady ? "准备" : "未准备";
    }

    public void SetGender(GENDER playerGender) {
        gender.text = playerGender == GENDER.Male ? "男" : "女";
    }
}
