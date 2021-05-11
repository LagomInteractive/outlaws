using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUIController : MonoBehaviour {
    public string id;
    public Transform sacrifices, mana;

    public Sprite manaFilled, manaEmpty, manaOvercap;

    public Text hp;

    public Text manaText;
    public Image targeted;
    public Image passiveSlider, hpCircle;

    public CosmicAPI api;


    public void UpdateUI(Player player) {
        hp.text = player.hp.ToString();
        hpCircle.fillAmount = player.hp / 30f;
        passiveSlider.fillAmount = player.passive / 5f;

        for (int i = 0; i < sacrifices.childCount; i++) {
            GameObject sacrificeCrystal = sacrifices.GetChild(i).gameObject;
            bool active = player.buff.sacrifices >= (i + 1);
            sacrificeCrystal.GetComponent<Image>().color = active ? api.elementColors[player.buff.element.ToString().ToLower()] : Color.white;
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
        mana.GetComponent<Image>().color = Color.black;
    }

    public void EnableMana() {
        mana.GetComponent<Image>().color = new Color(0, 255, 246);
    }

    public void SetMana(int manaAmount, int totalAmount) {

        int maxManaDefault = 9;
        // Max amount of mana that can be visualized is 18 (9 is default max amount)
        for (int i = 0; i < maxManaDefault * 2; i++) {
            int index = i % maxManaDefault; // It goes over the 9 mana crystals two times, but the index will always be between 0-9
            if (i >= maxManaDefault && i >= manaAmount) break; // Stop rendering after first round of mana crystals if the user has less than 9 mana.

            bool active = (i + 1) <= manaAmount; // If this index mana is active
            bool extra = i >= maxManaDefault; // If the mana is active above the default max

            mana.GetChild(index).GetComponent<Image>().sprite = extra ? manaOvercap : (active ? manaFilled : manaEmpty);
            manaText.text = $"{manaAmount}\n\n{totalAmount}";
        }
    }
}
