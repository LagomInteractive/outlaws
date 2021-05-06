using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.UI;

public class AnimationManager : MonoBehaviour
{
    public CosmicAPI api;
    public Transform enemyCardEnter;
    public AnimatorController animatorController;

    private void Start()
    {
        api = FindObjectOfType<CosmicAPI>();
        api.OnOpponentUsedCard += (cardID) =>
        {
            GameObject enemyCardUsed = api.InstantiateCard(cardID, enemyCardEnter, false);
            enemyCardUsed.transform.localScale *= 0.2f;
            enemyCardUsed.AddComponent<Animations>();
            Animator animator = enemyCardUsed.AddComponent<Animator>();
            animator.runtimeAnimatorController = animatorController;
            //GameObject enemyCard = api.InstantiateCard(cardID, enemyCardEnter, false);
            //anim.SetTrigger("OnOpponentCardExit");
        };
    }
    void Update()
    {
        
    }
}
