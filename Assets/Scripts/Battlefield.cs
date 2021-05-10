using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Battlefield : MonoBehaviour {

    public CosmicAPI api;
    public Transform friendlyUnits, opponentUnits;
    public EffectsManager effectsManager;

    List<WorldCard> unitsList = new List<WorldCard>();

    GameObject previewCard;

    WorldCard GetMinion(string id) {
        foreach (WorldCard wc in unitsList) {
            if (wc.GetMinionId() == id) {
                return wc;
            }
        }
        return null;
    }

    void AddMinion(string id) {
        Minion minion = (Minion)api.GetCharacter(id);

        GameObject card = api.InstantiateMinionCard(minion.id, minion.owner == api.GetPlayer().id ? friendlyUnits : opponentUnits);

        unitsList.Add(card.GetComponent<WorldCard>());

        CenterBoards();
    }

    void CenterBoards() {
        CenterBoard(friendlyUnits);
        CenterBoard(opponentUnits);
    }

    /// <summary>
    /// Centeres all the minions and moves them closer or further apart incase a minion has spawned or died/sacrificed.
    /// </summary>
    void CenterBoard(Transform parent) {
        float spacing = 7;

        for (int i = 0; i < parent.childCount; i++) {
            Transform minion = parent.GetChild(i);
            minion.localPosition = new Vector3(spacing * i, 0, 0);
        }

        parent.localPosition = new Vector3(-((spacing * parent.childCount) / 2 - (parent.childCount > 0 ? spacing / 2 : 0)), parent.localPosition.y, parent.localPosition.z);
    }

    /// <summary>
    /// Updates general things to battlefield minions.
    /// i.e Battlecry status, Can attack status. Runs on every server update
    /// </summary>
    void GeneralUpdate() {
        foreach (WorldCard wc in unitsList) {
            Minion minion = (Minion)api.GetCharacter(wc.GetMinionId());
            if (minion != null) {
                wc.SetBattlecryActive(minion.battlecryActive);
                wc.SetActive(minion.canAttack(api));
                wc.SetDamage(minion.damage);
            }
        }
    }

    /// <summary>
    /// Clear the field of every minion, used when the game is over
    /// </summary>
    void ClearField() {
        while (unitsList.Count > 0) unitsList.RemoveAt(0);
        while (friendlyUnits.childCount > 0) DestroyImmediate(friendlyUnits.GetChild(0).gameObject);
        while (opponentUnits.childCount > 0) DestroyImmediate(opponentUnits.GetChild(0).gameObject);
    }



    IEnumerator RemoveMinion(string id, float delay = 0) {
        WorldCard minion = GetMinion(id);
        yield return new WaitForSeconds(delay);
        DestroyImmediate(minion.gameObject);
        CenterBoards();
    }

    private void Update() {
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Physics.Raycast(ray, out hit, Mathf.Infinity, LayerMask.GetMask("ArrowTarget"));
        bool activePreview = false;
        if (hit.collider) {
            if (hit.collider.gameObject.tag == "Minion") {
                activePreview = true;
                if (previewCard != hit.collider.gameObject) {
                    if (previewCard != null) previewCard.GetComponent<WorldCard>().HidePreviewCard();

                    previewCard = hit.collider.gameObject;
                    previewCard.GetComponent<WorldCard>().ShowPreviewCard(api);

                }
            }
        }
        if (!activePreview && previewCard != null) {
            previewCard.GetComponent<WorldCard>().HidePreviewCard();
            previewCard = null;
        }

    }


    void Start() {
        api.OnMinionSpawned += (id) => {
            AddMinion(id);
        };

        api.OnMinionDamage += (id, damage) => {
            WorldCard minion = GetMinion(id);
            if (minion == null) return;
            minion.AnimateDamage(damage);
            minion.SetHp(minion.GetHp() - damage);
            minion.PlayEffect(effectsManager.GetEffect("ON_HIT"), effectsManager.audioSource);
        };

        api.OnMinionSacrificed += (id) => {
            WorldCard minion = GetMinion(id);
            minion.PlayEffect(effectsManager.GetEffect("ON_SACRIFICED"), effectsManager.audioSource);
            StartCoroutine(RemoveMinion(id, 1));
        };

        api.OnMinionHeal += (id, amount) => {
            WorldCard minion = GetMinion(id);
            //minion.AnimateDamage(damage);
            minion.SetHp(minion.GetHp() + amount);
        };

        api.OnMinionDeath += (id) => {
            StartCoroutine(RemoveMinion(id));
        };

        api.OnGameEnd += winner => {
            ClearField();
        };

        api.OnUpdate += () => {
            GeneralUpdate();
        };
    }
}
