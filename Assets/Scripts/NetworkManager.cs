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

public class NetworkManager : MonoBehaviourPunCallbacks
{
    #region Private Serializable Fields
    [Tooltip("The maximum number of players per room. If a room is full and someone tries to join, it will instead create a new room.")]
    [SerializeField]
    private byte maxPlayersPerRoom = 8;
    string roomCode;

    #endregion

    #region Private Fields
    /// <summary>
    /// This client's version number. Users are separated from each other by gameVersion (which allows you to make breaking changes).
    /// </summary>
    string gameVersion = "1";
    StringBuilder playerList = new StringBuilder("Player List:\n");
    #endregion

    #region Public Fields
    public bool HasNickname, ConnectMenuActive, RoomMenuActive;
    [Header("UI Linking")]
    public GameObject ConnectMenuPanel;
    public GameObject RoomMenuPanel;
    public TMP_InputField nicknameField;
    public TMP_InputField roomCodeField;
    public GameObject GameStartButton;
    public TMP_Text Status;
    public TMP_Text playerNicknameText;
    public TMP_Text PlayerDescriptionText;
    public TMP_Text RoomCodeText;
    public TMP_Text PlayerListText;

    //Game Settings
    public Slider PotSlider;
    public TextMeshProUGUI PotNumberText;
    
    public GameObject MainMenu;
    bool MenuActive;

    public List<string> RussianNames = new List<string>() { "Alexei", "Aleksandr", "Boris", "Anatoly", "Yuri", "Nikolai", "Viktor", "Artem", "Lev", "Daniil" };
    public List<string> CharacterDescriptions = new List<string>() { "A seedy Moscow businessman.", "A disgraced party member with nothing to lose.", "A mysterious stranger.", "A western spy, trying to blend in.", "A member of the mafia, looking for respect.", "A bored babushka, playing for kicks", "A nuclear reactor worker, heavily irradiated", "An army officer, playing for sport", "A revolutionary, showing off to the crowd", "A homeless beggar, playing for money", "A hopeless drunk, playing for vodka" };
    #endregion



        /// <summary>
        /// MonoBehaviour method called on GameObject by Unity during early initialization phase.
        /// </summary>
    void Awake()
    {
        // #Critical
        // this makes sure we can use PhotonNetwork.LoadLevel() on the master client and all clients in the same room sync their level automatically
        PhotonNetwork.AutomaticallySyncScene = true;
    }

