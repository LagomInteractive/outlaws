using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Example : MonoBehaviour
{
    public GameObject[] Effects;
    private int EffectIndex;
    private GameObject PreEffect;
    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < this.Effects.Length; i++)
        {
            this.Effects[i].SetActive(false);
        }
        this.Effects[0].SetActive(true);
        this.PreEffect = this.Effects[0];
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.W))
        {
            this.EffectIndex++;
            if(this.EffectIndex>this.Effects.Length-1)
                this.EffectIndex = 0;
            this.Effects[this.EffectIndex].SetActive(true);
            this.PreEffect.SetActive(false);
            this.PreEffect = this.Effects[this.EffectIndex];
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            this.EffectIndex--;
            if (this.EffectIndex < 0)
                this.EffectIndex = this.Effects.Length-1;
            this.Effects[this.EffectIndex].SetActive(true);
            this.PreEffect.SetActive(false);
            this.PreEffect = this.Effects[this.EffectIndex];
        }
    }
}
