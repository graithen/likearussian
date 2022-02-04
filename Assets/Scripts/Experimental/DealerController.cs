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

public class DealerController : MonoBehaviour, IPunTurnManagerCallbacks
{
    //Card Related
    [Header("Cards")]
    public List<int> cardsHand = new List<int> { 0, 1, 2, 3, 4, 5 };
    public List<int> cardsShuffled;
    public int DrawCount = 0;

    public int[] DrawnCardArray = new int[6];

    //Network Related
    [Header("Network")]
    PhotonView PV;
    NetworkController NC;
    NetworkUtility NetworkUtil;
    PunTurnManager TurnManager;
    public Dictionary<int, Player> Controllers;
    public GameObject[] StartupDisableObject;
    public int PlayerTurn = 1;
    public bool IsPlayerTurn;
    public bool Forfeit;

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

    //Active controller for the round
    public GameObject LocalController;
    public ExperimentalMultiplayerController LocalControllerScript;


    private void Start()
    {
        PV = GetComponent<PhotonView>();
        NC = GameObject.FindWithTag("NetworkController").GetComponent<NetworkController>();
        NetworkUtil = GetComponent<NetworkUtility>();
        TurnManager = GetComponent<PunTurnManager>();
        TurnManager.TurnManagerListener = this.GetComponent<DealerController>();
        
        TurnManager.TurnDuration = 120;
        Initialise();
    }

    public void Initialise()
    {
        EndTurn();
        LocalControllerScript.SetButtonsActive();
    }

    private void LateUpdate()
    {
        Controllers = PhotonNetwork.CurrentRoom.Players;
    }

    #region Draw Card
    public void DrawCard()
    {
        Debug.Log("Attempting drawcard RPC");
        
        //Should be called from the controller
        PV.RPC("RPC_ServerDrawCard", RpcTarget.MasterClient);
    }

    [PunRPC]
    public void RPC_ServerDrawCard()
    {
        //server copy of controller draws card using its logic
        if (PhotonNetwork.IsMasterClient)
        {
            DrawnCardArray[DrawCount] = cardsHand[DrawCount];

            if (DrawnCardArray[DrawCount] == 5)
            {
                Pot += Score;
                Score = 0;

                //string scoreBoard = BuildScoreList();

                Debug.Log("Attempting to update clients");
            }
        }

        if (Pot > 0)
        {
            Pot--;
            Score++;

            //string scoreBoard = BuildScoreList();

        }

        PV.RPC("RPC_UpdateClientCards", RpcTarget.AllBuffered, DrawCount, cardsHand[DrawCount]);

        DrawCount++;

        if (DrawCount > 5)
            DrawCount = DrawnCardArray.Length - 1;
    }

    [PunRPC]
    public void RPC_UpdateClientCards(int drawCount, int cardValue)
    {
        DrawCount = drawCount;
        DrawnCardArray[DrawCount] = cardValue;

        LocalControllerScript.ChangeCard(DrawCount, DrawnCardArray[DrawCount]);
        
        LocalControllerScript.ChangeForfeitValues(true, "Pass");
        Debug.Log("Update called!");
    }
    #endregion



    #region Shuffle Cards
    public void ShuffleCards()
    {
        //Should be called from the controller
        PV.RPC("RPC_ServerShuffleCards", RpcTarget.MasterClient);
    }

    [PunRPC]
    public void RPC_ServerShuffleCards()
    {
        //server copy of controller draws card using its logic
        if (PhotonNetwork.IsMasterClient)
        {
            Debug.Log("Attempting shuffle card RPC");

            for (int i = 0; i < cardsHand.Count; i++)
            {
                int temp = cardsHand[i];
                int randomIndex = Random.Range(i, cardsHand.Count);
                cardsHand[i] = cardsHand[randomIndex];
                cardsHand[randomIndex] = temp;

                DrawCount = 0;
            }
            PV.RPC("RPC_ShuffleClients", RpcTarget.AllBuffered);
        }
    }           

    [PunRPC]
    public void RPC_ShuffleClients()
    {
        LocalControllerScript.ResetCards();
        Debug.Log("Update called!");
    }
    #endregion



    #region Player Turns
    public void EndTurn()
    {
        //Should be called from the controller
        PV.RPC("RPC_ServerEndTurn", RpcTarget.MasterClient);
        //Also shuffle cards
        PV.RPC("RPC_ServerShuffleCards", RpcTarget.MasterClient);
    }

    [PunRPC]
    public void RPC_ServerEndTurn()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            PlayerTurn++;
            if (PlayerTurn > PhotonNetwork.CurrentRoom.Players.Count)
                PlayerTurn = 1;

            PV.RPC("RPC_SyncTurn", RpcTarget.AllBufferedViaServer, PlayerTurn);
        }
    }

    [PunRPC]
    public void RPC_SyncTurn(int TurnNumber)
    {
        PlayerTurn = TurnNumber;
        TurnManager.BeginTurn();
    }

    public void OnTurnBegins(int turn)
    {   
        if (PhotonNetwork.CurrentRoom.Players[PlayerTurn] == LocalController.GetComponent<PhotonView>().Controller)
        {
            Debug.Log(LocalController.GetComponent<PhotonView>().Controller.NickName + " has turn");
            LocalControllerScript.IsPlayerTurn = true;
            LocalControllerScript.SetButtonsActive();
        }
        else
        {
            LocalControllerScript.IsPlayerTurn = false;
            LocalControllerScript.SetButtonsActive();
            Debug.Log("Not this controllers turn!");
        }
    }

    public void OnTurnCompleted(int turn)
    {
        Debug.Log("Turn ended!");
    }

    public void OnTurnTimeEnds(int turn)
    {
        TurnManager.BeginTurn();
    }

    public void OnPlayerMove(Player player, int turn, object move)
    {
        throw new System.NotImplementedException();
    }

    public void OnPlayerFinished(Player player, int turn, object move)
    {
        throw new System.NotImplementedException();
    }
    #endregion
}
