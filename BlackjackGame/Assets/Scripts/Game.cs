using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

//Event that tells the client that this player's initial bet is ready
[System.Serializable]
public class InitialBetReadyEvent : UnityEvent<int>
{
}

//Event that tells the client that this player's blackjack bet is ready
[System.Serializable]
public class BlackjckBetReadyEvent : UnityEvent<int>
{
}

//Event that puts the coins of a remote player in the table
[System.Serializable]
public class PutRemoteCoinsEvent : UnityEvent<int, int, string>
{
}

//Event that activates the buttons of the player
[System.Serializable]
public class ActivateButtonsEvent : UnityEvent<bool, bool>
{
}

public struct ExtraCardParameters
{
    public int playerPosition;
    public GameObject cardToInstantiate;
    public Vector3 goalPosition;

    public ExtraCardParameters(int position, GameObject prefab, Vector3 goal)
    {
        this.playerPosition = position;
        this.cardToInstantiate = prefab;
        this.goalPosition = goal;
    }
}

public class Game : MonoBehaviour
{
    //public fields
    public float moveSpeed;
    public Transform[] fieldsPosition;
    public GameObject cardPrefab;

    public InitialBetReadyEvent betReady;
    public BlackjckBetReadyEvent blackjackBetReady;
    public PutRemoteCoinsEvent putRemoteCoins;
    public ActivateButtonsEvent activateButtons;
    public UnityEvent deactivateSecondaryButtons;
    public UnityEvent deactivateButtons;
    public UnityEvent blackjackBetRight;
    public UnityEvent passTurn;

    //private fields
    private int currentCard;
    private List<GameObject> initialCardsToDeal; 
    private const int totalCards = 52;
    private List<Card> cardsDealed = new List<Card>();
    private List<PlayerInGame> playersInGame;
    private ExtraCardParameters extraCardParameters;
    private bool extraCardRequest;

    //tmp new functionality
    private List<Card> cardToInstante = new List<Card>(); // pile with the 52 cards

    // Use this for initialization
    void Start()
    {
        playersInGame = new List<PlayerInGame>();
        initialCardsToDeal = new List<GameObject>();
        currentCard = 0;
        extraCardRequest = false;
    }

    // Update is called once per frame
    void Update()
    {
        //To give every player 2 cards, when everyone make their initial bets
        if (currentCard < initialCardsToDeal.Count)
        {
            //the current card is already in the corresponding position
            if (initialCardsToDeal[currentCard].transform.position == fieldsPosition[currentCard].position)
            {
                //to flip all the card, (except the first dealer's card)
                if (currentCard > 0)
                {
                    //rotate the card in the X axis
                    Vector3 newRotation = new Vector3(180, 0, 0);
                    //put the rotation to the card
                    initialCardsToDeal[currentCard].transform.Rotate(newRotation * 5f);
                }
                currentCard++;
                ReduceThicknessOfDeck();
            }
            //the current card is still traveling from the deck to the corresponding position
            else
            {
                //To move the card from deck to corresponding position with an effect and certain velocity
                initialCardsToDeal[currentCard].transform.position = Vector3.MoveTowards(initialCardsToDeal[currentCard].transform.position, fieldsPosition[currentCard].position, moveSpeed * Time.deltaTime);

            }
        }

        //To give one extra card to player 1
        else if (extraCardRequest)
        {
            if (extraCardParameters.cardToInstantiate.transform.position == extraCardParameters.goalPosition)
            {
                Vector3 newRotation = new Vector3(180, 0, 0);
                extraCardParameters.cardToInstantiate.transform.Rotate(newRotation * 5f);
                ReduceThicknessOfDeck();
                extraCardRequest = false;
            }
            else
            {
                extraCardParameters.cardToInstantiate.transform.position = Vector3.MoveTowards(extraCardParameters.cardToInstantiate.transform.position, extraCardParameters.goalPosition, moveSpeed * Time.deltaTime);
            }
        }        
    }

