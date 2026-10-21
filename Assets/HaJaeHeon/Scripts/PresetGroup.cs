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

        if (spriteCatalog == null || presetSlots == null || spriteCatalog[0] == null)
        {
            Debug.LogWarning("전체 그림 목록의 0번에 기본 그림을 등록해야 합니다.");
            return false;
        }

        for (int i = 0; i < 5; i++)
        {
            int spriteId = 0;

            ImageSocket slot = presetSlots != null && i < presetSlots.Count ? presetSlots[i] : null;

            if (slot != null && slot.CurrentImage != null)
            {
                Image image = slot.CurrentImage.GetComponent<Image>();

                if (image != null && image.sprite != null)
                {
                    int foundId = spriteCatalog.IndexOf(image.sprite);

                    if (foundId >= 0)
                        spriteId = foundId;
                }
            }

            spriteIds.Add(spriteId);
        }

        return true;
    }
}
