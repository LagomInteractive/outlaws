using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OpenLink : MonoBehaviour {
    public string link;

    public void Open() {
        Application.OpenURL(link);
    }

}