    private void ReduceThicknessOfDeck() // animation of taking a card from the deck (reduces the Scale Y of the deck)
    {
        GameObject objectDeck = GameObject.Find("Deck");
        objectDeck.transform.localScale = (objectDeck.transform.localScale - new Vector3(0, 0.00943f, 0));
    }

    public void PutAPlayerInTheTable(int position, string remotePlayer)
    {
        //Puts the local player in the table
        if (remotePlayer == null)
        {
            Player.PlayerPosition = position;
            //create the local player object and add it to the array that manage the players information
            PlayerInGame tmpPlayer = new PlayerInGame(Player.Nickname, Player.CoinsInGame, position, true);
            playersInGame.Add(tmpPlayer);
            PlayersFields.PlayerNames[position].text = tmpPlayer.PlayerNickname;
            PlayersFields.PlayersCoins[position].text = tmpPlayer.PlayerTotalCoinsInGame.ToString();
        }
        //Puts a remote player in the table
        else
        { 
            PlayerInGame tmpPlayer = new PlayerInGame(remotePlayer, 0, position, false);
            playersInGame.Add(tmpPlayer);
            PlayersFields.PlayerNames[position].text = tmpPlayer.PlayerNickname;
        }
    }

    //Saves the local player bet and tells the server that this client is ready
    public void SaveLocalInitialBet(int bet)
    {
        GetCurrentPlayer(Player.PlayerPosition).SetInitialBet(bet);
        GetCurrentPlayer(Player.PlayerPosition).PlayerTotalCoinsInGame -= bet;
        this.betReady.Invoke(bet);
    }

    //Puts an initial bet from another player
    public void PutRemoteInitialBet(int position, string bet)
    {
        PlayersFields.PlayersBets[position].text = "Bet: " + bet;
        GetCurrentPlayer(position).SetInitialBet(int.Parse(bet));
        this.putRemoteCoins.Invoke(position, int.Parse(bet), "initial");
    }

    //Recieves a logical card with format material+suit+number and turns them into Card objects and put them in the table
    public void PutInitialCards(List<string> cards)
    {
        int i = 0;
        foreach (string logicalCard in cards)
        {
            if (i == 0 || i == 1)
            {
                cardToInstante.Add(CreateCard(logicalCard, -1));
            }
            else if (i == 2 || i == 3)
            {
                cardToInstante.Add(CreateCard(logicalCard, 0));
            }
            else if (i == 4 || i == 5)
            {
                cardToInstante.Add(CreateCard(logicalCard, 1));
            }
            else if (i == 6 || i == 7)
            {
                cardToInstante.Add(CreateCard(logicalCard, 2));
            }
            i++;
        }
        InstantiateInitialCardsPrefab();
    }

    //creates a card from a logical card, and asign that card to one player
    public Card CreateCard(string logicalCard, int player)
    {
        Card newCard = new Card(logicalCard);
        GetCurrentPlayer(player).AddCardToPlayer(newCard);
        return newCard;
    }

    void InstantiateInitialCardsPrefab()
    {
        foreach (Card tmpLogicalCard in cardToInstante)
        {
            //reference the position of the deck
            Transform deckPosition = GameObject.Find("Deck").GetComponent<Transform>();
            GameObject cardPrefab = (GameObject)Instantiate(Resources.Load("Prefabs/Card"), deckPosition.position, deckPosition.rotation);
            cardPrefab.transform.Rotate(0, -90, 0);
            // generates the Card prefab
            ObjectCard tmpObjectCard = cardPrefab.GetComponent<ObjectCard>();
            // set the logicalCard values to the prefab object   
            tmpObjectCard.gameObject.GetComponent<ObjectCard>().SetCard(tmpLogicalCard);
            //add the current to an a list to manage the card in the game 
            cardsDealed.Add(tmpLogicalCard);
            //add the prefab to move the card from deckposition to cardposition
            initialCardsToDeal.Add(cardPrefab);
        }
        //clear the pile to avoid any error
        cardToInstante.Clear();
    }

