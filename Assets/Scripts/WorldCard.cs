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

    public Animation onDamage;

    public TextMeshProUGUI minionTitle, spellTitle;
    public Text descriptionText, elementText, damageText, hpText, manaText;

    public Text minionHp, minionDamage;

    public Sprite minionCard, spellCard, minionMask, spellMask, minionBanner, spellBanner;
    public Image frame, mask, image, banner;
    public Transform activeBorder, targetBorder, battlecryActiveBorder, manaContainer, hpContainer, damageContainer, tauntBorder, minionFrame;
    public Transform activeBorderMinion, targetBorderMinion;

    public Sprite deathrattleBorder, lunarBorder, novaBorder, ripsoteBorder, solarBorder, zenithBorder;

    bool isMinion, isCard;

    public Transform effectsSpawn, previewCard;
    public Animation showPreviewCard;
    public Image rarity;

    public Sprite common, uncommon, rare, epic, celestial, developer;

    public void SetActive(bool active) {
        activeBorder.gameObject.SetActive(active);
    }

    public void SetTargeted(bool active) {
        targetBorder.gameObject.SetActive(active);
    }

    public void SetBattlecryActive(bool active) {
        battlecryActiveBorder.gameObject.SetActive(active);
    }

    public void ShowPreviewCard(CosmicAPI api) {
        if (previewCard.childCount == 0) {
            api.InstantiateCard(id, previewCard);
        } else {
            previewCard.gameObject.SetActive(true);
        }
        showPreviewCard.clip = showPreviewCard["ShowMinionsOriginAnimation"].clip;
        showPreviewCard["ShowMinionsOriginAnimation"].speed = 1;
        showPreviewCard["ShowMinionsOriginAnimation"].time = 0;
        showPreviewCard.Play();
    }

    public void HidePreviewCard() {
        showPreviewCard.clip = showPreviewCard["ShowMinionsOriginAnimation"].clip;
        showPreviewCard["ShowMinionsOriginAnimation"].speed = -1;
        showPreviewCard["ShowMinionsOriginAnimation"].time = showPreviewCard["ShowMinionsOriginAnimation"].time;
        showPreviewCard.Play();
        StartCoroutine(DisablePreviewCard());
    }

    public IEnumerator DisablePreviewCard() {
        yield return new WaitForSeconds(.02f);
        previewCard.gameObject.SetActive(false);
    }

    public void AnimateDamage(int damage) {
        onDamage.gameObject.GetComponent<Text>().text = "-" + damage.ToString();
        onDamage.Rewind();
        onDamage.Play();
    }

    public void Setup(string id, CosmicAPI api) {
        minionId = id;
        Setup(-1, api);
    }

    public void Setup(int id, CosmicAPI api) {
        Minion minion = null;
        isCard = true;
        if (id != -1) {
            this.id = id;
            origin = api.GetCard(id);
        } else {
            minion = (Minion)api.GetCharacter(minionId);
            origin = api.GetCard(minion.origin);
            isCard = false;
        }


        image.sprite = origin.image;

        if (isCard) {
            switch (origin.rarity) {
                case Rarity.Common:
                    rarity.sprite = common;
                    break;
                case Rarity.Uncommon:
                    rarity.sprite = uncommon;
                    break;
                case Rarity.Rare:
                    rarity.sprite = rare;
                    break;
                case Rarity.Epic:
                    rarity.sprite = epic;
                    break;
                case Rarity.Celestial:
                    rarity.sprite = celestial;
                    break;
                case Rarity.Developer:
                    rarity.sprite = developer;
                    break;
            }
        } else {
            rarity.gameObject.SetActive(false);
        }

        onDamage.Play();
        onDamage.Sample();
        onDamage.Stop();

        if (isCard) {
            isMinion = origin.type == CardType.Minion;
            mask.sprite = isMinion ? minionMask : spellMask;
            frame.sprite = isMinion ? minionCard : spellCard;

            minionTitle.gameObject.SetActive(isMinion);
            spellTitle.gameObject.SetActive(!isMinion);


            banner.sprite = isMinion ? minionBanner : spellBanner;

            string elementName = origin.element.ToString().ToLower();
            if (elementName == "rush" || elementName == "taunt") elementName = "neutral";
            elementText.color = api.elementColors[elementName];

            if (!isMinion) {
                Destroy(hpContainer.gameObject);
                Destroy(damageContainer.gameObject);
            }

            descriptionText.text = origin.description;
            elementText.text = origin.element.ToString().ToUpper();



            if (!isMinion) {
                hpText.gameObject.SetActive(false);
                damageText.gameObject.SetActive(false);
                spellTitle.text = origin.name;
            } else {
                minionTitle.text = origin.name;
            }
        } else {
            // This is a board minion
            minionFrame.gameObject.SetActive(true);
            Destroy(frame.gameObject);
            hpText = minionHp;
            damageText = minionDamage;

            if (origin.element == Element.Taunt) tauntBorder.gameObject.SetActive(true);

            Image minionFrameImage = minionFrame.GetComponent<Image>();

            if (minion.deathrattle)
                minionFrameImage.sprite = deathrattleBorder;
            else if (minion.riposte) minionFrameImage.sprite = ripsoteBorder;
            else if (origin.element == Element.Nova) minionFrameImage.sprite = novaBorder;
            else if (origin.element == Element.Lunar) minionFrameImage.sprite = lunarBorder;
            else if (origin.element == Element.Solar) minionFrameImage.sprite = solarBorder;
            else if (origin.element == Element.Zenith) minionFrameImage.sprite = zenithBorder;

            activeBorder = activeBorderMinion;

            targetBorder = targetBorderMinion;
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
        if (hp < 0) hp = 0;
        this.hp = hp;
        UpdateCardValues();
    }
    public void SetDamage(int damage) {
        this.damage = damage;
        UpdateCardValues();
    }

    public void PlayEffect(Effect effect, AudioSource source) {
        while (effectsSpawn.childCount > 0) DestroyImmediate(effectsSpawn.GetChild(0).gameObject);

        GameObject effectObject = Instantiate(effect.particles, effectsSpawn);
        effectObject.transform.localPosition = Vector3.zero;
        effect.PlayParticles(effectObject);
        effect.PlaySound(source);
    }

    public void SetMana(int mana) {
        this.mana = mana;
        UpdateCardValues();
    }

    void UpdateCardValues() {
        manaText.text = mana.ToString();
        if (isMinion) {
            if (hpText == null) return;
            hpText.text = hp.ToString();
            damageText.text = damage.ToString();

            if (damage == origin.damage) damageText.color = Color.white;
            else damageText.color = damage > origin.damage ? Color.green : Color.red;

            if (hp == origin.hp) hpText.color = Color.white;
            else hpText.color = hp > origin.hp ? Color.green : Color.red;
        }
    }

    void Update() {

    }
}
