using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Link {
    public string key;
    public Transform page;
}

public class MenuSystem : MonoBehaviour {

    public List<Link> links = new List<Link>();
    public string at = null;
    // If the client is out of date
    bool outOfDate = false;

    public AudioClip click;

    public AudioSource audioPlayer;

    public void PlaySoundEffect(AudioClip effect) {
        audioPlayer.Stop();
        audioPlayer.clip = effect;
        audioPlayer.time = 0;
        audioPlayer.Play();
    }

    public void PlayClick() {
        PlaySoundEffect(click);
    }

    /// <summary>
    /// Go to a menu page, hides all other pages
    /// </summary>
    /// <param name="key">Key to the page</param>
    public void NavigateTo(string key) {
        PlaySoundEffect(click);
        NavigateSilent(key);
    }

    public void NavigateSilent(string key) {
        if (outOfDate) return;
        if (key == "out_of_date_warning") outOfDate = true;
        foreach (Link link in links) {
            link.page.gameObject.SetActive(link.key == key);
        }
        at = key;
    }

    /// <summary>
    /// Ovarlay a page ontop of the current one. Does not hide any other page
    /// Make sure the page to overlay is above in the higharcy
    /// </summary>
    /// <param name="key">Key to the page</param>
    public void Overlay(string key) {
        foreach (Link link in links) {
            if (link.key == key) link.page.gameObject.SetActive(true);
        }
        at = key;
    }

    public void CloseMenu() {
        foreach (Link link in links) {
            link.page.gameObject.SetActive(false);
        }
        at = null;
    }

    public bool IsMenusOpen() {
        return at != null;
    }

    public string GetLocation() {
        return at;
    }
}
