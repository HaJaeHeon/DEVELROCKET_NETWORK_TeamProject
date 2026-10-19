using System.Collections.Generic;
using UnityEngine;

public class MapPath
{
    public int Index { get; }                      // 네트워크로 보낼 id
    public Vector2Int StartCell { get; }
    public IReadOnlyList<Vector2> Points { get; }  // 내부는 Vector2[]

    public MapPath(int index, Vector2Int startCell, Vector2[] points)
    {
        Index = index;
        StartCell = startCell;
        Points = points;
    }
}