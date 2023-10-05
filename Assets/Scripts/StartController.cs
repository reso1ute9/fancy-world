using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class StartController : MonoBehaviour {
    [SerializeField] private Transform canvers;
    
    // Start is called before the first frame update
    void Start() {
        Button createButton = canvers.Find("CreateButton").GetComponent<Button>();
        Button jointButton = canvers.Find("JointButton").GetComponent<Button>();
        
        createButton.onClick.AddListener(OnCreateButtonClick);
        jointButton.onClick.AddListener(OnJointButtonClick);
    }
    
    // 点击创建房间时玩家会作为主机
    private void OnCreateButtonClick() {
        NetworkManager.Singleton.StartHost();
    }

    // 点击加入房间时玩家会加入主机房间
    private void OnJointButtonClick() {
        NetworkManager.Singleton.StartClient();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
