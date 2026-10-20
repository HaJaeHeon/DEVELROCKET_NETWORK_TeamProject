using UnityEngine;

public class MapTile : ITileMap
{
    public Vector3Int Cell { get; }   // 칸 좌표
    public Vector2 Pos { get; }       // 칸 중앙 월드 좌표
    public TileType Type { get; }

    public MapTile(Vector3Int cell, Vector2 pos, TileType type)
    {
        Cell = cell; Pos = pos; Type = type;
    }
}