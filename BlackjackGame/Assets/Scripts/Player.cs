using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;

public static class Player
{
    //Player fields
    public static int Id { get; set; }
    public static string Nickname { get; set; }
    public static int CoinsAmount { get; set; }
    public static int GamesWon { get; set; }
    public static int CoinsInGame
    {
        get
        {
            return s_gameCoins;
        }
        set
        {
            s_gameCoins = value;
            CoinsAmount -= CoinsInGame;
        }
    }
    public static string NicknameToConsult { get; set; }
    public static int PlayerPosition { get; set; }

    //server fields
    private static readonly string securityPassword = "BlackjackTEAM3";
    private static readonly string facadeServer = "http://localhost/DatabaseBlackjack/php/FacadeBlackjack.php";
    
    //the response of the server consult, this field is used in the session screen to know the status of the consult
    public static string playerResponse;
    private static int s_gameCoins;
    

    /*
     * Load the player information from the server, this method make a request to the server and get the user 
     * information from the database also pass the result to a handler method
     */
    public static IEnumerator LoadPlayerInformation()
    {
        //make the form data that we'll send to the server
        WWWForm data = new WWWForm();
        data.AddField("action", "getData");
        data.AddField("nickname", WWW.EscapeURL(NicknameToConsult));
        data.AddField("authenticatePassword", securityPassword);

        //make the request
        WWW informationRequest = new WWW(facadeServer, data);
        yield return informationRequest;

        //verify if the request has an error
        if (!String.IsNullOrEmpty(informationRequest.error))
        {
            //there was a problem getting the information from the database
            playerResponse = "loadPlayerError";
        }
        else
        {
            //pass the response to handler method that make the corresponding validation
            string serverResponse = informationRequest.text;
            if (!String.IsNullOrEmpty(serverResponse.Trim()))
            {
                HandlerServerResponse(serverResponse);
            }
        }
    }

    /*
     * Handle the response from the server, any error or data result, in case of error it puts the error into the 
     * playerResponse field (to notify to session class)
     */
    private static void HandlerServerResponse(string serverResponse)
    {
        if (serverResponse == "nicknameError01")
        {
            playerResponse = "nicknamePlayerError";
        }
        else if (serverResponse == "securityError" || serverResponse == "serverError")
        {
            playerResponse = "serverError";
        }
        else
        {
            SavePlayerData(serverResponse);
        }
    }

    //classes that are used like a model to store the data from the server (JSON)
    [Serializable]
    public class PlayerModel
    {
        public List<Model> user;
    }

    [Serializable]
    public class Model
    {
        //Model fields
        public int user_id;
        public string nickname;
        public int amount_chips;
        public int amount_games_won;
    }

    /*
     * Convert the string response (JSON string) to a JSON object (like a PlayerModel class)
     * Then, put the corresponding informantion of the user into the fields of the Player class
     */
    private static void SavePlayerData(string serverResponse)
    {
        //create the JSON object like a PlayerModel class
        PlayerModel responseJSON = JsonUtility.FromJson<PlayerModel>(serverResponse);
        //save the information into the Player fields
        Id = responseJSON.user[0].user_id;
        Nickname = responseJSON.user[0].nickname;
        CoinsAmount = responseJSON.user[0].amount_chips;
        GamesWon = responseJSON.user[0].amount_games_won;
        
        //notify to the session that all are completed.
        playerResponse = "loadPlayerOK";
    }
}
