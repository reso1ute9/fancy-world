using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

// 存放player数据并获取content下的组件
public class PlayerListCellController : MonoBehaviour 
{
    private TMP_Text playerName;
    private TMP_Text ready;
    
    public void Init(ulong playerId, bool isReady) {
        playerName = transform.Find("Name").GetComponent<TMP_Text>();
        ready = transform.Find("Ready").GetComponent<TMP_Text>();
        playerName.text = "玩家:" + playerId.ToString();
        ready.text = isReady ? "准备" : "未准备";
    }
}
