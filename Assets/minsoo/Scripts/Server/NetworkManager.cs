using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using System.Threading.Tasks;
using System;
using Unity.VisualScripting;
using System.Collections.Generic;

public enum SceneNumber
{
    Title,
    Lobby,
    Room,
    InGame,

}

public class NetworkManager : MonoBehaviourPunCallbacks
{
    public static NetworkManager Instance;
    ProfileRepository profileRepo;
    
    public SceneNumber sceneNumber;
    public Dictionary<string, RoomInfo> RoomDicts = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
            
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        PhotonNetwork.AutomaticallySyncScene = true;
    }

    public async Task<string> Connect()
    {
        if (PhotonNetwork.IsConnected)
            return "Is already connected.";

        try
        {
            var profile = await profileRepo?.LoadProfileAsync();
            var auth = new AuthenticationValues(AuthManager.Instance.Auth.CurrentUser.UserId);
            PhotonNetwork.AuthValues = auth;
            PhotonNetwork.NickName = profile.Name;
            PhotonNetwork.LocalPlayer
                .SetCustomProperties(new Hashtable 
                {
                    {"rate",  profile.Rate},
                    { "ready", false }
                });

            PhotonNetwork.ConnectUsingSettings();
            return null;
        }
        catch(Exception e)
        {
            return $"Connect Failed : {e.Message}";
        }
    }


    // 로비씬 입장시 UI 캐싱
    public async Task LobbyCaching()
    {
        var profile = await profileRepo?.LoadProfileAsync();
    }

    

    // 방목록 갱신
    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        
        foreach (var room in roomList)
        {
            if(room.RemovedFromList) RoomDicts.Remove(room.Name);
            else RoomDicts[room.Name] = room;
        }
    }

    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinLobby();
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.LogWarning($"Photon Connect Failed : {cause}");
    }

    public override void OnJoinedLobby()
    {
        //로비씬 이동
        PhotonNetwork.LoadLevel((int)SceneNumber.Lobby);
    }

    public override void OnLeftLobby()
    {
        //타이틀씬 이동
        PhotonNetwork.LoadLevel((int)SceneNumber.Title);
    }

    public override void OnCreatedRoom()
    {
        base.OnCreatedRoom();
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        base.OnCreateRoomFailed(returnCode, message);
    }

    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        base.OnJoinRoomFailed(returnCode, message);
    }

    public override void OnLeftRoom()
    {
        base.OnLeftRoom();
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        base.OnPlayerEnteredRoom(newPlayer);
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        base.OnPlayerLeftRoom(otherPlayer);
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        base.OnPlayerPropertiesUpdate(targetPlayer, changedProps);
    }

}