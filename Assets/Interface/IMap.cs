

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public interface IMap
{
    public List<ITileMap> GetStartTile();
    public ITileMap GetGoalTile();
    public List<ITileMap> GetPath(Tilemap cur);
}