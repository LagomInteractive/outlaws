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

    /// <summary>
    /// Go to a menu page, hides all other pages
    /// </summary>
    /// <param name="key">Key to the page</param>
    public void NavigateTo(string key) {
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
