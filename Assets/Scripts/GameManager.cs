using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour {

    public CosmicAPI api;
    public HandPlacement hand;
    public Battlefield battlefield;

    public Transform manaBar;

    public Transform playerStats, opponentStats;

    public Text infoText;
    public float roundTimer;

    public Transform endTurnButton, endGameTitle;
    public Sprite winner, loser;

    public GameObject buttons;
    public Button startMatchmakingButton;
    bool matchmaking = false;

    public Transform GameClientOutOfDateWarning;

    public PlayerUIController playerUI, opponentUI;

    Game game;

    public InputField deckIdInput;


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

    void UpdateMatchmakingButton() {
        startMatchmakingButton.GetComponentInChildren<Text>().text = matchmaking ? "Searching game..." : "Start match making";
    }

    void SaveLatestCardDeckId() {
        PlayerPrefs.SetString("lastUsedDeck", deckIdInput.text);
    }

    void Start() {

        if (PlayerPrefs.HasKey("lastUsedDeck")) {
            deckIdInput.text = PlayerPrefs.GetString("lastUsedDeck");
        }

        startMatchmakingButton.onClick.AddListener(() => {
            matchmaking = !matchmaking;
            SaveLatestCardDeckId();
            if (matchmaking) api.StartMatchMaking(deckIdInput.text);
            else api.StopMatchMaking();
            UpdateMatchmakingButton();
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

            playerUI.UpdateUI(player);
            opponentUI.UpdateUI(api.GetOpponent());

            SetEndRoundButtonState(player.turn);


        };

        api.OnGameStart += () => {
            endGameTitle.gameObject.SetActive(false);
            matchmaking = false;
            UpdateMatchmakingButton();
        };

        api.OnCardUsed += (index) => {
            hand.UpdateHand();
        };

        api.OnDamage += (id, damage) => {
            //battlefield.AnimateDamage(id, damage);
            Debug.Log("On damage called!");
        };

        api.OnMinionSpawned += (id) => {
            hand.UpdateHand();
        };

        api.OnGameEnd += (winningPlayer) => {
            game = null;
            endGameTitle.gameObject.SetActive(true);
            endGameTitle.Find("Text").GetComponent<Image>().sprite = (winningPlayer == api.GetProfile().id) ? winner : loser;
        };

        api.OnClientOutdated += (server_version, client_version) => {
            GameClientOutOfDateWarning.gameObject.SetActive(true);
        };

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
            SaveLatestCardDeckId();
            api.StartTestGame(deckIdInput.text);
        }
    }
}
