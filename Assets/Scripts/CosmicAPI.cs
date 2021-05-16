using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// Websockets to connect with the server
using NativeWebSocket;
// JSON protocol to parse and pack packets from and to the server.
using Newtonsoft.Json;

/// <summary>
/// Can be a Player or active Unit
/// </summary>
public class Character {
    public string id;
    public int hp, maxHp;
    public bool isAttacking, hasAttacked, hasBeenAttacked;
}

/// <summary>
/// An active Unit
/// </summary>
[Serializable]
public class Minion : Character {
    public int origin, spawnRound, damage;
    public bool battlecryActive, hasEverAttacked;
    public bool riposte, deathrattle;
    public string owner; // Player ID

    /// <summary>
    /// Get the owner of this Unit
    /// </summary>
    Player GetOwner(CosmicAPI api) {
        return (Player)api.GetCharacter(owner);
    }

    /// <summary>
    /// If the Unit can be sacrificed
    /// </summary>
    public bool canSacrifice(CosmicAPI api) {
        if (api.GetGame().round != spawnRound && api.IsElemental(api.GetCard(origin).element) && !hasAttacked) {
            return true;
        }
        return false;
    }

    /// <summary>
    /// If the Unit can attack
    /// </summary>
    public bool canAttack(CosmicAPI api) {
        Player ownerPlayer = (Player)api.GetCharacter(owner);
        if (!hasAttacked && ownerPlayer.turn)
            if (api.GetGame().round != spawnRound || api.GetCard(origin).element == Element.Rush) return true;
        return false;
    }

    /// <summary>
    /// If the unit can be attacked, takes into account enemey Taunt status.
    /// </summary>
    public bool canBeAttacked(CosmicAPI api) {
        if (api.GetCard(origin).element != Element.Taunt) {
            Player opponent = api.GetOpponent();
            foreach (Minion minion in opponent.minions) {
                if (api.GetCard(minion.origin).element == Element.Taunt) return false;
            }
        }
        return true;
    }
}

/// <summary>
/// An indexed Card pack and it's content
/// </summary>
[Serializable]
public class Pack {
    public string name, id;
    public int[] cards;
}

/// <summary>
/// A player in an active game
/// </summary>
[Serializable]
public class Player : Character {
    public string name;
    public bool isBot, turn;
    public int level, manaLeft, totalMana;
    public int[] cards;
    public int[] deck; // Cards in their deckm. This is only a list of nulls, can only be used to get amount of cards left (not cheat)
    public int passive;
    public Outlaw outlaw;
    public Minion[] minions;
    public Profile profile; // Their account
    public Buff buff;

    public bool canBeAttacked(CosmicAPI api) {
        foreach (Minion minion in minions) {
            if (api.GetCard(minion.origin).element == Element.Taunt) return false;
        }
        return true;
    }

    public bool canAttack(CosmicAPI api) {
        return hasAttacked;
    }
}

/// <summary>
/// Search options for matchmaking
/// gameType is Pvp or Campaign
/// Deck is the ID of the choosen deck
/// </summary>
[Serializable]
public class SearchOptions {
    public string gameType, deck, outlaw;
}

[Serializable]
public enum Outlaw {
    Necromancer, Mercenary
}

/// <summary>
/// Player Buff
/// </summary>
[Serializable]
public class Buff {
    public int sacrifices = 0;
    public Element element = Element.Nova;
}

/// <summary>
/// Incoming response to redeeming a store code
/// </summary>
[Serializable]
public class RedeemResponse {
    public bool success;
    public string message;
}

/// <summary>
/// One tip from the list of Tips
/// </summary>
[Serializable]
public class Tip {
    public string title, category, body, content, number;
}

/// <summary>
/// A card / Unit origin
/// </summary>
[Serializable]
public class Card {
    public int id, mana, damage, hp;
    public Rarity rarity;

    // The loaded image from Resources/card-images
    public Sprite image = null;
    public string name, description;
    public CardType type;
    public Element element;
}

public enum CardType {
    Minion, TargetSpell, AOESpell
}

public enum Element {
    Lunar, Solar, Zenith, Nova, Rush, Taunt, Neutral
}

