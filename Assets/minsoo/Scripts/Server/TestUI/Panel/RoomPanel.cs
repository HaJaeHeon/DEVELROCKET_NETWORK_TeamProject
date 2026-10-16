using Photon.Pun;
using TMPro;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.tvOS;
using UnityEngine.UI;
using static UnityEditor.Progress;

public class RoomPanel : UIPanel
{
    [SerializeField] private Button homeButton;
    [SerializeField] private Button settingButton;
    [SerializeField] private Button leaderBoardButton;
    [SerializeField] private Button presetButton;

    [Header("방")]
    [SerializeField] private Button readyButton;
    [SerializeField] private TMP_Text readyButtonText;
    [SerializeField] private Button gameStartButton;
    [SerializeField] private RoomPlayerSlot playerPrefabs;
    [SerializeField] private Transform playerPanel;

    NetworkManager NM => NetworkManager.Instance;

    private void Awake()
    {
        var ui = GetComponentInParent<PanelController>(true);
        homeButton.onClick.AddListener(() => ui.Open(PanelId.Title)); // 타이틀로 가면 로그아웃
        settingButton.onClick.AddListener(() => ui.Open(PanelId.Room, PanelId.Setting));
        leaderBoardButton.onClick.AddListener(() => ui.Open(PanelId.Room, PanelId.LeaderBoard));
        presetButton.onClick.AddListener(() => ui.Open(PanelId.Room, PanelId.Preset));

        
        
    }

    private void OnEnable()
    {
        NM.RoomPlayersChanged += Refresh;
        readyButton.onClick.AddListener(() => NM.OnReadyGame());
        gameStartButton.onClick.AddListener(() => NM.OnStartGame());
        Refresh();                     // 내가 들어온 순간 현재 인원으로 한 번 그림
    }

    private void OnDisable()
    {
        NM.RoomPlayersChanged -= Refresh;
    }

    [ContextMenu("Refresh")]
    private void Refresh()
    {
        if (!PhotonNetwork.InRoom) return;
        var players = PhotonNetwork.PlayerList;
        foreach (var name in playerPanel.GetComponentsInChildren<RoomPlayerSlot>())
        {
            Destroy(name.gameObject);
        }

           // ActorNumber 순 = 들어온 순서
        for (int i = 0; i < players.Length; i++)
        {
            var go = Instantiate(playerPrefabs, playerPanel);
            go.Set(i < players.Length ? players[i] : null);
        }
            

        bool myReady = NetworkManager.IsReady(PhotonNetwork.LocalPlayer);
        readyButtonText.text = myReady ? "Cancel" : "Ready";

        gameStartButton.gameObject.SetActive(PhotonNetwork.IsMasterClient);
        gameStartButton.interactable = NM.AllReady();
    }
}
