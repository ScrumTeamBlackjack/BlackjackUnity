using UnityEngine;
using System.Collections.Generic;

public class ObjectCard : MonoBehaviour
{
    private int value;
    private string symbol; // handles de Ace, Jack, Queen and King
    private string suit;
    private string imagePath;

    public void SetCard(Card pCard)
    {
        this.value = pCard.Value; // sets the value of the logical card to the object
        this.symbol = pCard.Symbol; // sets the symbol of the logical card to the object
        this.suit = pCard.Suit; // sets the suit of the logical card to the object
        this.imagePath = pCard.ImagePath;

        // set the correct texture to the child(0) of the prefab (front face of the card)
        this.transform.GetChild(0).GetComponent<MeshRenderer>().material.SetTexture("_MainTex", getTexture());
        this.transform.name = pCard.GetCardName();
    }

    // fetches the correct texture according to logical card values
    private Texture getTexture()
    {
        string cardImage = this.imagePath;
        // creates and returns the texture
        Texture cardTexture = Resources.Load("Textures/AllCards/" + cardImage, typeof(Texture)) as Texture; // get the texture from assets                                 
        return cardTexture;
    }

    void OnMouseDown() // handles interaction with the 3Dobject when is touched
    {
        // Print in console the values of the card when is touched
        switch (this.symbol)
        {
            case "None": //Is not an Ace, then
                print(this.value + " of " + this.suit + " Value: " + this.value);
                break;
            default: // is an Ace
                print(this.symbol + " of " + this.suit + " Value: " + this.value);
                break;
        }
    }
}
