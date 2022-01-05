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


    private void Start()
    {
        PV = GetComponent<PhotonView>();
        NetworkUtil = GameObject.FindWithTag("NetworkUtility").GetComponent<NetworkUtility>();

        NetworkUtil.AddController(this.gameObject);


        if(PV.IsMine)
        {
            PV.RPC("RPC_ChangeName", RpcTarget.AllBuffered, PhotonNetwork.NickName);
        }

        if (!PV.IsMine)
        {
            //Disable the other controller elements
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




    //GENERAL METHODS 
    #region General Methods
    
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
            Pot = PlayerPrefs.GetInt("PotValue");
            
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

        PotText.text = Pot.ToString();
        ScoreText.text = Score.ToString();
    }
    #endregion








    //BUTTON METHODS
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
        //Toggle between forfeiting and shuffling
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

    public void ToggleMainMenu()
    {
        MenuActive = !MenuActive;
        MainMenu.SetActive(MenuActive);
    }

    public void RematchButton()
    {
        foreach (GameObject obj in Controllers)
        {
            obj.GetComponent<RefactoredMultiplayerController>().PV.RPC("RPC_ReloadLevel", RpcTarget.All);
        }
    }

    public void LeaveButton()
    {
        if(PV.IsMine)
        {
            Debug.LogWarning("Leaving the game!");
        }
    }

    void ChangeForfeitValues(bool forfeitValue, string buttonText)
    {
        Forfeit = forfeitValue;
        ShuffleButtonText.text = buttonText;

        Debug.Log("Changing forfeit values " + forfeitValue + " " + buttonText);
    }






    //DRAW CARDS - This controls the proces of drawing cards and syncing them to all clients
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
            
            string scoreBoard = BuildScoreList();

            foreach (GameObject controller in Controllers)
            {
                controller.GetComponent<RefactoredMultiplayerController>().PV.RPC("RPC_SyncScore", RpcTarget.AllBuffered, Pot, controller.GetComponent<RefactoredMultiplayerController>().Score, scoreBoard);
            }
            return;
        }

        if(Pot > 0)
        {
            Pot--;
            Score++;

            string scoreBoard = BuildScoreList();

            foreach (GameObject controller in Controllers)
            {
                controller.GetComponent<RefactoredMultiplayerController>().PV.RPC("RPC_SyncScore", RpcTarget.AllBuffered, Pot, controller.GetComponent<RefactoredMultiplayerController>().Score, scoreBoard);
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


    #region Shuffle Cards
    //Triggers a card shuffle on the local player controller
    [PunRPC]
    public void RPC_Shuffle()
    {
        ShuffleCards();
    }

    //Run on the local controller. Updates values locally and then distributes to other controllers
    void ShuffleCards()
    {
        //Card shuffles should only happen on the master client version of a controller, and then have values distributed. Prevents cheating and means that clients dont all generate individual hands
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

    //Resets the card backs on all clients
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
    #endregion


    #region Forfeit

    //Forfeit turn triggers a turn change. Done if players want to skip turn or have died
    [PunRPC]
    void RPC_Forfeit()
    {
        ChangeTurn();
    }

    //Change turn called locally
    void ChangeTurn()
    {
        //Only trigger turn change on master client, so that only the master client is handling the turn states.
        if (PhotonNetwork.IsMasterClient)
        {
            Debug.LogWarning("Changing turn on master client!");

            //increment player turn
            PlayerTurn++;

            if (PlayerTurn > Controllers.Count - 1)
                PlayerTurn = 0;


            //Sync turn name here so correct turn name is only triggered once on master client and not multiple times in UpdateUI
            if (PhotonNetwork.IsMasterClient)
            {
                string name = Controllers[PlayerTurn].gameObject.name;
                foreach (GameObject obj in Controllers)
                {
                    obj.GetComponent<RefactoredMultiplayerController>().PV.RPC("RPC_ChangeTurnName", RpcTarget.AllBuffered, name);
                }
            }


            Debug.Log("Player turn " + PlayerTurn);

            //Set all controllers player turn bool to false on the master client, just to be safe
            foreach (GameObject obj in Controllers)
            {
                obj.GetComponent<RefactoredMultiplayerController>().IsPlayerTurn = false;
            }

            //set the correct players turn to true and shuffle their hand
            Controllers[PlayerTurn].GetComponent<RefactoredMultiplayerController>().PlayerTurn = PlayerTurn;
            Controllers[PlayerTurn].GetComponent<RefactoredMultiplayerController>().IsPlayerTurn = true;
            Controllers[PlayerTurn].GetComponent<RefactoredMultiplayerController>().ShuffleCards();

            //update each controller on the master client 
            foreach (GameObject obj in Controllers)
            {
                //trigger RPC calls to sync player turn on each client controller
                obj.GetComponent<RefactoredMultiplayerController>().PV.RPC("RPC_SyncPlayerTurn", RpcTarget.All, obj.GetComponent<RefactoredMultiplayerController>().PlayerTurn, obj.GetComponent<RefactoredMultiplayerController>().IsPlayerTurn);
            }
        }
    }

    //Syncs the player turn value on the local controller with the controller values on master client
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

    //Change the name of the players UI name text that indicates who's turn it is
    [PunRPC]
    public void RPC_ChangeTurnName(string name)
    {
        TurnName.text = name;
    }
    #endregion




    //These handle the UI and score changes!
    void FailState()
    {
        DrawButton.SetActive(false);
        ChangeForfeitValues(true, "Forfeit");
    }



    [PunRPC]
    public void RPC_GameOver()
    {
        CheckWinner();
    }

    [PunRPC]
    public void RPC_SyncScore(int pot, int score, string scoreList)
    {
        Pot = pot;
        Score = score;
        ScoreBoard.text = scoreList;

        if (PhotonNetwork.IsMasterClient && Pot == 0)
        {
            Debug.LogWarning("Triggering game over, as pot is now 0!");
            IsGameOver = true;
            PV.RPC("RPC_GameOver", RpcTarget.MasterClient);
        }

        UpdateUI();
    }







    void CheckWinner()
    {
        int winningScore = 0;

        List<string> losers = new List<string>();

        foreach (GameObject obj in Controllers)
        {
            int score = obj.GetComponent<RefactoredMultiplayerController>().Score;
            if (score > winningScore)
            {
                winningScore = score;
                WinnerName = obj.name;
            }
        }

        foreach (GameObject obj in Controllers)
        {
            int score = obj.GetComponent<RefactoredMultiplayerController>().Score;
            if (score < winningScore)
            {
                losers.Add(obj.name);
            }
        }

        LosersList = BuildLoserList(losers);

        Debug.Log("Winner " + WinnerName);
        Debug.Log("Losers " + LosersList);

        foreach (GameObject obj in Controllers)
        {
            obj.GetComponent<RefactoredMultiplayerController>().PV.RPC("RPC_SyncGameOver", RpcTarget.All, IsGameOver, WinnerName, LosersList);
        }
    }

    [PunRPC]
    public void RPC_SyncGameOver(bool gameOver, string victor, string loserList)
    {

        IsGameOver = gameOver;
        GameOverPanel.SetActive(IsGameOver);

        WinnerText.text = victor;
        LosersText.text = loserList;

        if (!PhotonNetwork.IsMasterClient)
            GameRematchButton.SetActive(false);
    }

    [PunRPC]
    public void RPC_ReloadLevel()
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            Debug.LogError("PhotonNetwork : Trying to Load a level but we are not the master Client");
        }
        Debug.LogFormat("PhotonNetwork : Loading Level : {0}", PhotonNetwork.CurrentRoom.PlayerCount);
        PhotonNetwork.LoadLevel("GameScene");
    }








    //Should only run on the server, as clients have their own controller lists and they will be different
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

    string BuildLoserList(List<string> list)
    {
        StringBuilder loserList;
        loserList = new StringBuilder("");
        Debug.Log("OnJoinedRoom() called by PUN. Now this client is in a room.");
        foreach (string str in list)
        {
            loserList.Append(name + "\n");
        }

        Debug.Log(loserList.ToString());
        return loserList.ToString();
    }
}
