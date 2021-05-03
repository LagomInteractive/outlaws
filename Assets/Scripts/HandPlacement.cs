using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HandPlacement : MonoBehaviour {

    public CosmicAPI api;
    public LayerMask draggableCardLayerMask;

    public void Start() {

        api.OnUpdate += () => {
            SetActiveCards();
        };
    }

    public void SetActiveCards() {
        for (int i = 0; i < transform.childCount; i++) {
            WorldCard wc = transform.GetChild(i).GetComponent<WorldCard>();
            wc.SetActive(api.GetPlayer().turn && api.GetPlayer().manaLeft >= wc.GetMana());
        }
    }

    void DeleteCards() {
        while (transform.childCount > 0) {
            DestroyImmediate(transform.GetChild(0).gameObject);
        }
    }

    public void UpdateHand() {
        DeleteCards();
        int[] cards = api.GetPlayer().cards;

        // Distance between each card, including card width
        float distance = .7f;
        float handWidth = cards.Length * distance;
        float start = -(handWidth / 2);

        for (int i = 0; i < cards.Length; i++) {
            GameObject cardObject = api.InstantiateCard(cards[i], transform, true);

            DragableObject draggable = cardObject.AddComponent<DragableObject>();
            draggable.api = api;
            draggable.id = cards[i];
            draggable.layermask = draggableCardLayerMask;

            cardObject.GetComponent<WorldCard>().handIndex = i;
            cardObject.transform.position = transform.position + new Vector3(start + (i * distance), 0, 0);
        }
        SetActiveCards();
    }
}
