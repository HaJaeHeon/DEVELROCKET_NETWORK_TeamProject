using UnityEngine;

public class SoundPlayer : MonoBehaviour
{
    [SerializeField] private SOAudio soBGMAudio;

    [SerializeField] private SOAudio soSFXAudio;


    private void OnEnable()
    {
        if (soBGMAudio == null)
            return;

        SoundManager.m_Instance.PlayBgm(soBGMAudio);
    }


    public void PlaySFXSound()
    {
        if (soSFXAudio == null)
            return;

        SoundManager.m_Instance.PlaySound(soBGMAudio);

    }


    public class Missile
    {
        [SerializeField] private SOAudio m_refMissilkeSOund;
    }

    //public void OnEnable()
    //{
    //    var handle = SoundManager.m_Instance.PlaySound(m_refMissilkeSOund);


    //    SoundManager.m_Instance.StopSfx(handle);
    //}
}
