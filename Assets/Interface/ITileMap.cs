using UnityEngine;

public interface ITileMap
{
    public Vector2 Pos{get;}
    public TileType Type{get;}
}
public enum TileType
{
    Block,
    Road,
    Build,
    Start,
    Goal
}