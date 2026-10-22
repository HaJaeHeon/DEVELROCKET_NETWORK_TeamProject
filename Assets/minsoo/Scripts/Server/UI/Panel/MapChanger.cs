using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MapChanger : MonoBehaviourPunCallbacks
{
    [Header("UI")]
    [SerializeField] private TMP_Dropdown mapsDropdown;
    [SerializeField] private Image mapImage;
    [SerializeField] private Sprite randomThumbnail;
    [SerializeField] private Sprite userMapThumbnail;   // 비어 있으면 randomThumbnail 사용

    [Header("MapDB")]
    [SerializeField] private MapDB mapDB;

    // 드롭다운 순서: Random, MapDB 맵들, 게시된 유저 맵들
    private readonly List<UserMapData> userMaps = new();
    private UserMapRepository repo;
    private UserMapRepository Repo => repo ??= new UserMapRepository();
    private bool isLoading;   // 유저 맵 내용을 받아오는 중

    private void Awake()
    {
        RebuildOptions();
        mapsDropdown.onValueChanged.AddListener(OnSelectMap);
    }

    private void RebuildOptions()
    {
        var options = new List<string> { "Random" };

        foreach (var map in mapDB.MapDatas)
            options.Add(map.Name);

        foreach (var map in userMaps)
            options.Add($"{map.Name} (by {map.AuthorName})");

        mapsDropdown.ClearOptions();
        mapsDropdown.AddOptions(options);
    }

    // 게시된 유저 맵 목록을 받아와 드롭다운에 추가
    private async void RefreshUserMaps()
    {
        try
        {
            var maps = await Repo.ListPublishedAsync();
            if (this == null) return;   // 기다리는 동안 파괴됨

            userMaps.Clear();
            userMaps.AddRange(maps);
            RebuildOptions();
            ApplyFromRoom();
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"유저 맵 목록을 불러오지 못했습니다 : {e.Message}");
        }
    }

    private void OnSelectMap(int index)
    {
        if (!PhotonNetwork.IsMasterClient) return;

        int userIndex = index - 1 - mapDB.MapDatas.Count;
        if (userIndex >= 0)
        {
            SelectUserMap(userMaps[userIndex]);
            return;
        }

        string id = index == 0 ? RoomProps.RandomId : mapDB.MapDatas[index - 1].Id;
        PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable
        {
            { RoomProps.MapIdKey, id },
            { RoomProps.MapName, null },   // 이전 유저 맵 내용 정리
            { RoomProps.MapWidth, null },
            { RoomProps.MapHeight, null },
            { RoomProps.MapCells, null },
        });
    }

    // 유저 맵은 내용(cells)까지 받아서 방에 한 번에 넣음 → 게임 씬에서 바로 맵 생성 가능
    private async void SelectUserMap(UserMapData summary)
    {
        isLoading = true;
        UpdateInteractable();
        try
        {
            var map = await Repo.LoadAsync(summary.Id);
            if (this == null) return;
            if (!PhotonNetwork.InRoom || !PhotonNetwork.IsMasterClient) return;   // 기다리는 동안 방장이 바뀜

            if (map == null || !map.Published)
            {
                Debug.LogError($"공개된 맵을 찾을 수 없습니다 : {summary.Name}");
                return;
            }

            var errors = MapValidator.Validate(map.ToGrid());
            if (errors.Count > 0)
            {
                Debug.LogError($"맵 검사 실패 : {summary.Name}\n{string.Join("\n", errors)}");
                return;
            }

            PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable
            {
                { RoomProps.MapIdKey, RoomProps.UserMapPrefix + map.Id },
                { RoomProps.MapName, map.Name },
                { RoomProps.MapWidth, map.Width },
                { RoomProps.MapHeight, map.Height },
                { RoomProps.MapCells, map.Cells },
            });
        }
        catch (System.Exception e)
        {
            Debug.LogError(e);
        }
        finally
        {
            if (this != null)
            {
                isLoading = false;
                UpdateInteractable();
                ApplyFromRoom();   // 실패했으면 이전 선택으로 되돌림
            }
        }
    }

    public override void OnRoomPropertiesUpdate(Hashtable changed)
    {
        if (changed.ContainsKey(RoomProps.MapIdKey)) ApplyFromRoom();
    }

    // 방장이 바뀌면 조작 권한 갱신
    public override void OnMasterClientSwitched(Player newMaster) => UpdateInteractable();

    public override void OnEnable()
    {
        base.OnEnable();
        UpdateInteractable();
        ApplyFromRoom();
        RefreshUserMaps();
    }

    private void UpdateInteractable() => mapsDropdown.interactable = PhotonNetwork.IsMasterClient && !isLoading;

    private void ApplyFromRoom()
    {
        var room = PhotonNetwork.CurrentRoom;
        string id = room != null && room.CustomProperties.TryGetValue(RoomProps.MapIdKey, out var v) ? v as string : RoomProps.RandomId;

        int index = 0;   // Random
        Sprite thumbnail = randomThumbnail;
        for (int i = 0; i < mapDB.MapDatas.Count; i++)
            if (mapDB.MapDatas[i].Id == id) { index = i + 1; thumbnail = mapDB.MapDatas[i].Thumbnail; break; }

        if (RoomProps.IsUserMap(id))
        {
            thumbnail = userMapThumbnail != null ? userMapThumbnail : randomThumbnail;
            for (int i = 0; i < userMaps.Count; i++)
                if (RoomProps.UserMapPrefix + userMaps[i].Id == id) { index = 1 + mapDB.MapDatas.Count + i; break; }
        }

        mapsDropdown.SetValueWithoutNotify(index);

        // 목록에 아직 없는 유저 맵이면 방에 저장된 이름만 표시
        if (index == 0 && RoomProps.IsUserMap(id))
            mapsDropdown.captionText.text = room.CustomProperties.TryGetValue(RoomProps.MapName, out var n) ? n as string : "User Map";

        mapImage.sprite = thumbnail;
        mapImage.SetNativeSize();
    }

    private void OnDestroy()
    {
        mapsDropdown.onValueChanged.RemoveAllListeners();
    }
}
