using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MapManager : SingletonBehavior<MapManager>, IMap
{
    [SerializeField] private Tilemap tilemap;

    private TileType[,] grid;
    private readonly List<Vector2Int> startCells = new();
    private Vector2Int endPos;
    private readonly List<MapPath> paths = new();
    public Tilemap Tilemap => tilemap;
    public override void Awake()
    {
        base.Awake();
        GetGridTileType();
        PrintGrid(); // 확인용
        BuildPath();
    }

    private void GetGridTileType()
    {
        startCells.Clear();
        BoundsInt b = tilemap.cellBounds;
        grid = new TileType[b.size.x, b.size.y];
        foreach (var p in b.allPositionsWithin)
        {
            var t = tilemap.GetTile<GameTile>(p);
            var type = t ? t.Type : TileType.Block;
            int x = p.x - b.xMin, y = p.y - b.yMin;
            grid[x, y] = type;
            if (type == TileType.Start) startCells.Add(new(x, y));
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
        paths.Clear();

        BoundsInt b = tilemap.cellBounds;
        for(int i = 0; i < startCells.Count; i++)
        {
            var raw = Pathfinder.FindPath(grid, startCells[i], endPos);
            if (raw == null)
            {
                Debug.LogError("시작점에서 도착점까지 길이 없습니다.");
                continue;
            }
            var corners = Pathfinder.Simplify(raw);
            var points = new Vector2[corners.Count];

            for (int j = 0; j < corners.Count; j++)
            {
                var cell = new Vector3Int(corners[j].x + b.xMin, corners[j].y + b.yMin, 0);
                points[j] = tilemap.GetCellCenterWorld(cell);
            }
            paths.Add(new MapPath(paths.Count, startCells[i], points));
        }

        if (paths.Count == 0)
            Debug.LogError("도착점까지 갈 수 있는 시작점이 없습니다.");

    }

    // 모든 경로 보내기
    public IReadOnlyList<MapPath> GetPaths() => paths;

    // 경로 하나 보내기
    public IReadOnlyList<Vector2> GetPath(int index) => paths[index].Points;

    // 현재 타일의 타입 보내주기
    public TileType GetTileType(Vector2Int curPos) => grid[curPos.x, curPos.y];

    // GetTile로 타입 보내주기
    public TileType GetTileType(Vector3Int cell)
    {
        var t = tilemap.GetTile<GameTile>(cell);
        return t ? t.Type : TileType.Block;
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
