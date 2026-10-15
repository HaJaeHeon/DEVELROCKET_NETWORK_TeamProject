using TMPro;
using UnityEngine;
public class InGameUIManager : SingletonBehavior<InGameUIManager>
{

    private void Update()
    {
        UpdateWaveState();
        UpdateGold();
    }
    private void UpdateWaveState()
    {
        var stage = GameManager.Instance != null ? GameManager.Instance.StageManager : null;
        if(stage == null || stage.Data == null) return;

        if(m_WaveNumText != null)
        {
            m_WaveNumText.SetText("Wave {0}", stage.DisplayWaveNumber);
        }
        if(m_WaveStateText != null)
        {
            if(stage.IsSyncedCleared)
            {
                m_WaveStateText.SetText("All Waves Cleared");
            }
            else if(stage.State == StageManager.WaveState.Countdown)
            {
                m_WaveStateText.SetText("Remain Time : {0}", Mathf.CeilToInt(stage.RemainCountdown));
            }
            else
            {
                m_WaveStateText.SetText("Progress... Remain Enemy : {0}", stage.RemainEnemy);
            }
        }
    }
    private void UpdateGold()
    {
        if(m_GoldText == null) return;
        m_GoldText.SetText("{0:0} G", PlayerGold.Local);
    }
    [SerializeField]private TMP_Text m_WaveNumText;
    [SerializeField]private TMP_Text m_WaveStateText;
    [SerializeField]private TMP_Text m_GoldText;
}
