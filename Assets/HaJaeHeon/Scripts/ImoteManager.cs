using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ImoteManager : MonoBehaviourPunCallbacks
{
    private const string PresetImote = "EmotePresetIds";

    [SerializeField] private PresetGroup presetGroup;

    [SerializeField] private List<Sprite> imoteSprites = new List<Sprite>();
    [SerializeField] private Transform contentTransform;
    [SerializeField] private GameObject imotePrefab;
    
    private List<Sprite> customImote = new List<Sprite>();



    private void Start()
    {
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

    private void SetImotePreset(Hashtable properties)
    {
        if (imoteSprites == null)
            return;

        if (!properties.TryGetValue(PresetImote, out object value) || !(value is int[] spriteIds) || spriteIds.Length != 5)
            return;

        var selectedSprites = new List<Sprite>(5);

        foreach (int spriteId in spriteIds)
        {
            if (spriteId < 0 || spriteId >= imoteSprites.Count || imoteSprites[spriteId] == null)
            {
                Debug.LogWarning($"등록되지 않은 그림 번호입니다: {spriteId}");
                return;
            }

            selectedSprites.Add(imoteSprites[spriteId]);
        }

        customImote = selectedSprites;
    }

    private void SavePreset(List<int> spriteIds)
    {
        if (PhotonNetwork.LocalPlayer == null || spriteIds == null || spriteIds.Count != 5)
            return;

        var properties = new Hashtable
        {
            { PresetImote, spriteIds.ToArray() }
        };

        if (!PhotonNetwork.LocalPlayer.SetCustomProperties(properties))
        {
            Debug.LogWarning("프리셋 저장 요청에 실패했습니다.");
            return;
        }

        SetImotePreset(properties);
    }

    public void OnClickSavePreset()
    {
        if (presetGroup == null)
        {
            Debug.LogWarning("PresetGroup 이 없음");
            return;
        }

        if (!presetGroup.TryGetPresetSpriteIds(imoteSprites, out List<int> spriteIds))
            return;

        SavePreset(spriteIds);
    }

    public Sprite GetPresetSpriteId(int slotIndex)
    {
        // 안정해져있으면 그냥 기본(0번) sprite가져와서 사용 
        if(customImote == null || slotIndex < 1 || slotIndex > imoteSprites.Count)
            return imoteSprites[0];

        return imoteSprites[slotIndex - 1];
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        base.OnPlayerPropertiesUpdate(targetPlayer, changedProps);

        if (targetPlayer.ActorNumber != photonView.OwnerActorNr)
            return;

        if (!changedProps.ContainsKey(PresetImote))
            return;

        SetImotePreset(targetPlayer.CustomProperties);
    }
}
