using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using System;
using System.Text;
using UnityEngine.SceneManagement;

public class Session : MonoBehaviour {
    //public fields
    public GameObject coinsToGame;
    public GameObject playButton;
    public GameObject logoutButton;
    public Text currentPlayerName;
    public Text coins;
    public Text games;
    public Text errorText;
    public RectTransform mainMenuPanel;

    //private fields
    private int intCoinsToGame;
    

    // Use this for initialization
    void Start() {
        //Sets every button event on click
        this.logoutButton.GetComponent<Button>().onClick.AddListener(Logout);
        this.playButton.GetComponent<Button>().onClick.AddListener(LoadGame);
    }

    //The player enters a new game
    public void LoadGame()
    {
        if (Int32.TryParse(this.coinsToGame.GetComponent<InputField>().text, out this.intCoinsToGame))
        {
            if (this.intCoinsToGame <= Player.CoinsAmount)
            {
                //When the coin amount for the next game is correct (doesn´t overstep the total coin amount), it moves to the next scene, the game scene
                Player.CoinsInGame = this.intCoinsToGame;
                this.errorText.text = "";
                this.coinsToGame.GetComponent<InputField>().text = "";
                SceneManager.LoadScene("Game");
            }
            else
            {
                errorText.text = "You don´t have enough coins";
            }
        }
        else
        {
            errorText.text = "You must write a number";
        }
    }

    //When the user wants to logout, cleans every space and returns to the main menu background
    private void Logout()
    {
        this.errorText.text = "";
        this.coinsToGame.GetComponent<InputField>().text = "";
        this.mainMenuPanel.SetAsLastSibling();
    }
	
	// Update is called once per frame
	void Update () {
        if (Player.playerResponse != "loadPlayerOK" && Player.playerResponse != null && Player.playerResponse != "")
        {
            Debug.Log("There was a problem: " + Player.playerResponse);
            //TODO: make the corresponding actions to manage this error
        }
        else
        {
            //insert the player data into the corresponding field in the game screen
            this.currentPlayerName.text = Player.Nickname;
            this.coins.text = Player.CoinsAmount.ToString();
            this.games.text = Player.GamesWon.ToString();

            if (Input.GetKeyDown(KeyCode.Tab))
            {
                if (this.coinsToGame.GetComponent<InputField>().isFocused)
                {
                    this.playButton.GetComponent<Button>().Select();
                }
            }
        }
    }
}