public enum Rarity {
    Common,
    Uncommon,
    Rare,
    Epic,
    Celestial,
    Developer
}


[Serializable]
public class Deck {
    public string title, owner, owner_username, id;
    // The content of the deck
    public Dictionary<int, int> cards;

    // Get the total amount of cards in this deck (0-30)
    public int GetSize() {
        int size = 0;
        foreach (KeyValuePair<int, int> card in this.cards) {
            size += card.Value;
        }
        return size;
    }
}

[Serializable]
public class Profile {
    public string id, username;
    public int level, xp;
    public Dictionary<int, int> cards;
    public Dictionary<string, int> packs;
    public Deck[] decks;
    public bool admin, hasLoggedInViaGameClient;
    public Record record;

    /// <summary>
    /// Get amount of a certain card in the users inventory
    /// </summary>
    public int GetInventoryAmount(int cardId) {
        if (!cards.ContainsKey(cardId)) return 0;
        else return cards[cardId];
    }

    /// <summary>
    /// Get amount of packs in total the user has in their inventory
    /// </summary>
    public int GetPacksAmount() {
        int total = 0;
        foreach (KeyValuePair<string, int> pack in packs) {
            total += pack.Value;
        }
        return total;
    }
}

/// <summary>
/// Win / Loss record for Profiles
/// </summary>
[Serializable]
public class Record {
    public int wins, losses;
}

/// <summary>
/// Every package sent and recived to the server is packaged in this way.
/// </summary>
[Serializable]
public class SocketPackage {
    public string identifier, packet, token;
}

[Serializable]
public class Game {
    public string id;
    public long gameStarted, roundStarted;
    public float roundTimeLeft;
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

/// <summary>
/// All events that happen in a games is processed in a GameEvent
/// Some events are run in order with a certain delay between each event.
/// </summary>
[Serializable]
public class GameEvent {
    public string identifier;
    public Dictionary<string, string> values;
}

[Serializable]
public class CosmicAPI : MonoBehaviour {

    // API Version is checked with the server and will prompt the user to update
    // the client if they are running an outdated version. This is critical because
    // the old versions of the game would not work at all with the current server version.
    public const string API_VERSION = "3.1";

    // The prefab for every card and Unit
    public GameObject cardPrefab;

    bool runningEvents = false;

    List<GameEvent> events = new List<GameEvent>();

    public Dictionary<string, Color> elementColors = new Dictionary<string, Color>() {
        {"lunar", new Color32(0, 255, 249, 255)},
        {"solar", new Color32(255, 209, 0, 255)},
        {"zenith", new Color32(50, 255, 0, 255)},
        {"nova", new Color32(255, 28, 0, 255)},
        {"neutral", new Color32(230, 230, 230, 255)},
        {"rush", new Color32(230, 230, 230, 255)},
        {"taunt", new Color32(230, 230, 230, 255)}
    };

    // Socket connection wit the server
    WebSocket ws;

    // On everything loaded (Logged in & Cards loaded)
    public Action OnEverythingLoaded { get; set; }

    // Everytime the profile of the user updates (Deck edits, XP gain, Card pack gain)
    public Action OnProfileUpdate { get; set; }

    // On connection with the cosmic game server
    public Action OnConnected { get; set; }
    // When the client loses connection to the server socket
    public Action OnDisconnected { get; set; }
    // On User info
    public Action OnLogin { get; set; }
    /// <summary>
    /// When a login attempt with password fails, 
    /// string: reason for fail
    /// </summary>
    public Action<string> OnLoginFail { get; set; }
    // On every game update from the server, not specifik
    public Action OnUpdate { get; set; }

    /// <summary>
    /// When the user logs in with a token that doesnt exists or token is null
    /// The user should be redirect to the login system.
    /// </summary>
    public Action OnNoToken { get; set; }

    /// <summary>
    /// Every time an Game event has run
    /// </summary>
    public Action OnEventFinished { get; set; }

    /// <summary>
    /// When all Game events have finished running
    /// </summary>
    public Action OnEventsDone { get; set; }

    /// <summary>
    /// When an event starts to run
    /// </summary>
    public Action OnEventsStarted { get; set; }


