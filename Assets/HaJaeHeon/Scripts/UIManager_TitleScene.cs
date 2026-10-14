using System.Collections.Generic;
using UnityEngine;

public class UIManager_TitleScene : MonoBehaviour
{
    //public enum MainUIs
    //{
    //    Title,
    //    Login,
    //    Lobby,
    //    Room,
    //}

    //public enum SubUIs
    //{
    //    SignUp,
    //    Settings,
    //    LeaderBoard,
    //    CreateRoom,
    //    SearchRoom,
    //    TowerPreset,
    //    GameStart
    //}

    //private enum TitleSubUIs
    //{
    //    SignUp,
    //    Settings,
    //}

    //private enum LobbySubUIs
    //{
    //    TowerPreset,
    //    CreateRoom,
    //    SearchRoom,
    //    LeaderBoard,
    //}

    //private enum RoomSubUIs
    //{
    //    GameStart,
    //    TowerPreset,
    //    LeaderBoard,
    //    Settings,
    //}

    //private MainUIs currentMainUIs;
    //private TitleSubUIs currentTitleSubUIs;
    //private LobbySubUIs currentLobbySubUIs;
    //private RoomSubUIs currentRoomSubUIs;



    [SerializeField] private GameObject titlePanel;
    [SerializeField] private GameObject loginPanel;
    [SerializeField] private GameObject signUpPanel;
    [SerializeField] private GameObject lobbyPanel;
    [SerializeField] private GameObject createRoomPanel;
    [SerializeField] private GameObject searchRoomPanel;
    [SerializeField] private GameObject leaderBoardPanel;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject roomPanel;
    [SerializeField] private GameObject TowerPresetPanel;

    public List<GameObject> mainPanelList = new List<GameObject>();
    public List<GameObject> subPanelList = new List<GameObject>();

    //private void SwitchMainPanels(MainUIs uis)
    //{
    //    switch(uis)
    //    {
    //        case MainUIs.Title:
    //            break;
    //        case MainUIs.Login:
    //            break;
    //        case MainUIs.Lobby:
    //            break;
    //        case MainUIs.Room:
    //            break;
    //    }
    //}

    //private void SwitchTitleSubPanel(TitleSubUIs uis)
    //{
    //    switch(uis)
    //    {
    //        case TitleSubUIs.SignUp:
    //            break;
    //        case TitleSubUIs.Settings:
    //            break;
    //    }
    //}

    //private void SwitchLobbyPanels(LobbySubUIs uis)
    //{
    //    switch(uis)
    //    {
    //        case LobbySubUIs.TowerPreset:
    //            break;
    //        case LobbySubUIs.CreateRoom:
    //            break;
    //        case LobbySubUIs.SearchRoom:
    //            break;
    //        case LobbySubUIs.LeaderBoard:
    //            break;
    //    }
    //}

    //private void SwitchRoomPanel(RoomSubUIs uis)
    //{
    //    switch(uis)
    //    {
    //        case RoomSubUIs.GameStart:
    //            break;
    //        case RoomSubUIs.TowerPreset:
    //            break;
    //        case RoomSubUIs.Settings:
    //            break;
    //        case RoomSubUIs.LeaderBoard:
    //            break;
    //    }
    //}


    // 버튼 클릭 시 호출 될 함수들=================
    private void OnClickButton(GameObject main)
    {
        mainPanelList.ForEach(panel => panel.SetActive(false));

        main.SetActive(true);
    }

    private void OnClickButton(GameObject main, GameObject sub)
    {
        subPanelList.ForEach(panel => panel.SetActive(false));

        main.SetActive(true);
        sub.SetActive(true);
    }

    public void OnClickCloseSubPanel()
    {
        subPanelList.ForEach(panel => panel.SetActive(false));
    }

    // 버튼마다  클릭 함수에 필요한 내용 들어가는 함수==================================

    // 타이틀 (타이틀, 세팅, 로그인들어가기)
    public void OnClickTitle()
    {
        OnClickButton(titlePanel);
    }

    public void OnClickTitleSettings()
    {
        OnClickButton(titlePanel, settingsPanel);
    }

    public void OnClickTitleToLogIn()
    {
        OnClickButton(loginPanel);
    }


    // 로비 (로비, 리더보드, 세팅, 타워프리셋, 방찾기, 방만들기, 타이틀로 나가기, 방 들어가기)
    public void OnClickTitleToLobby()
    {
        OnClickButton(lobbyPanel);
    }

    public void OnClickLobbyLeaderBoard()
    {
        OnClickButton(lobbyPanel, leaderBoardPanel);
    }

    public void OnClickLobbySettings()
    {
        OnClickButton(lobbyPanel, settingsPanel);
    }

    public void OnClickLobbyTowerPreset()
    {
        OnClickButton(lobbyPanel, TowerPresetPanel);
    }

    public void OnClickLobbySearchRoom()
    {
        OnClickButton(lobbyPanel, searchRoomPanel);
    }

    public void OnClickLobbyCreateRoom()
    {
        OnClickButton(lobbyPanel, createRoomPanel);
    }

    public void OnClickLobbyToTitle()
    {
        OnClickButton(titlePanel);
    }

    public void OnCLickLobbyToRoom()
    {
        OnClickButton(roomPanel);
    }


    // 로그인 (회원가입, 타이틀, 로그인)
    public void OnClickLoginySignUp()
    {
        OnClickButton(loginPanel, signUpPanel);
    }

    public void OnClickLogin()
    {
        OnClickButton(loginPanel);
    }

    public void OnClickLoginToLobby()
    {
        OnClickButton(lobbyPanel);
    }

    public void OnClickLoginToTitle()
    {
        OnClickButton(titlePanel);
    }


    // 룸 (프리셋, 세팅, 리더보드, 로비로 나가기)
    public void OnClickRoomPreset()
    {
        OnClickButton(roomPanel, TowerPresetPanel);
    }

    public void OnClickRoomSettings()
    {
        OnClickButton(roomPanel, settingsPanel);
    }

    public void OnClickRoomLeaderboard()
    {
        OnClickButton(roomPanel, leaderBoardPanel);
    }

    public void OnClickRoomToLobby()
    {
        OnClickButton(lobbyPanel);
    }



    // 종료
    public void OnClickQuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
