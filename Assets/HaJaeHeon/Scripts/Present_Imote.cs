using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using System.Collections;
using UnityEngine.UI;

public class Present_Imote : MonoBehaviourPunCallbacks
{
    public const string PRESETIMOTE = "EmotePresetIds";

    public float imoteTime = 1f;

    [SerializeField] private Image imoteImage;

    private Coroutine currentCoroutine;

    private void Start()
    {
        imoteImage.enabled = false;
    }


    private void Update()
    {
        if (!PhotonNetwork.InRoom || !photonView.IsMine)
            return;

        if(Input.GetKeyDown(KeyCode.Alpha1))
        {
            PressAlpha1();
        }

        if(Input.GetKeyDown(KeyCode.Alpha2))
        {
            PressAlpha2();
        }

        if(Input.GetKeyDown(KeyCode.Alpha3))
        {
            PressAlpha3();
        }

        if(Input.GetKeyDown(KeyCode.Alpha4))
        {
            PressAlpha4();
        }

        if(Input.GetKeyDown(KeyCode.Alpha5))
        {
            PressAlpha5();
        }
    }

    private void PressAlpha1()
    {
        SendPreset(1);
    }
    private void PressAlpha2()
    {
        SendPreset(2);
    }
    private void PressAlpha3()
    {
        SendPreset(3);
    }
    private void PressAlpha4()
    {
        SendPreset(4);
    }
    private void PressAlpha5()
    {
        SendPreset(5);
    }

    private void SendPreset(int InputNum)
    {
        var properties = PhotonNetwork.LocalPlayer.CustomProperties;

        int slotIndex = InputNum - 1;
        int spriteId = 0;

        if (properties.TryGetValue(PRESETIMOTE, out object value) && value is int[] spriteIds && slotIndex < spriteIds.Length)
            spriteId = spriteIds[slotIndex];

        photonView.RPC(nameof(PresentRpc), RpcTarget.All, spriteId);
    }

    [PunRPC]
    public void PresentRpc(int inputNum, PhotonMessageInfo info)
    {
        if (info.Sender == null || info.Sender.ActorNumber != photonView.OwnerActorNr)
        {
            Debug.Log("여기서 return");
            return;
        }
        Sprite targetSprite = ImoteManager.Instance.GetPresetSpriteId(inputNum);

        if (targetSprite == null)
        {
            Debug.Log("여기서 return");
            return;
        }


        if (currentCoroutine != null)
        {
            return;
        }
        currentCoroutine = StartCoroutine(ShowImote(targetSprite));

    }

    private IEnumerator ShowImote(Sprite targetSprite)
    {
        imoteImage.sprite = targetSprite;
        imoteImage.enabled = true;

        yield return new WaitForSeconds(imoteTime);
        imoteImage.sprite = null;
        imoteImage.enabled = false;
        currentCoroutine = null;
    }
}
