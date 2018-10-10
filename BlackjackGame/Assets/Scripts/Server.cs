using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

public class Server : MonoBehaviour
{
    //Private fields
    private List<Observer> observers;
    private List<Observer> disconnectList;
    private TcpListener server;
    private bool serverStarted;
    private int port = 6321;
    private int playersReady = 0;
    private Deck deck;
    private int playerInTurn = 10;
    private List<Card> casinoCards;

    // Use this for initialization
    void Start()
    {
        deck = new Deck();
        casinoCards = new List<Card>();
        this.observers = new List<Observer>();
        this.disconnectList = new List<Observer>();

        try
        {
            //Creates a new server
            this.server = new TcpListener(IPAddress.Any, port);
            this.server.Start();
            this.server.BeginAcceptTcpClient(acceptTcpClient, server);
            this.serverStarted = true;
        }
        catch (Exception e)
        {
            Debug.Log("Socket Error: " + e.Message);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!this.serverStarted)
        {
            return;
        }

        foreach (Observer o in this.observers)
        {
            //is the observer still connected?
            if (!isConnected(o.getTcp()))
            {
                o.getTcp().Close();
                disconnectList.Add(o);
                continue;
            }
            // Check for message from the client
            else
            {
                NetworkStream s = o.getTcp().GetStream();
                if (s.DataAvailable)
                {
                    StreamReader reader = new StreamReader(s, true);
                    onIncomingData(reader);

                }
            }
        }
        //Deletes a client when it disconnects
        for (int i = 0; i < this.disconnectList.Count - 1; i++)
        {
            this.observers.Remove(disconnectList[i]);
            this.disconnectList.RemoveAt(i);
        }
    }

    //When one client send a message
    private void onIncomingData(StreamReader reader)
    {
        string typeOfData = reader.ReadLine();
        if (typeOfData == null)
        {
            return;
        }
        //When a new player connects and confirms that his table is ready
        if (typeOfData == "0")
        {
            StartCoroutine(notifyNewClientToConnectedClients(reader));
        }

        //When a player makes his initial bet,the server broadcasts that bet to the other players
        else if (typeOfData == "1")
        {
            this.playersReady = this.playersReady + 1;
            List<string> data = new List<string>();
            data.Add("3");
            string position = reader.ReadLine();
            data.Add(position);
            string betValue = reader.ReadLine();
            data.Add(betValue);
            broadcast(data, this.observers[int.Parse(position)].getName());
            if (this.playersReady == 3)
            {
                StartCoroutine(distributeCards());
            }
        }

        //When a player pass the turn to the next client
        else if (typeOfData == "2")
        {
            turn();
        }

        //A player asks for an extra card
        else if (typeOfData == "3")
        {
            string player = reader.ReadLine();
            giveExtraCard(player, "6");
        }

        //A player asks for an extra card and double the bet
        else if (typeOfData == "4")
        {
            string player = reader.ReadLine();
            giveExtraCard(player, "7");
            StartCoroutine(waitForTurnChange());
        }

        //A player sends his blackjack bet 
        else if(typeOfData == "5")
        {
            List<string> data = new List<string>();
            data.Add("8");
            string position = reader.ReadLine();
            data.Add(position);
            string blackjackBetValue = reader.ReadLine();
            data.Add(blackjackBetValue);
            broadcast(data, this.observers[int.Parse(position)].getName());
        }
    }

    private void acceptTcpClient(IAsyncResult ar)
    {
        //Only accept 3 clients
        if (this.observers.Count < 3)
        {
            TcpListener listener = (TcpListener)ar.AsyncState;
            this.observers.Add(new Observer(listener.EndAcceptTcpClient(ar), this.observers.Count));
            this.server.BeginAcceptTcpClient(acceptTcpClient, server);
            notifyNewClient();
        }
        else
        {
            print("The table already has 3 players");
        }
    }

    //When a new client connects, the server sends its position and the name of the other clients connected
    private void notifyNewClient()
    {
        StreamWriter writer = new StreamWriter(this.observers[this.observers.Count - 1].getTcp().GetStream());
        writer.WriteLine(0);
        writer.WriteLine(this.observers[this.observers.Count - 1].getObserverNumber());
        int i = 0;
        while (i < (this.observers.Count - 1))
        {
            writer.WriteLine(this.observers[i].getName());
            i++;
        }
        writer.Flush();
    }

    //When a new client connects, the server sends his name to the clients that were already connected, so they can put him in their tables
    private IEnumerator notifyNewClientToConnectedClients(StreamReader reader)
    {
        string newName = reader.ReadLine();
        this.observers[this.observers.Count - 1].setName(newName);
        List<string> data = new List<string>();
        data.Add("1");
        data.Add(this.observers[this.observers.Count - 1].getObserverNumber().ToString());
        data.Add(this.observers[this.observers.Count - 1].getName());
        broadcast(data, newName);

        yield return new WaitForSeconds(1);
        //If there is already 3 players on the table, the game begins
        if (this.observers.Count == 3)
        {
            startGame();
        }
    }

    //Tells the clients that the game begins, and the initial bets appears
    private void startGame()
    {
        List<string> data = new List<string>();
        data.Add("2");
        broadcast(data, null);
    }

