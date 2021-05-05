using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AttackLine : MonoBehaviour {

    LineRenderer line;

    public CosmicAPI api;

    // Start point of the line (origin)
    Vector3 start;

    /// If the current action is a damage attack (else it's a spell target attack)
    bool attackMode = false;
    // If the line is currently active, being used and drawn
    public bool active = false;

    WorldCard activeSpellCard;
    WorldCard attackerCard;
    WorldCard targetMinion;
    PlayerUIController targetPlayer;

    public Image sacrificeButton;

    public PlayerUIController player, opponent;
    bool canSacrifice = false;
    bool sacrificeActive = false;

    void Start() {
        line = GetComponent<LineRenderer>();
    }

    public void StopDrawing() {
        for (int i = 0; i < 2; i++) line.SetPosition(i, Vector3.zero);

        if (attackerCard) attackerCard.SetTargeted(false);
        if (activeSpellCard) {
            activeSpellCard.SetTargeted(false);
            activeSpellCard.GetComponent<DragableObject>().SetActivePosition(false);
        }
        attackerCard = null;
        activeSpellCard = null;
    }

    GameObject GetTarget() {
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Physics.Raycast(ray, out hit, Mathf.Infinity, LayerMask.GetMask("ArrowTarget"));
        if (hit.collider) {
            return hit.collider.gameObject;
        }
        return null;
    }

    Vector3 GetWorldSpaceMousePosition() {
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Physics.Raycast(ray, out hit, Mathf.Infinity, LayerMask.GetMask("AttackArrow"));
        if (hit.collider) {
            return hit.point;
        }
        return Vector3.zero;
    }

    void SetLineColor(bool attack = true) {
        line.material.color = attack ? Color.red : Color.green;
    }

    void SetStart() {
        start = GetWorldSpaceMousePosition();
    }

    void SetSacrificeActive(bool active) {
        sacrificeButton.color = active ? Color.red : Color.white;
        sacrificeActive = active;
    }

    void ShowSacrifice(bool visible) {
        sacrificeButton.gameObject.SetActive(visible);
    }

    void Update() {



        if (!active && Input.GetMouseButtonDown(0)) {

            GameObject origin = GetTarget();
            if (origin) {
                if (origin.tag == "Minion") {
                    WorldCard wc = origin.GetComponent<WorldCard>();
                    Minion minion = (Minion)api.GetCharacter(wc.GetMinionId());

                    if (minion.owner == api.GetPlayer().id && (minion.canAttack(api) || minion.battlecryActive || minion.canSacrifice(api))) {
                        active = true;
                        attackMode = !minion.battlecryActive;
                        if (minion.canSacrifice(api))
                            canSacrifice = true;
                        attackerCard = wc;
                        attackerCard.SetTargeted(true);
                        SetStart();
                        SetLineColor(attackMode); // Set the line to attack color
                    }
                } else if (origin.tag == "Card") {
                    WorldCard spellCard = origin.GetComponent<WorldCard>();
                    if (spellCard.GetMana() <= api.GetPlayer().manaLeft && api.GetCard(spellCard.GetId()).type == CardType.TargetSpell) {
                        active = true;
                        attackMode = false;
                        activeSpellCard = spellCard;
                        activeSpellCard.SetTargeted(true);
                        activeSpellCard.GetComponent<DragableObject>().SetActivePosition(true);
                        SetStart();
                    }
                }
            }
        }

        if (active) {
            ClearTarget();
            line.SetPosition(0, start);
            line.SetPosition(1, GetWorldSpaceMousePosition());

            GameObject target = GetTarget();

            targetMinion = null;
            targetPlayer = null;

            if (target) {
                if (target.tag == "Action") {
                    if (target.name == "Sacrifice") {
                        SetSacrificeActive(true);
                    }
                }
                if (target.tag == "Player") {
                    bool isOpponent = target.GetComponent<PlayerHit>().opponent;
                    targetPlayer = isOpponent ? opponent : player;
                    targetPlayer.id = isOpponent ? api.GetOpponent().id : api.GetPlayer().id;
                    targetPlayer.targeted.gameObject.SetActive(true);
                } else if (target.tag == "Minion") {
                    WorldCard wc = target.GetComponent<WorldCard>();
                    if (!attackerCard || wc.GetMinionId() != attackerCard.GetMinionId()) {
                        targetMinion = wc;
                        targetMinion.SetTargeted(true);
                    }

                }
            }
        }

        if (canSacrifice) ShowSacrifice(true);

        if (active && Input.GetMouseButtonUp(0)) {
            active = false;
            string targetId = null;
            if (targetPlayer) targetId = targetPlayer.id;
            if (targetMinion) targetId = targetMinion.GetMinionId();

            if (sacrificeActive) {
                api.Sacrifice(attackerCard.GetMinionId());
            }

            if (targetId != null) {
                if (attackerCard) {
                    if (attackMode) api.Attack(attackerCard.GetMinionId(), targetId);
                    else api.Battlecry(attackerCard.GetMinionId(), targetId);
                } else if (activeSpellCard) {
                    api.PlaySpell(activeSpellCard.handIndex, targetId);
                }
            }

            canSacrifice = false;
            ClearTarget();
            StopDrawing();
        }
    }

    void ClearTarget() {
        SetSacrificeActive(false);
        ShowSacrifice(false);
        if (targetMinion) targetMinion.SetTargeted(false);
        if (targetPlayer) targetPlayer.targeted.gameObject.SetActive(false);
    }
}
