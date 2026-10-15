using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class PanelController : MonoBehaviour
{
    private readonly Dictionary<PanelId, GameObject> panels = new();

    private PanelId curPanel = PanelId.None;
    private bool isFail;


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
    }

    private void OnDestroy()
    {
        if (NetworkManager.Instance == null) return;
        NetworkManager.Instance.JoinedLobby -= OpenLobby;
        NetworkManager.Instance.JoinedRoom -= OpenRoom;
    }

    private void AuthFailed(string text)
    {
        isFail = true;
    }

    private void AuthSuccessed(string text)
    {
        isFail = false;
    }

    public async void Open(PanelId main, PanelId sub = PanelId.None)
    {
        // 만약 로그인 실패 시 리턴
        if(curPanel == PanelId.Login && isFail)
        {
            return;
        }

        // 타이틀에서 로그인한 상태면 문제없으면 바로 로비로 이동
        if (curPanel == PanelId.Title)
        {
            await AuthManager.Instance.TryAutoLoginAsync();
            if (AuthManager.Instance.Auth.CurrentUser != null)
            {
                main = PanelId.Lobby;
                sub = PanelId.None;
            }
        }
            
            
        // 타이틀로 돌아가는 경우는 로그아웃
        //if (main == PanelId.Title)
        //    AuthManager.Instance.Auth.SignOut();


        foreach (var (id, go) in panels)
            go.SetActive(id == main || id == sub);

        curPanel = main;
    }

    public async Task<bool> CheckLogIn()
    {
        if (AuthManager.Instance.Auth != null)
            return true;

        
        return false;
    }

}
