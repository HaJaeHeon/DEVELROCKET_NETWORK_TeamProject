using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class TestNetworkConnecter : MonoBehaviourPunCallbacks
{
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private Button connectButton;
    [SerializeField] private TMP_Text buttonText;
    [SerializeField] private TMP_Dropdown sceneDropdown;

    public int SelectedBuildIndex => sceneDropdown.value;

    private void Awake()
    {
        OnSceneDropdown();
        PhotonNetwork.AutomaticallySyncScene = true;
        connectButton.onClick.AddListener(() => ConnectInGame());
        SetStatus("Disconnect");
    }

    private void Start()
    {
        SetStatus("Connecting...");
        PhotonNetwork.ConnectUsingSettings();
    }


    private void OnSceneDropdown()
    {
        var names = new List<string>();
        int count = SceneManager.sceneCountInBuildSettings;

        for (int i = 0; i < count; i++)
        {
            string path = SceneUtility.GetScenePathByBuildIndex(i);
            names.Add(Path.GetFileNameWithoutExtension(path));
        }

        sceneDropdown.ClearOptions();
        sceneDropdown.AddOptions(names);
    }

    public void ConnectInGame()
    {
        if (!PhotonNetwork.InRoom)
        {
            connectButton.interactable = false;
            PhotonNetwork.JoinOrCreateRoom("TestRoom", new RoomOptions { MaxPlayers = 4 }, TypedLobby.Default);
        }
        else if (PhotonNetwork.IsMasterClient)
        {
            connectButton.interactable = false;
            PhotonNetwork.LoadLevel(SelectedBuildIndex);
        }

    }

    public override void OnConnectedToMaster()
    {
        SetStatus("Connect!");
    }

    public override void OnCreatedRoom()
    {
        base.OnCreatedRoom();
    }

    public override void OnJoinedRoom()
    {
        SetStatus($"JoinRoom ({PhotonNetwork.CurrentRoom.PlayerCount})");
        if (PhotonNetwork.IsMasterClient)
        {
            buttonText.text = "InGame";
            connectButton.interactable = true;
        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        SetStatus($"JoinRoom ({PhotonNetwork.CurrentRoom.PlayerCount})");
        Debug.Log("Player 입장");
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        SetStatus($"JoinRoom ({PhotonNetwork.CurrentRoom.PlayerCount})");
        Debug.Log("Player 퇴장");
    }

    public void SetStatus(string text)
    {
        statusText.text = text;
    }
}
