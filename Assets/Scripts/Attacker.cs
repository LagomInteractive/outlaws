using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackInfo {
    public string attacker, target;
}

public class Attacker : MonoBehaviour {

    WorldCard wc;
    public CosmicAPI api;

    AttackLine line;

    bool dragging = false;
    Vector3 start;

    WorldCard targetMinion;
    bool opponentTargeted = false;

    GameObject opponentTargetUI;

    private void Start() {
        wc = GetComponent<WorldCard>();
        line = GameObject.Find("AttackLine").GetComponent<AttackLine>();
        opponentTargetUI = GameObject.Find("Camera").transform.Find("UI").transform.Find("OpponentTargeted").gameObject;
    }

    private void OnMouseDown() {
        Minion minion = (Minion)api.GetCharacter(wc.GetMinionId());
        if (minion.canAttack(api)) {
            dragging = true;
            start = GetMousePosition();
        }
    }

    GameObject GetTarget() {
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Physics.Raycast(ray, out hit, Mathf.Infinity, LayerMask.GetMask("Attackable"));
        if (hit.collider) {
            return hit.transform.gameObject;
        }
        return null;
    }

    Vector3 GetMousePosition() {
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Physics.Raycast(ray, out hit, Mathf.Infinity, LayerMask.GetMask("AttackArrow"));
        if (hit.collider) {
            return hit.point;
        }
        return new Vector3();
    }

    private void OnMouseDrag() {
        if (dragging) {


            GameObject target = GetTarget();

            if (target && target.name == "OpponentTarget") {
                if (api.GetOpponent().canBeAttacked(api))
                    opponentTargeted = true;
            } else if (target) {
                if (targetMinion != null && target != targetMinion.gameObject) UpdateTarget(false);

                Minion minion = (Minion)api.GetCharacter(target.GetComponent<WorldCard>().GetMinionId());
                if (minion.canBeAttacked(api)) {
                    targetMinion = target.GetComponent<WorldCard>();
                    targetMinion.SetTargeted(true);
                }

            } else {
                UpdateTarget(true);
                opponentTargeted = false;
            }
        }
    }

    void UpdateTarget(bool mouseDown) {
        if ((!mouseDown || GetTarget() == null) && targetMinion != null) {
            targetMinion.SetTargeted(false);
            targetMinion = null;
        }
    }

    void Attack(string target) {
        api.Attack(wc.GetMinionId(), target);
    }

    private void OnMouseUp() {

        if (opponentTargeted) {
            Attack(api.GetOpponent().id);
        } else if (targetMinion) {
            Attack(targetMinion.GetMinionId());
        }

        dragging = false;
        opponentTargeted = false;
        UpdateTarget(false);
        line.StopDrawing();
        opponentTargetUI.SetActive(opponentTargeted);
    }

    void Update() {
        if (dragging) opponentTargetUI.SetActive(opponentTargeted);
    }
}
