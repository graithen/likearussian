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

public class RefactoredMultiplayerController : MonoBehaviour
{
    public GameObject[] Cards = new GameObject[6];
    public Sprite[] Sprites = new Sprite[6];
    public Sprite CardBack;
    public List<int> cardsHand = new List<int> { 0, 1, 2, 3, 4, 5 };
    public List<int> cardsShuffled;
    public int DrawCount;

    public int[] DrawnCardArray = new int[6];

    PhotonView PV;
    NetworkUtility NetworkUtil;
    public List<GameObject> Controllers;
    public GameObject[] StartupDisableObject;
    public int PlayerTurn = 0;
    public bool IsPlayerTurn;
    public bool Forfeit;

    public GameObject DrawButton, ShuffleButton;
    public TextMeshProUGUI ShuffleButtonText;

    public TextMeshProUGUI TurnName;
    public TextMeshProUGUI PotText, ScoreText, ScoreBoard;
    public int Pot = 12;
    public int Score = 0;

    [Header("Audio")]
    public AudioSource Audio;
    public AudioClip Gunshot, Hammer, Prepare, Cheer, Dissapoint;


    private void Start()
    {
        PV = GetComponent<PhotonView>();
        NetworkUtil = GameObject.FindWithTag("NetworkUtility").GetComponent<NetworkUtility>();

        NetworkUtil.AddController(this.gameObject);

        PV.RPC("RPC_ChangeName", RpcTarget.AllBuffered, PhotonNetwork.NickName);


        if (!PV.IsMine)
        {
            foreach (GameObject obj in StartupDisableObject)
            {
                obj.SetActive(false);
            }
        }

        InitializePlayerTurn();
    }

    private void LateUpdate()
    {
        UpdateControllerList();
    }











    [PunRPC]
    public void RPC_ChangeName(string name)
    {
        this.gameObject.name = name;
    }


