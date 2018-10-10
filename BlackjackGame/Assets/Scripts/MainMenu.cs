using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour {

    public GameObject btnPlayGame;
    public GameObject btnSettings;
    public GameObject btnProfile;
    public GameObject btnExitGame;
    public RectTransform prePlayScreen;
    //public GameObject settingsScreen;
    //public GameObject profileScreen;

	// Use this for initialization
	void Start () {
        this.btnPlayGame.GetComponent<Button>().onClick.AddListener(LaunchPrePlayScreen);
        this.btnSettings.GetComponent<Button>().onClick.AddListener(LaunchSettingsScreen);
        this.btnProfile.GetComponent<Button>().onClick.AddListener(LaunchProfileScreen);
        this.btnExitGame.GetComponent<Button>().onClick.AddListener(ExitGame);
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    private void LaunchPrePlayScreen()
    {
        this.prePlayScreen.SetAsLastSibling();
    }

    private void LaunchProfileScreen()
    {

    }

    private void LaunchSettingsScreen()
    {

    }

    private void ExitGame()
    {
        Application.Quit();
    }
}
