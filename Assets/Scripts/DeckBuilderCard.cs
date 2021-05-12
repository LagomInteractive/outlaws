using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DeckBuilderCard : MonoBehaviour {

    public GameObject placeholderCard;
    public Text amountText;
    public GameObject locked;
    public Card card;
    public Transform cardContainer;

    public void SetAmount(int amount) {
        amountText.text = amount + "x";
        locked.SetActive(amount == 0);
        if (placeholderCard != null) Destroy(placeholderCard);
    }
}
