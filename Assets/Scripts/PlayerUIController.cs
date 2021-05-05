using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUIController : MonoBehaviour {
    public string id;
    public Transform sacrifices, passives, mana;
    public Image targeted;

    public Sprite sacrificeEmpty, sacrificeFilled;
    public CosmicAPI api;


    public void UpdateUI(Player player) {
        transform.Find("hp").GetComponent<Text>().text = player.hp.ToString();



        for (int i = 0; i < sacrifices.childCount; i++) {
            GameObject sacrificeCrystal = sacrifices.GetChild(i).gameObject;
            bool active = player.buff.sacrifices >= (i + 1);
            sacrificeCrystal.GetComponent<Image>().color = active ? api.elementColors[player.buff.element.ToString().ToLower()] : Color.white;
            sacrificeCrystal.GetComponent<Image>().sprite = active ? sacrificeFilled : sacrificeEmpty;
        }

        if (!player.turn) {
            SetMana(0, player.totalMana);
            DisableMana();
        } else {
            EnableMana();
            SetMana(player.manaLeft, player.totalMana);
        }
    }

    public void DisableMana() {
        mana.Find("Mana").GetComponent<Image>().color = Color.black;
    }

    public void EnableMana() {
        mana.Find("Mana").GetComponent<Image>().color = new Color(0, 255, 246);
    }

    public void SetMana(int manaAmount, int totalAmount) {
        int defaultAmount = 9;
        Transform crystals = mana.Find("Mana");
        for (int i = 0; i < totalAmount; i++) {
            int index = i % 9;
            bool active = (i + 1) <= manaAmount;
            crystals.GetChild(i).GetComponent<Image>().color = active ? Color.white : Color.black;
            GetComponentInChildren<Text>().text = $"{manaAmount}/{totalAmount}";
        }
    }
}
