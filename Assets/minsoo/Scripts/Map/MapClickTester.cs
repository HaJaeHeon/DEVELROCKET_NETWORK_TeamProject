using System;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class MapClickTester : MonoBehaviour
{
    [SerializeField] private MapManager map;
    [SerializeField] private Camera cam;
    [SerializeField] private Color highlight = new Color(1f, 0f, 1f, 0.5f);
    [SerializeField] private Tower towerPrefabs;

    private bool hasSelection;
    private Vector3Int selectedCell;

    private void Awake()
    {
        if (cam == null) cam = Camera.main;
    }

    private void Update()
    {
        if (!ClickedThisFrame(out Vector2 screen)) return;

        // 화면 좌표 → 월드 좌표
        Vector3 world = cam.ScreenToWorldPoint(screen);
        world.z = 0f;

        // 월드 좌표 → 셀 좌표
        Vector3Int cell = map.Tilemap.WorldToCell(world);

        // 셀 좌표로 바로 조회 (맵 밖이면 타일 null, 타입 Block)
        var tile = map.Tilemap.GetTile(cell);
        TileType type = map.GetTileType(cell);

        selectedCell = cell;
        hasSelection = true;

        if (map.GetTileType(cell) == TileType.Build)
        {
            var m = Instantiate(towerPrefabs, new Vector2(cell.x + 0.5f, cell.y + 0.5f), Quaternion.identity);
        }
        Debug.Log($"[Click] 월드 {world.ToString("F2")} | 셀 {cell} | " +
                  $"타입 {type} | 타일 {(tile != null ? tile.name : "없음")}");
    }

    private bool ClickedThisFrame(out Vector2 screen)
    {
#if ENABLE_INPUT_SYSTEM
        var mouse = Mouse.current;
        screen = mouse != null ? mouse.position.ReadValue() : Vector2.zero;
        return mouse != null && mouse.leftButton.wasPressedThisFrame;
#else
        screen = Input.mousePosition;
        return Input.GetMouseButtonDown(0);
#endif
    }

    private void OnDrawGizmos()
    {
        if (!hasSelection || map == null) return;
        Gizmos.color = highlight;
        Gizmos.DrawCube(map.Tilemap.GetCellCenterWorld(selectedCell),
                        map.Tilemap.cellSize * 0.95f);
    }
}