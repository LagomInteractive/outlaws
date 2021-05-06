using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class Tests : MonoBehaviour {

    public CosmicAPI api;
    public GameObject cardPrefab;
    public GameObject canvas;

    public InputField usernameInput, passwordInput;
    public Button loginButton;
    public Text wrongPasswordWarning, pingText;

    public Transform cardHand;
    public Text stats;

    public InputField commandInput;
    public Button commandPlayCard;

    List<GameObject> handCards = new List<GameObject>();

    void Start() {



        api.OnLogin += () => {
            GameObject.Find("LoggedInStatus").GetComponent<Text>().text = "Logged in as " + api.GetProfile().username;
            wrongPasswordWarning.gameObject.SetActive(false);
            usernameInput.text = api.GetProfile().username;
            passwordInput.text = "";
        };

        api.OnConnected += () => {
            Task.Run(async () => {
                for (; ; ) {
                    await Task.Delay(400);
                    api.Ping();
                }

            });
        };

        api.OnDisconnected += () => {
            GameObject.Find("LoggedInStatus").GetComponent<Text>().text = "Disconnected, trying to relogin";
        };

        api.OnPing += (int ping) => {
            pingText.text = "Ping: " + ping + "ms";
        };

        loginButton.onClick.AddListener(() => {
            api.Login(usernameInput.text, passwordInput.text);
        });

        api.OnLoginFail += () => {
            wrongPasswordWarning.gameObject.SetActive(true);
        };

        api.OnGameStart += () => {
            Debug.Log("New game started!");
        };

        api.OnOpponentCard += () => {
            Debug.Log("Opponent drew a card");
        };

        api.OnCard += (cardId) => {
            /*Card card = api.GetCard(cardId);
            Debug.Log("Got card: id " + cardId);
            GameObject cardObject = api.InstantiateCard(cardId, cardHand);
            handCards.Add(cardObject);*/
            ArrangeHand();
        };

        api.OnTurn += (attackingPlayer) => {
            string attackingPlayerName = "Opponent is";
            if (attackingPlayer == api.GetPlayer().id) attackingPlayerName = "You are";
            Debug.Log("New round (" + api.GetGame().round + ") starting! " + attackingPlayerName + " attacking!");

        };

        api.OnUpdate += () => {
            Game game = api.GetGame();
            Player me = api.GetPlayer();
            stats.text =
            "Mana: " + me.manaLeft + "/" + me.totalMana + "\n" +
            "Round: " + game.round + "\n" +
            "Attacking: " + (me.turn ? "You" : "Opponent") + "\n" +
            "Opponent cards: " + api.GetOpponent().cards.Length;
        };


        commandPlayCard.onClick.AddListener(() => {
            int cardIndex = int.Parse(commandInput.text);
            api.PlayMinion(cardIndex);

        });

        api.OnMinionSpawned += (id) => {
            api.InstantiateMinionCard(id);
        };

        api.OnFriendlyCardUsed += (index) => {
            Destroy(handCards[index]);
            handCards.RemoveAt(index);
            ArrangeHand();
        };

    }


    // Removes spaces between cards
    public void ArrangeHand() {

        foreach (GameObject card in handCards.ToList()) {
            Destroy(card);
            handCards.Remove(card);
        }

        foreach (int card in api.GetPlayer().cards) {
            handCards.Add(api.InstantiateCard(card, cardHand));
        }

        for (int i = 0; i < handCards.Count; i++) {
            Transform card = handCards[i].transform;
            card.position = new Vector3(i * 3f, 0, 0) + card.parent.transform.position;
        }
    }
}
