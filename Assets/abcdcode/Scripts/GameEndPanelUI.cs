using Photon.Pun;
using UnityEngine;
using UnityEngine.SceneManagement;
public class GameEndPanelUI : SingletonBehavior<GameEndPanelUI>
{
    public override void Awake()
    {
        base.Awake();
        m_GameOver.SetActive(false);
        m_GameClear.SetActive(false);
    }
    public void OpenGameOver()
    {
        m_GameOver.SetActive(true);
    }
    public void OpenGameClear()
    {
        m_GameClear.SetActive(true);
    }
    public void OnClickBack()
    {
        if (PhotonNetwork.InRoom)
            PhotonNetwork.LeaveRoom();
        SceneManager.LoadScene(m_gotoScene);
    }
    [SerializeField]private GameObject m_GameOver;
    [SerializeField]private GameObject m_GameClear;
    [SerializeField]private string m_gotoScene;
}