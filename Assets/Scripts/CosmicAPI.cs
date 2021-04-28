using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Websockets to connect with the server
using NativeWebSocket;
// JSON protocol to parse and pack packets from and to the server.
using Newtonsoft.Json;
using UnityEditor;

public class Character {
    public string id;
    public int hp;
    public bool isAttacking, hasAttacked, hasBeenAttacked;
    public Buff buff;


}

[Serializable]
public class Minion : Character {
    public int origin, spawnRound;
    public bool canSacrifice;
    public string owner;
    Player GetOwner(CosmicAPI api) {
        return (Player)api.GetCharacter(owner);
    }
    public bool canAttack(CosmicAPI api) {
        if (api.GetGame().round != spawnRound && !hasAttacked) return true;
        return false;
    }
}

[Serializable]
public class Player : Character {
    public string name;
    public bool isBot, turn;
    public int level, manaLeft, totalMana;
    public int[] cards;
    public int[] deck;
    public Minion[] minions;
    public Profile profile;

    public bool canAttack(CosmicAPI api) {
        return hasAttacked;
    }
}

[Serializable]
public class Buff {
    public int sacrifices, damage, mana;
}

[Serializable]
public class Card {
    public int id, mana, damage, hp;
    public string rarity;
    public Sprite image = null;
    public string name, description;
    public CardType type;
    public Element element;
}

public enum CardType {
    Minion, TargetSpell, AOESpell
}

public enum Element {
    Lunar, Solar, Zenith, Nova, rush, taunt
}

public enum Rarity {
    Common,
    Uncommon,
    Rare,
    Epic,
    Legendary
}

[Serializable]
public class Profile {
    public string id, username;
    public int level, xp;
    public int[] cards;
    public bool admin;
    public Record record;
}
[Serializable]
public class Record {
    public int wins, losses;
}

[Serializable]
public class SocketPackage {
    public string identifier, packet, token;
}

[Serializable]
public class Game {
    public string id;
    public long gameStarted, roundStarted;
    public int roundLength, round, turn;
    public Player[] players;
    public GameEvent[] events;
    public bool OpponentIsBot() {
        foreach (Player player in this.players) {
            if (player.isBot) return true;
        }
        return false;
    }
}

[Serializable]
public class GameEvent {
    public string identifier;
    public Dictionary<string, string> values;
}

[Serializable]
public class CosmicAPI : MonoBehaviour {

    public GameObject cardPrefab;

    // Socket connection wit the server
    WebSocket ws;

    // On everything loaded (Logged in & Cards loaded)
    public Action OnEverythingLoaded { get; set; }

    // On connection with the cosmic game server
    public Action OnConnected { get; set; }
    // When the client loses connection to the server socket
    public Action OnDisconnected { get; set; }
    // On User info
    public Action OnLogin { get; set; }
    // When the login attempt fails
    public Action OnLoginFail { get; set; }
    // On every game update from the server, not specifik
    public Action OnUpdate { get; set; }
    // (int index) When one of the players cards from the hand is used (removed)
    public Action<int> OnCardUsed { get; set; }
    // When a new game starts (from main menu)
    public Action OnGameStart { get; set; }
    // UUID Minion
    public Action<string> OnMinionSpawned { get; set; }
    // UUID Minion
    public Action<string> OnMinionDeath { get; set; }
    // When the client draws a card from the deck, int is the card ID
    public Action<int> OnCard { get; set; }
    // When the opponent draws a card (card is secret)
    public Action OnOpponentCard { get; set; }
    // UUID Attacking Player
    public Action<string> OnTurn { get; set; }
    // UUID Winning player
    public Action<string> OnGameEnd { get; set; }
    // UUID from, UUID to, float amount
    public Action<string, string, float> OnDamage { get; set; }


    // Diagnostics
    public Action<int> OnPing { get; set; }

    // List of all cards in the game
    Card[] cards;

    // Client cosmic account
    Profile me;

    // The active game (if == null the game is not active)
    Game game;

    // The last calculated ping in ms
    int ping;
    // If the cards are loaded
    bool cardsLoaded;
    // If the user is connected and logged in
    bool loggedIn;

