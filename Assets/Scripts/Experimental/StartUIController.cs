using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using TMPro;
using UnityEngine.UI;

public class StartUIController : MonoBehaviour
{
    public NetworkController NC;

    [Header("UI Linking")]
    public GameObject ConnectionPanel;
    public TMP_InputField NicknameField;
    public TMP_InputField RoomCodeField;
    public TextMeshProUGUI NameText;
    public TextMeshProUGUI DescriptionText;

    // Start is called before the first frame update
    void Start()
    {
        NC = GameObject.FindGameObjectWithTag("NetworkController").GetComponent<NetworkController>();

        ConnectionPanel.SetActive(NC.HasNickname);
    }

    // Update is called once per frame
    void LateUpdate()
    {
        ConnectionPanel.SetActive(NC.HasNickname);
        UpdateNameValues();
    }

    public void EnterNickname()
    {
        NC.SetNickname(NicknameField.text);
        UpdateNameValues();

        ConnectionPanel.SetActive(NC.HasNickname);
    }

    public void JoinRoomWithCode()
    {
        NC.JoinRoomWithCode(RoomCodeField.text);
    }

    public void CreateRoom()
    {
        NC.CreateRoom();
    }

    void UpdateNameValues()
    {
        NameText.text = NC.Nickname;
        DescriptionText.text = NC.PlayerDescription;
    }
}