    // Start is called before the first frame update
    void Start()
    {
        RussianNames = new List<string>() { "Alexei", "Aleksandr", "Boris", "Anatoly", "Yuri", "Nikolai", "Viktor", "Artem", "Lev", "Daniil" };
        CharacterDescriptions = new List<string>() { "A seedy Moscow businessman.", "A disgraced party member with nothing to lose.", "A mysterious stranger.", "A western spy, trying to blend in.", "A member of the mafia, looking for respect.", "A bored babushka, playing for kicks.", "A slight glowing nuclear reactor worker.", "An army officer, playing for sport.", "A revolutionary, showing off to the crowd.", "A homeless beggar, playing for money.", "A hopeless drunk, playing for vodka.", "A tired game developer looking for work." };


        // #Critical, we must first and foremost connect to Photon Online Server.
        PhotonNetwork.ConnectUsingSettings();
        PhotonNetwork.GameVersion = gameVersion;

        if(HasNickname)
        {
            ConnectMenuActive = true;
        }

        ConnectMenuPanel.SetActive(ConnectMenuActive);
        RoomMenuPanel.SetActive(RoomMenuActive);

        PlayerPrefs.SetInt("PotValue", Mathf.RoundToInt(PotSlider.value));
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    #region Private Methods

    void InitializeConnection()
    {
        // we check if we are connected or not, we join if we are , else we initiate the connection to the server.
        if (PhotonNetwork.IsConnected)
        {
            // #Critical we need at this point to attempt joining a Random Room. If it fails, we'll get notified in OnJoinRandomFailed() and we'll create one.
            Debug.Log("Already connected to master!");
        }
        else
        {
            // #Critical, we must first and foremost connect to Photon Online Server.
            PhotonNetwork.ConnectUsingSettings();
            PhotonNetwork.GameVersion = gameVersion;
        }
    }
    #endregion

    #region Public Methods

    public void ChangeNickname(string nickname)
    {
        PhotonNetwork.LocalPlayer.NickName = nickname;
        //playerNicknameText.text = nicknameFieldConnect.text;

        /*if (alreadySet)
        {
            PhotonNetwork.LocalPlayer.NickName = nicknameFieldChange.text;
            playerNicknameText.text = nicknameFieldChange.text;
        }
        else
        {
            PhotonNetwork.LocalPlayer.NickName = nicknameFieldConnect.text;
            playerNicknameText.text = nicknameFieldConnect.text;
        }*/
    }

    public void SetInitialNickname()
    {
        string nickname = nicknameField.text;
        if (!string.IsNullOrWhiteSpace(nickname))
        {
            ChangeNickname(nicknameField.text);

            if (PhotonNetwork.IsConnected)
            {
                playerNicknameText.text = PhotonNetwork.NickName;
                ConnectMenuActive = true;
                ConnectMenuPanel.SetActive(ConnectMenuActive);
                HasNickname = true;
            }
        }
        else
        {
            string newName = RussianNames[Random.Range(0, RussianNames.Count - 1)].ToUpper() + " " + Random.Range(1000, 9999).ToString();
            ChangeNickname(newName);

            Debug.Log("trying to construct random name " + PhotonNetwork.NickName);

            if (PhotonNetwork.IsConnected)
            {
                playerNicknameText.text = PhotonNetwork.NickName;
                ConnectMenuActive = true;
                ConnectMenuPanel.SetActive(ConnectMenuActive);
                HasNickname = true;
            }
        }
        PlayerDescriptionText.text = CharacterDescriptions[Random.Range(0, CharacterDescriptions.Count - 1)];
    }

    public void CreateRoom()
    {
        if (PhotonNetwork.IsConnected)
        {
            RoomOptions roomOptions = new RoomOptions { MaxPlayers = maxPlayersPerRoom, EmptyRoomTtl = 120, IsVisible = false };

            roomCode = RandomString(4);
            Debug.Log("Creating room with name: " + roomCode);
            PhotonNetwork.CreateRoom(roomCode, new RoomOptions());
        }
        else
        {
            // #Critical, we must first and foremost connect to Photon Online Server.
            PhotonNetwork.ConnectUsingSettings();
            PhotonNetwork.GameVersion = gameVersion;
        }
    }

    public void JoinRoomWithCode()
    {
        roomCode = roomCodeField.text;
        roomCode.ToUpper();
        Status.text = "Connecting to " + roomCode + "...";

        // we check if we are connected or not, we join if we are , else we initiate the connection to the server.
        if (PhotonNetwork.IsConnected)
        {
            PhotonNetwork.JoinRoom(roomCode);
            Status.text = "Joining room " + roomCode + "...";
        }
        else
        {
            // #Critical, we must first and foremost connect to Photon Online Server.
            PhotonNetwork.ConnectUsingSettings();
            PhotonNetwork.GameVersion = gameVersion;
        }
    }

    public void LoadGameScene()
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            Debug.LogError("PhotonNetwork : Trying to Load a level but we are not the master Client");
        }
        Debug.LogFormat("PhotonNetwork : Loading Level : {0}", PhotonNetwork.CurrentRoom.PlayerCount);
        PhotonNetwork.LoadLevel("GameScene");
    }

    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
    }

    public void Disconnect()
    {
        PhotonNetwork.Disconnect();
        SceneManager.LoadScene(0);
    }

    public void PotSliderChange()
    {
        PlayerPrefs.SetInt("PotValue", Mathf.RoundToInt(PotSlider.value));
        PotNumberText.text = PlayerPrefs.GetInt("PotValue").ToString();
    }

    public void ToggleMainMenu()
    {
        MenuActive = !MenuActive;
        MainMenu.SetActive(MenuActive);
    }
    #endregion


    #region MonoBehaviourPunCallbacks Callbacks

    public override void OnConnectedToMaster()
    {
        Debug.Log("OnConnectedToMaster() was called by PUN");

        Status.text = "Connected To Master";
    }

    public override void OnCreatedRoom()
    {
        Debug.Log("OnCreatedRoom() was called by PUN, room name is: " + PhotonNetwork.CurrentRoom.Name);
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("OnJoinedRoom() was called by PUN, room name is: " + PhotonNetwork.CurrentRoom.Name);

        RoomCodeText.text = PhotonNetwork.CurrentRoom.Name;
        
        //Turn on the room menu panel
        RoomMenuActive = true;
        RoomMenuPanel.SetActive(RoomMenuActive);
        GameStartButton.SetActive(PhotonNetwork.IsMasterClient);

        PlayerListText.text = CallPlayerList();

        if(!PhotonNetwork.IsMasterClient)
        {
            PotSlider.interactable = false;
        }
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Status.text = "No room found with code " + roomCode + ".";
    }

    public override void OnLeftRoom()
    {
        SceneManager.LoadScene(0);
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        //Update player list
        PlayerListText.text = CallPlayerList();

        Status.text = newPlayer.NickName + " connected!";
    }
    #endregion

    System.Random random = new System.Random();
    string RandomString(int length) //Generates a random string from capital letters.
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        return new string(Enumerable.Repeat(chars, length).Select(s => s[random.Next(s.Length)]).ToArray());
    }

    string CallPlayerList()
    {
        playerList = new StringBuilder("Player List:\n");
        Debug.Log("OnJoinedRoom() called by PUN. Now this client is in a room.");
        foreach (var player in PhotonNetwork.PlayerList)
        {
            playerList.Append(player.NickName + "\n");
        }

        Debug.Log(playerList.ToString());
        return playerList.ToString();
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.LogWarningFormat("PUN Basics Tutorial/Launcher: OnDisconnected() was called by PUN with reason {0}", cause);
    }
}

