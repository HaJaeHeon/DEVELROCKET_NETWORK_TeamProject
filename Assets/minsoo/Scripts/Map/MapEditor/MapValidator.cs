using System.Collections.Generic;
using UnityEngine;

public static class MapValidator
{
    public const int MinBuild = 3;

    public static List<string> Validate(TileType[,] grid)
    {
        var errors = new List<string>();
        int w = grid.GetLength(0), h = grid.GetLength(1);

        var starts = new List<Vector2Int>();
        var goals = new List<Vector2Int>();
        int build = 0;

        for (int x = 0; x < w; x++)
            for (int y = 0; y < h; y++)
            {
                switch (grid[x, y])
                {
                    case TileType.Start: starts.Add(new(x, y)); break;
                    case TileType.Goal: goals.Add(new(x, y)); break;
                    case TileType.Build: build++; break;
                }
            }

        if (starts.Count == 0) errors.Add("Start tile is missing.");
        if (goals.Count != 1) errors.Add($"Need exactly 1 Goal (now {goals.Count}).");
        if (build < MinBuild) errors.Add($"Need at least {MinBuild} Build tiles (now {build}).");

        // Start마다 Goal까지 길이 이어지는지 (인게임과 같은 Pathfinder)
        if (goals.Count == 1)
            foreach (var s in starts)
                if (Pathfinder.FindPath(grid, s, goals[0]) == null)
                    errors.Add($"No road from Start ({s.x}, {s.y}) to Goal.");

        return errors;
    }
}