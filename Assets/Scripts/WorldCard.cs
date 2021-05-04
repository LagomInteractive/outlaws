using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// This script should be added to in game 3D cards
public class WorldCard : MonoBehaviour {


    public int handIndex;
    int id, hp, damage, mana;
    string minionId;
    Card origin;

    public TextMeshProUGUI minionTitle, spellTitle;
    public Text descriptionText, elementText, damageText, hpText, manaText;

    public Sprite minionCard, spellCard, minionMask, spellMask, minionBanner, spellBanner;
    public Image frame, mask, image, banner;
    public Transform activeBorder, targetBorder, battlecryActiveBorder, manaContainer, hpContainer, damageContainer;



    bool isMinion;

    Dictionary<string, Color> elementColors = new Dictionary<string, Color>() {
        {"lunar", new Color(0, 162, 255)},
        {"solar", new Color(255, 38, 0)},
        {"zenith", new Color(46, 255, 99)},
        {"nova", new Color(147, 40, 246)},
        {"taunt", new Color(230, 230, 230)},
        {"rush", new Color(22, 22, 22)}
    };

    public void SetActive(bool active) {
        activeBorder.gameObject.SetActive(active);
    }

    public void SetTargeted(bool active) {
        targetBorder.gameObject.SetActive(active);
    }

    public void SetBattlecryActive(bool active) {
        battlecryActiveBorder.gameObject.SetActive(active);
    }

    public void Setup(string id, CosmicAPI api) {
        minionId = id;
        Setup(-1, api);
    }


    public void Setup(int id, CosmicAPI api) {
        if (id != -1) {
            this.id = id;
            origin = api.GetCard(id);
        } else {
            Minion minion = (Minion)api.GetCharacter(minionId);
            origin = api.GetCard(minion.origin);
        }

        isMinion = origin.type == CardType.Minion;

        minionTitle.gameObject.SetActive(isMinion);
        spellTitle.gameObject.SetActive(!isMinion);

        mask.sprite = isMinion ? minionMask : spellMask;
        frame.sprite = isMinion ? minionCard : spellCard;
        banner.sprite = isMinion ? minionBanner : spellBanner;

        if (!isMinion) {
            Destroy(hpContainer.gameObject);
            Destroy(damageContainer.gameObject);
        }

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

    /// <summary>
    /// If this world card is a spawned minion, this will return its UUID
    /// </summary>
    /// <returns>Minion UUID</returns>
    public string GetMinionId() {
        return minionId;
    }

    public int GetMana() {
        return mana;
    }

    public void SetHp(int hp) {
        this.hp = hp;
        UpdateCardValues();
    }
    public void SetDamage(int damage) {
        this.damage = damage;
        UpdateCardValues();
    }
    public void SetMana(int mana) {
        this.mana = mana;
        UpdateCardValues();
    }

    void UpdateCardValues() {
        manaText.text = mana.ToString();
        if (isMinion) {
            hpText.text = hp.ToString();
            damageText.text = damage.ToString();


            damageText.color = damage != origin.damage ? Color.green : Color.white;

        }
    }

    void Update() {

    }
}