    //Recieves an extra card for one player and put it in the table
    public void PutExtraCard(int player, string logicalCard, bool turnChange)
    {
        this.deactivateSecondaryButtons.Invoke();

        Card tmpLogicalCard = CreateCard(logicalCard, player);

        //reference the position of the deck
        Transform deckPosition = GameObject.Find("Deck").GetComponent<Transform>();
        GameObject cardPrefab = (GameObject)Instantiate(Resources.Load("Prefabs/Card"), deckPosition.position, deckPosition.rotation);
        cardPrefab.transform.Rotate(0, -90, 0);
        // generates the Card prefab
        ObjectCard tmpObjectCard = cardPrefab.GetComponent<ObjectCard>();
        // set the logicalCard values to the prefab object   
        tmpObjectCard.gameObject.GetComponent<ObjectCard>().SetCard(tmpLogicalCard);
        //add the current to an a list to manage the card in the game 
        cardsDealed.Add(tmpLogicalCard);

        //To put a card in the table depends if is the casino or player
        Vector3 goalPosition = GetGoalPosition(player);
        extraCardParameters = new ExtraCardParameters(player, cardPrefab, goalPosition);
        extraCardRequest = true;

        //to modify the player hand count
        int currentPlayerPunctuation = GetCurrentPlayer(player).GetPlayerHandCount();
        UpdatePlayersHandsCount(false);
        if (turnChange && currentPlayerPunctuation > 21)
        {
            if (player == Player.PlayerPosition)
            {
                StartCoroutine(WaitForTurnChange());
            }
        }
    }

    /*
     * To put a card in the table depends if is the casino or player
     * It recives an int that means what player is calling this method
     */
    private Vector3 GetGoalPosition(int player)
    {
        Vector3 goalPosition;
        //to put a casino's card in the table 
        if (player == -1)
        {
            Transform dealerLastCard = PlayersFields.DealerLastCardPosition;
            goalPosition = new Vector3(dealerLastCard.position.x - 1, dealerLastCard.position.y, dealerLastCard.position.z);
            PlayersFields.DealerLastCardPosition.position += new Vector3(-1,0,0);
        }
        //to put a player's card int the table
        else
        {
            Transform playerLastCard = PlayersFields.PlayersLastCardPosition[player];
            goalPosition = new Vector3(playerLastCard.position.x - 0.70f, playerLastCard.position.y + 0.01f, playerLastCard.position.z - 0.50f);
            PlayersFields.PlayersLastCardPosition[player].position += new Vector3(-0.70f, 0.01f, -0.50f);
        }
        return goalPosition;
    }
    
    //This methos search the texture of every card in the resources file and returns it
    private Texture GetCardTexture(string cardValue, int cardSuit)
    {
        string[] suits = { "hearts", "clubs", "diamonds", "spades" };

        // temp texture path
        string cardImage = (cardValue + "_of_" + suits[cardSuit - 1]);
        // get the texture from assets
        return Resources.Load("Textures/AllCards/" + cardImage, typeof(Texture)) as Texture;
    }

    public void TurnChange(string name, int number)
    {
        UpdatePlayersHandsCount(false);
        PlayersFields.TurnText.text = name + "'s Turn";
        //Determines if this client is the next in turn
        // If current player has the turn, every buttons are activated
        if (name == Player.Nickname)
        {
            DetermineButtons();
        }
    }

    //Determines the buttons tha has to be actvated
    private void DetermineButtons()
    {
        int playerCoinsLeft = GetCurrentPlayer(Player.PlayerPosition).PlayerTotalCoinsInGame;
        int playerBet = GetCurrentPlayer(Player.PlayerPosition).PlayerInitialBet;
        bool doubleBet = false;
        bool blackjackBet = false;
        if (playerCoinsLeft >= playerBet)
        {
            doubleBet = true;
        }

        if (cardsDealed[1].Value == 11 || cardsDealed[1].Value == 10)
        {
            if (playerCoinsLeft > 0)
            {
                blackjackBet = true;
            }
        }
        this.activateButtons.Invoke(doubleBet, blackjackBet);
    }

