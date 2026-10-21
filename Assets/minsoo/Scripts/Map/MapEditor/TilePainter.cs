using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

public class TilePainter : MonoBehaviour
{
    [SerializeField] private Camera cam;
    [SerializeField] private Tilemap logicTilemap;
    [SerializeField] private Tilemap visualTilemap;
    [SerializeField] private MapTileSet tileSet;
    [SerializeField] private Transform cursor;              // 반투명 사각형
    [SerializeField] private Vector2Int size = new(20, 12);

    private TileType[,] grid;
    private TileType brush = TileType.Road;
    private Vector3Int? lastCell;
    private TilemapRenderer tileRenderer;

    public TileType[,] Grid => grid;
    public Vector2Int Size => size;

    // 칸이 바뀔 때마다 알림 (UI 정보 갱신용)
    public event Action Changed;

    private void Awake()
    {
        if (cam == null) cam = Camera.main;
        grid = new TileType[size.x, size.y];
        Clear();
        CameraCenter();
    }

    private void Start()
    {
        tileRenderer = logicTilemap.GetComponent<TilemapRenderer>();
    }

    public void CameraCenter()
    {
        cam.transform.position = new Vector3(size.x / 2, size.y / 2, -10);
    }

    public void SizeChanged(int x, int y)
    {
        size.x = x;
        size.y = y;
        grid = new TileType[size.x, size.y];
        Clear();
        CameraCenter();
    }

    // 팔레트 버튼 OnClick에 연결 (0=Block, 1=Road, 2=Build, 3=Start, 4=Goal)
    public void SetBrush(int type) => brush = (TileType)type;

    // 전체를 Block으로 초기화
    public void Clear()
    {
        logicTilemap.ClearAllTiles();
        visualTilemap.ClearAllTiles();
        lastCell = null;
        FillAll(TileType.Block);
        logicTilemap.CompressBounds();
        visualTilemap.CompressBounds();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            tileRenderer.enabled = !tileRenderer.enabled;
        }


        var mouse = Mouse.current;
        if (mouse == null) return;

        Vector3 world = cam.ScreenToWorldPoint(mouse.position.ReadValue());
        world.z = 0f;
        Vector3Int cell = logicTilemap.WorldToCell(world);

        // 커서 표시
        bool inside = InBounds(cell);
        cursor.gameObject.SetActive(inside);
        if (inside) cursor.position = logicTilemap.GetCellCenterWorld(cell);

        bool paint = mouse.leftButton.isPressed;
        bool erase = mouse.rightButton.isPressed;
        bool overUI = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();

        if (overUI || (!paint && !erase))
        {
            lastCell = null;
            return;
        }

        TileType type = erase ? TileType.Block : brush;
        if (lastCell.HasValue) PaintLine(lastCell.Value, cell, type);
        else PaintCell(cell, type);
        lastCell = cell;
    }

    // 빠르게 드래그하면 프레임 사이 칸이 비므로 직선으로 이어서 칠함 (Bresenham)
    private void PaintLine(Vector3Int from, Vector3Int to, TileType type)
    {
        int dx = Mathf.Abs(to.x - from.x), dy = -Mathf.Abs(to.y - from.y);
        int sx = from.x < to.x ? 1 : -1, sy = from.y < to.y ? 1 : -1;
        int err = dx + dy;
        var p = from;
        while (true)
        {
            PaintCell(p, type);
            if (p == to) break;
            int e2 = 2 * err;
            if (e2 >= dy) { err += dy; p.x += sx; }
            if (e2 <= dx) { err += dx; p.y += sy; }
        }
    }

    private void PaintCell(Vector3Int cell, TileType type)
    {
        if (!InBounds(cell) || grid[cell.x, cell.y] == type) return;
        grid[cell.x, cell.y] = type;
        logicTilemap.SetTile(cell, tileSet.Logic(type));
        visualTilemap.SetTile(cell, tileSet.Visual(type));
        // 칠한 칸과 주변 8칸을 다시 그림 (풀 RuleTile이 바뀐 길 모양을 반영하도록)
        for (int dx = -1; dx <= 1; dx++)
            for (int dy = -1; dy <= 1; dy++)
                visualTilemap.RefreshTile(cell + new Vector3Int(dx, dy, 0));
        Changed?.Invoke();
    }

    private void FillAll(TileType type)
    {
        var bounds = new BoundsInt(0, 0, 0, size.x, size.y, 1);
        var logic = new TileBase[size.x * size.y];
        var visual = new TileBase[size.x * size.y];
        for (int i = 0; i < logic.Length; i++)
        {
            logic[i] = tileSet.Logic(type);
            visual[i] = tileSet.Visual(type);
        }
        logicTilemap.SetTilesBlock(bounds, logic);
        visualTilemap.SetTilesBlock(bounds, visual);
        for (int x = 0; x < size.x; x++)
            for (int y = 0; y < size.y; y++)
                grid[x, y] = type;
        Changed?.Invoke();
    }

    private bool InBounds(Vector3Int c) => c.x >= 0 && c.y >= 0 && c.x < size.x && c.y < size.y;

    // 저장된 격자를 에디터에 통째로 올림 (불러오기용)
    public void LoadGrid(TileType[,] source)
    {
        size = new Vector2Int(source.GetLength(0), source.GetLength(1));
        grid = (TileType[,])source.Clone();

        logicTilemap.ClearAllTiles();
        visualTilemap.ClearAllTiles();

        var bounds = new BoundsInt(0, 0, 0, size.x, size.y, 1);
        var logic = new TileBase[size.x * size.y];
        var visual = new TileBase[size.x * size.y];
        for (int y = 0; y < size.y; y++)
            for (int x = 0; x < size.x; x++)
            {
                int i = x + y * size.x;   // SetTilesBlock은 x가 먼저 도는 순서
                logic[i] = tileSet.Logic(grid[x, y]);
                visual[i] = tileSet.Visual(grid[x, y]);
            }
        logicTilemap.SetTilesBlock(bounds, logic);
        visualTilemap.SetTilesBlock(bounds, visual);

        lastCell = null;
        CameraCenter();
        Changed?.Invoke();
    }
}