using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TestRoomItemUI : MonoBehaviour
{
    [SerializeField] TMP_Text nameText;
    [SerializeField] TMP_Text countText;
    [SerializeField] Button joinButton;

    string roomName;

    void Awake()
    {
        joinButton.onClick.AddListener(() => PhotonNetwork.JoinRoom(roomName));
    }

    public void Set(RoomInfo info)
    {
        roomName = info.Name;
        nameText.text = info.Name;
        countText.text = $"{info.PlayerCount} / {info.MaxPlayers}";
        joinButton.interactable = info.IsOpen && info.PlayerCount < info.MaxPlayers;
    }
}
