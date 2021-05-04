using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ClientUpdate : MonoBehaviour {


    void Start() {
        GetComponent<Button>().onClick.AddListener(() => {
            Application.OpenURL("https://cosmic.ygstr.com/download");
        });
    }


    void Update() {

    }
}
