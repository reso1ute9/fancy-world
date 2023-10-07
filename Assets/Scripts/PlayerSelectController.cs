using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSelectController : MonoBehaviour {
    public static PlayerSelectController Instance;

    private void Start() {
        Instance = this;
    }

    public void SwitchGender(GENDER gender) {
        transform.GetChild((int)gender).gameObject.SetActive(true);
        transform.GetChild(1 - (int)gender).gameObject.SetActive(false);
    }
}
