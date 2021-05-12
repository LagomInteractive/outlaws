using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DeckBuilder : MonoBehaviour {

    public CosmicAPI api;
    public Transform cardsScroll;
    public GameObject cardContainerPrefab;
    public List<DeckBuilderCard> cardsList;

    public InputField searchInput;

    public Transform manaSort;
    public GameObject manaSortCrystal;
    public Sprite mana, noMana;
    public Text cardAmount;

    bool loadedCards = false;

    int manaSortOption = -1;

    public Transform sideBarContent;
    public InputField sideBarHeaderText;
    public Text sideBarAmount;
    public Button sideBarBackButton;
    /*public Image sideBarBackButtonIcon;*/
    public GameObject emptyDeckTip;

    public GameObject deckInsertPrefab, cardInsertPrefab;

    public Toggle showLockedCards;

    public Sprite backIcon, newIcon;

    Deck activeDeck = null;

    public void ShowDecks() {
        ClearSidebar();
        activeDeck = null;
        Deck[] decks = api.GetProfile().decks;
        foreach (Deck deck in decks) {
            Transform sidebarDeck = Instantiate(deckInsertPrefab, sideBarContent).transform;

            sidebarDeck.Find("Delete").GetComponent<Button>().onClick.AddListener(() => {
                api.DeleteDeck(deck.id);
            });
            sidebarDeck.Find("name").GetComponent<Text>().text = deck.title;
            sidebarDeck.Find("number").GetComponent<Text>().text = deck.GetSize() + "/30";
            sidebarDeck.GetComponent<Button>().onClick.AddListener(() => {
                LoadDeck(deck);
            });
        }

        sideBarHeaderText.interactable = false;
        sideBarHeaderText.text = api.GetProfile().username + "'s decks";
        sideBarAmount.text = "";

        sideBarBackButton.GetComponentInChildren<Text>().text = "NEW DECK";
        sideBarBackButton.transform.Find("Icon").GetComponent<Image>().sprite = newIcon;
    }

    public void LoadDeck(Deck deck) {

        ClearSidebar();

        sideBarHeaderText.interactable = true;
        sideBarHeaderText.text = deck.title;

        foreach (KeyValuePair<int, int> entry in deck.cards) {
            if (entry.Value > 0) {
                Card card = api.GetCard(entry.Key);
                GameObject cardInsert = Instantiate(cardInsertPrefab, sideBarContent);
                cardInsert.transform.Find("name").GetComponent<Text>().text = card.name;

                Text element = cardInsert.transform.Find("element").GetComponent<Text>();
                element.text = card.element.ToString();
                element.color = api.elementColors[card.element.ToString().ToLower()];

                Transform count = cardInsert.transform.Find("count");

                count.Find("Number").GetComponent<Text>().text = entry.Value.ToString();
                count.Find("Plus").GetComponent<Button>().onClick.AddListener(() => {
                    AddCardToDeck(card.id);
                });
                count.Find("Minus").GetComponent<Button>().onClick.AddListener(() => {
                    RemoveCardFromDeck(card.id);
                });
            }
        }

        sideBarBackButton.GetComponentInChildren<Text>().text = "VIEW DECKS";
        sideBarBackButton.transform.Find("Icon").GetComponent<Image>().sprite = backIcon;
        sideBarAmount.text = deck.GetSize() + "/30";

        activeDeck = deck;
    }



    public void AddCardToDeck(int id) {
        if (activeDeck == null) return;

        if (api.GetProfile().cards.ContainsKey(id) && api.GetProfile().cards[id] > 0) {
            if (activeDeck.cards.ContainsKey(id)) {
                if (activeDeck.cards[id] < 2) activeDeck.cards[id]++;
            } else {
                activeDeck.cards.Add(id, 1);
            }
            LoadDeck(activeDeck);
            api.ModifyCardInDeck(activeDeck.id, id, true);
        }
    }

    public void RemoveCardFromDeck(int id) {
        if (activeDeck == null) return;
        if (activeDeck.cards.ContainsKey(id) && activeDeck.cards[id] > 0) {
            activeDeck.cards[id]--;
            if (activeDeck.cards[id] < 1) {
                activeDeck.cards.Remove(id);
            }
            LoadDeck(activeDeck);
            api.ModifyCardInDeck(activeDeck.id, id, false);
        }
    }

    public void ClearSidebar() {
        while (sideBarContent.childCount > 0) DestroyImmediate(sideBarContent.GetChild(0).gameObject);
    }

    void OnEnable() {
        if (!api.IsLoggedIn()) return;
        if (!loadedCards) {
            showLockedCards.isOn = false;
            ShowDecks();
        }
        LoadCards();
    }

    void Start() {

        api.OnProfileUpdate += () => {
            if (activeDeck == null) ShowDecks();
        };

        searchInput.onValueChanged.AddListener(val => {
            LoadCards();
        });

        sideBarBackButton.onClick.AddListener(() => {
            if (activeDeck != null) ShowDecks();
            else api.NewDeck();
        });

        showLockedCards.onValueChanged.AddListener(val => {
            LoadCards();
        });

        sideBarHeaderText.onValueChanged.AddListener(val => {
            if (activeDeck != null) {

                if (val.Length > 0) {
                    api.RenameDeck(activeDeck.id, val);
                    activeDeck.title = val;
                }
            }
        });
    }

    void SetManaSort(int manaNumber) {
        if (manaSortOption == manaNumber) manaSortOption = -1;
        else manaSortOption = manaNumber;

        for (int i = 0; i < 11; i++) {
            GameObject crystal = manaSort.GetChild(i).gameObject;
            bool active = false;
            if (i == manaSortOption) active = true;
            if (manaSortOption == -1) active = true;

            crystal.GetComponentInChildren<Text>().color = active ? Color.white : Color.gray;
            crystal.GetComponent<Image>().sprite = active ? mana : noMana;
        }
        LoadCards();
    }

    void LoadCards() {
        if (!loadedCards) InitiateCards();
        Profile profile = api.GetProfile();

        int matches = 0;
        foreach (DeckBuilderCard deckCard in cardsList) {
            bool match = true;
            Card card = deckCard.card;

            if (!showLockedCards.isOn)
                if (!profile.cards.ContainsKey(card.id) || profile.cards[card.id] == 0) match = false;


            if (manaSortOption != -1) {
                if (manaSortOption < 10) {
                    if (card.mana != manaSortOption) match = false;
                } else if (card.mana < 10) match = false;
            }

            if (searchInput.text.Length > 0) {
                string[] searches = searchInput.text.Split(' ');
                foreach (string s in searches) {
                    string search = s.ToLower();

                    if (card.name.ToLower().IndexOf(search) == -1
                    && card.description.ToLower().IndexOf(search) == -1
                    && card.element.ToString().ToLower().IndexOf(search) == -1
                    && card.type.ToString().ToLower().IndexOf(search) == -1) {
                        match = false;
                    }
                }

            }

            deckCard.gameObject.SetActive(match);
            if (match) {
                deckCard.SetAmount(profile.GetInventoryAmount(deckCard.card.id));
                matches++;
            }
        }

        cardAmount.text = matches + "x cards";
    }

    void InitiateCards() {

        for (int i = 0; i < 11; i++) {
            int manaNumber = i;
            GameObject manaCrystalObj = Instantiate(manaSortCrystal, manaSort);
            manaCrystalObj.GetComponentInChildren<Text>().text = i < 10 ? manaNumber.ToString() : ">9";
            manaCrystalObj.GetComponent<Button>().onClick.AddListener(() => {
                SetManaSort(manaNumber);
            });
        }

        Card[] cards = api.GetAllCards();

        foreach (Card card in cards) {
            GameObject cardContainer = Instantiate(cardContainerPrefab, cardsScroll);
            DeckBuilderCard deckBuilderCard = cardContainer.GetComponent<DeckBuilderCard>();
            api.InstantiateCard(card.id, deckBuilderCard.cardContainer.transform);

            deckBuilderCard.card = card;

            cardContainer.GetComponent<Button>().onClick.AddListener(() => {
                AddCardToDeck(card.id);
            });

            cardsList.Add(deckBuilderCard);
        }

        loadedCards = true;
    }

    void Update() {

    }
}
