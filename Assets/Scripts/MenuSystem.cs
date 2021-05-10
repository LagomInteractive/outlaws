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

    public void NavigateTo(string key) {
        foreach (Link link in links) {
            link.page.gameObject.SetActive(link.key == key);
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
        Debug.Log("Returning " + at != null);
        return at != null;
    }

    public string GetLocation() {
        return at;
    }
}
