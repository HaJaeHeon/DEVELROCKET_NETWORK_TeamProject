public static class MapCodec
{
    // TileType 순서(Block, Road, Build, Start, Goal)와 같은 순서
    private const string Chars = "#.BSG";

    // 인덱스 = y * width + x, y=0(맨 아래 줄)부터 저장
    public static string Encode(TileType[,] grid)
    {
        int w = grid.GetLength(0), h = grid.GetLength(1);
        var sb = new System.Text.StringBuilder(w * h);
        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
                sb.Append(Chars[(int)grid[x, y]]);
        return sb.ToString();
    }

    public static TileType[,] Decode(string cells, int w, int h)
    {
        if (cells == null || cells.Length != w * h)
            throw new System.ArgumentException($"cells 길이가 {w}x{h}와 맞지 않습니다.");

        var grid = new TileType[w, h];
        for (int i = 0; i < cells.Length; i++)
        {
            int t = Chars.IndexOf(cells[i]);
            grid[i % w, i / w] = t < 0 ? TileType.Block : (TileType)t;  // 모르는 문자는 Block
        }
        return grid;
    }
}