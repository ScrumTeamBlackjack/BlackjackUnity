using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class Login : MonoBehaviour {
    //public fields
    public GameObject nickname;
    public GameObject password;
    public GameObject loginButton;
    public GameObject returnButton;
    public RectTransform userPanel;
    public RectTransform mainMenuPanel;
    public Text errorText;

    //private fields
    private string stringNickname;
    private string stringPassword;
    private string securityPassword = "BlackjackTEAM3";
    private string facadeServer = "http://localhost/DatabaseBlackjack/php/FacadeBlackjack.php";

    // Use this for initialization
    void Start () {
        //Sets every button event on click
        this.loginButton.GetComponent<Button>().onClick.AddListener(CheckInput);
        this.returnButton.GetComponent<Button>().onClick.AddListener(ReturnToMainMenu);
    }

    //Checks that every input is valid
    private void CheckInput()
    {
        //make the corresponding validations to avoid problems like sql injection or another internal error in the web service side
        if (this.stringNickname == "" || this.stringPassword == "")
        {
            errorText.text = "All fields are required, you can not leave fields blank";
        }
        else if (this.stringNickname.Length > 10 || this.stringNickname.Length < 4)
        {
            errorText.text = "Incorrect nickname or password";
        }
        else if (ContainsInvalidCharacters(this.stringNickname))
        {
            errorText.text = "Check the nickname again, there are some invalid characters";
        }
        else if (this.stringPassword.Length < 8 || this.stringPassword.Length > 16)
        {
            errorText.text = "Incorrect nickname or password";
        }
        else if (ContainsInvalidCharacters(this.stringPassword))
        {
            errorText.text = "Check the password again, there are some invalid characters";
        }
        else
        {
            try
            {
                //start the consult to the server with the current params 
                StartCoroutine(ConsultUser(this.stringNickname, this.stringPassword, this.securityPassword));
            }
            catch (Exception e)
            {
                errorText.text = "Cannot connect to server: " + e.Message;
            }
        }
    }

    //It verify if a string have certain invalid characters, it returns true if found any invalid character or false if it doesn't found
    private bool ContainsInvalidCharacters(string stringToCheck)
    {
        string[] invalidCharacters = {"select", ";", "insert", "from", "where", "=", "/", "?", "\\", " "};
        bool found = false;
        for (int i = 0; i < invalidCharacters.Length; i++)
        {
            if (stringToCheck.Contains(invalidCharacters[i]))
            {
                found = true;
            }
        }
        return found;
    }

    /*
     * Make the consult to the server with the nickname and password, the server return a ronsponse (error or ok status)
     * In this case, the action is 'login' and we pass this param to the server with the form and also the security
     * password to authenticate as admin to the database 
     */
    private IEnumerator ConsultUser(string nicknameVal, string passwordVal, string authenticatePassword)
    {
        errorText.text = "Verifying the user";

        //create the form data that we'll pass to the server 
        WWWForm data = new WWWForm();
        data.AddField("action", "login");
        data.AddField("nickname", WWW.EscapeURL(nicknameVal));
        data.AddField("password", WWW.EscapeURL(passwordVal));
        data.AddField("authenticatePassword", authenticatePassword);

        //make the request to the server with the form data and the url
        WWW loginRequest = new WWW(this.facadeServer, data);
        yield return loginRequest;

        //verify if the request has an error
        if (!String.IsNullOrEmpty(loginRequest.error))
        {
            errorText.text = "There was a problem consulting the database";
        }
        else
        {
            //pass the response to handler method that make the corresponding validation
            string serverResponse = loginRequest.text;
            if (!String.IsNullOrEmpty(serverResponse.Trim()))
            {
                HandlerServerResponse(serverResponse, nicknameVal);
            }

        }
    }

    /*
     * Handle the response from the server, this method takes the corresponfing action to each response that the server
     * can give
     */
    private void HandlerServerResponse(string serverResponse, string nicknameVal)
    {
        if (serverResponse == "userError")
        {
            errorText.text = "Incorrect nickname or password";
        }
        else if (serverResponse == "nicknameError01")
        {
            errorText.text = "User are not register";
        }
        else if (serverResponse == "securityError" || serverResponse == "serverError")
        {
            errorText.text = "Server error: cannot authenticate the game ID";
        }
        else
        {
            //if every input is ok, opens a new session with every characteristic of the logged player
            Player.NicknameToConsult = nicknameVal;
            StartCoroutine(Player.LoadPlayerInformation());
            errorText.text = "";
            this.nickname.GetComponent<InputField>().text = "";
            this.password.GetComponent<InputField>().text = "";
            userPanel.SetAsLastSibling();
        }
    }

    //Cleans every space and returns to the main menu background
    private void ReturnToMainMenu()
    {
        this.errorText.text = "";
        this.nickname.GetComponent<InputField>().text = "";
        this.password.GetComponent<InputField>().text = "";
        this.mainMenuPanel.SetAsLastSibling();
    }

    // Update is called once per frame
    void Update () {
        this.stringNickname = this.nickname.GetComponent<InputField>().text;
        this.stringPassword = this.password.GetComponent<InputField>().text;
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (this.nickname.GetComponent<InputField>().isFocused)
            {
                this.password.GetComponent<InputField>().Select();
            }
            else if (this.password.GetComponent<InputField>().isFocused)
            {
                this.loginButton.GetComponent<Button>().Select();
            }
        }
    }
}
