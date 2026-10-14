using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MapManager : SingletonBehavior<MapManager>, IMap
{

    // 타일맵 가져오기

    // 맵 시작점 보내주기

    // 길찾기
    // 찾은 경로 보내주기
    // BuildType 보내주기

    [SerializeField] private Tilemap tilemap;

    Dictionary<Vector2, Tower> towerDict;
    private TileType[,] grid;
    private Vector2Int startPos;
    private Vector2Int endPos;
    private List<Vector2> path;
    public override void Awake()
    {
        base.Awake();
        GetGridTileType();
        PrintGrid(); // 확인용
        BuildPath();
    }

    private void GetGridTileType()
    {
        BoundsInt b = tilemap.cellBounds;
        grid = new TileType[b.size.x, b.size.y];
        foreach (var p in b.allPositionsWithin)
        {
            var t = tilemap.GetTile<GameTile>(p);
            var type = t ? t.Type : TileType.Block;
            int x = p.x - b.xMin, y = p.y - b.yMin;
            grid[x, y] = type;
            if (type == TileType.Start) startPos = new(x, y);
            if (type == TileType.Goal) endPos = new(x, y);
        }
    }

    void PrintGrid()
    {
        var sb = new System.Text.StringBuilder();
        int w = grid.GetLength(0), h = grid.GetLength(1);

        for (int y = h - 1; y >= 0; y--)
        {
            for (int x = 0; x < w; x++)
            {
                sb.Append(grid[x, y] switch
                {
                    TileType.Block => '#',
                    TileType.Road => '.',
                    TileType.Build => 'B',
                    TileType.Start => 'S',
                    TileType.Goal => 'G',
                    _ => '?'
                });
            }
            sb.AppendLine();
        }
        Debug.Log(sb.ToString());
    }

    private void BuildPath()
    {
        BoundsInt b = tilemap.cellBounds;

        var raw = Pathfinder.FindPath(grid, startPos, endPos);
        if (raw == null)
        {
            Debug.LogError("시작점에서 도착점까지 길이 없습니다.");
            return;
        }

        path = new List<Vector2>();
        foreach (var idx in Pathfinder.Simplify(raw))
        {
            var cell = new Vector3Int(idx.x + b.xMin, idx.y + b.yMin, 0);
            path.Add(tilemap.GetCellCenterWorld(cell));
        }
    }

    // 전체 경로 보내주기
    public List<Vector2> GetPath()
    {
        return path;
    }


    //경로 리스트 중 원하는 시작위치에 따른 경로 보내주기
    public List<Vector2> GetStartToPath(Vector2 startPos)
    {
        return path;
    }

    // 현재 타일의 타입 보내주기
    public TileType GetTileType(Vector2Int curPos)
    {
        return grid[curPos.x, curPos.y];
    }

    public ITileMap GetGoalTile()
    {

        throw new System.NotImplementedException();
    }

    public List<ITileMap> GetStartTile()
    {
        throw new System.NotImplementedException();
    }

    public List<ITileMap> GetPath(Tilemap cur)
    {
        throw new System.NotImplementedException();
    }
}
