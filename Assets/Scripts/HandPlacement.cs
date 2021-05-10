using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HandPlacement : MonoBehaviour {

    public CosmicAPI api;
    public LayerMask draggableCardLayerMask;
    public List<WorldCard> cards = new List<WorldCard>();
    int lastCardPlayedIndex = -1;

    public void Start() {

        api.OnUpdate += () => {
            SetActiveCards();
        };

        api.OnEverythingLoaded += () => {
            RearrangeCards();
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

    void DeleteCard(int index) {
        DestroyImmediate(cards[index].gameObject);
        RearrangeCards();
    }



    public void DealCard(int id) {
        GameObject cardObject = api.InstantiateCard(id, transform, true);
        cards.Add(cardObject.GetComponent<WorldCard>());

        DragableObject draggable = cardObject.AddComponent<DragableObject>();
        draggable.api = api;
        draggable.id = id;
        draggable.layermask = draggableCardLayerMask;

        RearrangeCards();
    }

    public void RearrangeCards() {

        int amountOfCards = cards.Count;
        if (amountOfCards == 0) return;
        float positionScale = 50;
        float spacing = 7;

        int startIndex = 4 - amountOfCards / 2;

        for (int i = 0; i < 8; i++) {
            if (startIndex <= i && i - startIndex > -1 && i - startIndex < amountOfCards) {

                float angle = ((spacing * i) + 67.5f);
                if (amountOfCards % 2 != 0) angle += -5;
                float x = Mathf.Cos(Mathf.Deg2Rad * angle) * positionScale;
                float y = Mathf.Sin(Mathf.Deg2Rad * angle) * positionScale;

                Transform card = cards[i - startIndex].transform;
                card.transform.localPosition = new Vector3(x, y, 0f);
                card.transform.localRotation = Quaternion.Euler(0, 0, angle - 90);
                card.GetComponent<DragableObject>().SetOriginalPosition();
            }
        }
    }
}
