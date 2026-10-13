using System.Collections.Generic;
using UnityEngine;

public class PresetGroup : MonoBehaviour
{
    private readonly Dictionary<string, DraggableImage> usedImages = new();

    public bool CanUse(DraggableImage image)
    {
        if (image == null || string.IsNullOrWhiteSpace(image.imageId))
            return false;

        if (!usedImages.TryGetValue(image.imageId, out var existing))
            return true;

        // 제거된 물체의 기록은 정리합니다.
        if (existing == null)
        {
            usedImages.Remove(image.imageId);
            return true;
        }

        // 이미 장착된 동일 물체의 자리 이동은 허용합니다.
        // 같은 식별값을 가진 별도 물체는 허용하지 않습니다.
        return existing == image;
    }

    public void Register(DraggableImage image)
    {
        usedImages[image.imageId] = image;
    }

    public void Unregister(DraggableImage image)
    {
        // 이 물체가 등록한 정보만 제거합니다.
        if (usedImages.TryGetValue(image.imageId, out var existing)
            && existing == image)
        {
            usedImages.Remove(image.imageId);
        }
    }
}