    //Sends every initial cards to every client 
    private IEnumerator distributeCards()
    {
        yield return new WaitForSeconds(1);
        List<string> data = new List<string>();
        //4 = initial cards
        data.Add("4");
        //to put 8 cards into a list and send it to the clients
        for (int i = 0; i < 8; i++)
        {
            //validate if the cards are for the casino
            if (i == 0 || i == 1)
            {
                Card tmpCard = deck.TakeACard();
                data.Add(tmpCard.SerializeCardToString());
                //save it into a separate array (it's 'cause the casino plays in the server)
                casinoCards.Add(tmpCard);
            }
            else
            {
                data.Add(deck.TakeACard().SerializeCardToString());
            }

        }
        broadcast(data, null);
        yield return new WaitForSeconds(5);
        turn();
    }

    //Changes the turns between all the players
    private void turn()
    {
        List<string> data = new List<string>();
        data.Add("5");
        bool playerTurn = true;
        if (this.playerInTurn == 10)
        {
            data.Add(this.observers[0].getName());
            this.playerInTurn = 0;
        }
        else if (this.playerInTurn == 0)
        {
            data.Add(this.observers[1].getName());
            this.playerInTurn = 1;
        }
        else if (this.playerInTurn == 1)
        {
            data.Add(this.observers[2].getName());
            this.playerInTurn = 2;
        }
        else if (this.playerInTurn == 2)
        {
            this.playerInTurn = 10;
            playerTurn = false;
            casinoTurn();
        }
        if (playerTurn)
        {
            broadcast(data, null);
        }
    }

    //Gives an extra card to one player
    //the parameter type is for when it is just to add a new card or to double the bet
    private void giveExtraCard(string player, string type)
    {
        List<string> data = new List<string>();
        data.Add(type);
        data.Add(player);
        data.Add(deck.TakeACard().SerializeCardToString());
        broadcast(data, null);
    }

    //Waits 5 seconds to change the turn
    private IEnumerator waitForTurnChange()
    {
        yield return new WaitForSeconds(3);
        turn();
    }

    //The casino plays his hand
    private void casinoTurn()
    {
        int casinoCard1 = casinoCards[0].Value;
        int casinoCard2 = casinoCards[1].Value;
        int count = casinoCard1 + casinoCard2;
        List<string> data = new List<string>();
        //9 = it means the action is for the casino's turn
        data.Add("9");
        //if the casino's hand is greater than 16, the casino's turn ends
        if (count > 16)
        {
            //0 = ask 0 cards
            data.Add("0");
            broadcast(data, null);
        }
        else
        {
            //it saves the current cards of the casino and the extra cards
            List<string> casinoTotalsCards = new List<string>();
            casinoTotalsCards.Add(casinoCards[0].SerializeCardToString());
            casinoTotalsCards.Add(casinoCards[1].SerializeCardToString());
            //this is for put the information in the message, like to say: casino takes 3 cards
            int extraCardAmount = 0;
            //to save the position if the casino's hand is greater than 21
            int i = 0;
            while (count < 17)
            {
                Card tmpCard = deck.TakeACard();
                int newCard = tmpCard.Value;
                casinoTotalsCards.Add(tmpCard.SerializeCardToString());
                count += newCard;
                extraCardAmount++;

                if (count > 21)
                {
                    while (i < casinoTotalsCards.Count)
                    {
                        if (getLogicalCardValue(casinoTotalsCards[i]) == 11)
                        {
                            count -= 10;
                            i++;
                            break;
                        }
                        i++;
                    }
                }
            }
            data.Add(extraCardAmount.ToString());
            for (int j = 2; j < casinoTotalsCards.Count; j++)
            {
                data.Add(casinoTotalsCards[j]);
            }
            broadcast(data, null);
        }
    }

    //Returns the value of a logical card
    //Note: The A always return 11, not 1
    public int getLogicalCardValue(string logicalCard)
    {
        Card tmpLogicalCard = new Card(logicalCard);
        return tmpLogicalCard.Value;
    }

    //Transmit a message with "data" to every client except "client"
    private void broadcast(List<string> data, string client)
    {
        foreach (Observer o in this.observers)
        {
            if (o.getName() == client)
            {
                continue;
            }
            try
            {
                StreamWriter writer = new StreamWriter(o.getTcp().GetStream());
                foreach (string d in data)
                {
                    writer.WriteLine(d);
                }
                writer.Flush();
            }
            catch (Exception e)
            {
                Debug.Log("Write error: " + e.Message + " to client " + o.getObserverNumber().ToString());
            }
        }
    }

    //Determines if the client c is still connected to the server
    private bool isConnected(TcpClient c)
    {
        try
        {
            if (c != null && c.Client != null && c.Client.Connected)
            {
                if (c.Client.Poll(0, SelectMode.SelectRead))
                {
                    return !(c.Client.Receive(new byte[1], SocketFlags.Peek) == 0);
                }
                return true;
            }
            else
            {
                return false;
            }
        }
        catch
        {
            return false;
        }
    }
}

//Stores data of the observers
public class Observer
{
    // Private fields
    private TcpClient tcp;
    private int observerNumber;
    private string name;

    public Observer(TcpClient observerSocket, int number)
    {
        this.observerNumber = number;
        this.tcp = observerSocket;
        this.name = "";
    }

    public int getObserverNumber()
    {
        return this.observerNumber;
    }

    public TcpClient getTcp()
    {
        return this.tcp;
    }

    public string getName()
    {
        return this.name;
    }

    public void setName(string newName)
    {
        this.name = newName;
    }
}