    // The user login token, stored in PlayerPrefs.String("token")
    string token;
    // Used to track ping delay
    System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();


    // The time difference between the local clock on this clien and the server clock
    // Game times are tracked in date time, since the clock on the server and client will sometimes not
    // line up on the millisecond, this gives us the diffrence and is used to calculate time left on 
    // a round for example.
    long timeDifference = 0;

    public void StartTestGame() {
        Send("start_test");
    }

    // Get client player in a game
    public Player GetPlayer() {
        foreach (Player player in game.players) {
            if (player.id == me.id) return player;
        }
        return null;
    }

    // Get my user profile
    public Profile GetProfile() {
        return me;
    }

    public Player GetOpponent() {
        foreach (Player player in game.players) {
            if (player.id == me.id) return player;
        }
        return null;
    }

    // Get any player or minion on the field (from ID)
    public Character GetCharacter(string id) {
        List<Character> characters = GetAllCharacters();
        foreach (Character character in characters) {
            if (character.id == id) return character;
        }
        return null;
    }

    /// <summary>
    /// Get a List of all characters (Players and minions on the field)
    /// </summary>
    public List<Character> GetAllCharacters() {
        List<Character> characters = new List<Character>();
        // Get all players and add them the the List
        foreach (Player player in game.players) {
            characters.Add(player);
            // Add all the players minions to the List
            foreach (Minion minion in player.minions) characters.Add(minion);

        }

        return characters;
    }

    /// <summary>
    /// If the user is currently in a game or not
    /// </summary>
    public bool IsInGame() {
        return game != null;
    }

    /// <summary>
    /// Get the current game with all of its info
    /// </summary>
    public Game GetGame() {
        return game;
    }

    /// <summary>
    /// Get information about a card
    /// </summary>
    public Card GetCard(int id) {
        foreach (Card card in cards) {
            if (card.id == id) return card;
        }
        return null;
    }

    /// <summary>
    /// Play a minion card from the hand
    /// </summary>
    public void PlayMinion(int index) {
        Send("play_minion", index.ToString());
    }

    /// <summary>
    /// Gives you all card ids (For testing)
    /// </summary>
    public int[] GetAllCardIDs() {

        int[] cardIDs = new int[cards.Length];
        for (int i = 0; i < cards.Length; i++) {
            cardIDs[i] = cards[i].id;
        }
        return cardIDs;
    }

    /// <summary>
    /// Play a spell card, only if the card is a targeted spell provide a target player or minion
    /// </summary>
    public void PlaySpell(int id, string target = null) {
        Debug.Log("Played spell: " + id + " targeted: " + target);
    }

    /// <summary>
    /// Sacrifice a minion
    /// </summary>
    public void Sacrifice(string id) {
        Debug.Log("Sacrificed minion ID: " + id);
    }

    /// <summary>
    /// End the turn early
    /// </summary>
    public void EndTurn() {
        Send("end_turn");
    }

    // Try to reconnect if it loses conection to the server
    IEnumerator Reconnect() {
        yield return new WaitForSeconds(1);
        ws.Connect();
    }

    public void Ping() {
        stopwatch.Reset();
        stopwatch.Start();
        Send("ping");
    }

    void OnApplicationQuit() {
        ws.Close();
    }

    async void Start() {

        token = PlayerPrefs.GetString("token");

        // Create socket and input the game server URL
        ws = new WebSocket("wss://api.cosmic.ygstr.com");


        ws.OnOpen += () => {
            Debug.Log("Connected to Cosmic server");
            OnConnected?.Invoke();
            Login();
        };

        ws.OnMessage += (bytes) => {

            string message = System.Text.Encoding.UTF8.GetString(bytes);
            SocketPackage package = JsonUtility.FromJson<SocketPackage>(message);

            switch (package.identifier) {
                case "ping":
                    stopwatch.Stop();
                    OnPing?.Invoke((int)stopwatch.ElapsedMilliseconds);
                    break;
                case "cards":
                    LoadCards(package.packet);
                    break;
                case "new_token":
                    token = package.packet;
                    PlayerPrefs.SetString("token", token);
                    Login();
                    break;
                case "user":
                    OnUser(package.packet);
                    break;
                case "login_fail":
                    OnLoginFail?.Invoke();
                    loggedIn = false;
                    break;
                case "game_update":
                    GameUpdate(package.packet);
                    break;
            }

        };


        ws.OnClose += (e) => {
            Debug.Log("Connection closed");
            loggedIn = false;
            OnDisconnected?.Invoke();
            StartCoroutine(Reconnect());
        };

        // Connect to the server
        await ws.Connect();
    }

