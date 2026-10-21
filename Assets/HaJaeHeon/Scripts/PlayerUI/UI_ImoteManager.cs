using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_ImoteManager : SingletonBehavior<UI_ImoteManager>
{
    public const string PRESETIMOTE = "EmotePresetIds";

    [SerializeField] private PresetGroup presetGroup;

    [SerializeField] private List<Sprite> imoteSprites = new List<Sprite>();
    [SerializeField] private Transform contentTransform;
    [SerializeField] private GameObject imotePrefab;
    



    private void Start()
    {
        if (!ImoteManager.Instance.Initialize(imoteSprites))
            return;

        if (imotePrefab == null || contentTransform == null)
        {
            Debug.LogError("생성 원본과 배치 위치를 연결해야 합니다.");
            return;
        }

        Canvas targetCanvas = contentTransform.GetComponentInParent<Canvas>(true);

        for (int i = 0; i < imoteSprites.Count; i++)
        {
            GameObject go = Instantiate(imotePrefab, contentTransform);
            Image[] images = go.GetComponentsInChildren<Image>(true);

            foreach (Image imote in images)
            {
                imote.sprite = imoteSprites[i];
            }

            DraggableImage draggable = go.GetComponentInChildren<DraggableImage>(true);

            draggable.imageId = i.ToString();
            draggable.canvas = targetCanvas;
            draggable.rootPanel = targetCanvas.gameObject;
        }
    }

    private void SavePreset(List<int> spriteIds)
    {
        if (PhotonNetwork.LocalPlayer == null || spriteIds == null)
        {
            Debug.Log("여기서 return");
            return;
        }

        var properties = new Hashtable
        {
            { PRESETIMOTE, spriteIds.ToArray() }
        };

        if (!PhotonNetwork.LocalPlayer.SetCustomProperties(properties))
        {
            Debug.LogWarning("프리셋 저장 요청에 실패했습니다.");
            return;
        }

        Debug.Log("저장 완료");
    }

    public void OnClickSavePreset()
    {
        Debug.Log("저장 버튼 눌림");
        if (presetGroup == null)
        {
            Debug.LogWarning("PresetGroup 이 없음");
            return;
        }

        if (!presetGroup.TryGetPresetSpriteIds(imoteSprites, out List<int> spriteIds))
        {
            Debug.Log("여기서 return");
            return;
        }
        SavePreset(spriteIds);
    }
}
