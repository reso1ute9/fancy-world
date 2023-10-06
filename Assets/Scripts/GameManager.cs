using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : NetworkBehaviour {
    public static GameManager Instance;
    
    // Start is called before the first frame update
    void Start() {
        Instance = this;
        // 在切换场景时挂载该脚本的物体会保留
        DontDestroyOnLoad(gameObject);
        SceneManager.LoadScene("StartScene");
    }

    public void LoadScene(string sceneName) {
        // 网络上加载场景
        NetworkManager.SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
    }

    // Update is called once per frame
    void Update() {
        
    }
}