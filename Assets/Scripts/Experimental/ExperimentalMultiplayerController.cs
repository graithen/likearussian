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

public class ExperimentalMultiplayerController : MonoBehaviour
{
    //Card Related
    [Header("Cards")]
    public GameObject[] Cards = new GameObject[6];
    public Sprite[] Sprites = new Sprite[6];
    public Sprite CardBack;

    //Network Related
    [Header("Network")]
    public PhotonView PV;
    NetworkController NC;
    NetworkUtility NetworkUtil;
    DealerController NetworkDealer;
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
    public AudioClip Gunshot, Hammer, Prepare, Gasp, Dissapoint;

    // Start is called before the first frame update
    private void Start()
    {
        PV = GetComponent<PhotonView>();
        NC = GameObject.FindWithTag("NetworkController").GetComponent<NetworkController>();
        NetworkUtil = GameObject.FindWithTag("NetworkUtility").GetComponent<NetworkUtility>();
        NetworkDealer = GameObject.FindWithTag("NetworkUtility").GetComponent<DealerController>();

        NetworkUtil.AddController(this.gameObject);


        if (PV.IsMine)
        {
            NC.IsInGame = true;
            NC.MenuMusicActivation(false);
            NetworkDealer.LocalController = this.gameObject;
            NetworkDealer.LocalControllerScript = GetComponent<ExperimentalMultiplayerController>();

            //Close room to late joiners trying to get in!
            PhotonNetwork.CurrentRoom.IsOpen = false;
        }

        if (!PV.IsMine)
        {
            //Disable the other controller elements
            foreach (GameObject obj in StartupDisableObject)
            {
                obj.SetActive(false);
            }
        }
    }



    // Update is called once per frame
    void LateUpdate()
    {

    }

    //Local actions
    #region Local Buttons
    //Draw card button
    public void DrawCardButton()
    {
        if (PV.IsMine)
        {
            DrawButton.GetComponent<Button>().interactable = !DrawButton.GetComponent<Button>().interactable;
            NetworkDealer.DrawCard();
        }
    }

    //Shuffle cards button
    public void ShuffleCardsButton()
    {
        if (PV.IsMine)
        {
            ShuffleButton.GetComponent<Button>().interactable = !ShuffleButton.GetComponent<Button>().interactable;
            
            //Toggle between forfeiting and shuffling
            if (!Forfeit)
            {
                NetworkDealer.ShuffleCards();
                PlayAudio(Prepare);
                ShuffleButtonText.text = "Pass";
            }
            if (Forfeit)
            {
                DrawButton.SetActive(false);
                ShuffleButton.SetActive(false);

                NetworkDealer.EndTurn();
                ShuffleButtonText.text = "Shuffle";
            }
            Forfeit = !Forfeit;
        }
    }

    public void ChangeForfeitValues(bool forfeitValue, string buttonText)
    {
        if (PV.IsMine)
        {
            Forfeit = forfeitValue;
            ShuffleButtonText.text = buttonText;

            Debug.Log("Changing forfeit values " + forfeitValue + " " + buttonText);
        }
    }

    public void SetButtonsActive()
    {
        DrawButton.SetActive(IsPlayerTurn);
        ShuffleButton.SetActive(IsPlayerTurn);

        if(IsPlayerTurn)
        {

        }
    }
    #endregion



    //Card actions
    #region Card Logic
    public void ChangeCard(int drawCount, int cardValue)
    {
        if (PV.IsMine)
        {
            //Enable the button again because server has returned the action
            DrawButton.GetComponent<Button>().interactable = !DrawButton.GetComponent<Button>().interactable;

            //get the local values of drawcount and card value, as these should now be stored!
            Cards[drawCount].GetComponent<Image>().sprite = Sprites[cardValue];

            Debug.Log("Draw button is " + DrawButton.GetComponent<Button>().interactable);

            if (drawCount == 4)
                PlayAudio(Gasp);

            if (cardValue == 5)
            {
                //FailState();
                PlayAudio(Gunshot);
                PlayAudio(Dissapoint);
            }
            PlayAudio(Hammer);
        }
    }

    //Resets the card backs on all clients
    public void ResetCards()
    {
        if (PV.IsMine)
        {
            ShuffleButton.GetComponent<Button>().interactable = !ShuffleButton.GetComponent<Button>().interactable;
            foreach (GameObject card in Cards)
            {
                card.GetComponent<Image>().sprite = CardBack;
            }
            PlayAudio(Prepare);
        }
    }
    #endregion

    #region Turn Logic


    #endregion

    void PlayAudio(AudioClip audio)
    {
        Audio.pitch = Random.Range(0.9f, 1.1f);
        Audio.PlayOneShot(audio);
    }


































    /*/only good for syncing from owned PV
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(TestBool);
            stream.SendNext(TestInt);
            stream.SendNext(TestFloat);
        }
        else
        {
            TestBool = (bool)stream.ReceiveNext();
            TestInt = (int)stream.ReceiveNext();
            TestFloat = (float)stream.ReceiveNext();

            Debug.Log(TestBool + " " + TestFloat + " " + TestInt);
        }
    }*/
}
