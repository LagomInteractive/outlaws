using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Tips : MonoBehaviour {

    public CosmicAPI api;
    public Text body, title, category, number;
    public Slider progressBar;

    void Start() {
        api.OnTips += tips => {
            Debug.Log("Test");
        };
    }

    void Update() {

    }
}
