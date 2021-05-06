using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Effect {
    public string name;
    public int[] ids;
    public GameObject particles;
    public AudioClip[] sounds;
}

public class EffectsManager : MonoBehaviour {


    public Effect[] effects;

    void Start() {
        foreach (Effect effect in effects) {
            PrepareEffect(effect);
        }
    }

    public void PlayEffect(string name) {
        foreach (Effect effect in effects) {
            if (effect.name == name) PlayEffect(effect);
        }
    }

    public void PrepareEffect(Effect effect) {
        effect.particles.SetActive(true);
        foreach (ParticleSystem particles in effect.particles.GetComponentsInChildren<ParticleSystem>()) {
            ParticleSystem.MainModule main = particles.main;
            main.loop = false;
            particles.time = 0;
            particles.Stop();
        }
    }

    public void PlaySound(string name) {
        Effect effect = GetEffect(name);
        AudioClip sound = effect.sounds[(int)Random.Range(0, effect.sounds.Length - 1)];
    }

    public Effect GetEffect(string name) {
        foreach (Effect effect in effects) {
            if (effect.name == name) return effect;
        }
        return null;
    }

    public void PlayEffect(Effect effect) {
        PlayEffect(effect.particles);
        PlaySound(effect.name);
    }

    public void PlayEffect(GameObject effect) {
        foreach (ParticleSystem particles in effect.GetComponentsInChildren<ParticleSystem>()) {
            ParticleSystem.MainModule main = particles.main;
            main.loop = false;
            particles.time = 0;
            particles.Play();
        }
    }
}
