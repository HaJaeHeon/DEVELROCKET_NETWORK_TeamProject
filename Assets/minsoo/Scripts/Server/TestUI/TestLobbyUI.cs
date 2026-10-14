using Photon.Realtime;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TestLobbyUI : MonoBehaviour
{
    [SerializeField] Transform content;          // ScrollView의 Content
    [SerializeField] TestRoomItemUI roomItemPrefab;

    readonly Dictionary<string, TestRoomItemUI> items = new();

    [SerializeField] private Button searchButton;
    [SerializeField] private Button createButton;
    [SerializeField] private Button randomJoinButton;
    [SerializeField] private Button refreshButton;

    [SerializeField] private TMP_InputField roomNameField;

    NetworkManager NM => NetworkManager.Instance;

    private void Awake()
    {
        searchButton.onClick.AddListener(() => NM.OnSearchRoom());
        createButton.onClick.AddListener(() => NM.OnCreateRoom(roomNameField.text));
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
