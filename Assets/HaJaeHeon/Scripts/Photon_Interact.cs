using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;

public class Photon_Interact : MonoBehaviourPunCallbacks//, IPointerClickHandler
{
    private MapManager map;
    private Camera targetCamera;
    private Tilemap tilemap;


    private void Awake()
    {
        map = MapManager.Instance;
        targetCamera = Camera.main;
    }

    private void Start()
    {
        tilemap = map.GetTileMap();
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            PlayerClickTile();
        }
    }

    private void PlayerClickTile()
    {
        if (map == null || targetCamera == null)
            return;

        if (tilemap == null)
            return;

        Vector3 screenPosition = Input.mousePosition;

        screenPosition.z = targetCamera.WorldToScreenPoint(tilemap.transform.position).z;

        Vector3 worldPosition = targetCamera.ScreenToWorldPoint(screenPosition);

        Vector2Int cell = (Vector2Int)tilemap.WorldToCell(worldPosition);

        // 타일 맵이 아닌 위치를 클릭 시 return;
        if (!tilemap.HasTile(new Vector3Int(cell.x, cell.y, 0)))
            return;

        TileType cellType = map.GetTileType(cell);

        Debug.Log($"position2D : {cell}\n" +
            $"CellType : {cellType}");
    }
}
