using System;
using System.Collections;

using System.Collections.Generic;
using Newtonsoft.Json.Serialization;
using UnityEngine;
public class DragableObject : MonoBehaviour {
    private Vector3 mOffset;
    private float mZCoord;
    private RaycastHit hit;
    public CosmicAPI api;

    public float cardSpeed;
    public float cardRotation;
    public bool cardMoving = false;
    public int id;

    bool dragging = false;

    bool animatingBack = false;
    float activeOffset = .5f;

    Vector3 originalPosition;

    public LayerMask layermask;

    bool activeOverwrite = false;

    private void Start() {
        SetOriginalPosition();
    }

    public void SetOriginalPosition() {
        originalPosition = transform.position;
    }

    private Vector3 GetMouseAsWorldPoint() {
        // Pixel coordinates of mouse (x,y)
        Vector3 mousePoint = Input.mousePosition;
        // z coordinate of game object on screen
        mousePoint.z = mZCoord;
        // Convert it to world 
        return Camera.main.ScreenToWorldPoint(mousePoint);
    }

    void OnMouseDown() {
        if (GetComponent<WorldCard>().GetMana() <= api.GetPlayer().manaLeft && api.GetPlayer().turn && api.GetCard(id).type != CardType.TargetSpell) {
            dragging = true;
            mZCoord = Camera.main.WorldToScreenPoint(gameObject.transform.position).z;
            // Store offset = gameobject world pos - mouse world pos
            mOffset = gameObject.transform.position - GetMouseAsWorldPoint();
        }
    }

    private void OnMouseEnter() {
        if ((dragging || animatingBack)) return;
        MoveCardPosition(true);
    }

    public void SetActivePositionOverwrite(bool active) {
        activeOverwrite = active;
        MoveCardPosition(active);
    }

    public void MoveCardPosition(bool active) {
        StartCoroutine(MoveOverSeconds(gameObject, active ? (originalPosition + new Vector3(0, .2f, 0) + transform.TransformDirection(Vector3.up) * activeOffset) : originalPosition, .1f));
    }

    private void OnMouseExit() {
        if (dragging || animatingBack || activeOverwrite) return;
        StartCoroutine(MoveOverSeconds(gameObject, originalPosition, .1f));
    }

    void OnMouseDrag() {
        if (!dragging) return;
        transform.gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Physics.Raycast(ray, out hit, Mathf.Infinity, layermask);

        transform.position = GetMouseAsWorldPoint() + mOffset;
    }
    private void OnMouseUp() {
        if (!dragging) return;
        animatingBack = true;
        dragging = false;
        if (hit.collider == null || hit.collider.tag != "Board") {
            StartCoroutine(MoveOverSeconds(gameObject, originalPosition, 0.5f, () => {
                animatingBack = false;
            }));
            transform.gameObject.layer = LayerMask.NameToLayer("Default");
        } else if (hit.collider.tag == "Board") {
            WorldCard card = GetComponent<WorldCard>();
            switch (api.GetCard(card.GetId()).type) {
                case CardType.Minion:
                    api.PlayMinion(id);
                    break;
                case CardType.AOESpell:
                    api.PlaySpell(id);
                    break;
            }

        }
    }

    void RotateOnMove() {
        cardRotation = cardSpeed * 50;
        Mathf.Clamp(cardRotation, -40, 40);
        Quaternion target = Quaternion.Euler(0, 0, cardRotation);
        transform.rotation = Quaternion.Slerp(transform.rotation, target, Time.deltaTime * 5f);
    }

    public IEnumerator MoveOverSpeed(GameObject objectToMove, Vector3 end, float speed) {
        while (objectToMove.transform.position != end) {
            objectToMove.transform.position = Vector3.MoveTowards(objectToMove.transform.position, end, speed * Time.deltaTime);
            yield return new WaitForEndOfFrame();
        }
    }


    public IEnumerator MoveOverSeconds(GameObject objectToMove, Vector3 end, float seconds, Action callback = null) {
        float elapsedTime = 0;
        Vector3 startingPos = objectToMove.transform.position;
        while (elapsedTime < seconds) {
            objectToMove.transform.position = Vector3.Lerp(startingPos, end, (elapsedTime / seconds));
            elapsedTime += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        callback?.Invoke();
        objectToMove.transform.position = end;
        cardMoving = false;
    }
}