    void InitializePlayerTurn()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            IsPlayerTurn = true;
            ShuffleCards();
            
        }
        if (!PhotonNetwork.IsMasterClient)
        {
            IsPlayerTurn = false;
        }
        UpdateUI();
    }


    void UpdateControllerList()
    {
        Controllers = NetworkUtil.Controllers;
    }


    void PlayAudio(AudioClip audio)
    {
        Audio.PlayOneShot(audio);
    }


    void UpdateUI()
    {
        if (!Forfeit)
        {
            DrawButton.SetActive(IsPlayerTurn);
            ShuffleButton.SetActive(IsPlayerTurn);
        }

        if (PhotonNetwork.IsMasterClient)
        {
            string name = Controllers[PlayerTurn].gameObject.name;
            foreach (GameObject obj in Controllers)
            {
                obj.GetComponent<RefactoredMultiplayerController>().PV.RPC("RPC_ChangeTurnName", RpcTarget.AllBuffered, name);
            }
        }

        PotText.text = Pot.ToString();
        ScoreText.text = Score.ToString();

        ScoreBoard.text = BuildScoreList();
    }










    public void DrawCardButton()
    {
        if (PV.IsMine)
        {
            //send rpc to server to draw card
            PV.RPC("RPC_ServerDrawCard", RpcTarget.MasterClient);
            ChangeForfeitValues(true, "Forfeit");
        }
    }

    public void ForfeitShuffleButton()
    {
        if (!Forfeit)
        {
            PV.RPC("RPC_Shuffle", RpcTarget.MasterClient);
            PlayAudio(Prepare);
            ShuffleButtonText.text = "Forfeit";
        }
        if (Forfeit)
        {
            PV.RPC("RPC_Forfeit", RpcTarget.MasterClient);
            ShuffleButtonText.text = "Shuffle";
        }
        Forfeit = !Forfeit;
    }

    void ChangeForfeitValues(bool forfeitValue, string buttonText)
    {
        Forfeit = forfeitValue;
        ShuffleButtonText.text = buttonText;

        Debug.Log("Changing forfeit values " + forfeitValue + " " + buttonText);
    }







    #region Draw Card
    [PunRPC]
    public void RPC_ServerDrawCard()
    {
        //server copy of controller draws card using its logic
        DrawCard();
    }

    void DrawCard()
    {
        DrawnCardArray[DrawCount] = cardsHand[DrawCount];

        foreach (GameObject controller in Controllers)
        {
            //for each controller in the local scene send out rpc containing card and draw count to all clients. Distributes info to clients controllers
            controller.GetComponent<RefactoredMultiplayerController>().PV.RPC("RPC_ClientCardUpdate", RpcTarget.All, DrawCount, cardsHand[DrawCount]);
        }

        //if server draw card
        if (DrawnCardArray[DrawCount] == 5)
        {
            Pot += Score;
            Score = 0;
            foreach (GameObject controller in Controllers)
            {
                controller.GetComponent<RefactoredMultiplayerController>().PV.RPC("RPC_SyncScore", RpcTarget.AllBuffered, Pot, controller.GetComponent<RefactoredMultiplayerController>().Score);
            }
            return;
        }

        if(Pot > 0)
        {
            Pot--;
            Score++;

            foreach (GameObject controller in Controllers)
            {
                controller.GetComponent<RefactoredMultiplayerController>().PV.RPC("RPC_SyncScore", RpcTarget.AllBuffered, Pot, controller.GetComponent<RefactoredMultiplayerController>().Score);
            }
        }

        DrawCount++;
    }

    [PunRPC]
    public void RPC_ClientCardUpdate(int drawCount, int cardValue)
    {
        //assign the draw count to the local variable, and the card value to the card array
        DrawCount = drawCount;

        DrawnCardArray[DrawCount] = cardValue;
        ChangeCard();
    }

    void ChangeCard()
    {
        //get the local values of drawcount and card value, as these should now be stored!
        Cards[DrawCount].GetComponent<Image>().sprite = Sprites[DrawnCardArray[DrawCount]];

        if (DrawnCardArray[DrawCount] == 5)
        {
            FailState();
            PlayAudio(Gunshot);
            PlayAudio(Dissapoint);
        }
        PlayAudio(Hammer);
    }
    #endregion





    [PunRPC]
    public void RPC_Shuffle()
    {
        ShuffleCards();
    }

    void ShuffleCards()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            for (int i = 0; i < cardsHand.Count; i++)
            {
                int temp = cardsHand[i];
                int randomIndex = Random.Range(i, cardsHand.Count);
                cardsHand[i] = cardsHand[randomIndex];
                cardsHand[randomIndex] = temp;

                DrawCount = 0;
            }

            //PlayAudio(Prepare);
            Debug.Log("Completed shuffle!");

            foreach (GameObject controller in Controllers)
            {
                //for each controller in the local scene send out rpc containing telling the controllers to reset cards locally
                controller.GetComponent<RefactoredMultiplayerController>().PV.RPC("RPC_ResetCards", RpcTarget.All);
            }
        }
    }

        [PunRPC]
    public void RPC_ResetCards()
    {
        foreach (GameObject obj in Controllers)
        {
            foreach (GameObject card in obj.GetComponent<RefactoredMultiplayerController>().Cards)
            {
                card.GetComponent<Image>().sprite = CardBack;
            }
        }
    }








    [PunRPC]
    void RPC_Forfeit()
    {
        ChangeTurn();
    }

    void ChangeTurn()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            Debug.LogWarning("Changing turn on master client!");

            //increment player turn
            PlayerTurn++;

            if (PlayerTurn > Controllers.Count - 1)
                PlayerTurn = 0;


            Debug.Log("Player turn " + PlayerTurn);

            foreach (GameObject obj in Controllers)
            {
                obj.GetComponent<RefactoredMultiplayerController>().IsPlayerTurn = false;
            }

            //set the correct players turn to true and shuffle their hand
            Controllers[PlayerTurn].GetComponent<RefactoredMultiplayerController>().PlayerTurn = PlayerTurn;
            Controllers[PlayerTurn].GetComponent<RefactoredMultiplayerController>().IsPlayerTurn = true;
            Controllers[PlayerTurn].GetComponent<RefactoredMultiplayerController>().ShuffleCards();

            foreach (GameObject obj in Controllers)
            {
                //trigger RPC calls to sync player turn on each client controller
                obj.GetComponent<RefactoredMultiplayerController>().PV.RPC("RPC_SyncPlayerTurn", RpcTarget.All, obj.GetComponent<RefactoredMultiplayerController>().PlayerTurn, obj.GetComponent<RefactoredMultiplayerController>().IsPlayerTurn);
                //obj.GetComponent<RefactoredMultiplayerController>().PV.RPC("RPC_ChangeTurnName", RpcTarget.AllBuffered, PhotonNetwork.NickName);
            }
        }
    }

    [PunRPC]
    public void RPC_SyncPlayerTurn(int playerTurn, bool isPlayerTurn)
    {
        PlayerTurn = playerTurn;
        IsPlayerTurn = isPlayerTurn;

        Debug.Log("Player turn after syncing " + PlayerTurn);

        //reset shuffle button to default
        ChangeForfeitValues(false, "Shuffle");

        UpdateUI();
    }

    [PunRPC]
    public void RPC_ChangeTurnName(string name)
    {
        TurnName.text = name;
    }








    void FailState()
    {
        DrawButton.SetActive(false);
        ChangeForfeitValues(true, "Forfeit");
    }



    [PunRPC]
    public void RPC_SyncScore(int pot, int score)
    {
        Pot = pot;
        Score = score;

        UpdateUI();
    }



    string BuildScoreList()
    {
        StringBuilder scoreList;
        scoreList = new StringBuilder("Player List:\n");
        Debug.Log("OnJoinedRoom() called by PUN. Now this client is in a room.");
        foreach (GameObject obj in Controllers)
        {
            int score = obj.GetComponent<RefactoredMultiplayerController>().Score;
            string name = obj.name;
            

            scoreList.Append(name + ": " + score + "\n");
        }

        Debug.Log(scoreList.ToString());
        return scoreList.ToString();
    }
}
