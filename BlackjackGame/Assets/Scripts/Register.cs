using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

public class Register : MonoBehaviour {
    // public fields
    public GameObject nickname;
    public GameObject password;
    public GameObject registerButton;
    public GameObject returnButton;
    public RectTransform loginToRegisterPanel;
    public RectTransform mainMenuPanel;
    public Text errorText;

    // private fields
    private string stringNickname;
    private string stringPassword;
    private readonly string securityPassword = "BlackjackTEAM3";
    private readonly string facadeServer = "http://localhost/DatabaseBlackjack/php/FacadeBlackjack.php";

    // Use this for initialization
    void Start () {
        //Sets every button event on click
        this.registerButton.GetComponent<Button>().onClick.AddListener(CheckInput);
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
            errorText.text = "Nickname must have 4 to 10 characters";
        }
        else if (ContainsInvalidCharacters(this.stringNickname))
        {
            errorText.text = "Check the nickname again, there are some invalid characters";
        }
        else if (this.stringPassword.Length < 8 || this.stringPassword.Length > 16)
        {
            errorText.text = "Password must have 8 to 16 characters";
        }
        else if (ContainsInvalidCharacters(this.stringPassword))
        {
            errorText.text = "Check the password again, there are some invalid characters";
        }
        else
        {
            try
            {
                StartCoroutine(RegisterUser(this.stringNickname, this.stringPassword, this.securityPassword));
            }
            catch (Exception e)
            {

                errorText.text = "Cannot connect to server: " + e.Message;
            }
        }
    }

    private IEnumerator RegisterUser(string nicknameVal, string passwordVal, string authenticatePassword)
    {
        errorText.text = "Registering the user";

        //create the form data that we'll be pass to the server
        WWWForm data = new WWWForm();
        data.AddField("action", "register");
        data.AddField("nickname", WWW.EscapeURL(nicknameVal));
        data.AddField("password", WWW.EscapeURL(passwordVal));
        data.AddField("authenticatePassword", authenticatePassword);

        //make the request
        WWW registerRequest = new WWW(this.facadeServer, data);
        yield return registerRequest;

        //verify if the request has an error
        if (!String.IsNullOrEmpty(registerRequest.error))
        {
            errorText.text = "There was a problem registering in the database";
        }
        else
        {
            //pass the response to handler method that make the corresponding validation
            string serverResponse = registerRequest.text;
            if (!String.IsNullOrEmpty(serverResponse.Trim()))
            {
                HandlerServerResponse(serverResponse);
            }
        }
    }

    private void HandlerServerResponse(string serverResponse)
    {
        if (serverResponse == "nicknameError02")
        {
            errorText.text = "Nickname is already in use";
        }
        else if (serverResponse == "registerError")
        {
            errorText.text = "There was a problem, try again";
        }
        else if (serverResponse == "securityError" || serverResponse == "serverError")
        {
            errorText.text = "Server error: cannot authenticate the game ID";
        }
        else
        {
            errorText.text = "";
            this.nickname.GetComponent<InputField>().text = "";
            this.password.GetComponent<InputField>().text = "";
            loginToRegisterPanel.SetAsLastSibling();
        }
    }

    //It verify if a string have certain invalid characters, it returns true if found any invalid character or false if it doesn't found
    private bool ContainsInvalidCharacters(string stringToCheck)
    {
        string[] invalidCharacters = { "select", ";", "insert", "from", "where", "=", "/", "?", "\\", " " };
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
                this.registerButton.GetComponent<Button>().Select();
            }
        }
    }
}