    // Game update from the server
    void GameUpdate(string json) {
        Game update = JsonConvert.DeserializeObject<Game>(json);
        game = update;

        // Coroutine because the events have a spacer delay
        StartCoroutine(RunEvents(game.events));

        // Clear events for next update
        game.events = new GameEvent[0];

        OnUpdate?.Invoke();
    }

    // Runs all events (Usually just 1 event in total)
    // If there are more than 1 event, a delay is placed between each
    IEnumerator RunEvents(GameEvent[] events) {
        foreach (GameEvent gameEvent in events) {
            Debug.Log("Handling event " + gameEvent.identifier);
            switch (gameEvent.identifier) {
                case "game_start":
                    OnGameStart?.Invoke();
                    break;
                case "next_turn":
                    OnTurn(gameEvent.values["attacking_player"]);
                    break;
                case "player_deal_card":
                    OnPlayerDealCard(gameEvent.values);
                    break;
                case "minion_spawned":
                    OnMinionSpawned?.Invoke(gameEvent.values["id"]);
                    break;
                case "card_used":
                    if (gameEvent.values["player"] == me.id) OnCardUsed?.Invoke(Int32.Parse(gameEvent.values["index"]));
                    break;
            }
            yield return new WaitForSeconds(0);
        }
    }


    void OnPlayerDealCard(Dictionary<string, string> info) {
        if (info["player"] == me.id) {
            // Client was dealt a card
            OnCard(Int32.Parse(info["card"]));
        } else {
            // Opponent was dealt a card
            OnOpponentCard();
        }
    }

    void LoadCards(string cardsJson) {
        cards = JsonConvert.DeserializeObject<Card[]>(cardsJson);
        foreach (Card card in cards) {
            card.image = Resources.Load<Sprite>("card-images/" + card.id);
        }
        cardsLoaded = true;
        OnConnected?.Invoke();
        CallEverythingLoaded();
    }

    void CallEverythingLoaded() {
        if (cardsLoaded && loggedIn && ws.State == WebSocketState.Open) OnEverythingLoaded?.Invoke();
    }



    public Card[] GetCards() {
        return cards;
    }

    void Send(string identifier) {
        Send(identifier, "");
    }

    public GameObject InstantiateCard(int id) {
        return this.InstantiateCard(id, null);
    }


    public GameObject InstantiateCard(int id, Transform parent) {
        Card card = GetCard(id);
        GameObject worldCard = parent == null ? Instantiate(cardPrefab) : Instantiate(cardPrefab, parent);

        worldCard.GetComponent<WorldCard>().Setup(id, this);
        worldCard.name = "Card - " + card.name;
        return worldCard;
    }

    public void InstantiateMinionCard(string id) {
        Minion minion = (Minion)GetCharacter(id);
        Debug.Log("Got the character " + minion.owner);
    }

    void Send(string identifier, string data) {
        SocketPackage package = new SocketPackage {
            identifier = identifier,
            packet = data,
            token = token
        };
        string json = JsonUtility.ToJson(package);
        ws.SendText(json);
    }



    void OnUser(string packet) {
        me = JsonConvert.DeserializeObject<Profile>(packet);
        loggedIn = true;
        Debug.Log("Logged in as " + me.username);
        OnLogin?.Invoke();
        CallEverythingLoaded();
    }

    void Login() {
        Send("login_with_token");
    }

    public void Login(string username, string password) {
        SocketPackage package = new SocketPackage() { identifier = "login", packet = username, token = password };
        ws.SendText(JsonConvert.SerializeObject(package));
    }

    void Update() {
        // Makes sure incoming messages are received
#if !UNITY_WEBGL || UNITY_EDITOR
        ws.DispatchMessageQueue();
#endif
    }
}
