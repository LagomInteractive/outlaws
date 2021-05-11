using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HandPlacement : MonoBehaviour {

    public CosmicAPI api;
    public GameManager gm;
    public LayerMask draggableCardLayerMask;
    public List<WorldCard> cards = new List<WorldCard>();
    int lastCardPlayedIndex = -1;
    public GameObject cardBack;
    public bool opponent;

    public void Start() {

        if (!opponent) {
            api.OnCard += id => {
                StartCoroutine(DealCardDelay(id, 1.8f));
            };

            api.OnFriendlyCardUsed += (id) => {
                DeleteCard(id);
            };

            api.OnUpdate += () => {
                SetActiveCards();
            };
        } else {
            api.OnOpponentCard += () => {
                DealCard();
            };

            api.OnOpponentUsedCard += id => {
                DeleteCard();
            };
        }


    }

    public void SetActiveCards() {
        if (opponent) return;
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

    WorldCard GetCard(int id) {
        foreach (WorldCard card in cards) {
            if (card.GetId() == id) return card;
        }
        return null;
    }

    void DeleteCard(int id = -1) {
        if (opponent) {
            Destroy(transform.GetChild(0).gameObject);
        } else {
            WorldCard card = GetCard(id);
            cards.Remove(card);
            DestroyImmediate(card.gameObject);
        }

        RearrangeCards();

        if (!opponent) SanityCheckHand();
    }

    bool SanityCheckHand() {
        for (int i = 0; i < transform.childCount; i++) {
            if (cards.IndexOf(transform.GetChild(0).GetComponent<WorldCard>()) == -1) {
                DestroyImmediate(transform.GetChild(i));
                return SanityCheckHand();
            }
        }
        return true;
    }

    IEnumerator DealCardDelay(int id, float delay) {
        yield return new WaitForSeconds(delay);
        DealCard(id);
    }

    public void DealCard(int id = -1) {
        if (!opponent) {
            GameObject cardObject = api.InstantiateCard(id, transform, true);
            cards.Add(cardObject.GetComponent<WorldCard>());

            DragableObject draggable = cardObject.AddComponent<DragableObject>();
            draggable.api = api;
            draggable.gm = gm;
            draggable.id = id;
            draggable.layermask = draggableCardLayerMask;
        } else {
            GameObject cardObject = Instantiate(cardBack, transform);
        }

        RearrangeCards();
    }

    public void RearrangeCards() {

        int amountOfCards = !opponent ? cards.Count : transform.childCount;
        if (amountOfCards == 0) return;
        float positionScale = 50;
        float spacing = 7;

        int startIndex = 4 - amountOfCards / 2;

        for (int i = 0; i < 8; i++) {
            int index = i - startIndex;
            if (startIndex <= i && i - startIndex > -1 && index < amountOfCards) {

                float angle = ((spacing * i) + 67.5f);
                if (amountOfCards % 2 != 0) angle += -5;
                else angle += -2;

                if (opponent) angle += 180;

                float x = Mathf.Cos(Mathf.Deg2Rad * angle) * positionScale;
                float y = Mathf.Sin(Mathf.Deg2Rad * angle) * positionScale;

                Transform card = !opponent ? cards[index].transform : transform.GetChild(index);
                card.transform.localPosition = new Vector3(x, y, 0f);
                card.transform.localRotation = Quaternion.Euler(0, 0, angle - 90);
                if (!opponent) card.GetComponent<DragableObject>().SetOriginalPosition();
            }
        }
        SetActiveCards();
    }
}
