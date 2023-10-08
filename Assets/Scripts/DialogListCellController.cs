using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class DialogListCellController : MonoBehaviour 
{
    private TMP_Text playerName;
    private TMP_Text dialogContent;
    
    public void Initial(string playerName, string dialogContent) {
        this.playerName = transform.Find("Name").GetComponent<TMP_Text>();
        this.playerName.text = playerName + ":";
        this.dialogContent = transform.Find("Content").GetComponent<TMP_Text>();
        this.dialogContent.text = dialogContent;
    }
}
