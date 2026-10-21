using UnityEngine;
using Photon.Pun;
using UnityEngine.Tilemaps;


/// <summary>
/// 카메라 이동 범위 제한 존재해야함
/// </summary>


public class CameraMove : MonoBehaviourPunCallbacks
{
    public float cameraSpeed;

    private Camera myCamera;
    private Tilemap mapTilemap;

    private void Start()
    {
        if (!PhotonNetwork.InRoom)
            return;

        myCamera = Camera.main;

        MapManager mapManager = MapManager.Instance;

        if (myCamera == null || mapManager == null || mapManager.Tilemap == null)
        {
            Debug.LogWarning("카메라 또는 이동 범위를 정할 타일맵이 없습니다.");
            enabled = false;
            return;
        }

        mapTilemap = mapManager.Tilemap;

        // 시작 위치도 범위 안으로 맞춥니다.
        myCamera.transform.position = ClampCameraPosition(myCamera.transform.position);
    }

    private void Update()
    {
        if (!PhotonNetwork.InRoom)
            return;

        if (myCamera == null)
            return;

        MoveCamera();
    }

    private void MoveCamera()
    {
        Vector3 moveDirection = new Vector3(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"), 0f);

        Vector3 nextPosition = myCamera.transform.position + moveDirection.normalized * cameraSpeed * Time.deltaTime;

        myCamera.transform.position = ClampCameraPosition(nextPosition);
    }

    private Vector3 ClampCameraPosition(Vector3 position)
    {
        Bounds bounds = mapTilemap.localBounds;

        // 타일맵 내부 좌표를 전역 좌표로 변환합니다.
        Vector3 cornerA = mapTilemap.transform.TransformPoint(bounds.min);
        Vector3 cornerB = mapTilemap.transform.TransformPoint(bounds.max);

        float left = Mathf.Min(cornerA.x, cornerB.x);
        float right = Mathf.Max(cornerA.x, cornerB.x);
        float bottom = Mathf.Min(cornerA.y, cornerB.y);
        float top = Mathf.Max(cornerA.y, cornerB.y);

        float mapWidth = right - left;
        float mapHeight = top - bottom;

        if (mapWidth <= 0f || mapHeight <= 0f)
            return position;

        // 화면이 맵보다 크면, 맵 밖이 보이지 않도록 표시 범위를 줄입니다.
        float maxCameraSize = Mathf.Min(mapHeight * 0.5f, mapWidth / (2f * myCamera.aspect));
        myCamera.orthographicSize = Mathf.Min(myCamera.orthographicSize, maxCameraSize);

        float halfHeight = myCamera.orthographicSize;
        float halfWidth = halfHeight * myCamera.aspect;

        position.x = Mathf.Clamp(position.x, left + halfWidth, right - halfWidth);
        position.y = Mathf.Clamp(position.y, bottom + halfHeight, top - halfHeight);

        return position;
    }
}
