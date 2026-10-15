using System.Collections.Generic;
using UnityEngine;

public static class Pathfinder
{
    // 탐색 순서 고정: 상 → 우 → 하 → 좌
    static readonly Vector2Int[] Dirs =
        { Vector2Int.up, Vector2Int.right, Vector2Int.down, Vector2Int.left };

    static bool IsWalkable(TileType t) =>
        t == TileType.Road || t == TileType.Start || t == TileType.Goal;

    // 좌표는 모두 배열 인덱스 기준, 길이 없으면 null
    public static List<Vector2Int> FindPath(TileType[,] grid, Vector2Int start, Vector2Int goal)
    {
        int w = grid.GetLength(0), h = grid.GetLength(1);

        var g = new int[w, h];
        var cameFrom = new Vector2Int[w, h];
        var closed = new bool[w, h];
        var inOpen = new bool[w, h];

        for (int x = 0; x < w; x++)
            for (int y = 0; y < h; y++)
                g[x, y] = int.MaxValue;

        int H(Vector2Int p) => Mathf.Abs(p.x - goal.x) + Mathf.Abs(p.y - goal.y);
        int F(Vector2Int p) => g[p.x, p.y] + H(p);

        var open = new List<Vector2Int> { start };
        g[start.x, start.y] = 0;
        inOpen[start.x, start.y] = true;

        while (open.Count > 0)
        {
            int best = 0;
            for (int i = 1; i < open.Count; i++)
            {
                int fi = F(open[i]), fb = F(open[best]);
                if (fi < fb || (fi == fb && H(open[i]) < H(open[best])))
                    best = i;
            }

            Vector2Int cur = open[best];
            open.RemoveAt(best);
            inOpen[cur.x, cur.y] = false;

            if (cur == goal)
                return BuildPath(cameFrom, start, goal);

            closed[cur.x, cur.y] = true;

            foreach (var d in Dirs)
            {
                Vector2Int n = cur + d;
                if (n.x < 0 || n.y < 0 || n.x >= w || n.y >= h) continue;
                if (closed[n.x, n.y] || !IsWalkable(grid[n.x, n.y])) continue;

                int newG = g[cur.x, cur.y] + 1;
                if (newG >= g[n.x, n.y]) continue;

                g[n.x, n.y] = newG;
                cameFrom[n.x, n.y] = cur;
                if (!inOpen[n.x, n.y])
                {
                    open.Add(n);
                    inOpen[n.x, n.y] = true;
                }
            }
        }
        return null;
    }

    static List<Vector2Int> BuildPath(Vector2Int[,] cameFrom, Vector2Int start, Vector2Int goal)
    {
        var path = new List<Vector2Int>();
        for (var p = goal; p != start; p = cameFrom[p.x, p.y])
            path.Add(p);
        path.Add(start);
        path.Reverse();
        return path;
    }

    // 꺾이는 지점만 남기기
    public static List<Vector2Int> Simplify(List<Vector2Int> path)
    {
        if (path == null || path.Count <= 2) return path;

        var result = new List<Vector2Int> { path[0] };
        for (int i = 1; i < path.Count - 1; i++)
        {
            if (path[i] - path[i - 1] != path[i + 1] - path[i])
                result.Add(path[i]);
        }
        result.Add(path[path.Count - 1]);
        return result;
    }
}