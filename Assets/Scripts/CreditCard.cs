using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreditCard : MonoBehaviour {
    public int CardId;

    void Start() {
        CosmicAPI api = GameObject.Find("API").GetComponent<CosmicAPI>();
        Transform cardContainer = transform.Find("PreviewCard");
        DestroyImmediate(cardContainer.GetChild(0).gameObject);
        api.InstantiateCard(CardId, cardContainer);
    }
}
