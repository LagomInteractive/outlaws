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

    // Current active game
    Game game;

    public Transform deckPickList;

    public AudioClip gameMusic, menuMusic;
    public AudioSource musicPlayer;

    public InputField deckIdInput;
    public MenuSystem menus;

    public Transform cardPreview;

    public MainMenu menu;
    public GameObject deckPrefab;

    public Image countdownDial;
    public Text countdownText;

    float searchingForSeconds = 0;

    public Color32 timerGreen, timerOrange, timerRed;

    SearchOptions searchOptions = new SearchOptions();

    public Transform playerSpawn, opponentSpawn;
    public GameObject Necromancer, Mercenary;

    public Transform winningBanner, losingBanner, lostConnectionBanner;

    public AudioClip winningSound, losingSound;

    // Playes particle effects and sound effects
    public EffectsManager effects;

    // Display the XP gain at the end of the game
    public XPDisplay xpDisplay;

    // JSON of the XP update, saved once received and displayed at the end of the game.
    string xpUpdate = null;

    // Only visible to mobile players
    public GameObject pauseButton;

    private void Update() {
        if (api.IsSearchingGame()) {
            searchingForSeconds += Time.deltaTime;
        }

        if (game != null && api.IsLoggedIn()) {
            roundTimer -= Time.deltaTime;

            Color32 dialColor = timerGreen;
            int timeLeft = (int)Math.Round(roundTimer);
            if (timeLeft <= 30) dialColor = timerOrange;
            if (timeLeft <= 10) dialColor = timerRed;

            countdownDial.fillAmount = roundTimer / 60f;
            countdownDial.color = dialColor;
            countdownText.text = timeLeft.ToString();

            if (Input.GetKeyDown(KeyCode.Escape)) {
                // Show pause menu
                if (menus.IsMenusOpen()) menus.CloseMenu();
                else {
                    if (api.IsInGame()) menus.NavigateTo("in_game_options");
                }
            }
        }
    }

    public float GetSearchTime() {
        return searchingForSeconds;
    }

    public void ReportBug() {
        Application.OpenURL("https://github.com/LagomInteractive/cosmic-game/issues");
    }

    void SetEndRoundButtonState(bool enabled) {
        endTurnButton.GetComponent<Button>().interactable = enabled;
        endTurnButton.GetComponent<Image>().color = enabled ? Color.white : Color.grey;
        endTurnButton.Find("Text").GetComponent<Text>().color = enabled ? Color.white : Color.grey;
    }

    public void PrepareForGame() {
        if (api.IsSearchingGame()) {
            return;
        } else menus.NavigateTo("pick_gameplay");
    }

    public void StopSearching() {
        api.CancelSearch();
        menu.StoppedSearchingGame();
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
        searchingForSeconds = 0;
        menus.NavigateTo("main");
        menu.StartedSearchingGame();
    }

    public SearchOptions GetSearchOptions() {
        return searchOptions;
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

    void ChangeMusic(AudioClip music) {
        musicPlayer.Stop();
        musicPlayer.clip = music;
        musicPlayer.loop = true;
        musicPlayer.time = 0;
        musicPlayer.Play();
    }

    void Start() {
        if (Application.platform != RuntimePlatform.Android && Application.platform != RuntimePlatform.IPhonePlayer) {
            Destroy(pauseButton);
        }

        menus.NavigateSilent("loading");
        Input.simulateMouseWithTouches = true;

        if (PlayerPrefs.HasKey("lastUsedDeck")) {
            deckIdInput.text = PlayerPrefs.GetString("lastUsedDeck");
        }

        ChangeMusic(menuMusic);

        api.OnNoToken += () => {
            menus.NavigateSilent("login");
        };

        api.OnLogin += () => {
            menus.NavigateSilent("main");

            if (!api.GetProfile().hasLoggedInViaGameClient) menus.Overlay("welcome");
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
            if (!api.IsInGame()) return;
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
            menu.StoppedSearchingGame();
            LoadCharacters();
            menus.CloseMenu();

            effects.GetEffect("GAME_START").Play(effects.audioSource);

            ChangeMusic(gameMusic);
        };

        api.OnXPUpdate += json => {
            xpUpdate = json;
        };

        api.OnGameEnd += (winningPlayer) => {
            game = null;
            bool won = (winningPlayer == api.GetProfile().id);
            menus.NavigateSilent("end_game");

            if (xpUpdate != null) {
                xpDisplay.XpUpdate(xpUpdate);
            }

            winningBanner.gameObject.SetActive(won);
            losingBanner.gameObject.SetActive(!won);
            lostConnectionBanner.gameObject.SetActive(false);

            menus.PlaySoundEffect(won ? winningSound : losingSound);
            ChangeMusic(menuMusic);
        };

        api.OnDisconnected += () => {
            if (api.IsInGame()) {
                game = null;
                menus.NavigateSilent("end_game");
                xpDisplay.ShowNoXpGain();
                lostConnectionBanner.gameObject.SetActive(true);
                winningBanner.gameObject.SetActive(false);
                losingBanner.gameObject.SetActive(false);
            }
        };

        api.OnClientOutdated += (server_version, client_version) => {
            menus.NavigateSilent("out_of_date_warning");
        };
    }


    public void LoadCharacters() {
        LoadCharacter(api.GetPlayer(), playerSpawn);
        LoadCharacter(api.GetOpponent(), opponentSpawn);
    }

    public void LoadCharacter(Player player, Transform spawn) {
        while (spawn.childCount > 0) DestroyImmediate(spawn.GetChild(0).gameObject);

        Debug.Log("Load player: " + player);
        GameObject outlaw = null;
        switch (player.outlaw) {
            case Outlaw.Mercenary:
                outlaw = Mercenary;
                break;
            case Outlaw.Necromancer:
                outlaw = Necromancer;
                break;
        }

        Instantiate(outlaw, spawn);
    }

    public void PreviewCard(int id) {
        if (!api.IsInGame()) return;
        while (cardPreview.childCount > 0) DestroyImmediate(cardPreview.GetChild(0).gameObject);

        GameObject card = api.InstantiateCard(id, cardPreview);
        menus.NavigateTo("card_preview");
    }
}
