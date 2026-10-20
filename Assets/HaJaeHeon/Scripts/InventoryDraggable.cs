using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventoryDraggable : MonoBehaviour, IDragHandler, IEndDragHandler, IBeginDragHandler
{
    public string imageId;
    public ImageSocket currentSocket { get; private set; }
    public GameObject InventoryItemObject { get; private set; }

    public Canvas canvas;
    public GameObject rootPanel;

    private CanvasGroup canvasGroup;

    private RectTransform rect;

    private Transform beforeParentTransform;
    private Vector3 beforeAnchoredPosition;
    private Quaternion beforeLocalRotation;
    private Vector3 beforeLocalScale;
    private int beforeSiblingIndex;

    private bool isDragging;
    private bool dropSuccessed;

    private void Awake()
    {
        rect = GetComponent<RectTransform>();
        if (canvas == null)
            canvas = gameObject.transform.root.GetComponent<Canvas>();

        canvasGroup = GetComponent<CanvasGroup>();
    }

    // 드래그가 시작될 떄 한 번 호출 (원래 위치 저장)
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (SocketManager.Instance == null || SocketManager.Instance.IsLocalOperationPending)
            return;

        if (rect == null || canvas == null || rootPanel == null)
            return;

        Button inventoryButton = GetComponentInParent<Button>();

        if (inventoryButton == null)
        {
            Debug.LogError("인벤토리 버튼을 찾을 수 없습니다.");
            return;
        }

        InventoryItemObject = inventoryButton.gameObject;

        beforeParentTransform = rect.parent;
        beforeAnchoredPosition = rect.anchoredPosition3D;
        beforeLocalRotation = rect.localRotation;
        beforeLocalScale = rect.localScale;
        beforeSiblingIndex = rect.GetSiblingIndex();

        gameObject.transform.SetParent(rootPanel.transform);
        canvasGroup.blocksRaycasts = false;

        isDragging = true;
        dropSuccessed = false;
    }

    // 드래그 중일때 호출(이미지 위치 이동)
    public void OnDrag(PointerEventData eventData)
    {
        if (rect == null)
            return;

        if (!isDragging)
            return;

        rect.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }

    // 드래그가 종료되면 한 번 호출 (소켓에 끼워넣거나 원래 위치로 복귀)
    public void OnEndDrag(PointerEventData eventData)
    {
        if (rect == null)
            return;

        if (!isDragging)
            return;

        if (!dropSuccessed)
        {
            rect.SetParent(beforeParentTransform, false);
            rect.SetSiblingIndex(beforeSiblingIndex);
            rect.anchoredPosition3D = beforeAnchoredPosition;
            rect.localRotation = beforeLocalRotation;
            rect.localScale = beforeLocalScale;
        }

        canvasGroup.blocksRaycasts = true;

        isDragging = false;
    }

    // 이미지 장착
    public void AttatchImage(ImageSocket socket, RectTransform tr)
    {
        currentSocket = socket;

        // 이미지를 장착 기준점의 자식으로 옮기기
        rect.SetParent(tr, false);

        SetRect();

        dropSuccessed = true;
    }

    // 이미지의 rectTransform을 가지고 위치를 중앙으로 맞추고 회전도 맞춤
    public void SetRect()
    {
        // 이미지의 기준과 중심을 중앙으로 정렬
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);

        // 크기는 유지하고 위치와 회전을 정렬
        rect.anchoredPosition3D = Vector3.zero;
        rect.localRotation = Quaternion.identity;
        rect.localScale = Vector3.one;
    }
}
