using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OpenLink : MonoBehaviour {
    public string link;
    void Start() {

    }

    public void Open() {
        Debug.Log("Opening link: " + transform.parent.name);
        Application.OpenURL(link);
    }

    void Update() {

    }
}
