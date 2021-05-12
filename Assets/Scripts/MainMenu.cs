using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour {
    public CosmicAPI api;
    public GameManager gm;
    public Text username, level, version;
    public MenuSystem menus;

    public Button playButton;
    public GameObject searchingGame;
    public Text gamemodeText, timeQueued;

    public void Setup() {
        Profile me = api.GetProfile();
        username.text = me.username;
        level.text = "Level " + me.level;
        version.text = "V" + CosmicAPI.API_VERSION;
        searchingGame.SetActive(false);
    }

    void Update() {
        TimeSpan t = TimeSpan.FromSeconds(gm.GetSearchTime());
        timeQueued.text = string.Format("{0:D2}:{1:D2}",
                                t.Minutes,
                                t.Seconds);
    }

    public void OpenWebsite() {
        Application.OpenURL("https://outlaws.ygstr.com/");
    }

    public void StartedSearchingGame() {
        searchingGame.SetActive(true);
        playButton.interactable = false;

    }

    public void StoppedSearchingGame() {
        searchingGame.SetActive(false);
        playButton.interactable = true;
    }


    public void QuitGame() {
        Application.Quit();
    }
}
