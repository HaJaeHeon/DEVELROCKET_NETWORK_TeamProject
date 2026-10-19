using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

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

    public event Action JoinedLobby;
    public event Action JoinedRoom;

    public override void OnJoinedLobby() => JoinedLobby?.Invoke();

    private void Start()
    {
        profileRepo = new ProfileRepository();
        roomOptions = new RoomOptions
        {
            MaxPlayers = 4,
            IsVisible = true,
            IsOpen = true,
        };
        PhotonNetwork.AutomaticallySyncScene = true;

        AuthManager.Instance.OnSignedIn += OnFirebaseSignedIn;
    }

    private async void OnFirebaseSignedIn(Firebase.Auth.FirebaseUser user)
    {
        string error = await Connect();
        if (!string.IsNullOrEmpty(error))
            Debug.LogWarning(error);
    }

    // Photon 연결
    public async Task<string> Connect()
    {
        if (PhotonNetwork.IsConnected)
            return "Is already connected!";

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
    public void OnCreateRoom(bool isOffline, string roomName, int playerCount, string password, bool isVisible, bool isOpen)
    {
        if (isOffline)
        {
            OnSiglePlay();
            return;
        }
            

        RoomOptions roomOtions = new RoomOptions
        {
            MaxPlayers = playerCount,
            CustomRoomProperties = new Hashtable { { "pw", password }, { "locked", true } },
            CustomRoomPropertiesForLobby = new[] { "locked" },
            IsVisible = isVisible,
            IsOpen = isOpen,
        };

        PhotonNetwork.CreateRoom(roomName ?? null, roomOtions);

    }

    public void OnSiglePlay()
    {
        PhotonNetwork.Disconnect();
        PhotonNetwork.OfflineMode = true;
    }

    // 로비 - 방 랜덤입장
    public void OnRandomJoinRoom()
    {
        PhotonNetwork.JoinRandomOrCreateRoom(
        expectedCustomRoomProperties: null,
        expectedMaxPlayers: maxPlayers,
        roomOptions: roomOptions
        );
    }

    // 방 인원 / 준비 상태 / 방장이 바뀔 때마다 알림
    public event Action RoomPlayersChanged;

    // static으로 바꾸고 public으로 → UI에서도 사용
    public static bool IsReady(Player player) =>
        player.CustomProperties.TryGetValue(PlayerProps.Ready, out object v) && v is bool b && b;

    public bool AllReady()
    {
        foreach (Player p in PhotonNetwork.PlayerList)
            if (!IsReady(p)) return false;
        return true;
    }

    public void OnReadyGame()
    {
        if (!PhotonNetwork.InRoom) return;
        bool ready = IsReady(PhotonNetwork.LocalPlayer);
        PhotonNetwork.LocalPlayer.SetCustomProperties(
            new Hashtable { { PlayerProps.Ready, !ready } });
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log($"선수 입장 : {newPlayer.NickName}");
        RoomPlayersChanged?.Invoke();
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        Debug.Log($"선수 퇴장 : {otherPlayer.NickName}");
        RoomPlayersChanged?.Invoke();
    }

    // 방장이 나가면 방장 표시와 시작 버튼이 바뀌어야 함
    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        RoomPlayersChanged?.Invoke();
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        if (changedProps.ContainsKey(PlayerProps.Ready))
            RoomPlayersChanged?.Invoke();
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
        PhotonNetwork.LoadLevel("GameScene");
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
        if(PhotonNetwork.OfflineMode)
            PhotonNetwork.CreateRoom(null);
        else
            PhotonNetwork.JoinLobby();
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        RoomDicts.Clear();
        Debug.LogWarning($"Photon Connect Failed : {cause}");
    }


    public override void OnLeftLobby()
    {
        RoomDicts.Clear();
    }

    public override void OnCreatedRoom()
    {
        base.OnCreatedRoom();
    }


    public override void OnJoinedRoom()
    {
        RoomDicts.Clear();
        JoinedRoom?.Invoke();
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
        RoomListChanged?.Invoke(RoomDicts);

        Scene currentScene = SceneManager.GetActiveScene();

        if(currentScene.name == "GameScene")
            SceneManager.LoadScene("StartScene");
    }
    /*
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
    */
    private void OnDestroy()
    {
        if (AuthManager.Instance != null)
            AuthManager.Instance.OnSignedIn -= OnFirebaseSignedIn;
    }
}