using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class ManageWindows : MonoBehaviour
{
    private bool showInitialBetWindow;
    private bool showBlackjackBetWindow;
    private bool showAnotherRoundWindow;
    private Rect initialBetWindow = new Rect(100, 100, 400, 200);
    private Rect blackjackBetWindow = new Rect(100, 100, 400, 200);
    private Rect anotherRoundWindow = new Rect(100, 100, 400, 200);
    private string playerInitialBet;
    private int initialBetValue;
    private string playerBlackjackBet;
    private int blackjackBet;

    void Start()
    {
        playerInitialBet = "";
        playerBlackjackBet = "";
        showInitialBetWindow = false;
        showBlackjackBetWindow = false;
        showAnotherRoundWindow = false;
    }

    void Update()
    {
    }

    void OnGUI()
    {
        //creates the windos with the corresponding rect and the function that calls
        if (showInitialBetWindow)
        {
            initialBetWindow = GUI.Window(0, initialBetWindow, DoInitialBetWindow, "Initial Bet");
        }
        if (showBlackjackBetWindow)
        {
            blackjackBetWindow = GUI.Window(1, blackjackBetWindow, DoBlackjackBetWindow, "Blackjack Bet");
        }
        if (showAnotherRoundWindow)
        {
            anotherRoundWindow = GUI.Window(2, anotherRoundWindow, DoAnotherRoundWindow, "Another round?");
        }
    }

    /*
    * This window asks for the player initial bet
    */
   void DoInitialBetWindow(int windowID)
    {
        //create the labels and the texfield in the window
        GUI.Label(new Rect(25, 20, 390, 60), "Please, insert your initial bet in the next field and press Start Button! to continue.");
        GUI.Label(new Rect(25, 65, 100, 30), "Initial Bet: ");
        playerInitialBet = GUI.TextField(new Rect(130, 65, 200, 25), playerInitialBet, 25);

        if (GUI.Button(new Rect(290, 170, 100, 20), "Continue"))
        {
            //validate if the input is a number
            if (int.TryParse(playerInitialBet, out initialBetValue))
            {
                int playerCoins = Player.CoinsInGame;

                if (initialBetValue <= playerCoins)
                {
                    PlayersFields.PlayersBets[Player.PlayerPosition].text = "Bet: " + playerInitialBet;
                    PlayersFields.PlayersCoins[Player.PlayerPosition].text = (playerCoins - initialBetValue).ToString();
                    showInitialBetWindow = false;
                    DealCoins(Player.PlayerPosition, initialBetValue);
                    this.betReady.Invoke(initialBetValue);
                }
            }
        }
    }

    //Window that asks for the secondary bet (bet the house has blackjack)
    void DoBlackjackBetWindow(int windowID)
    {
        GUI.Label(new Rect(25, 20, 390, 60), "Please, insert your secondary bet in the next field and press Start Button! to continue.");
        GUI.Label(new Rect(25, 65, 100, 30), "Secondary Bet: ");
        playerBlackjackBet = GUI.TextField(new Rect(130, 65, 200, 25), playerBlackjackBet, 25);
        if (GUI.Button(new Rect(290, 170, 100, 20), "Start Button!"))
        {
            //validate if the input is a number
            if (int.TryParse(playerBlackjackBet, out blackjackBet))
            {
                int playerCoins = int.Parse(playersCoins[this.currentPlayerPosition].text);

                if (blackjackBet <= (initialBetValue / 2) && blackjackBet <= int.Parse(this.playersCoins[this.currentPlayerPosition].text))
                {
                    playersBlackjackBet[this.currentPlayerPosition].text = "BJ Bet: " + blackjackBet;
                    playersCoins[this.currentPlayerPosition].text = (playerCoins - blackjackBet).ToString();
                    hideBlackjackBetWindow = true;
                    DealCoins(this.currentPlayerPosition + 3, blackjackBet);
                    this.blackjackBetReady.Invoke(blackjackBet);
                    deactivateButtons();
                    activateButtons();
                }
            }
        }
    }

    void DoAnotherRoundWindow(int windowID)
    {
        new Rect(25, 65, 100, 30);
        if (GUI.Button(new Rect(50, 65, 80, 30), "Yes"))
        {
            this.hideAnotherRoundWindow = true;
        }
        if (GUI.Button(new Rect(300, 65, 80, 30), "No"))
        {
            this.hideAnotherRoundWindow = true;
        }
    }

    /*
     * Manage all the windows in the game, it actives the windows depends in the string that it recive
     */
    public void ToggleWindows(string selectedWindow)
    {
        if (selectedWindow == "initialBet")
        {
            showInitialBetWindow = true;
        }
        else if (selectedWindow == "blackjack")
        {
            showBlackjackBetWindow = true;
        }
        else if (selectedWindow == "anotherRound")
        {
            showAnotherRoundWindow = true;
        }
    }
}

