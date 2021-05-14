using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Login : MonoBehaviour {

    public CosmicAPI api;

    public InputField username, password;
    public Text loginFail;
    public void LoginWithPass() {
        api.Login(username.text, password.text);
    }

    private void Start() {
        api.OnLoginFail += reason => {
            loginFail.text = reason;
        };
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.Tab)) {
            if (username.isFocused) {
                password.Select();
                password.ActivateInputField();
            } else if (password.isFocused) {
                username.Select();
                username.ActivateInputField();
            }
        }
        if (Input.GetKeyDown(KeyCode.Return)) {
            LoginWithPass();
        }
    }
}
