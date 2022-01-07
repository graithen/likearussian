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

public class NetworkController : MonoBehaviourPunCallbacks
{
    void Awake()
    {
        // #Critical
        // this makes sure we can use PhotonNetwork.LoadLevel() on the master client and all clients in the same room sync their level automatically
        PhotonNetwork.AutomaticallySyncScene = true;
        
        Object.DontDestroyOnLoad(this.gameObject);
    }

    // Start is called before the first frame update
    void Start()
    {
        // #Critical, we must first and foremost connect to Photon Online Server.
        PhotonNetwork.ConnectUsingSettings();
        PhotonNetwork.GameVersion = gameVersion;

    }

    private void LateUpdate()
    {
        CheckClientStatus();
    }

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
    string status;
    
    bool isMasterClient;
    public bool IsMasterClient { get { return isMasterClient; } }
    #endregion

    #region Public Fields
    public bool HasNickname;
    public string Nickname;
    public string PlayerDescription;
    public string PlayerList;
    public string RoomCode;

    public List<string> RussianNames = new List<string>() { "Alexei", "Aleksandr", "Boris", "Anatoly", "Yuri", "Nikolai", "Viktor", "Artem", "Lev", "Daniil" };
    public List<string> CharacterDescriptions = new List<string>() { "A seedy Moscow businessman.", "A disgraced party member with nothing to lose.", "A mysterious stranger.", "A western spy, trying to blend in.", "A member of the mafia, looking for respect.", "A bored babushka, playing for kicks.", "A slight glowing nuclear reactor worker.", "An army officer, playing for sport.", "A revolutionary, showing off to the crowd.", "A homeless beggar, playing for money.", "A hopeless drunk, playing for vodka.", "A tired game developer looking for work." };
    #endregion


    void CheckClientStatus()
    {
        //Runs in late update to see if any change to client status
        isMasterClient = PhotonNetwork.IsMasterClient;

        //Could also log connection status here
    }


    public void SetNickname(string str)
    {
        string nickname = str;
        if (!string.IsNullOrWhiteSpace(nickname))
        {
            if (PhotonNetwork.IsConnected)
            {
                PhotonNetwork.NickName = str;
                Nickname = PhotonNetwork.NickName;
                HasNickname = true;
            }
        }
        else
        {
            string newName = RussianNames[Random.Range(0, RussianNames.Count - 1)].ToUpper() + " " + Random.Range(1000, 9999).ToString();

            Debug.Log("trying to construct random name " + PhotonNetwork.NickName);

            if (PhotonNetwork.IsConnected)
            {
                PhotonNetwork.NickName = newName;
                Nickname = PhotonNetwork.NickName;
                HasNickname = true;
            }
        }
        PlayerDescription = CharacterDescriptions[Random.Range(0, CharacterDescriptions.Count - 1)].ToUpper();
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

    public void JoinRoomWithCode(string code)
    {
        roomCode = code;
        roomCode.ToUpper();
        status = "Connecting to " + roomCode + "...";

        // we check if we are connected or not, we join if we are , else we initiate the connection to the server.
        if (PhotonNetwork.IsConnected)
        {
            PhotonNetwork.JoinRoom(roomCode);
            status = "Joining room " + roomCode + "...";
        }
        else
        {
            // #Critical, we must first and foremost connect to Photon Online Server.
            PhotonNetwork.ConnectUsingSettings();
            PhotonNetwork.GameVersion = gameVersion;
        }
    }

    public void LoadScene(string sceneName)
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            Debug.LogWarning("PhotonNetwork : Trying to Load a level but we are not the master client, trying to load scene via SceneManager");
            SceneManager.LoadScene(sceneName);
        }
        //Debug.LogFormat("PhotonNetwork : Loading Level : {0}", PhotonNetwork.CurrentRoom.PlayerCount);
        PhotonNetwork.LoadLevel(sceneName);
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

    

    #region MonoBehaviourPunCallbacks Callbacks
    public override void OnConnectedToMaster()
    {
        Debug.Log("OnConnectedToMaster() was called by PUN");

        status = "Connected To Master";
    }

    public override void OnCreatedRoom()
    {
        Debug.Log("OnCreatedRoom() was called by PUN, room name is: " + PhotonNetwork.CurrentRoom.Name);
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("OnJoinedRoom() was called by PUN, room name is: " + PhotonNetwork.CurrentRoom.Name);

        //Load into the game room lobby
        if (SceneManager.GetActiveScene().name != "GameLobby")
        {
            SceneManager.LoadScene("GameLobby");
            Debug.Log("Would be loading level");
        }

        RoomCode = PhotonNetwork.CurrentRoom.Name;
        PlayerList = CallPlayerList();
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        status = "No room found with code " + roomCode + ".";
    }

    public override void OnLeftRoom()
    {
        LoadScene("StartScene");
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        //Update player list
        PlayerList = CallPlayerList();

        status = newPlayer.NickName + " connected!";
    }
    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.Log("Disconnected: " + cause);
        HasNickname = false;
    }
    #endregion





    //Helper functions
    System.Random random = new System.Random();
    string RandomString(int length) //Generates a random string from capital letters.
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        return new string(Enumerable.Repeat(chars, length).Select(s => s[random.Next(s.Length)]).ToArray());
    }

    public string CallPlayerList()
    {
        playerList = new StringBuilder("Player List:\n");
        foreach (var player in PhotonNetwork.PlayerList)
        {
            playerList.Append(player.NickName + "\n");
        }

        //Debug.Log(playerList.ToString());
        return playerList.ToString();
    }
}