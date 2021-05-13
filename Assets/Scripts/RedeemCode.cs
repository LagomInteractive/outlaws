using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class RedeemCode : MonoBehaviour {

    public Color32 redeemSuccess, redeemFail;

    public CosmicAPI api;
    public Text redeemText;
    public InputField codeInput;
    public Button redeemButton;

    void Start() {
        codeInput.onValidateInput += (string text, int index, char addedChar) => {
            return Char.ToUpper(addedChar);
        };
        redeemButton.onClick.AddListener(() => {
            api.RedeemCode(codeInput.text);
        });

        api.OnCodeRedeem += (success, message) => {
            redeemText.text = message;
            redeemText.color = success ? redeemSuccess : redeemFail;
            if (success) codeInput.text = "";
        };
        redeemText.text = "";
    }

    void Update() {

    }
}
