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



public class MultiplayerGameController : MonoBehaviour
{
    public GameObject[] Cards = new GameObject[6];
    public Sprite[] Sprites = new Sprite[6];
    public Sprite CardBack;
    public List<int> cardsHand = new List<int> { 0, 1, 2, 3, 4, 5 };
    public List<int> cardsShuffled;
    public int DrawCount;

    public int[] CardArray = new int[6];

    PhotonView PV;
    NetworkUtility NetworkUtil;
    public List<GameObject> Controllers;
    public GameObject[] StartupDisableObject;
    public int PlayerTurn = 0;
    public bool IsPlayerTurn;
    public bool Forfeit;
    public GameObject[] TurnUI;
    public TextMeshProUGUI ShuffleButtonText;

    public int Pot = 12;
    public int Score = 0;

    private void Start()
    {
        PV = GetComponent<PhotonView>();
        NetworkUtil = GameObject.FindWithTag("NetworkUtility").GetComponent<NetworkUtility>();

        NetworkUtil.AddController(this.gameObject);

        if (!PV.IsMine)
        {
            foreach (GameObject obj in StartupDisableObject)
            {
                obj.SetActive(false);
            }
        }

        PV.RPC("RPC_ChangeName", RpcTarget.AllBuffered, PhotonNetwork.NickName);
        UpdateControllerList();

        if (PV.IsMine && PhotonNetwork.IsMasterClient)
        {
            IsPlayerTurn = true;
            PV.RPC("RPC_SyncPlayerTurn", RpcTarget.All, PlayerTurn, IsPlayerTurn);
        }
    }

    void UpdateUI()
    {
        Debug.Log("Updating UI, player turn is " + IsPlayerTurn);
        foreach(GameObject obj in TurnUI)
        {
            obj.SetActive(IsPlayerTurn);
        }
    }

    public void ShuffleForfeitButton()
    {
        if(!Forfeit)
        {
            ShuffleCardButton();
            Forfeit = true;
            ShuffleButtonText.text = "Forfeit";
        }
        ChangePlayerTurn();
        ShuffleButtonText.text = "Shuffle";
        Forfeit = false;
    }

    public void DrawCardButton()
    {
        Debug.Log("Draw button pressed");
        PV.RPC("RPC_DrawCard", RpcTarget.MasterClient);
    }

    #region Draw Card
    [PunRPC]
    public void RPC_DrawCard()
    {
        DrawCard();
    }

    public void DrawCard()
    {
        PV.RPC("RPC_UpdateClients", RpcTarget.All, DrawCount, cardsHand[DrawCount]);
        PV.RPC("RPC_ShowCard", RpcTarget.All);
        if (cardsHand[DrawCount] == 5)
        {
            //FailState();
            return;
        }
        DrawCount++;
    }
    #endregion

    #region ShuffleDeck
    public void ShuffleCardButton()
    {
        Debug.Log("Shuffle button pressed");
        PV.RPC("RPC_ShuffleDeck", RpcTarget.MasterClient);
    }

    [PunRPC]
    public void RPC_ShuffleDeck()
    {
        ShuffleDeck();
    }

    public void ShuffleDeck()
    {
        for (int i = 0; i < cardsHand.Count; i++)
        {
            int temp = cardsHand[i];
            int randomIndex = Random.Range(i, cardsHand.Count);
            cardsHand[i] = cardsHand[randomIndex];
            cardsHand[randomIndex] = temp;
            
            DrawCount = 0;
            Debug.Log("Completed shuffle!");

            //PlayAudio(Prepare);
        }
        PV.RPC("ResetCards", RpcTarget.All);
    }

    [PunRPC]
    public void ResetCards()
    {
        foreach(GameObject obj in Controllers)
        {
            obj.GetComponent<MultiplayerGameController>().DrawCount = 0; //all DRAWCOUNT should be reset 0 
            foreach (GameObject card in obj.GetComponent<MultiplayerGameController>().Cards)
            {
                card.GetComponent<Image>().sprite = CardBack;
            }
        }
    }
    #endregion

    [PunRPC]
    public void RPC_UpdateClients(int drawCount, int cardValue)
    {
        Debug.Log("Updating client controller with " + drawCount + " " + cardValue);

        foreach(GameObject obj in Controllers)
        {
            Debug.Log("Updating " + gameObject.name);
            obj.GetComponent<MultiplayerGameController>().UpdateClients(drawCount, cardValue);
        }
    }

    public void UpdateClients(int drawCount, int cardValue)
    {
        DrawCount = drawCount;
        CardArray[DrawCount] = cardValue;
    }




    public void ChangePlayerTurn()
    {
        UpdateControllerList();
        PV.RPC("RPC_ChangePlayerTurn", RpcTarget.MasterClient);
    }

    [PunRPC]
    public void RPC_ChangePlayerTurn()
    {
        PlayerTurn++;

        if (PlayerTurn > Controllers.Count - 1)
            PlayerTurn = 0;

        foreach (GameObject obj in Controllers)
        {
            PV.RPC("RPC_ShuffleDeck", RpcTarget.MasterClient);

            obj.GetComponent<MultiplayerGameController>().IsPlayerTurn = false;
            PV.RPC("RPC_SyncPlayerTurn", RpcTarget.All, PlayerTurn, false);
        }

       Controllers[PlayerTurn].GetComponent<MultiplayerGameController>().IsPlayerTurn = true;
       Controllers[PlayerTurn].GetComponent<MultiplayerGameController>().PV.RPC("RPC_SyncPlayerTurn", RpcTarget.All, PlayerTurn, true);
    }

    [PunRPC]
    public void RPC_SyncPlayerTurn(int playerTurn, bool isPlayerTurn)
    {
        PlayerTurn = playerTurn;
        IsPlayerTurn = isPlayerTurn;
        UpdateUI();
    }





    [PunRPC]
    public void RPC_ShowCard()
    {
        Debug.Log("Showing card!");
        ShowCard();
    }

    void ShowCard()
    {
        foreach (GameObject obj in Controllers)
        {
            obj.GetComponent<MultiplayerGameController>().Cards[DrawCount].GetComponent<Image>().sprite = Sprites[CardArray[DrawCount]];
        }
    }

    [PunRPC]
    public void RPC_ChangeName(string name)
    {
        this.gameObject.name = name;
    }

    void UpdateControllerList()
    {
        Controllers = NetworkUtil.Controllers;
    }
}
