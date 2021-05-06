using System;
using System.Collections;
using System.Collections.Generic;
/*using UnityEditor.Animations;*/
using UnityEngine;
using UnityEngine.UI;

public class AnimationManager : MonoBehaviour {
    public CosmicAPI api;
    public Transform enemyCardEnter;
    public Transform cardDrawPlayer;
    /*public AnimatorController animatorController;
    public AnimatorController animatorController2;*/

    private void Start() {
        api = FindObjectOfType<CosmicAPI>();
        api.OnOpponentUsedCard += (cardID) => {
            GameObject enemyCardUsed = api.InstantiateCard(cardID, enemyCardEnter, false);
            enemyCardUsed.transform.localScale *= 0.2f;
            enemyCardUsed.AddComponent<Animations>();
            Animator animator = enemyCardUsed.AddComponent<Animator>();
            // Bad solution, needs to fix
            animator.runtimeAnimatorController = Resources.Load("Card") as RuntimeAnimatorController;
            animator.SetTrigger("OnOpponentCard");
        };

        api.OnCard += (cardID) => {
            GameObject enemyCardUsed = api.InstantiateCard(cardID, cardDrawPlayer, false);
            enemyCardUsed.transform.localScale *= 0.2f;
            enemyCardUsed.transform.Rotate(90f, 0f, 62.473f);
            enemyCardUsed.AddComponent<Animations>();
            Animator animator = enemyCardUsed.AddComponent<Animator>();
            // Bad solution, needs to fix
            animator.runtimeAnimatorController = Resources.Load("OnCard") as RuntimeAnimatorController;
        };
    }
    void Update() {

    }
}
