using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using TMPro;
using UnityEngine.UI;

public class GameLobbyUIController : MonoBehaviour
{
    public NetworkController NC;
    bool menuActive;

    public GameObject OptionsMenu;
    public GameObject GameStartButton;
    public TextMeshProUGUI RoomCodeText;
    public TextMeshProUGUI PlayerListText;
    public TextMeshProUGUI PotNumberText;
    public Slider PotSlider;

    // Start is called before the first frame update
    void Start()
    {
        NC = GameObject.FindGameObjectWithTag("NetworkController").GetComponent<NetworkController>();
    }

    // Update is called once per frame
    void LateUpdate()
    {
        UpdateRoom();
    }

    public void PotSliderChange()
    {
        PlayerPrefs.SetInt("PotValue", Mathf.RoundToInt(PotSlider.value));
        PotNumberText.text = PlayerPrefs.GetInt("PotValue").ToString();
    }

    void UpdateRoom()
    {
        bool isMasterClient = NC.IsMasterClient;

        RoomCodeText.text = NC.RoomCode;

        GameStartButton.SetActive(isMasterClient);

        PlayerListText.text = NC.CallPlayerList();

        PotSlider.interactable = isMasterClient;
    }

    public void StartGame()
    {
        NC.StartGame();
        //NC.LoadScene("GameScene");
    }

    public void LeaveRoom()
    {
        NC.LeaveRoom();
    }

    public void ToggleMenu()
    {
        menuActive = !menuActive;
        OptionsMenu.SetActive(menuActive);
    }
}
