
public class Card
{
    public int Value { get; set; }
    public string Symbol { get; set; }
    public string Suit { get; set; }
    public string ImagePath { get; set; }

    public Card(int pValue, string pSymbol, string pSuit)
    {
        this.Value = pValue;
        this.Symbol = pSymbol;
        this.Suit = pSuit;
    }

    //constructor to deserialize from a logical card
    public Card(string cardToDeserilze)
    {
        string[] atrrCard = cardToDeserilze.Split(',');
        this.Value = int.Parse(atrrCard[0]);
        this.Symbol = atrrCard[1];
        this.Suit = atrrCard[2];
        this.ImagePath = atrrCard[3];
    }

    public Card(int pValue, string pSuit)
    {
        this.Value = pValue;
        this.Symbol = "None";
        this.Suit = pSuit;
    }

    public void ChangeValue(int pValue) // to change the Ace value
    {
        this.Value = pValue;
    }

    public string GetCardName()
    {
        string res = "";
        switch (this.Symbol)
        {
            case "None": //Is not an Ace, then
                res = this.Value + " of " + this.Suit;
                break;
            default: // is an Ace
                res = this.Symbol + " of " + this.Suit;
                break;
        }
        return res;
    }

    /*
     * It takes all the properties of the class and returns it like a string separate with a comma
     */
    public string SerializeCardToString()
    {
        return this.Value.ToString() + "," + this.Symbol + "," + this.Suit + "," + this.ImagePath;
    }

}