    //Doubles de bet of the player
    public void DoubleBet(int player, string card)
    {
        GetCurrentPlayer(player).SetInitialBet(GetCurrentPlayer(player).PlayerInitialBet * 2);
        PlayersFields.PlayersBets[player].text = "Bet: " + GetCurrentPlayer(player).PlayerInitialBet.ToString();
        PutExtraCard(player, card, false);
        if (Player.PlayerPosition == player)
        {
            GetCurrentPlayer(player).PlayerTotalCoinsInGame -= (GetCurrentPlayer(player).PlayerInitialBet / 2);
            PlayersFields.PlayersCoins[player].text = GetCurrentPlayer(player).PlayerTotalCoinsInGame.ToString();
        }
        this.deactivateButtons.Invoke();
    }

    //Checks that the local blackjack bet is right
    public void CheckLocalBlackjackBet(int blackjackBet)
    {
        if (blackjackBet <= (GetCurrentPlayer(Player.PlayerPosition).PlayerInitialBet / 2) && blackjackBet <= GetCurrentPlayer(Player.PlayerPosition).PlayerTotalCoinsInGame)
        {
            GetCurrentPlayer(Player.PlayerPosition).SetBlackjackBet(blackjackBet);
            PlayersFields.PlayersBlackjackBet[Player.PlayerPosition].text = "BJ Bet: " + blackjackBet;
            GetCurrentPlayer(Player.PlayerPosition).PlayerTotalCoinsInGame -= blackjackBet;
            PlayersFields.PlayersCoins[Player.PlayerPosition].text = GetCurrentPlayer(Player.PlayerPosition).PlayerTotalCoinsInGame.ToString();
            this.blackjackBetRight.Invoke();
            this.deactivateButtons.Invoke();
            DetermineButtons();
            this.blackjackBetReady.Invoke(blackjackBet);
        }
    }

    //Makes the casino play
    public void CasinoPlay(List<string> casinoExtraCards)
    {
        PlayerInGame casinoPlayer = new PlayerInGame("Dealer", 0, -1, false);
        casinoPlayer.AddCardToPlayer(cardsDealed[0]);
        casinoPlayer.AddCardToPlayer(cardsDealed[1]);
        playersInGame.Add(casinoPlayer);
        PlayersFields.TurnText.text = "Casino Turn";
        //Turns the hidden card, so everyone can see it
        Vector3 newRotation = new Vector3(180, 0, 0);
        initialCardsToDeal[0].transform.Rotate(newRotation * 5f);
        UpdatePlayersHandsCount(true);
        if (casinoExtraCards == null)
        {
            DetermineWinners();
        }
        //If the casino ask for extra cards
        else
        {
            StartCoroutine(PutCasinoExtraCards(casinoExtraCards));
        }
    }

    //Puts the casino extra cards in the table
    public IEnumerator PutCasinoExtraCards(List<string> casinoExtraCards)
    {
        foreach (string card in casinoExtraCards)
        {
            PutExtraCard(-1, card, false);
            UpdatePlayersHandsCount(true);
            yield return new WaitForSeconds(2);
        }
        DetermineWinners();
    }

