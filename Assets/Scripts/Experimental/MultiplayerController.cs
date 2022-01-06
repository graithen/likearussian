using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;
using System.Text;
using TMPro;
using UnityEngine.UI;
using Photon.Pun.UtilityScripts;
using System.Linq;

public class MultiplayerController : MonoBehaviour
{
    public NetworkController NC;
    private bool menuActive;

    //Card Related
    [Header("Cards")]
    public GameObject[] Cards = new GameObject[6];
    public Sprite[] Sprites = new Sprite[6];
    public Sprite CardBack;
    public List<int> cardsHand = new List<int> { 0, 1, 2, 3, 4, 5 };
    public List<int> cardsShuffled;
    public int DrawCount;

    public int[] DrawnCardArray = new int[6];

    //Network Related
    [Header("Network")]
    PhotonView PV;
    NetworkUtility NetworkUtil;
    public List<GameObject> Controllers;
    public GameObject[] StartupDisableObject;
    public int PlayerTurn = 0;
    public bool IsPlayerTurn;
    public bool Forfeit;

    //Button Related
    [Header("Buttons")]
    public GameObject DrawButton, ShuffleButton;
    public TextMeshProUGUI ShuffleButtonText;

    public GameObject MainMenu;
    bool MenuActive;

    //Turn & Score Related
    [Header("Turn & Scores")]
    public GameObject GameOverPanel;
    public TextMeshProUGUI TurnName;
    public TextMeshProUGUI PotText, ScoreText, ScoreBoard;
    public int Pot = 12;
    public int Score = 0;
    public bool IsGameOver;
    public string WinnerName;
    public string LosersList;
    public TextMeshProUGUI WinnerText;
    public TextMeshProUGUI LosersText;
    public GameObject GameRematchButton;


    [Header("Audio")]
    public AudioSource Audio;
    public AudioClip Gunshot, Hammer, Prepare, Cheer, Dissapoint;

    // Start is called before the first frame update
    void Start()
    {
        NC = GameObject.FindGameObjectWithTag("NetworkUtility").GetComponent<NetworkController>();
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (NC.IsMasterClient)
        {

        }
    }
}
