using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PresetGroup : MonoBehaviour
{
    // 속성 창(Inspector)에서 1번~5번 칸 순서대로 연결합니다.
    [SerializeField] private List<ImageSocket> presetSlots = new List<ImageSocket>();

    private readonly Dictionary<string, DraggableImage> usedImages = new();

    public bool CanUse(DraggableImage image)
    {
        if (image == null || string.IsNullOrWhiteSpace(image.imageId))
            return false;

        if (!usedImages.TryGetValue(image.imageId, out var existing))
            return true;

        if (existing == null)
        {
            usedImages.Remove(image.imageId);
            return true;
        }

        return existing == image;
    }

    public void Register(DraggableImage image)
    {
        usedImages[image.imageId] = image;
    }

    public void Unregister(DraggableImage image)
    {
        if (usedImages.TryGetValue(image.imageId, out var existing) && existing == image)
            usedImages.Remove(image.imageId);
    }

    public bool TryGetPresetSpriteIds(List<Sprite> spriteCatalog, out List<int> spriteIds)
    {
        spriteIds = new List<int>(5);

        if (spriteCatalog == null || presetSlots == null || presetSlots.Count != 5)
        {
            Debug.LogWarning("전체 그림 목록과 프리셋 5개 칸을 연결해야 합니다.");
            return false;
        }

        for (int i = 0; i < presetSlots.Count; i++)
        {
            ImageSocket slot = presetSlots[i];

            if (slot == null || slot.CurrentImage == null)
            {
                Debug.LogWarning($"{i + 1}번 프리셋이 비어 있습니다.");
                return false;
            }

            Image image = slot.CurrentImage.GetComponent<Image>();

            if (image == null || image.sprite == null)
            {
                Debug.LogWarning($"{i + 1}번 프리셋에 그림이 없습니다.");
                return false;
            }

            int spriteId = spriteCatalog.IndexOf(image.sprite);

            if (spriteId < 0)
            {
                Debug.LogWarning($"{i + 1}번 그림이 전체 그림 목록에 없습니다.");
                return false;
            }

            spriteIds.Add(spriteId);
        }

        return true;
    }
}
