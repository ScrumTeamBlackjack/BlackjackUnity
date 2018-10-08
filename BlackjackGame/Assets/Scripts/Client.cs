using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;

//This event triggers a function in the class Game, to put one player in the table
[System.Serializable]
public class PutPlayerEvent : UnityEvent<int, string>
{
}

//This event triggers a function in the class Game, to put one initial bet from another player
[System.Serializable]
public class PutRemoteBetEvent : UnityEvent<int, string>
{
}

//This event triggers a function in the class Game, every initial card for every player
[System.Serializable]
public class PutInitialCards : UnityEvent<List<string>>
{
}

//This event triggers a function in the class Game, that changes the current turn
[System.Serializable]
public class TurnChange : UnityEvent<string, int>
{
}

//This event triggers a function in the class Game, to receive one card to one player
[System.Serializable]
public class RecieveCard : UnityEvent<int, string, bool>
{
}

//This event triggers a function in the class Game, to receive one card to one player and doubles his bet
[System.Serializable]
public class DoubleBet : UnityEvent<int, string>
{
}

//This event triggers a function in the class Game, to put the casino play in the table
[System.Serializable]
public class CasinoPlay : UnityEvent<List<string>>
{
}

//This event triggers a function in the class Game, to put one blackjack bet from another player
[System.Serializable]
public class PutRemoteBlackjackBetEvent : UnityEvent<int, string>
{
}

public class Client : MonoBehaviour
{
    //private fields
    private bool socketReady;
    private TcpClient socket;
    private NetworkStream stream;
    private StreamWriter writer;
    private StreamReader reader;
    private int positionInTable;

    //public fields
    public PutPlayerEvent putPlayer;
    public UnityEvent startGame;
    public PutRemoteBetEvent putRemoteBet;
    public PutInitialCards putInitialCards;
    public TurnChange turnChange;
    public RecieveCard recieveCard;
    public DoubleBet doubleBetEvent;
    public CasinoPlay casinoPlayEvent;
    public PutRemoteBlackjackBetEvent putRemoteBlackjackBet;

