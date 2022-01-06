using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class GameOverMenu : MonoBehaviour
{
    public PhotonView PV;

    // Start is called before the first frame update
    void Start()
    {
        this.gameObject.SetActive(false);
    }
}
