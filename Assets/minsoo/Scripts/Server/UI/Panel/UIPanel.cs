using UnityEngine;

public enum PanelId
{
    None,
    Title,
    Login,
    SignUp,
    Lobby,
    Room,
    LeaderBoard,
    Setting,
    Preset,
    CreateRoom,
    SearchRoom
}

public class UIPanel : MonoBehaviour
{
    [SerializeField] private PanelId id;
    public PanelId Id => id;
}
