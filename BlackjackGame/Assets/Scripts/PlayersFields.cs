using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public static class PlayersFields
{
    public static List<Text> PlayerNames { get; private set; }
    public static List<Text> PlayersCoins { get; private set; }
    public static List<Text> PlayersBets { get; private set; }
    public static List<Text> PlayersHandCount { get; private set; }
    public static List<Text> PlayersBlackjackBet { get; private set; }
    public static Text TurnText { get; private set; }
    public static List<GameObject> PlayersCardsPositions { get; private set; }
    public static List<Transform> DealerPositions { get; private set; }
    public static Transform DealerLastCardPosition { get; set; }
    public static List<Transform> PlayersLastCardPosition { get; set; }
    public static Text CasinoHand { get; set; }

    private static void InitializeAttr()
    {
        PlayerNames = new List<Text>();
        PlayersCoins = new List<Text>(); ;
        PlayersBets = new List<Text>();
        PlayersHandCount = new List<Text>();
        PlayersBlackjackBet = new List<Text>();
        TurnText = GameObject.Find("TurnText").GetComponent<Text>();
        PlayersCardsPositions = new List<GameObject>();
        DealerPositions = new List<Transform>();
        PlayersLastCardPosition = new List<Transform>();
        CasinoHand = GameObject.Find("CasinoHand").GetComponent<Text>();
    }

    public static void FindFields()
    {   
        //first we need to initialize all the attributes in the class
        InitializeAttr();

        //find the players (and all the other texts) in the table
        int i = 0;
        while (i < 3)
        {
            PlayerNames[i] = GameObject.Find("Player" + (i + 1)).GetComponent<Text>();
            PlayersCoins[i] = GameObject.Find("Player" + (i + 1) + "Coins").GetComponent<Text>();
            PlayersBets[i] = GameObject.Find("Player" + (i + 1) + "Bet").GetComponent<Text>();
            PlayersHandCount[i] = GameObject.Find("Player" + (i + 1) + "Hand").GetComponent<Text>();
            PlayersBlackjackBet[i] = GameObject.Find("Player" + (i + 1) + "Blackjack").GetComponent<Text>();
            PlayersCardsPositions[i] = GameObject.Find("cardFieldPlayer" + (i + 1));
        }

        //find the cards positions for the dealer
        DealerPositions.Add(GameObject.Find("DealerFieldCard1").GetComponent<Transform>());
        DealerPositions.Add(GameObject.Find("DealerFieldCard2").GetComponent<Transform>());
        DealerLastCardPosition = GameObject.Find("DealerFieldCard2").GetComponent<Transform>();

        //set the last card position of all the player
        foreach (GameObject tmpObjectPosition in PlayersCardsPositions)
        {
            PlayersLastCardPosition.Add(tmpObjectPosition.transform.GetChild(1));
        }
    }
}

