using Photon.Pun;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class PanelController : MonoBehaviour
{
    private readonly Dictionary<PanelId, GameObject> panels = new();

    private string roomName;

    private void Awake()
    {
        foreach (var p in GetComponentsInChildren<UIPanel>(true))
            panels[p.Id] = p.gameObject;
    }

    private void OpenLobby() => Open(PanelId.Lobby);
    private void OpenRoom() => Open(PanelId.Room);

    private void Start()
    {
        NetworkManager.Instance.JoinedLobby += OpenLobby;
        NetworkManager.Instance.JoinedRoom += OpenRoom;

        // 다른 씬(맵 에디터 등)에서 돌아온 경우: 이미 로비/방에 있으면 바로 그 화면으로
        if (PhotonNetwork.InRoom) Open(PanelId.Room);
        else if (PhotonNetwork.InLobby) Open(PanelId.Lobby);
    }

    private void OnDestroy()
    {
        if (NetworkManager.Instance == null) return;
        NetworkManager.Instance.JoinedLobby -= OpenLobby;
        NetworkManager.Instance.JoinedRoom -= OpenRoom;
    }

    public async void Open(PanelId main, PanelId sub = PanelId.None)
    {             
        // 타이틀로 돌아가는 경우는 로그아웃
        if (main == PanelId.Title)
        {
            AuthManager.Instance.SignOut();
            if (PhotonNetwork.IsConnected)
                PhotonNetwork.Disconnect();
        }
            
        foreach (var (id, go) in panels)
            go.SetActive(id == main || id == sub);
    }


    public void SaveRoom(string name)
    {
        roomName = name;
    }

    public string LoadRoom()
    {
        return roomName;
    }

    public async Task<bool> CheckLogIn()
    {
        if (AuthManager.Instance.Auth != null)
            return true;

        
        return false;
    }

}
