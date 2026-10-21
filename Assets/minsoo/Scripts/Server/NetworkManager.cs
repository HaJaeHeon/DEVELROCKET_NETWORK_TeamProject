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

public static class RoomProps
{
    public const string MapIdKey = "MapId";
    public const string RandomId = "Random";
    public const string Locked = "locked";
    public const string Password = "pw";
}



public class NetworkManager : MonoBehaviourPunCallbacks
{
    public static NetworkManager Instance;
    ProfileRepository profileRepo;

    public Dictionary<string, RoomInfo> RoomDicts = new();

    [SerializeField] private MapDB mapDB;
    private bool isStarting;

    public event Action<Dictionary<string, RoomInfo>> RoomListChanged;
    public event Action<Player> PlayerLefted; //플레이어가 나갈 때 이벤트
    public event Action<Player> MasterChanged; //방장이 나갈 때 이벤트

    private int masterActor = -1;      // 현재 방장
    private int prevMasterActor = -1;  // 바로 직전 방장

    private bool goOffline;

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
            CustomRoomProperties = new Hashtable { { "locked", false } },
            CustomRoomPropertiesForLobby = new[] { "locked" },
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
        if (PhotonNetwork.OfflineMode) PhotonNetwork.OfflineMode = false;  // 싱글플레이 후 다시 온라인 로그인
        if (PhotonNetwork.IsConnected) return "Is already connected!";

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
    public void OnCreateRoom(bool isOffline, string roomName, int playerCount, string password)
    {
        if (isOffline)
        {
            OnSiglePlay();
            return;
        }

        bool locked = !string.IsNullOrEmpty(password);
        RoomOptions roomOtions = new RoomOptions
        {
            MaxPlayers = playerCount,
            CustomRoomProperties = new Hashtable
            {
                { RoomProps.Locked, locked },
                { RoomProps.Password, locked ? password : "" }
            },
            CustomRoomPropertiesForLobby = new[] { RoomProps.Locked, RoomProps.Password },
            IsVisible = true,
            IsOpen = true,
        };

        PhotonNetwork.CreateRoom(roomName ?? null, roomOtions);

    }

    public bool CheckRoomPassword(RoomInfo info, string input)
    {
        return info.CustomProperties.TryGetValue(RoomProps.Password, out object v)
            && v is string pw
            && pw == input;
    }

    public void OnSiglePlay()
    {
        goOffline = true;
        if (PhotonNetwork.NetworkingClient.IsConnected) PhotonNetwork.Disconnect(); // 끊긴 뒤 OnDisconnected에서 오프라인 전환
        else PhotonNetwork.OfflineMode = true;                                       // 원래 연결이 없으면 바로 전환
    }

    // 로비 - 방 랜덤입장
    public void OnRandomJoinRoom()
    {
        PhotonNetwork.JoinRandomOrCreateRoom
        (
            expectedCustomRoomProperties: new Hashtable { { RoomProps.Locked, false } },
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
        PlayerLefted?.Invoke(otherPlayer);
        // OnMasterClientSwitched가 항상 먼저 호출되므로, 직전 방장과 비교
        if (otherPlayer.ActorNumber == prevMasterActor)
        {
            prevMasterActor = -1;
            MasterChanged?.Invoke(otherPlayer);
        }
    }

    // 방장이 나가면 방장 표시와 시작 버튼이 바뀌어야 함
    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        prevMasterActor = masterActor;
        masterActor = newMasterClient.ActorNumber;
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
        if (!PhotonNetwork.IsMasterClient || isStarting)
        {
            Debug.Log("방장이 아닙니다.");
            return;
        }

        if (!AllReady())
        {
            Debug.Log("모든 플레이어가 준비되지 않았습니다.");
            return;
        }

        string mapId = GetMapId();

        // 이미 실제 맵이 선택돼 있으면 바로 이동
        if (mapId != RoomProps.RandomId)
        {
            isStarting = true;
            PhotonNetwork.CurrentRoom.IsOpen = false;
            PhotonNetwork.LoadLevel("GameScene");
            return;
        }

        // Random → 하나 골라서 저장하고, 반영될 때까지 기다림
        if (mapDB.MapDatas.Count == 0) { Debug.LogError("MapDB에 맵이 없습니다."); return; }
        string pickedId = mapDB.MapDatas[UnityEngine.Random.Range(0, mapDB.MapDatas.Count)].Id;

        isStarting = true;
        PhotonNetwork.CurrentRoom.IsOpen = false;
        if (!PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable { { RoomProps.MapIdKey, pickedId } }))
        {
            isStarting = false;
            PhotonNetwork.CurrentRoom.IsOpen = true;
        }

        //PhotonNetwork.LoadLevel("GameScene");
    }


    public string GetMapId()
    {
        var room = PhotonNetwork.CurrentRoom;
        if (room != null && room.CustomProperties.TryGetValue(RoomProps.MapIdKey, out object v) && v is string id)
            return id;
        return "Random";
    }

    public override void OnRoomPropertiesUpdate(Hashtable changed)
    {
        if (!isStarting || !PhotonNetwork.IsMasterClient) return;

        if (changed.TryGetValue(RoomProps.MapIdKey, out object v) && v is string id && id != RoomProps.RandomId)
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
        if (PhotonNetwork.OfflineMode)
            PhotonNetwork.CreateRoom(null);
        else
            PhotonNetwork.JoinLobby();
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        RoomDicts.Clear();
        if (goOffline)
        {
            goOffline = false;
            PhotonNetwork.OfflineMode = true;
        }
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
        masterActor = PhotonNetwork.MasterClient.ActorNumber;
        prevMasterActor = -1;

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
        new Hashtable { { PlayerProps.Ready, false } });
        RoomListChanged?.Invoke(RoomDicts);

        Scene currentScene = SceneManager.GetActiveScene();

        if (currentScene.name == "GameScene")
            SceneManager.LoadScene("StartScene");

        isStarting = false;
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

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F9)) PhotonNetwork.LeaveRoom();   // 게임 중 나가기
        if (Input.GetKeyDown(KeyCode.F10)) PhotonNetwork.Disconnect(); // 연결 끊김
    }
}