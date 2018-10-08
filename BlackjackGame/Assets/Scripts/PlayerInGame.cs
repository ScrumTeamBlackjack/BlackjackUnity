using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

class PlayerInGame
{
    public string PlayerNickname { get; private set; }
    public int PlayerInitialBet { get; private set; }
    public int PlayerBlackjackBet { get; private set; }
    public List<Card> PlayerCards { get; private set; }
    public int PlayerTotalCoinsInGame { get; set; }
    public int PlayerPosition { get; private set; }
    public bool IsLocal { get; private set; }

    //default constructor
    public PlayerInGame()
    {
        this.PlayerNickname = "";
        this.PlayerInitialBet = 0;
        this.PlayerBlackjackBet = 0;
        this.PlayerCards = new List<Card>();
        this.PlayerTotalCoinsInGame = 0;
        this.PlayerPosition = 0;
        this.IsLocal = false;
    }

    //to create the player object initiality, it's because at the beginning the server only pass the name (and the coins but only logic)
    public PlayerInGame(string nickname, int totalCoins, int position, bool isLocal)
    {
        this.PlayerNickname = nickname;
        this.PlayerTotalCoinsInGame = totalCoins;
        this.PlayerInitialBet = 0;
        this.PlayerBlackjackBet = 0;
        this.PlayerCards = new List<Card>();
        this.PlayerPosition = position;
        this.IsLocal = isLocal;
    }

    //to add a card to the pleyer
    public void AddCardToPlayer(Card newCard)
    {
        PlayerCards.Add(newCard);
    }

    //returns the hand count of the player
    public int GetPlayerHandCount()
    {
        int handCount = 0;
        foreach (Card tmpCard in PlayerCards)
        {
            handCount += tmpCard.Value;
            //to change the Ace value when the hand count is greater than 21
            if (handCount > 21)
            {
                //seach if in the current cards exist an Ace to substract it value to the hand count
                bool tmpFlag = true;
                foreach (Card tmpSearchedCard in PlayerCards)
                {
                    //takes the actual card that it value is 11 and change it to 1, then substract 10 to handCount
                    if (tmpSearchedCard.Value == 11 && tmpFlag)
                    {
                        //find the index of the tmpCard in the player cards list
                        int index = PlayerCards.BinarySearch(tmpSearchedCard);
                        //change the value of this card in the index that the binary search returned 
                        PlayerCards[index].ChangeValue(1);
                        //we subtract 10 from the current value to compensate the new value of Ace
                        handCount -= 10;
                        tmpFlag = false;
                    }
                }
            }
        }
        return handCount;
    }

    //to set the initial bet of the player
    public void SetInitialBet(int initialBet)
    {
        if (initialBet > 0)
        {
            this.PlayerInitialBet = initialBet;
        }
    }

    //to set the blackjack bet of the player, it needs to be at least the half of the initial bet
    public void SetBlackjackBet(int blackjackBet)
    {
        if (blackjackBet > 0 && blackjackBet <= (this.PlayerInitialBet/2))
        {
            this.PlayerBlackjackBet = blackjackBet;
        }
    }

}