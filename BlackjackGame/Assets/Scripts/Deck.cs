using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System;

public class Deck
{
    private Stack<Card> logicalDeck; // pile with the 52 cards
    //private Stack<ObjectCard> ObjecCardtDump; // pile that handles the Dump of cards 

    private List<Card> toShuffleDeck; // list to shuffle the deck, then add it to the pile (logicalDeck)    

    //private float size; // to handle the locaScale Y position (increase it), for the new card (prefab) created
    //private float thicknessOfACard; // variable to handle the thickness of the card 3Dobject

    /*
    public GameObject objectDeck; // 3DObject Deck
    public ObjectCard objectCard; // 3DObject Card    */


    // Use this for initialization
    public Deck()
    {
        // initates variables
        logicalDeck = new Stack<Card>();
        //ObjecCardtDump = new Stack<ObjectCard>();
        toShuffleDeck = new List<Card>();
        //size = 53f; // quantity of cards in deck plus one
        //thicknessOfACard = 0.00943f; // thikness of the prefab "card"
        FillDeck(); // starts a new filled and shuffled deck
    }

    void FillDeck()
    {
        string[] suit = { "diamonds", "clubs", "hearts", "spades" }; // cards suit
        string[] symbol = { "ace", "jack", "queen", "king" }; // cards symbol
        string path = ""; // name of the card, path of the image source

        logicalDeck = new Stack<Card>(); // restarts the deck        

        Card tempCard; // temporal card

        for (int i = 0; i < 4; i++) // iterates over card suit array
        {
            for (int j = 1; j <= 13; j++) // iterates over the total of cards BY SUIT
            {
                tempCard = new Card(j, suit[i]); // creates the card                                                        

                switch (j) // assigned the special symbol (ace, jack, queen or king)
                {
                    case 1:
                        tempCard.Symbol = symbol[0]; // symbol[0] == ace
                        break;
                    case 11:
                        tempCard.Symbol = symbol[1]; // symbol[1] == jack
                        break;
                    case 12:
                        tempCard.Symbol = symbol[2]; // symbol[2] == queen
                        break;
                    case 13:
                        tempCard.Symbol = symbol[3]; // symbol[3] == king
                        break;
                    default:
                        break;
                }

                if (tempCard.Value >= 2 && tempCard.Value <= 13)
                {  // handles the "2-13 _of_suit"
                    path = tempCard.Value.ToString() + "_of_" + tempCard.Suit.ToString();
                }
                else // handles the "ace_of_suit"
                {
                    path = tempCard.Symbol.ToString() + "_of_" + tempCard.Suit.ToString();
                }

                tempCard.ImagePath = path; // set the path to the card 

                if (j >= 11) // handles the special black jack deck, for the jack, queen and king the value is 10
                {
                    tempCard.ChangeValue(10);
                }
                if (j == 1)
                {
                    tempCard.ChangeValue(11);
                }

                toShuffleDeck.Add(tempCard); // adds the card to a List, in order to shuffle the List       
            }
        }

        int index = 0;
        while (toShuffleDeck.Count > 0) // shuffle the List and adds the cards to the stack (logicalDeck)
        {
            index = UnityEngine.Random.Range(0, toShuffleDeck.Count); // random index ~ total of the elements in the list
            logicalDeck.Push(toShuffleDeck[index]); // picks a random card from the List and Pushes the card into the logicaDeck
            toShuffleDeck.RemoveAt(index); // removes the card from the shuffled list
        }
        // variable (Stack) 'logicalDeck' is now filled and shuffled
    }

    public Card TakeACard() // pops a card from the Deck and displays it on the Dump
    {
        return this.logicalDeck.Pop(); // gets a card from the Deck        
        /*ObjectCard tempCard;

        float rotationY = UnityEngine.Random.Range(75f, 110f); // random 'rotation Y' parameter, "natural" drop of the card

        // 'position Y' that handles the position of each card in the Dump (increasing height)
        Vector3 positionY = new Vector3(1.5f, (this.size - this.logicalDeck.Count) * this.thicknessOfACard, 0);
        Quaternion rotation = Quaternion.Euler(180, rotationY, 0); // 'rotation X' or 'rotation Z' "flips" the card to see the front face

        GameObject newCard = (GameObject)Instantiate(Resources.Load("Prefabs/Card"), positionY, rotation);
        tempCard = newCard.GetComponent<ObjectCard>(); // generates the Card prefab

        // set the logicalCard values to the prefab object   
        tempCard.gameObject.GetComponent<ObjectCard>().SetCard(logicalCard);

        ObjecCardtDump.Push(tempCard); // Stack of prefab Card objects (Dump)*/

    }

    /*void animationTakeACard(float thicknessOfACard) // animation of taking a card from the deck (reduces the Scale Y of the deck)
    {
        objectDeck.transform.localScale = (objectDeck.transform.localScale - new Vector3(0, thicknessOfACard, 0));
    }*/


    /*void OnMouseDown() // handles interaction with the 3Dobject Deck when is touched
    {
        if (logicalDeck.Count > 0) // check the size of the logical deck
        {
            takeACard(); // method that handles the logical and 3DObject Deck
            animationTakeACard(this.thicknessOfACard); // animation that decreases the height of the 3Dobject Deck             
            print(logicalDeck.Count);
        }
        else
        {
            print("Deck is empty");
        }
    }*/
}