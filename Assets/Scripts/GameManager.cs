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

    public Transform endTurnButton, endGameTitle;
    public Sprite winner, loser;

    public GameObject buttons;
    public Button startMatchmakingButton;
    bool matchmaking = false;

    Game game;


    private void Update() {
        buttons.SetActive(game == null);
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

        startMatchmakingButton.onClick.AddListener(() => {
            matchmaking = !matchmaking;
            startMatchmakingButton.GetComponentInChildren<Text>().text = matchmaking ? "Searching game..." : "Start match making";
        });

        api.OnTurn += (attackingPlayer) => {
            game = api.GetGame();
            roundTimer = game.roundLength;
        };

        api.OnCard += (id) => {
            hand.UpdateHand();
        };


        api.OnUpdate += () => {
            UpdateManaBar();
            Player player = api.GetPlayer();
            UpdateStats(playerStats, player);
            UpdateStats(opponentStats, api.GetOpponent());
            SetEndRoundButtonState(player.turn);
        };

        api.OnGameStart += () => {
            endGameTitle.gameObject.SetActive(false);
        };

        api.OnCardUsed += (index) => {
            hand.UpdateHand();
        };

        api.OnMinionSpawned += (id) => {
            hand.UpdateHand();
        };

        api.OnGameEnd += (winningPlayer) => {
            game = null;
            endGameTitle.gameObject.SetActive(true);
            endGameTitle.Find("Text").GetComponent<Image>().sprite = (winningPlayer == api.GetProfile().id) ? winner : loser;
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
        if (!matchmaking) {
            api.StartTestGame();
        }
    }
}
