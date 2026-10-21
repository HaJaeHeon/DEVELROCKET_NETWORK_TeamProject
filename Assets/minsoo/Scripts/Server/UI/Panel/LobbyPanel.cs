using Photon.Realtime;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LobbyPanel : UIPanel
{
    [SerializeField] private Button homeButton;
    [SerializeField] private Button settingButton;
    [SerializeField] private Button leaderBoardButton;
    [SerializeField] private Button presetButton;
    [SerializeField] private Button searchRoomButton;
    [SerializeField] private Button createRoomButton;
    [SerializeField] private string lobbySceneName = "EditorScene";

    [SerializeField] Transform content;          // ScrollView의 Content
    [SerializeField] TestRoomItemUI roomItemPrefab;

    readonly Dictionary<string, TestRoomItemUI> items = new();

    [SerializeField] private Button randomJoinButton;
    [SerializeField] private Button refreshButton;

    

    NetworkManager NM => NetworkManager.Instance;

    private void Awake()
    {
        var ui = GetComponentInParent<PanelController>(true);
        homeButton.onClick.AddListener(() => ui.Open(PanelId.Title)); // 타이틀로 가면 로그아웃
        settingButton.onClick.AddListener(() => ui.Open(PanelId.Lobby, PanelId.Setting));
        leaderBoardButton.onClick.AddListener(() => SceneManager.LoadScene(lobbySceneName));
        presetButton.onClick.AddListener(() => ui.Open(PanelId.Lobby, PanelId.Preset));
        searchRoomButton.onClick.AddListener(() => ui.Open(PanelId.Lobby, PanelId.SearchRoom));
        createRoomButton.onClick.AddListener(() => ui.Open(PanelId.Lobby, PanelId.CreateRoom));

        
        randomJoinButton.onClick.AddListener(() => NM.OnRandomJoinRoom());
        refreshButton.onClick.AddListener(() => RefreshRoomInfo(NM.RoomDicts));
    }

    private void OnEnable()
    {
        NM.RoomListChanged += RefreshRoomInfo;
        RefreshRoomInfo(NM.RoomDicts);
    }

    private void OnDisable()
    {
        NM.RoomListChanged -= RefreshRoomInfo;
    }


    private void RefreshRoomInfo(Dictionary<string, RoomInfo> rooms)
    {
        var removed = new List<string>();
        foreach (var name in items.Keys)
            if (!rooms.ContainsKey(name)) removed.Add(name);

        foreach (var name in removed)
        {
            Destroy(items[name].gameObject);
            items.Remove(name);
        }

        foreach (var kv in rooms)
        {
            if (!items.TryGetValue(kv.Key, out var item))
            {
                item = Instantiate(roomItemPrefab, content);
                items[kv.Key] = item;
            }
            item.Set(kv.Value);
        }
    }
}
