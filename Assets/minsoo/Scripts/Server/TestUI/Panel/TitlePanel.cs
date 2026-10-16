using UnityEngine;
using UnityEngine.UI;

public class TitlePanel : UIPanel
{
    [SerializeField] private Button gameStartButton;
    [SerializeField] private Button settingButton;

    private void Awake()
    {
        var ui = GetComponentInParent<PanelController>(true);
        gameStartButton.onClick.AddListener(() => ui.Open(PanelId.Login));
        settingButton.onClick.AddListener(() => ui.Open(PanelId.Title, PanelId.Setting));
    }
}
