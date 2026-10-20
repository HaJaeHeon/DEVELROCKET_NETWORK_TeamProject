using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TestRoomItemUI : MonoBehaviour
{
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text countText;
    [SerializeField] private Button joinButton;

    [SerializeField] private Image lockImage;
    [SerializeField] private List<Sprite> lockSprite;

    private bool isPrivate;

    void Awake()
    {
        joinButton.onClick.AddListener(OnJoinRoom);
    }

    private void OnJoinRoom()
    {
        if (isPrivate)
        {
            var ui = GetComponentInParent<PanelController>(true);
            ui.SaveRoom(nameText.text);
            ui.Open(PanelId.Lobby, PanelId.JoinRoom);
        }
        else
            PhotonNetwork.JoinRoom(nameText.text);
    }

    public void Set(RoomInfo info)
    {
        nameText.text = info.Name;
        countText.text = $"{info.PlayerCount} / {info.MaxPlayers}";
        joinButton.interactable = info.IsOpen && info.PlayerCount < info.MaxPlayers;
        isPrivate = info.CustomProperties.TryGetValue(RoomProps.Locked, out object v) && v is bool b && b;
        if (isPrivate)
            lockImage.sprite = lockSprite[1];
        else
            lockImage.sprite = lockSprite[0];

    }
}
