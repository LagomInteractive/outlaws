using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardPreview : MonoBehaviour {

    public MenuSystem menus;

    private void OnMouseDown() {
        menus.CloseMenu();
    }

}
