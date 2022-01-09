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
    NetworkController NC;
    PhotonView PV;
    public List<GameObject> Controllers;

    // Start is called before the first frame update
    void Start()
    {
        NC = GameObject.FindGameObjectWithTag("NetworkController").GetComponent<NetworkController>();
        GameObject SpawnedObject = PhotonNetwork.Instantiate("GameController", Vector3.zero, Quaternion.identity);
    }

    private void LateUpdate()
    {
        CheckListChange();
    }

    public void AddController(GameObject controller)
    {
        Controllers.Add(controller);
    }

    void CheckListChange()
    {
        foreach(GameObject obj in Controllers)
        {
            if(obj == null)
            {
                Debug.Log("Controller missing, possibly disconnected. Removing!");
                Controllers.Remove(obj);
                CheckEnoughPlayers();
            }
        }
    }

    void CheckEnoughPlayers()
    {
        if (Controllers.Count <= 1)
        {
            NC.DisplayErrorMessage("Not enough players, returned to menu!");
            NC.LeaveRoom();
        }
    }
}
