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
public class NetworkUtility : MonoBehaviour
{
    PhotonView PV;
    public List<GameObject> Controllers;

    public GameObject[] Cards = new GameObject[6];
    public Sprite[] Sprites = new Sprite[6];
    public Sprite CardBack;
    public List<int> cardsHand = new List<int> { 0, 1, 2, 3, 4, 5 };
    public List<int> cardsShuffled;
    public int DrawCount = 0;

    public TextMeshProUGUI ScoreText;
    public TextMeshProUGUI PotText;
    public int Pot = 12;

    public AudioClip Gunshot, Hammer, Prepare;

    // Start is called before the first frame update
    void Start()
    {
        GameObject SpawnedObject = PhotonNetwork.Instantiate("GameController", Vector3.zero, Quaternion.identity);
    }

    public void AddController(GameObject controller)
    {
        Controllers.Add(controller);
    }
}
