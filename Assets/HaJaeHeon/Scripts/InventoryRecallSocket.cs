using Photon.Pun;
using UnityEngine;

public class InventoryRecallSocket : MonoBehaviour
{
    // 마우스 주변, 타워 선택할 최대 거리
    [SerializeField] private float towerSelectionRadius = 0.6f;

    private TowerService towerService;

    private RectTransform inventoryRect;
    private Canvas canvas;
    private Camera targetCamera;
    private Tower selectedTower;

    private void Awake()
    {
        inventoryRect = transform as RectTransform;
        canvas = GetComponentInParent<Canvas>();
    }

    private void Start()
    {
        targetCamera = Camera.main;

        towerService = TowerService.Instance;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
            BeginTowerDrag();

        if (selectedTower != null && Input.GetMouseButtonUp(0))
            EndTowerDrag();
    }

    private void BeginTowerDrag()
    {
        if (!PhotonNetwork.InRoom || targetCamera == null || towerService == null)
            return;

        Tower tower = FindTowerAtMousePosition();

        if (tower == null)
            return;

        if (tower.OwnerPlayer == null || PhotonNetwork.LocalPlayer == null)
        {
            Debug.Log("타워 소유자 정보가 아직 준비되지 않았습니다.");
            return;
        }

        if (tower.OwnerPlayer.ActorNumber != PhotonNetwork.LocalPlayer.ActorNumber)
        {
            Debug.Log("다른 플레이어의 타워는 회수할 수 없습니다.");
            return;
        }

        selectedTower = tower;
        Debug.Log($"타워 드래그 시작: {tower.Data?.Id}");
    }

    private void EndTowerDrag()
    {
        Tower tower = selectedTower;
        selectedTower = null;

        // 인벤토리 밖에 놓았을 경우
        if (!IsPointerInsideInventory(Input.mousePosition))
        {
            Debug.Log("인벤토리 밖에서 드래그가 종료되었습니다.");
            return;
        }

        Debug.Log("인벤토리에 타워 회수를 요청합니다.");
        towerService.RequestRecall(tower);
    }

    // 마우스 위치에서 가장 가까운 타워 검색
    private Tower FindTowerAtMousePosition()
    {
        if (GameManager.Instance == null || GameManager.Instance.Container_Tower == null)
            return null;

        // 마우스 화면 좌표를 게임 공간 좌표로 변경
        Vector3 mouseWorldPosition = targetCamera.ScreenToWorldPoint(Input.mousePosition);
        Vector2 pointerPosition = new Vector2(mouseWorldPosition.x, mouseWorldPosition.y);

        Tower nearestTower = null;
        float nearestDistance = towerSelectionRadius;

        // 모든 타워 비교
        foreach (ITower towerItem in GameManager.Instance.Container_Tower.GetList())
        {
            Tower tower = towerItem as Tower;

            if (tower == null)
                continue;

            float distance = Vector2.Distance(pointerPosition, tower.Position);

            if (distance > nearestDistance)
                continue;

            nearestTower = tower;
            nearestDistance = distance;
        }

        return nearestTower;
    }

    // 인벤토리 영역 판정
    private bool IsPointerInsideInventory(Vector2 screenPosition)
    {
        if (inventoryRect == null)
            return false;

        Camera eventCamera = canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay
            ? (canvas.worldCamera != null ? canvas.worldCamera : targetCamera)
            : null;

        return RectTransformUtility.RectangleContainsScreenPoint(
            inventoryRect, screenPosition, eventCamera);
    }
}
