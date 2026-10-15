using UnityEngine;

/// <summary>
/// 월드 공간 HP 바. fill의 x 스케일로 남은 체력 비율을 표시 (fill 피벗은 왼쪽 끝)
/// </summary>
public class HPBar : MonoBehaviour
{
    public void SetRatio(float ratio)
    {
        ratio = Mathf.Clamp01(ratio);
        Vector3 s = fill.localScale;
        s.x = ratio;
        fill.localScale = s;
        if(hideWhenFull)
        {
            gameObject.SetActive(ratio < 1f);
        }
    }
    [SerializeField]private Transform fill;
    [Tooltip("체력이 가득 찼을 때 바를 숨김")]
    [SerializeField]private bool hideWhenFull = false;
}
