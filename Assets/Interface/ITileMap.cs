using UnityEngine;

public interface ITileMap
{
    public Vector2 Pos{get;}
    public TileType Type{get;}
}
public enum TileType
{
    Road,
    Block,
    Build,
    Start,
    Goal
}