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
