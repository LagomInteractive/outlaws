using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// This script should be added to in game 3D cards
public class WorldCard : MonoBehaviour {


    int id, hp, damage, mana;
    Card origin;

    public TextMeshProUGUI minionTitle, spellTitle;
    public Text descriptionText, elementText, damageText, hpText, manaText;

    public Sprite minionCard, spellCard, minionMask, spellMask;
    public Image frame, mask, image;

    bool isMinion;

    Dictionary<string, Color> elementColors = new Dictionary<string, Color>() {
        {"lunar", new Color(0, 162, 255)},
        {"solar", new Color(255, 38, 0)},
        {"zenith", new Color(46, 255, 99)},
        {"nova", new Color(147, 40, 246)},
        {"taunt", new Color(230, 230, 230)},
        {"rush", new Color(22, 22, 22)}
    };

    public void Setup(int id, CosmicAPI api) {
        this.id = id;
        origin = api.GetCard(id);

        isMinion = origin.type == CardType.Minion;

        minionTitle.gameObject.SetActive(isMinion);
        spellTitle.gameObject.SetActive(!isMinion);

        mask.sprite = isMinion ? minionMask : spellMask;
        frame.sprite = isMinion ? minionCard : spellCard;

        descriptionText.text = origin.description;
        elementText.text = origin.element.ToString().ToUpper();

        image.sprite = origin.image;

        if (!isMinion) {
            hpText.gameObject.SetActive(false);
            damageText.gameObject.SetActive(false);
            spellTitle.text = origin.name;
        } else {
            minionTitle.text = origin.name;
        }

        hp = origin.hp;
        damage = origin.damage;
        mana = origin.mana;

        UpdateCardValues();
    }

    public int GetId() {
        return id;
    }
    public int GetHp() {
        return hp;
    }
    public int GetDamage() {
        return damage;
    }

    public void SetHp(int hp) {
        this.hp = hp;
    }
    public void SetDamage(int damage) {
        this.damage = damage;
    }
    public void SetMana(int mana) {
        this.mana = mana;
    }

    void UpdateCardValues() {
        manaText.text = mana.ToString();
        if (isMinion) {
            hpText.text = hp.ToString();
            damageText.text = damage.ToString();
        }
    }

    void Update() {

    }
}
