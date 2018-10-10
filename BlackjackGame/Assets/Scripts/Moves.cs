using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

//Event that activates the window for the player to introduce his blackjack bet
[System.Serializable]
public class ActivateBlackjackBetWindowEvent : UnityEvent<string>
{
}

class Moves : MonoBehaviour
{
    //Public Fields
    public UnityEvent askForACard;
    public UnityEvent passTurn;
    public UnityEvent doubleBet;
    public ActivateBlackjackBetWindowEvent activateBlackjackBetWindow;

    //Private Fields
    private Button askForACardButton;
    private Button bet21Button;
    private Button doubleBetButton;
    private Button passButton;

    void Start()
    {
       FindButtons();
    }

    private void FindButtons()
    {
        askForACardButton = GameObject.Find("AskForACardButton").GetComponent<Button>();
        askForACardButton.onClick.AddListener(AskForExtraCard);
        askForACardButton.interactable = false;
        bet21Button = GameObject.Find("Bet21Button").GetComponent<Button>();
        bet21Button.onClick.AddListener(BetFor21);
        bet21Button.interactable = false;
        doubleBetButton = GameObject.Find("DoubleBetButton").GetComponent<Button>();
        doubleBetButton.onClick.AddListener(AskForDoubleBet);
        doubleBetButton.interactable = false;
        passButton = GameObject.Find("PassButton").GetComponent<Button>();
        passButton.onClick.AddListener(PassTurn);
        passButton.interactable = false;
    }

    public void activateButtons(bool doubleBet, bool blackjackBet)
    {
        passButton.GetComponent<Button>().interactable = true;
        askForACardButton.GetComponent<Button>().interactable = true;

        if (doubleBet)
        {
            doubleBetButton.GetComponent<Button>().interactable = true;
        }
        if (blackjackBet)
        {
            bet21Button.GetComponent<Button>().interactable = true;
        }
    }

    //Deactivates all the buttons
    public void DeactivateButtons()
    {
        askForACardButton.GetComponent<Button>().interactable = false;
        doubleBetButton.GetComponent<Button>().interactable = false;
        passButton.GetComponent<Button>().interactable = false;
        bet21Button.GetComponent<Button>().interactable = false;
    }

    //Deactivtes the secondary buttons (double bet and blackjack bet)
    public void DeactivateSecondaryButtons()
    {
        this.bet21Button.GetComponent<Button>().interactable = false;
        this.doubleBetButton.GetComponent<Button>().interactable = false;

    }

    //Asks the server for an extra card  
    void AskForExtraCard()
    {
       this.askForACard.Invoke();
    }

    /*
     * This method lets one player to bet that the house or casino has blackjack in its initial hand
     * This method is only allowed when the face up card of the house is a 10, J, Q, K or an A
     */
   public void BetFor21()
    {
        this.activateBlackjackBetWindow.Invoke("blackjack");
    }

    //Asks the server for a new card, double the bet and pass the turn
    public void AskForDoubleBet()
    {
        this.doubleBet.Invoke();
    }

    public void PassTurn()
    {
        DeactivateButtons();
        this.passTurn.Invoke();
    }
}

