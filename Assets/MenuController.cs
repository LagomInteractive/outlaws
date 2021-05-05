using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading.Tasks;

public class MenuController : MonoBehaviour
{
    public GameObject SignUp;
    public GameObject SignIn;
    public GameObject LoadingScreen;
    public GameObject MenuDaily;
    public GameObject MenuMain;
    public GameObject MainQuit;
    public GameObject MainSettings;
    public GameObject Store;
    public GameObject StoreFeatured;
    public GameObject StorePacks;
    public GameObject StoreMicroTrans;
    public GameObject DeckBuilder;
    public GameObject GamemodePicker;
    public GameObject PVPPicker;
    public GameObject ClassSelect;
    public GameObject DeckSelect;
    public GameObject GameOptions;
    public GameObject DefeatScreen;
    public GameObject PVPVictoryScreen;
    public GameObject CampaignVictoryScreen;
    public GameObject Match;
    public GameObject Menus;



    public void StartMatch()
    {
        Match.SetActive(true);
        Menus.SetActive(false);
    }

    public void ExitGame()
    {
        Application.Quit();
    }

    public void ReturnExit()
    {
        MainQuit.SetActive(false);
    }

    public void ConfirmExit()
    {
        MainQuit.SetActive(true);
    }

    public void Login()
    {
        SignIn.SetActive(true);
        SignUp.SetActive(false);
    }

    public void SigningUp()
    {
        SignIn.SetActive(false);
        SignUp.SetActive(true);
    }

    public void DailyLogin()
    {
        MenuDaily.SetActive(true);
        SignIn.SetActive(false);
        SignUp.SetActive(false);
    }

    public void ToMenu()
    {
        MenuMain.SetActive(true);
        MenuDaily.SetActive(false);
        MainSettings.SetActive(false);
        GamemodePicker.SetActive(false);
        Store.SetActive(false);
    }

    public void ToStore()
    {
        StoreFeatured.SetActive(true);
        StorePacks.SetActive(false);
        StoreMicroTrans.SetActive(false);
        Store.SetActive(true);
        StoreMicroTrans.SetActive(false);
        MenuMain.SetActive(false);
    }

    public void FeaturedStore()
    {
        StoreFeatured.SetActive(true);
        StorePacks.SetActive(false);
    }

    public void PacksStore()
    {
        StoreFeatured.SetActive(false);
        StorePacks.SetActive(true);
    }

    public void MicroTrans()
    {
        Store.SetActive(false);
        StoreMicroTrans.SetActive(true);
    }

    public void PlayMode()
    {
        GamemodePicker.SetActive(true);
        MenuMain.SetActive(false);
        PVPPicker.SetActive(false);
        ClassSelect.SetActive(false);
    }

    public void BuildingDecks()
    {
        DeckBuilder.SetActive(true);
        MenuMain.SetActive(false);
    }

    public void GameSettings()
    {
        MainSettings.SetActive(true);
        MenuMain.SetActive(false);
    }

    public void Casual()
    {
        GamemodePicker.SetActive(false);
        ClassSelect.SetActive(true);
    }

    public void PvP()
    {
        ClassSelect.SetActive(false);
        GamemodePicker.SetActive(false);
        PVPPicker.SetActive(true);
    }

    public void PvPMode()
    {
        PVPPicker.SetActive(false);
        ClassSelect.SetActive(true);
    }

    public void ChangeClass()
    {
        ClassSelect.SetActive(true);
        DeckSelect.SetActive(false);
    }

    public void ClassSelection()
    {
        ClassSelect.SetActive(false);
        DeckSelect.SetActive(true);
    }

    public void DeckSelection()
    {
        DeckSelect.SetActive(false);
        LoadingScreen.SetActive(true);
    }

    public void MatchStart()
    {
        LoadingScreen.SetActive(false);
        MenuMain.SetActive(false);
        Match.SetActive(true);
    }
}