    // (int id) When one of the players cards from the hand is used (removed)
    public Action<int> OnFriendlyCardUsed { get; set; }
    public Action<int> OnCardUsed { get; set; }
    // When a new game starts (from main menu)
    public Action OnGameStart { get; set; }
    // UUID Minion
    public Action<string> OnMinionSpawned { get; set; }
    // When a minion is sacrificed, minion ID
    public Action<string> OnMinionSacrificed { get; set; }

    // When the opponent uses any card (CardID)
    public Action<int> OnOpponentUsedCard { get; set; }
    // UUID Minion
    public Action<string> OnMinionDeath { get; set; }

    // When a character gets healed, string id, int amount
    public Action<string, int> OnHeal { get; set; }
    public Action<string, int> OnMinionHeal { get; set; }

    /// <summary>
    /// When the user openes a pack
    /// int[] card ids that was opened
    /// </summary>
    public Action<int[]> OnPackOpened { get; set; }

    /// <summary>
    /// When the user gains XP (always at the end of a game)
    /// string JSON as XPUpdatePackage
    /// </summary>
    public Action<string> OnXPUpdate { get; set; }

    /// <summary>
    /// When tips are downloaded
    /// </summary>
    public Action<Tip[]> OnTips { get; set; }

    // UUID form, UUID to
    public Action<string, string> OnAttack { get; set; }

    // When the client draws a card from the deck, int is the card ID
    public Action<int> OnCard { get; set; }
    // When the opponent draws a card (card is secret)
    public Action OnOpponentCard { get; set; }
    // UUID Attacking Player
    public Action<string> OnTurn { get; set; }
    // UUID Winning player
    public Action<string> OnGameEnd { get; set; }
    // UUID to, int amount
    public Action<string, int> OnDamage { get; set; }

    /// <summary>
    /// When a minion takes damage
    /// string id, int amount of damage
    /// </summary>
    public Action<string, int> OnMinionDamage { get; set; }

    /// <summary>
    /// When the user redeems a code
    /// bool success, string message
    /// </summary>
    public Action<bool, string> OnCodeRedeem { get; set; }

    /// <summary>
    /// When the game client is running an older version, prevent the player from playing.
    /// </summary>
    public Action<string, string> OnClientOutdated { get; set; }


    // Diagnostics, ping the delay in ms for round trip time to the server
    public Action<int> OnPing { get; set; }

    // List of all cards in the game
    Card[] cards;

    // List of all card packs in the game
    Pack[] packs;

    // List of tips to display in main menu
    Tip[] tips;

    // Client cosmic account
    Profile me;

    // The active game (if == null the game is not active)
    Game game;

    // The last calculated ping in ms
    int ping;
    // If the cards are loaded
    bool cardsLoaded = false;

    // If the user is connected and logged in
    bool loggedIn = false;

    // The user login token, stored in PlayerPrefs.String("token")
    string token;
    // Used to track ping delay
    System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();

    // Currently searching in matchmaking
    bool searchingGame = false;

    /// <summary>
    /// Search for a game, any mode
    /// </summary>
    public void SearchGame(SearchOptions search) {
        if (searchingGame) return;
        searchingGame = true;
        Send("search_game", JsonConvert.SerializeObject(search));
    }

    public void CancelSearch() {
        Send("cancel_search");
        searchingGame = false;
    }

    public bool IsSearchingGame() {
        return searchingGame;
    }

    // Get the client player in an active game
    public Player GetPlayer() {
        if (game == null) return null;
        foreach (Player player in game.players) {
            if (player.id == me.id) return player;
        }
        return null;
    }

    // Get the current opponent in the active game
    public Player GetOpponent() {
        if (game == null) return null;
        foreach (Player player in game.players) {
            if (player.id != me.id) return player;
        }
        return null;
    }

    // Get my user profile
    public Profile GetProfile() {
        return me;
    }

    public Tip[] GetTips() {
        return tips;
    }

    /// <summary>
    /// Check if an element is Elemental
    /// This
    /// </summary>
    public bool IsElemental(Element element) {
        switch (element) {
            case Element.Lunar:
                return true;
            case Element.Nova:
                return true;
            case Element.Zenith:
                return true;
            case Element.Solar:
                return true;
        }
        return false;
    }

