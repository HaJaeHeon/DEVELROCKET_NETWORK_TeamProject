using UnityEngine;
using UnityEngine.Tilemaps;

// 격자(TileType[,])를 로직/비주얼 타일맵에 통째로 채움 (맵 에디터 불러오기, 인게임 유저 맵 공용)
public static class MapGridBuilder
{
    public static void Fill(Tilemap logic, Tilemap visual, MapTileSet tileSet, TileType[,] grid)
    {
        int w = grid.GetLength(0), h = grid.GetLength(1);

        logic.ClearAllTiles();
        visual.ClearAllTiles();

        var bounds = new BoundsInt(0, 0, 0, w, h, 1);
        var logicTiles = new TileBase[w * h];
        var visualTiles = new TileBase[w * h];
        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                int i = x + y * w;   // SetTilesBlock은 x가 먼저 도는 순서
                logicTiles[i] = tileSet.Logic(grid[x, y]);
                visualTiles[i] = tileSet.Visual(grid[x, y]);
            }
        logic.SetTilesBlock(bounds, logicTiles);
        visual.SetTilesBlock(bounds, visualTiles);
    }
}
