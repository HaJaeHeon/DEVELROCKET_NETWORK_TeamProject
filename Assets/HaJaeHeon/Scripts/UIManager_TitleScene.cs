using System.Collections.Generic;
using UnityEngine;

public class UIManager_TitleScene : MonoBehaviour
{
    public enum MainUIs
    {
        Title,
        Login,
        Lobby,
        Room,
    }

    public enum SubUIs
    {
        SignUp,
        Settings,
        LeaderBoard,
        CreateRoom,
        SearchRoom,
        TowerPreset,
        GameStart
    }

    private enum TitleSubUIs
    {
        SignUp,
        Settings,
    }

    private enum LobbySubUIs
    {
        TowerPreset,
        CreateRoom,
        SearchRoom,
        LeaderBoard,
    }

    private enum RoomSubUIs
    {
        GameStart,
        TowerPreset,
        LeaderBoard,
        Settings,
    }

    private MainUIs currentMainUIs;
    private TitleSubUIs currentTitleSubUIs;
    private LobbySubUIs currentLobbySubUIs;
    private RoomSubUIs currentRoomSubUIs;



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

    public List<GameObject> panelList = new List<GameObject>();

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

    public void OnClickButton(GameObject main)
    {
        panelList.ForEach(panel =>  panel.SetActive(false));

        main.SetActive(true);
    }

    public void OnClickButton(GameObject main, GameObject sub)
    {
        panelList.ForEach (panel => panel.SetActive(false));

        main.SetActive(true);
        sub.SetActive(true);
    }



    //public void OnClickTitleSignUp()
    //{
    //    SwitchMainPanels(MainUIs.Title);
    //    SwitchTitleSubPanel(TitleSubUIs.SignUp);
    //}

    //public void OnClickTitleSettings()
    //{
    //    SwitchMainPanels(MainUIs.Title);
    //    SwitchTitleSubPanel(TitleSubUIs.Settings);
    //}


}
