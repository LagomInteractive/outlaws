using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Effect {
    public string name;
    public int[] ids;
    public GameObject particles;
    public AudioClip[] sounds;


    public void Play(AudioSource source = null) {
        if (particles != null) PlayEffect();
        if (sounds.Length != 0 && source != null) PlaySound(source);
    }

    public void PlayEffect() {
        PlayParticles(particles);
    }

    public void PlayParticles(GameObject particles) {
        foreach (ParticleSystem particle in particles.GetComponentsInChildren<ParticleSystem>()) {
            ParticleSystem.MainModule main = particle.main;
            main.loop = false;
            particle.time = 0;
            particle.Play();
        }
    }

    public void PlaySound(AudioSource source) {
        AudioClip sound = sounds[(int)Random.Range(0, sounds.Length - 1)];
        source.PlayOneShot(sound, 1);
    }
}

public class EffectsManager : MonoBehaviour {

    public CosmicAPI api;


    public Effect[] effects;
    public AudioSource audioSource;

    void Start() {
        foreach (Effect effect in effects) {
            PrepareEffect(effect);
        }

        api.OnCardUsed += playedCard => {
            foreach (Effect effect in effects) {
                foreach (int id in effect.ids) {
                    if (id == playedCard) {
                        effect.Play();
                    }
                }
            }
        };

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

    public Effect GetEffect(string name) {
        foreach (Effect effect in effects) {
            if (effect.name == name) return effect;
        }
        return null;
    }



}
