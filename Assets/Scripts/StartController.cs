using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.UI;

public class StartController : MonoBehaviour {
    private static ushort _connectionPort = 7777;
    
    [SerializeField] private Transform gameCanvers;
    
    private TMP_InputField inputIp; 
    
    // Start is called before the first frame update
    void Start() {
        Button createButton = gameCanvers.Find("CreateButton").GetComponent<Button>();
        Button jointButton = gameCanvers.Find("JointButton").GetComponent<Button>();
        
        createButton.onClick.AddListener(OnCreateButtonClick);
        jointButton.onClick.AddListener(OnJointButtonClick);

        inputIp = gameCanvers.Find("IP").GetComponent<TMP_InputField>();
    }
    
    // 点击创建房间时玩家会作为主机
    private void OnCreateButtonClick() {
        UnityTransport transport = NetworkManager.Singleton.NetworkConfig.NetworkTransport as UnityTransport;
        transport.SetConnectionData(inputIp.text, StartController._connectionPort);
        
        NetworkManager.Singleton.StartHost();
        GameManager.Instance.LoadScene("LobbyScene");
    }

    // 点击加入房间时玩家会加入主机房间
    private void OnJointButtonClick() {
        UnityTransport transport = NetworkManager.Singleton.NetworkConfig.NetworkTransport as UnityTransport;
        transport.SetConnectionData(inputIp.text, StartController._connectionPort);
        
        NetworkManager.Singleton.StartClient();
    }

    // Update is called once per frame
    void Update() {
        
    }
}
