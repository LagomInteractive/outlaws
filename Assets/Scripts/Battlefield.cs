using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Battlefield : MonoBehaviour {

    public CosmicAPI api;
    public Transform playerCards, opponentCards;

    public List<WorldCard> battlefield = new List<WorldCard>();

    public void UpdateBoard() {
        foreach (Player player in api.GetGame().players) {
            DrawBoard(player == api.GetPlayer() ? playerCards : opponentCards, player);
        }
    }

    WorldCard GetMinion(string id) {
        foreach (WorldCard wc in battlefield) {
            if (wc.GetMinionId() == id) return wc;
        }
        return null;
    }

    void DrawBoard(Transform target, Player player) {
        Clear(target);
        float cardWidth = 2f;
        float totalWidth = player.minions.Length * cardWidth;
        float start = -(totalWidth / 2);

        for (int i = 0; i < player.minions.Length; i++) {
            Minion minion = player.minions[i];
            GameObject card = api.InstantiateMinionCard(minion.id, target);
            battlefield.Add(card.GetComponent<WorldCard>());
            card.transform.position = card.transform.position + new Vector3(start + (i * cardWidth), 0f, 0f);
        }
    }


    void Clear(Transform transform) {
        while (transform.childCount > 0) {
            DestroyImmediate(transform.GetChild(0).gameObject);
        }

        while (battlefield.Count > 0) {
            battlefield.RemoveAt(0);
        }
    }

    void Start() {
        api.OnUpdate += () => {
            UpdateBoard();
        };
    }

    void Update() {

    }
}
