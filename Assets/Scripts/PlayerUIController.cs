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

        for (int i = 0; i < passives.childCount; i++) {
            passives.GetChild(i).GetComponent<Image>().color = player.passive >= i + 1 ? Color.white : Color.black;
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

        Transform crystals = mana.Find("Mana");
        int maxManaDefault = 9;
        // Max amount of mana that can be visualized is 18 (9 is default max amount)
        for (int i = 0; i < maxManaDefault * 2; i++) {
            int index = i % maxManaDefault; // It goes over the 9 mana crystals two times, but the index will always be between 0-9
            if (i >= maxManaDefault && i >= manaAmount) break; // Stop rendering after first round of mana crystals if the user has less than 9 mana.

            bool active = (i + 1) <= manaAmount; // If this index mana is active
            bool extra = i >= maxManaDefault; // If the mana is active above the default max
            Color color = Color.black; // Color of an inactive mana crystal
            if (active) color = Color.white;
            if (extra) color = Color.red;

            crystals.GetChild(index).GetComponent<Image>().color = color;
            GetComponentInChildren<Text>().text = $"{manaAmount}/{totalAmount}";
        }
    }
}
