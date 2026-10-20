using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;
#if UNITY_EDITOR

public static class TilemapOriginTool
{
    [MenuItem("Tools/Tilemap/Move To Origin")]
    static void MoveToOrigin()
    {
        var go = Selection.activeGameObject;
        if (go == null) { Debug.LogWarning("Grid 오브젝트를 선택하세요"); return; }

        var tilemaps = go.GetComponentsInChildren<Tilemap>();
        if (tilemaps.Length == 0) { Debug.LogWarning("타일맵이 없어요"); return; }

        // 모든 타일맵 중 가장 왼쪽 아래 칸 찾기
        var min = new Vector3Int(int.MaxValue, int.MaxValue, 0);
        foreach (var tm in tilemaps)
        {
            tm.CompressBounds();
            if (tm.cellBounds.size.x == 0) continue;   // 빈 타일맵 건너뛰기
            min.x = Mathf.Min(min.x, tm.cellBounds.xMin);
            min.y = Mathf.Min(min.y, tm.cellBounds.yMin);
        }
        if (min.x == int.MaxValue) return;

        var offset = new Vector3Int(-min.x, -min.y, 0);

        foreach (var tm in tilemaps)
        {
            Undo.RegisterCompleteObjectUndo(tm, "Move Tilemap To Origin");

            // 타일과 회전/색상 정보까지 저장
            var cells = new System.Collections.Generic.List<(Vector3Int pos, TileBase tile, Matrix4x4 m, Color c)>();
            foreach (var pos in tm.cellBounds.allPositionsWithin)
            {
                var tile = tm.GetTile(pos);
                if (tile != null)
                    cells.Add((pos, tile, tm.GetTransformMatrix(pos), tm.GetColor(pos)));
            }

            tm.ClearAllTiles();

            foreach (var cell in cells)
            {
                var newPos = cell.pos + offset;
                tm.SetTile(newPos, cell.tile);
                tm.SetTransformMatrix(newPos, cell.m);
                tm.SetColor(newPos, cell.c);
            }
            tm.CompressBounds();
        }

        Debug.Log($"타일을 {offset}만큼 이동했어요");
    }
}
#endif