    //Determines the winners and losers of this match
    public void DetermineWinners()
    {
        List<int> playersGain = new List<int>
        {
            ComparePunctuations(0),
            ComparePunctuations(1),
            ComparePunctuations(2)
        };

        List<int> playersExtraGain = new List<int>
        {
             CheckForBlackjackBet(0),
             CheckForBlackjackBet(1),
             CheckForBlackjackBet(2)
        };
        int i = 0;
        foreach (int gain in playersGain)
        {
            //the player lost
            if (gain <= 0)
            {
                PlayersFields.PlayersBets[i].text = "Loser: " + gain.ToString();
            }
            //the player tied
            else if (gain == GetCurrentPlayer(i).PlayerInitialBet)
            {
                PlayersFields.PlayersBets[i].text = "Tie: + " + GetCurrentPlayer(i).PlayerInitialBet.ToString();
                //if the current player is local we need to update the text in the table and his information in PlayerInGame
                if (GetCurrentPlayer(i).IsLocal)
                {
                    PlayersFields.PlayersCoins[i].text = (GetCurrentPlayer(i).PlayerTotalCoinsInGame + GetCurrentPlayer(i).PlayerInitialBet).ToString();
                    //add also the extra gain (if the player bet for blackjack to dealer and won)
                    GetCurrentPlayer(i).PlayerTotalCoinsInGame += GetCurrentPlayer(i).PlayerInitialBet + (playersExtraGain[i] > 0 ? playersExtraGain[i] : 0);
                }
            }
            //the player won
            else
            {
                PlayersFields.PlayersBets[i].text = "Winner: + " + gain.ToString();
                if (GetCurrentPlayer(i).IsLocal)
                {
                    PlayersFields.PlayersCoins[i].text = (GetCurrentPlayer(i).PlayerTotalCoinsInGame + gain).ToString();
                    //add also the extra gain (if the player bet for blackjack to dealer and won)
                    GetCurrentPlayer(i).PlayerTotalCoinsInGame += gain + (playersExtraGain[i] > 0 ? playersExtraGain[i] : 0);
                }
            }
            i++;
        }
        GetCurrentPlayer(Player.PlayerPosition).PlayerTotalCoinsInGame += playersExtraGain[Player.PlayerPosition];
        PlayersFields.PlayersCoins[Player.PlayerPosition].text = GetCurrentPlayer(Player.PlayerPosition).PlayerTotalCoinsInGame.ToString();
    }

    private PlayerInGame GetCurrentPlayer(int player)
    {
        PlayerInGame tmp = new PlayerInGame();
        foreach (PlayerInGame tmpPlayer in playersInGame)
        {
            if (tmpPlayer.PlayerPosition == player)
            {
                tmp = tmpPlayer;
            }
        }
        return tmp;
    }

    /*
     * Compares the punctuations between the casino and one player
     * Returns the gain of the player
     */
    private int ComparePunctuations(int player)
    {
        int casinoPunctuation = GetPlayerPunctuation(-1);
        int playerPunctuation = GetPlayerPunctuation(player);
        int playerBet = GetPlayerBet(player);
        int gain = 0;
        PlayerInGame currentPlayer = GetCurrentPlayer(player);
        PlayerInGame casinoPlayer = GetCurrentPlayer(-1);
        
        //when the casino has a hand count greater than 21
        if (casinoPunctuation > 21)
        {
            if (playerPunctuation > 21)
            {
                gain += playerBet;
                //this.playersBets[player].text = "Tie: + " + playerBet.ToString();
            }
            //the player has a Blackjack
            else if (playerPunctuation == 21 && currentPlayer.PlayerCards.Count == 2)
            {
                playerBet = playerBet + ((playerBet / 2) * 3);
                gain += playerBet;
                //this.playersBets[player].text = "Winner: + " + playerBet.ToString();
            }
            //won but without blackjack 
            else
            {
                playerBet = playerBet * 2;
                gain += playerBet;
                //this.playersBets[player].text = "Winner: + " + playerBet.ToString();
            }
        }
        //when the casino has a hand count less than 21
        else
        {
            //the player has a hand count greater than 21 it gain is 0
            if(playerPunctuation > 21)
            {
                gain -= playerBet;
            }
            //the player and the casino have the same hand count
            else if (playerPunctuation == casinoPunctuation)
            {
                //the player won with blackjack
                if ((playerPunctuation == 21 && currentPlayer.PlayerCards.Count == 2) && (casinoPlayer.PlayerCards.Count > 2))
                {
                    playerBet = playerBet + ((playerBet / 2) * 3);
                    gain += playerBet;
                    //this.playersBets[player].text = "Winner: + " + playerBet.ToString();
                }
                //the player lost
                else if ((playerPunctuation == 21 && currentPlayer.PlayerCards.Count > 2) && (casinoPlayer.PlayerCards.Count == 2))
                {
                    gain -= playerBet;
                }
                //the casino and player tied with the same hand count
                else
                {
                    gain += playerBet;
                    //this.playersBets[player].text = "Tie: + " + playerBet.ToString();
                }
            }
            //player hand count is greater than casino's hand count
            else if (playerPunctuation > casinoPunctuation)
            {
                //player won with Blackjack
                if (playerPunctuation == 21 && currentPlayer.PlayerCards.Count == 2)
                {
                    playerBet = playerBet + ((playerBet / 2) * 3);
                    gain += playerBet;
                    //this.playersBets[player].text = "Winner: + " + playerBet.ToString();
                }
                //player won normal
                else
                {
                    playerBet = playerBet * 2;
                    gain += playerBet;
                    //this.playersBets[player].text = "Winner: + " + playerBet.ToString();
                }
            }
            //player lost
            else if (playerPunctuation < casinoPunctuation)
            {
                gain -= playerBet;
                //this.playersBets[player].text = "Loser: - " + playerBet.ToString();
            }
        }
        return gain;
    }

