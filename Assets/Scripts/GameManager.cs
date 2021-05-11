using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;



public class GameManager : MonoBehaviour {

    public CosmicAPI api;
    public HandPlacement hand;
    public Battlefield battlefield;

    public Transform manaBar;


    public float roundTimer;

    public Transform endTurnButton, endGameTitle;
    public Sprite winner, loser;

    public GameObject buttons;
    public Button startMatchmakingButton;
    bool matchmaking = false;

    public Transform GameClientOutOfDateWarning;

    public PlayerUIController playerUI, opponentUI;

    Game game;
    public Transform deckPickList;

    public AudioClip gameMusic;
    public AudioSource musicPlayer;

    public InputField deckIdInput;
    public MenuSystem menus;

    public Transform cardPreview;

    public MainMenu menu;
    public GameObject deckPrefab;



    SearchOptions searchOptions = new SearchOptions();

    private void Update() {

        if (game != null && api.IsLoggedIn()) {
            roundTimer -= Time.deltaTime;
            //infoText.text = $"V{CosmicAPI.API_VERSION} outlaws.ygstr.com\n---\nRound: {game.round}\nTurn: {(api.GetPlayer().turn ? "You" : "Opponent")}\nTime left: {Mathf.Round(roundTimer)}\nOpponent: {api.GetOpponent().name}\nOutlaw: {api.GetPlayer().outlaw} (You) vs. {api.GetOpponent().outlaw}";
            if (Input.GetKeyDown(KeyCode.Escape)) {
                // Show pause menu
                if (menus.IsMenusOpen()) menus.CloseMenu();
                else menus.NavigateTo("in_game_options");
            }
        }
    }

    public void ReportBug() {
        Application.OpenURL("https://github.com/LagomInteractive/cosmic-game/issues");
    }

    void SetEndRoundButtonState(bool enabled) {
        endTurnButton.GetComponent<Button>().interactable = enabled;
        endTurnButton.GetComponent<Image>().color = enabled ? Color.white : Color.grey;
        endTurnButton.Find("Text").GetComponent<Text>().color = enabled ? Color.white : Color.grey;
    }

    void UpdateMatchmakingButton() {
        startMatchmakingButton.GetComponentInChildren<Text>().text = matchmaking ? "Searching game..." : "Start match making";
    }

    public void PrepareForGame() {
        if (api.IsSearchingGame()) {
            api.CancelSearch();
            menu.StoppedSearchingGame();
        } else menus.NavigateTo("pick_gameplay");
    }

    public void PickGameType(string gameType) {
        searchOptions.gameType = gameType;
        menus.NavigateTo("pick_deck");
        while (deckPickList.childCount > 0) DestroyImmediate(deckPickList.GetChild(0).gameObject);

        foreach (Deck deck in api.GetProfile().decks) {
            AddDeckToPickList(deck);
        }
        AddDeckToPickList(new Deck { id = "BCBMo6PXLEFqO5_rxv8Fh", title = "Starter deck" });
    }

    public void PickOutlaw(string outlaw) {
        searchOptions.outlaw = outlaw;
        api.SearchGame(searchOptions);
        menus.NavigateTo("main");
        menu.StartedSearchingGame();
    }

    void AddDeckToPickList(Deck deck) {
        GameObject deckObj = Instantiate(deckPrefab, deckPickList);
        deckObj.transform.Find("name").GetComponent<Text>().text = deck.title;
        deckObj.transform.Find("size").GetComponent<Text>().text = deck.cards != null ? deck.GetSize() + "/30" : "30/30";
        deckObj.GetComponent<Button>().onClick.AddListener(() => {
            searchOptions.deck = deck.id;
            menus.NavigateTo("pick_outlaw");
        });
    }

    void SaveLatestCardDeckId() {
        PlayerPrefs.SetString("lastUsedDeck", deckIdInput.text);
    }

    void Start() {



        menus.NavigateTo("loading");
        Input.simulateMouseWithTouches = true;

        if (PlayerPrefs.HasKey("lastUsedDeck")) {
            deckIdInput.text = PlayerPrefs.GetString("lastUsedDeck");
        }

        musicPlayer.clip = gameMusic;
        musicPlayer.loop = true;
        musicPlayer.Play();


        startMatchmakingButton.onClick.AddListener(() => {
            matchmaking = !matchmaking;
            SaveLatestCardDeckId();
            if (matchmaking) api.StartMatchMaking(deckIdInput.text);
            else api.StopMatchMaking();
            UpdateMatchmakingButton();
        });

        api.OnNoToken += () => {
            menus.NavigateTo("login");
        };

        api.OnLogin += () => {
            menus.NavigateTo("main");
            menu.Setup();
        };

        api.OnUpdate += () => {
            game = api.GetGame();
            roundTimer = game.roundTimeLeft;
        };

        api.OnEventsStarted += () => {
            SetEndRoundButtonState(false);
        };

        api.OnEventsDone += () => {
            if (!api.IsRunningEvents() && api.GetPlayer().turn) {
                SetEndRoundButtonState(true);
            }
        };



        api.OnUpdate += () => {
            Player player = api.GetPlayer();

            playerUI.UpdateUI(player);
            opponentUI.UpdateUI(api.GetOpponent());

            SetEndRoundButtonState(player.turn);
        };

        api.OnGameStart += () => {
            endGameTitle.gameObject.SetActive(false);
            matchmaking = false;
            menus.CloseMenu();
            UpdateMatchmakingButton();
        };

        api.OnFriendlyCardUsed += (index) => {

        };

        api.OnMinionSpawned += (id) => {

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

    public void PreviewCard(int id) {
        while (cardPreview.childCount > 0) DestroyImmediate(cardPreview.GetChild(0).gameObject);

        GameObject card = api.InstantiateCard(id, cardPreview);
        menus.NavigateTo("card_preview");
    }


    /*public void StartTest() {
        if (!matchmaking) {
            SaveLatestCardDeckId();
            //api.PlayVsBot(deckIdInput.text);
        }
    }*/
}
