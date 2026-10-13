using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public static class PlayerProps 
{ 
    public const string Ready = "ready"; 
    public const string Rate = "rate"; 
}

public class NetworkManager : MonoBehaviourPunCallbacks
{
    public static NetworkManager Instance;
    ProfileRepository profileRepo;

    public Dictionary<string, RoomInfo> RoomDicts = new();

    public event Action<Dictionary<string, RoomInfo>> RoomListChanged;

    string ReadyKey = "ready";

    RoomOptions roomOptions;
    byte maxPlayers = 4;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;

        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        profileRepo = new ProfileRepository();
        roomOptions = new RoomOptions
        {
            MaxPlayers = 4,
            IsVisible = true,
            IsOpen = true,
        };
    }

    // Photon 연결
    public async Task<string> Connect()
    {
        if (PhotonNetwork.IsConnected)
            return "Is already connected.";

        try
        {
            var profile = await profileRepo.LoadProfileAsync();
            var auth = new AuthenticationValues(AuthManager.Instance.Auth.CurrentUser.UserId);
            PhotonNetwork.AuthValues = auth;
            PhotonNetwork.NickName = profile.Name;
            PhotonNetwork.LocalPlayer
                .SetCustomProperties(new Hashtable
                {
                    { PlayerProps.Rate,  profile.Rate},
                    { PlayerProps.Ready, false }
                });

            PhotonNetwork.ConnectUsingSettings();
            return null;
        }
        catch (Exception e)
        {
            return $"Connect Failed : {e.Message}";
        }
    }

    // 로비 - 방 검색
    public void OnSearchRoom()
    {

    }
    // 로비 - 방 생성
    public void OnCreateRoom(string roomName)
    {
        PhotonNetwork.CreateRoom(roomName, roomOptions);

    }
    // 로비 - 방 랜덤입장
    public void OnRandomJoinRoom()
    {
        /*
        var props = new Hashtable { { "map", "desert" }, { "mode", 1 } };
        var roomOptions = new RoomOptions
        {
            MaxPlayers = maxPlayers,
            //CustomRoomProperties = props,
            //CustomRoomPropertiesForLobby = new[] { "map", "mode" },
            IsVisible = true,
            IsOpen = true
        };
        */

        PhotonNetwork.JoinRandomOrCreateRoom(
        expectedCustomRoomProperties: null,
        expectedMaxPlayers: maxPlayers,
        roomOptions: roomOptions
        );
    }

    // 게임 준비
    public void OnReadyGame()
    {
        if (!PhotonNetwork.InRoom)
            return;

        bool ready = IsReady(PhotonNetwork.LocalPlayer);

        PhotonNetwork.LocalPlayer.SetCustomProperties(
            new Hashtable { { ReadyKey, !ready } });
    }

    // 게임 시작
    public void OnStartGame()
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            Debug.Log("방장이 아닙니다.");
            return;
        }

        if (!AllReady())
        {
            Debug.Log("모든 플레이어가 준비되지 않았습니다.");
            return;
        }

        PhotonNetwork.CurrentRoom.IsOpen = false;
        PhotonNetwork.LoadLevel((int)SceneNumber.InGame);
    }

    // 플레이어의 레디 상태를 반환
    private bool IsReady(Player player)
    {
        if (player.CustomProperties.TryGetValue(ReadyKey, out object value))
            return (bool)value;
        return false;
    }

    // 전체 플레이어의 레디 상태를 확인 -> 한 명이라도 준비상태가 아니면 false
    private bool AllReady()
    {
        foreach (Player p in PhotonNetwork.PlayerList)
        {
            if (!IsReady(p))
            {
                return false;
            }
        }
        return true;
    }

    // 방목록 갱신
    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {

        foreach (var room in roomList)
        {
            if (room.RemovedFromList) RoomDicts.Remove(room.Name);
            else RoomDicts[room.Name] = room;
        }
        RoomListChanged?.Invoke(RoomDicts);
    }

    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinLobby();
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        RoomDicts.Clear();
        Debug.LogWarning($"Photon Connect Failed : {cause}");
    }

    public override async void OnJoinedLobby()
    {
        PhotonNetwork.LoadLevel((int)SceneNumber.Lobby);
    }

    public override void OnLeftLobby()
    {
        RoomDicts.Clear();
        PhotonNetwork.LoadLevel((int)SceneNumber.Title);
    }

    public override void OnCreatedRoom()
    {
        base.OnCreatedRoom();
    }

    public override void OnJoinedRoom()
    {
        RoomDicts.Clear();
        foreach (var p in PhotonNetwork.PlayerList)
            UpdateReadyUI(p, IsReady(p));
        PhotonNetwork.LoadLevel((int)SceneNumber.Room);
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        base.OnCreateRoomFailed(returnCode, message);
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        base.OnJoinRoomFailed(returnCode, message);
    }

    public override void OnLeftRoom()
    {
        PhotonNetwork.LocalPlayer.SetCustomProperties(
        new Hashtable { { ReadyKey, false } });
        PhotonNetwork.LoadLevel((int)SceneNumber.Lobby);
        RoomListChanged?.Invoke(RoomDicts);
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log($"선수 입장 : {newPlayer.NickName}");
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        Debug.Log($"선수 퇴장 : {otherPlayer.NickName}");
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        if (!changedProps.ContainsKey(ReadyKey)) return;

        bool isReady = (bool)changedProps[ReadyKey];

        UpdateReadyUI(targetPlayer, isReady);
    }


    public void UpdateReadyUI(Player player, bool isReady)
    {
        //TODO 방 플레이어 상태 갱신
    }
}