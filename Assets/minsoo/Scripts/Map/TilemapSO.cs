using UnityEngine;
using UnityEngine.Tilemaps;


[CreateAssetMenu(menuName = "TD/GameTile")]
public class GameTile : Tile
{
    [SerializeField] private TileType type;
    public TileType Type => type;

    public bool IsWalkable =>
        Type == TileType.Road || Type == TileType.Start || Type == TileType.Goal;
    public bool IsBuildable => Type == TileType.Build;

}
