using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class ImoteManager : Singleton<ImoteManager>
{
    private readonly List<Sprite> imoteSprites = new List<Sprite>();

    public bool Initialize(IReadOnlyList<Sprite> sprites)
    {
        if (sprites == null || sprites.Count == 0 || sprites[0] == null)
        {
            Debug.LogWarning("전체 그림 목록의 0번에 기본 그림을 등록해야 합니다.");
            return false;
        }

        imoteSprites.Clear();

        for (int i = 0; i < sprites.Count; i++)
        {
            imoteSprites.Add(sprites[i]);
        }

        return true;
    }

    public Sprite GetPresetSpriteId(int spriteId)
    {
        if (imoteSprites.Count == 0)
            return null;

        if (spriteId < 0 || spriteId >= imoteSprites.Count || imoteSprites[spriteId] == null)
            return imoteSprites[0];

        return imoteSprites[spriteId];
    }
}