    public bool IsLoggedIn() {
        return loggedIn;
    }

    public void NewDeck() {
        Send("new_deck");
    }

    public void RenameDeck(string deck, string name) {
        Send("rename_deck", JsonConvert.SerializeObject(new Dictionary<string, string>() {
            {"deck", deck},
            {"name", name}
        }));
    }

    public void ModifyCardInDeck(string deck, int card, bool add) {
        Send("modify_card_in_deck", JsonConvert.SerializeObject(new Dictionary<string, string>() {
            {"deck", deck},
            {"card", card.ToString()},
            {"add", add.ToString()}
        }));
    }

    public void DeleteDeck(string id) {
        Send("delete_deck", id);
    }

    public void Logout() {
        PlayerPrefs.DeleteKey("token");
        token = null;
        me = null;
        loggedIn = false;
        OnNoToken?.Invoke();
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

    public void RedeemCode(string code) {
        Send("redeem_code", code);
        Debug.Log("Redeem code: " + code);
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
    /// Play a minion card from the hand
    /// </summary>
    public void PlayMinion(int id) {
        Debug.Log("ID: " + id);
        Send("play_minion", id.ToString());
    }

    /// <summary>
    /// Play a spell card, only if the card is a targeted spell provide a target player or minion
    /// </summary>
    /// <param name="id"> ID of the card to play</param>
    /// <param name="target">Spell target (Optional, only if the spell is a TargetSpell)</param>
    public void PlaySpell(int id, string target = null) {
        Send("play_spell", JsonConvert.SerializeObject(new Dictionary<string, string>() { { "id", id.ToString() }, { "target", target } }));
    }

    /// <summary>
    /// Sacrifice a minion
    /// </summary>
    public void Sacrifice(string id) {
        Send("sacrifice", id);
    }

    /// <summary>
    /// End the turn early
    /// </summary>
    public void EndTurn() {
        if (runningEvents) return;
        Send("end_turn");
    }

    public bool IsRunningEvents() {
        return runningEvents;
    }

    // Try to reconnect if it loses conection to the server
    IEnumerator Reconnect() {
        yield return new WaitForSeconds(1);
        ws.Connect();
    }

    /// <summary>
    /// Send a ping to the server. Listen to OnPing to catch it.
    /// </summary>
    public void Ping() {
        stopwatch.Reset();
        stopwatch.Start();
        Send("ping");
    }

    void OnApplicationQuit() {
        ws.Close();
    }

    void LoadPacks(string json) {
        packs = JsonConvert.DeserializeObject<Pack[]>(json);
    }

    public Pack[] GetPacks() {
        return packs;
    }

    async void Start() {

        token = PlayerPrefs.GetString("token");

        // Create socket and input the game server URL
        ws = new WebSocket("wss://api.cosmic.ygstr.com");

        OnDamage += (id, amount) => {
            Character target = GetCharacter(id);
            if (target is Minion || target == null) OnMinionDamage?.Invoke(id, amount);
        };

        OnEventFinished += () => {
            if (!runningEvents) OnEventsDone?.Invoke();
        };

        OnHeal += (id, amount) => {
            Character target = GetCharacter(id);
            if (target is Minion || target == null) OnMinionHeal?.Invoke(id, amount);
        };

        ws.OnOpen += () => {
            if (token == null) OnNoToken?.Invoke();
            else Login();
        };

        // Client gets a message from the server
        ws.OnMessage += (bytes) => {

            string message = System.Text.Encoding.UTF8.GetString(bytes);
            SocketPackage package = JsonUtility.FromJson<SocketPackage>(message);

            switch (package.identifier) {
                case "xp_update":
                    OnXPUpdate?.Invoke(package.packet);
                    break;
                case "tips":
                    // Tips download
                    tips = JsonConvert.DeserializeObject<Tip[]>(package.packet);
                    OnTips?.Invoke(tips);
                    break;
                case "pack_opened":
                    OnPackOpened?.Invoke(JsonConvert.DeserializeObject<int[]>(package.packet));
                    break;
                case "code_redeemed":
                    RedeemResponse response = JsonConvert.DeserializeObject<RedeemResponse>(package.packet);
                    OnCodeRedeem?.Invoke(response.success, response.message);
                    break;
                case "user_not_found":
                    Logout();
                    break;
                case "version":
                    if (package.packet != API_VERSION) {
                        OnClientOutdated?.Invoke(package.packet, API_VERSION);
                    }
                    break;
                case "ping":
                    // Diagnostic tool
                    stopwatch.Stop();
                    OnPing?.Invoke((int)stopwatch.ElapsedMilliseconds);
                    break;
                case "packs":
                    LoadPacks(package.packet);
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
                    Debug.Log(package.packet);
                    OnLoginFail?.Invoke(package.packet);
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
            // Without this nullcheck, it causes an error every time you quit the application.
            game = null;
            if (this != null) StartCoroutine(Reconnect());
        };

        // Connect to the server
        await ws.Connect();
    }

    public void OpenPack(string packId) {
        Send("open_pack", packId);
    }

    // Game update from the server
    void GameUpdate(string json) {
        if (me == null) return;

        Game update = JsonConvert.DeserializeObject<Game>(json);
        game = update;

        foreach (GameEvent e in game.events) {
            switch (e.identifier) {
                case "game_start":
                    searchingGame = false;
                    OnGameStart?.Invoke();
                    break;
                case "next_turn":
                    OnTurn?.Invoke(e.values["attacking_player"]);
                    break;
                case "minion_spawned":
                    OnMinionSpawned?.Invoke(e.values["id"]);
                    break;
                case "card_used":
                    OnCardUsed?.Invoke(int.Parse(e.values["card"]));
                    if (e.values["player"] == me.id) {
                        OnFriendlyCardUsed?.Invoke(int.Parse(e.values["card"]));
                    } else {
                        events.Add(e);
                    }
                    break;
                case "game_over":
                    game = null;
                    events.Clear();
                    OnGameEnd?.Invoke(e.values["winner"]);
                    return;
                case "attack":
                    OnAttack?.Invoke(e.values["from"], e.values["to"]);
                    break;
                case "minion_sacrificed":
                    OnMinionSacrificed?.Invoke(e.values["id"]);
                    break;
                default:
                    // Events that should be run with a delay, in order
                    events.Add(e);
                    break;
            }
        }

        // Start the event running callback loop
        RunEvents();

        // Clear events for next update
        game.events = new GameEvent[0];

        OnUpdate?.Invoke();
    }

    void RunEvents() {
        if (runningEvents) return;
        if (events.Count > 0) {
            StartCoroutine(RunEvent(events[0]));
        }
        OnEventsDone?.Invoke();
    }

    // Runs all events (Usually just 1 event in total)
    // If there are more than 1 event, a delay is placed between each
    IEnumerator RunEvent(GameEvent e) {
        runningEvents = true;
        OnEventsStarted?.Invoke();

        //Debug.Log("Handling event " + gameEvent.identifier);
        switch (e.identifier) {
            case "card_used":
                OnCardUsed?.Invoke(int.Parse(e.values["card"]));
                if (e.values["player"] != me.id) {
                    OnOpponentUsedCard?.Invoke(int.Parse(e.values["card"]));
                    yield return new WaitForSeconds(1.8f);
                }
                break;
            case "minion_death":
                OnMinionDeath?.Invoke(e.values["minion"]);
                yield return new WaitForSeconds(.3f);
                break;
            case "damage":
                OnDamage?.Invoke(e.values["id"], int.Parse(e.values["damage"]));
                yield return new WaitForSeconds(.2f);
                break;
            case "player_deal_card":
                yield return OnPlayerDealCard(e.values);
                break;
            case "heal":
                OnHeal?.Invoke(e.values["id"], int.Parse(e.values["hp"]));
                yield return new WaitForSeconds(.2f);
                break;
        }
        yield return new WaitForSeconds(.1f);
        runningEvents = false;
        events.Remove(e);
        RunEvents();
    }

    WaitForSeconds OnPlayerDealCard(Dictionary<string, string> info) {
        if (info["player"] == me.id) {
            // Client was dealt a card
            OnCard?.Invoke(Int32.Parse(info["card"]));
            return new WaitForSeconds(1.4f);
        } else {
            // Opponent was dealt a card
            OnOpponentCard?.Invoke();
            return new WaitForSeconds(0);
        }
    }

    public Card[] GetAllCards() {
        return cards;
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


    /// <summary>
    /// Get a database of all cards (not in a specific game)
    /// </summary>
    /// <returns>Array of all cards</returns>
    public Card[] GetCards() {
        return cards;
    }

    /// <summary>
    /// Send information to the server (only identifier)
    /// </summary>
    /// <param name="identifier"></param>
    void Send(string identifier) {
        Send(identifier, "");
    }


    /// <summary>
    /// Instantiate a card the hand or to display in the inventory
    /// </summary>
    /// <param name="id">Card id</param>
    /// <param name="parent">Instantiate parent</param>
    /// <param name="handCard"></param>
    /// <returns>The GameObject that was instantiated</returns>
    public GameObject InstantiateCard(int id, Transform parent = null, bool handCard = false) {
        Card card = GetCard(id);
        GameObject worldCard = parent == null ? Instantiate(cardPrefab) : Instantiate(cardPrefab, parent);

        if (handCard) {
            BoxCollider[] colliders = worldCard.GetComponents<BoxCollider>();
            Destroy(colliders[0]);
            colliders[1].enabled = true;

            if (card.type == CardType.TargetSpell) {
                worldCard.layer = 7;
            }
        }

        worldCard.GetComponent<WorldCard>().Setup(id, this);
        worldCard.name = "Card - " + card.name;
        return worldCard;
    }

    /// <summary>
    /// Instantiate a card for a spawned minion on the board
    /// </summary>
    /// <param name="id">UUID of the minion</param>
    /// <param name="parent">The parent (Optional)</param>
    /// <returns></returns>
    public GameObject InstantiateMinionCard(string id, Transform parent = null) {
        Minion minion = (Minion)GetCharacter(id);
        GameObject card = InstantiateCard(minion.origin, parent);

        card.layer = 7;
        card.tag = "Minion";

        WorldCard wc = card.GetComponent<WorldCard>();

        wc.Setup(minion.id, this);
        wc.SetDamage(minion.damage);
        wc.SetHp(minion.hp);

        if (minion.canAttack(this)) wc.SetActive(true);
        if (minion.battlecryActive) wc.SetBattlecryActive(true);
        return card;
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

    /// <summary>
    /// Perform a battecry from a minion
    /// </summary>
    /// <param name="origin">The minion performing the battlecry</param>
    /// <param name="target">The target receving the battlecry</param>
    public void Battlecry(string origin, string target) {
        Send("battlecry", JsonConvert.SerializeObject(new Dictionary<string, string>() {
            { "origin", origin },
            { "target", target}
        }));
    }

    /// <summary>
    /// Attack a player or minion with a minion
    /// </summary>
    /// <param name="attacker">The minion attacking</param>
    /// <param name="target">The target player or minion</param>
    public void Attack(string attacker, string target) {
        AttackInfo info = new AttackInfo() { attacker = attacker, target = target };
        Send("attack", JsonConvert.SerializeObject(info));
    }

    public void ConcedeGame() {
        Send("concede");
    }


    void OnUser(string packet) {
        me = JsonConvert.DeserializeObject<Profile>(packet);

        if (!loggedIn) {
            OnLogin?.Invoke();
            Debug.Log("Logged in as " + me.username);
            CallEverythingLoaded();
        }

        loggedIn = true;
        OnProfileUpdate?.Invoke();
    }

    // The login token is always sent with the Send function,
    // so it does not need to be provided here
    void Login() {
        Send("login_with_token");
    }

    // Login or sign up via the form
    public void Login(string username, string password) {
        SocketPackage package = new SocketPackage() { identifier = "login", packet = username, token = password };
        ws.SendText(JsonConvert.SerializeObject(package));
    }

    void Update() {
        // Handles incoming messages
#if !UNITY_WEBGL || UNITY_EDITOR
        ws.DispatchMessageQueue();
#endif
    }
}
