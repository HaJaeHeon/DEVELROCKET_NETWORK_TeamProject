using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(menuName = "TD/Grass Rule Tile")]
public class GrassRuleTile : RuleTile<GrassRuleTile.Neighbor>
{
    [SerializeField] private TileBase[] roadTiles;   // 길로 볼 비주얼 타일 (Road, Start, Goal)

    public class Neighbor : RuleTile.TilingRuleOutput.Neighbor { }

    // 초록 화살표(This) = 길이 아님 (풀, Build, 맵 밖)
    // 빨간 X(NotThis)  = 길
    public override bool RuleMatch(int neighbor, TileBase other)
    {
        bool isRoad = other != null && System.Array.IndexOf(roadTiles, other) >= 0;
        switch (neighbor)
        {
            case Neighbor.This: return !isRoad;
            case Neighbor.NotThis: return isRoad;
        }
        return base.RuleMatch(neighbor, other);
    }
}