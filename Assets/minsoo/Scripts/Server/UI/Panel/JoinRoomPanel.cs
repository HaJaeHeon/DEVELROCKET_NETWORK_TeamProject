using Photon.Pun;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class JoinRoomPanel : UIPanel
{
    [SerializeField] private TMP_InputField passwordField;
    [SerializeField] private Button joinButton;
    [SerializeField] private GameObject PopupPanel;
    [SerializeField] private TMP_Text messageText;   // 새로 추가 → Inspector 연결 필요

    private PanelController ui;
    NetworkManager NM => NetworkManager.Instance;
    WaitForSeconds wait = new WaitForSeconds(1.0f);

    private void Awake()
    {
        ui = GetComponentInParent<PanelController>(true);
        joinButton.onClick.AddListener(OnJoinRoom);
    }

    private void OnEnable()
    {
        // 팝업 다시 열 때 이전 입력/메시지 초기화
        passwordField.text = "";
        messageText.text = "";
    }

    private void OnJoinRoom()
    {
        string roomName = ui.LoadRoom();

        if (!NM.RoomDicts.TryGetValue(roomName, out var info))
        {
            ShowNotice("The room does not exist.");
            return;
        }

        if (!NM.CheckRoomPassword(info, passwordField.text))
        {
            ShowNotice("The password is incorrect.");
            return;
        }

        PhotonNetwork.JoinRoom(roomName);
    }


    private void ShowNotice(string message)
    {
        PopupPanel.SetActive(true);
        messageText.text = message;

        StartCoroutine("ClosePopup");
    }

    private IEnumerator ClosePopup()
    {
        yield return wait;
        PopupPanel.SetActive(false);
    }
}
