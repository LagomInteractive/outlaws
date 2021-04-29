using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour {

    public CosmicAPI api;
    public HandPlacement hand;

    public Transform manaBar;

    public Transform playerStats, opponentStats;

    public Text infoText;
    public float roundTimer;

    public Transform endTurnButton, winnerTitle;

    Game game;

    private void Update() {
        if (game != null) {
            roundTimer -= Time.deltaTime;
            infoText.text = $"Round: {game.round}\nTurn: {(api.GetPlayer().turn ? "You" : "Opponent")}\nTime left: {Mathf.Round(roundTimer)}";
        }
    }

    void SetEndRoundButtonState(bool enabled) {
        endTurnButton.GetComponent<Button>().interactable = enabled;
        endTurnButton.GetComponent<Image>().color = enabled ? Color.white : Color.grey;
        endTurnButton.Find("Text").GetComponent<Text>().color = enabled ? Color.white : Color.grey;
    }

    void Start() {

        api.OnTurn += (attackingPlayer) => {
            game = api.GetGame();
            roundTimer = game.roundLength;
        };

        api.OnCard += (id) => {
            hand.UpdateHand();
        };

        api.OnGameEnd += (winningPlayer) => {
            game = null;
        };

        api.OnUpdate += () => {
            UpdateManaBar();
            Player player = api.GetPlayer();
            UpdateStats(playerStats, player);
            UpdateStats(opponentStats, api.GetOpponent());
            SetEndRoundButtonState(player.turn);
        };

        api.OnCardUsed += (index) => {
            hand.UpdateHand();
        };

        api.OnMinionSpawned += (id) => {
            hand.UpdateHand();
        };

        api.OnGameEnd += (winner) => {

            winnerTitle.gameObject.SetActive(true);

        };
    }

    public void UpdateStats(Transform target, Player player) {
        target.Find("hp").GetComponent<Text>().text = player.hp.ToString();
    }

    public void UpdateManaBar() {
        int manaLeft = api.GetPlayer().manaLeft;
        if (!api.GetPlayer().turn) manaLeft = 0;
        for (int i = 0; i < manaBar.childCount; i++) {
            manaBar.GetChild(i).GetComponent<Image>().color = manaLeft >= i + 1 ? Color.white : Color.black;
        }
    }

    public void StartTest() {
        api.StartTestGame();
        Destroy(GameObject.Find("StartTestButton"));
    }
}