    //This function search a player initial bet from the table
    private int GetPlayerBet(int player)
    {
        int finalBet = 0;
        //search the player in the array with the player position that recives the method and returns his total bet
        foreach (PlayerInGame tmpPlayer in playersInGame)
        {
            if (tmpPlayer.PlayerPosition == player)
            {
                finalBet = tmpPlayer.PlayerInitialBet;
            }
        }
        return finalBet;
    }

    /*
     *checks for the players that made a secondary bet (if the casio has blackjack)
     *returns the total gain of the player
     */
    private int CheckForBlackjackBet(int player)
    {
        int gain = 0;
        //get the bet for blackjack of the current player
        int playerBlackjackBet = GetCurrentPlayer(player).PlayerBlackjackBet;
        if (playerBlackjackBet != 0)
        {
            //the casino has a Blackjack
            if (GetCurrentPlayer(-1).GetPlayerHandCount() == 21 && GetCurrentPlayer(-1).PlayerCards.Count == 2)
            {
                //(bet * 2) + bet
                playerBlackjackBet = playerBlackjackBet * 3;
                gain += playerBlackjackBet;
                PlayersFields.PlayersBlackjackBet[player].text = "BJ Winner: +" + playerBlackjackBet;
            }
            else
            {
                gain -= playerBlackjackBet;
                PlayersFields.PlayersBlackjackBet[player].text = "BJ Loser: -" + playerBlackjackBet;
            }
        }
        return gain;
    }

    //Returns the sum of every card of the player
    private int GetPlayerPunctuation(int player)
    {
        int total = 0;
        //search for the player in the array and puts his cards punctuation into the total variable and get the logical cards that he has
        foreach (PlayerInGame tmpPlayer in playersInGame)
        {
            if (tmpPlayer.PlayerPosition == player)
            {
                total = tmpPlayer.GetPlayerHandCount();
            }
        }
        return total;
    }

    //Updates every player hand count
    private void UpdatePlayersHandsCount(bool casinoHand)
    {
        for (int i = 0; i < 3; i++)
        {
            PlayersFields.PlayersHandCount[i].text = "Hand Count: " + GetCurrentPlayer(i).GetPlayerHandCount().ToString();
        }
        if (casinoHand)
        {
            PlayersFields.CasinoHand.text = "Casino Count: " + GetCurrentPlayer(-1).GetPlayerHandCount().ToString();
        }
    }

    //Waits one second and then, change turn
    private IEnumerator WaitForTurnChange()
    {
        yield return new WaitForSeconds(3);
        this.deactivateButtons.Invoke();
        this.passTurn.Invoke();
    }

    /*
     * Ponga esta picha en otro lado playo 
     */
    public void RecieveRemoteBlackjackBet(int player, string blackjackBet)
    {
        PlayersFields.PlayersBlackjackBet[player].text = "BJ Bet: " + blackjackBet;
        GetCurrentPlayer(player).SetBlackjackBet(int.Parse(blackjackBet));
        this.putRemoteCoins.Invoke(player, int.Parse(blackjackBet), "blackjack");
    }
}