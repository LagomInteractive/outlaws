using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour {
    public CosmicAPI api;
    public Text username, level;
    public MenuSystem menus;
    public Text searchingGameText, playButtonText;

    public void Setup() {
        Profile me = api.GetProfile();
        username.text = me.username;
        level.text = "Level " + me.level;
        searchingGameText.gameObject.SetActive(false);
    }

    public void StartedSearchingGame() {
        searchingGameText.gameObject.SetActive(true);
        playButtonText.text = "CANCEL SEARCH!";
    }

    public void StoppedSearchingGame() {
        searchingGameText.gameObject.SetActive(false);
        playButtonText.text = "PLAY";
    }

    public void QuitGame() {
        Application.Quit();
    }
}