    public void connectToServer()
    {
        //if already connected, ignore this function
        if (socketReady)
        {
            return;
        }

        //Default host / port
        string host = "127.0.0.1";
        int port = 6321;

        //Create the socket
        try
        {
            this.socket = new TcpClient(host, port);
            this.stream = socket.GetStream();
            this.writer = new StreamWriter(stream);
            this.reader = new StreamReader(stream);

            this.socketReady = true;
        }
        catch (Exception e)
        {
            print(e.Message); 
        }
    }

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        connectToServer();
        //Check for messages fom the server
        if (this.socketReady)
        {
            if (this.stream.DataAvailable)
            {
                onIncomingData();
            }
        }
    }

    //When the server sends a message
    private void onIncomingData()
    {
        string typeOfData = this.reader.ReadLine();
        if (typeOfData == null)
        {
            return;
        }

        //The server sends the position of the current client, and any other client connected to the server, and also their names
        else if (typeOfData == "0")
        {
            putInTable();
            List<string> data = new List<string>();
            data.Add("0");
            data.Add(Player.Nickname);
            sendMessage(data);

        //The server sends the name and the position of a new client connected
        }
        else if (typeOfData == "1")
        {
            int position = int.Parse(reader.ReadLine());
            string name = reader.ReadLine();
            this.putPlayer.Invoke(position, name);
        }

        //The server advise the start of the game
        else if (typeOfData == "2")
        {
            this.startGame.Invoke();
        }

        //A remote player puts his initial bet value
        else if (typeOfData == "3")
        {
            int position = int.Parse(reader.ReadLine());
            string bet = reader.ReadLine();
            this.putRemoteBet.Invoke(position, bet);
        }

        //Recieve the cards to deal
        else if (typeOfData == "4")
        {
            recieveCards();
        }

        //Recieves the player whose turn starts
        else if (typeOfData == "5")
        {
            string name = this.reader.ReadLine();
            this.turnChange.Invoke(name, 0);
        }

        //Recieves an extra card for one player
        else if (typeOfData == "6")
        {
            string player = reader.ReadLine();
            string card = reader.ReadLine();
            this.recieveCard.Invoke(int.Parse(player), card, true);
        }

        //Recieves an extra card for one player, doubles his bet and pass the turn
        else if (typeOfData == "7")
        {
            string player = reader.ReadLine();
            string card = reader.ReadLine();
            this.doubleBetEvent.Invoke(int.Parse(player), card);
        }

        //Recieves a remote client blakjack bet
        else if (typeOfData == "8")
        {
            int position = int.Parse(reader.ReadLine());
            string bet = reader.ReadLine();
            this.putRemoteBlackjackBet.Invoke(position, bet);
        }

        //The casino sends his play
        else if (typeOfData == "9")
        {
            string extraCards = this.reader.ReadLine();

            //When the casino didn´t ask for extra cards
            if (extraCards == "0")
            {
                this.casinoPlayEvent.Invoke(null);
            }

            //When the casino did ask for extra cards
            else
            {
                List<string> casinoExtraCards = new List<string>();
                int extraCardsAmount = int.Parse(extraCards);
                for (int i = 0; i < extraCardsAmount; i++)
                {
                    string card = this.reader.ReadLine();
                    casinoExtraCards.Add(card);
                }
                this.casinoPlayEvent.Invoke(casinoExtraCards);
            }
        }
    }

    /*
     * Puts the name and the coins of the player in the table
     * Also puts the names of the other players in the table if this client is not the first/
     */
    private void putInTable()
    {
        this.positionInTable = int.Parse(this.reader.ReadLine());
        //puts this client in the table
        this.putPlayer.Invoke(this.positionInTable, null);
        int i = 0;
        while (i < this.positionInTable)
        {
            //Puts the clients that are already registered
            string remotePlayer = this.reader.ReadLine();
            this.putPlayer.Invoke(i, remotePlayer);
            i++;
        }
    }

    //When this client's bet is ready, it is send to the server
    public void betReady(int initialBet)
    {
        List<string> data = new List<string>();
        data.Add("1");
        data.Add(this.positionInTable.ToString());
        data.Add(initialBet.ToString());
        sendMessage(data);
    }

    //The server sends the initial cards to every client
    private void recieveCards()
    {
        List<string> data = new List<string>();
        for (int i = 0; i < 8; i++)
        {
            data.Add(this.reader.ReadLine());
        }
        this.putInitialCards.Invoke(data);
    }

    //Tells the server to pass the current turn
    public void passTurn()
    {
        List<string> data = new List<string>();
        data.Add("2");
        sendMessage(data);
    }

    //Asks the server for a new card
    public void askForACard()
    {
        List<string> data = new List<string>();
        data.Add("3");
        data.Add(this.positionInTable.ToString());
        sendMessage(data);
    }

    //Asks the server for a new card and to double the bet
    public void doubleBet()
    {
        List<string> data = new List<string>();
        data.Add("4");
        data.Add(this.positionInTable.ToString());
        sendMessage(data);
    }

    public void blackjackBetReady(int blackjackBet)
    {
        List<string> data = new List<string>();
        data.Add("5");
        data.Add(this.positionInTable.ToString());
        data.Add(blackjackBet.ToString());
        sendMessage(data);
    }

    //Sends a message to the server
    private void sendMessage(List<string> data)
    {
        if (!this.socketReady)
        {
            return;
        }

        foreach (string d in data)
        {
            this.writer.WriteLine(d);
        }
        this.writer.Flush();

    }

    //Close and deletes the socket
    private void closeSocket()
    {
        if (!this.socketReady)
        {
            return;
        }

        this.writer.Close();
        this.reader.Close();
        this.socket.Close();
        this.socketReady = false;
    }

    private void OnApplicationQuit()
    {
        closeSocket();
    }

    private void OnDisable()
    {
        closeSocket();
    }
}
