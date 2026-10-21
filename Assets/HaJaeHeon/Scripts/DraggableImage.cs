using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class DraggableImage : MonoBehaviour, IDragHandler, IEndDragHandler, IBeginDragHandler
{
    public string imageId;
    public ImageSocket currentSocket {  get; private set; }
    
    public Canvas canvas;
    public GameObject rootPanel;
    
    private CanvasGroup canvasGroup;
    private Button imageButton;

    private RectTransform rect;

    private Transform originParentTransform;

    private Transform beforeParentTransform;
    private Vector3 beforeAnchoredPosition;
    private Vector3 beforeLocalScale;
    private Quaternion beforeLocalRotation;

    private RectTransform dragPlane;
    private Vector3 dragOffset;

    private bool isDragging;
    private bool dropSuccessed;

    private Image myImage;
    private Image backImage;


    private void Awake()
    {
        myImage = GetComponent<Image>();

        // 타워가 사용된거처럼 보이기 위해서 남기는 복제품
        backImage = transform.parent.GetComponent<Image>();
        backImage.sprite = myImage.sprite;
        backImage.color = Color.gray;

        rect = GetComponent<RectTransform>();
        imageButton = GetComponent<Button>();
        if(canvas == null)
            canvas = gameObject.transform.root.GetComponent<Canvas>();

        canvasGroup = GetComponent<CanvasGroup>();

        originParentTransform = rect.parent;
    }

    private void OnEnable()
    {
        imageButton.onClick.AddListener(DetachSocket);
    }
    private void OnDisable()
    {
        imageButton.onClick.RemoveListener(DetachSocket);
    }

    // 드래그가 시작될 떄 한 번 호출 (원래 위치 저장)
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (rect == null || rootPanel == null || canvasGroup == null)
            return;

        dragPlane = rootPanel.transform as RectTransform;

        if (dragPlane == null)
            return;

        // 원래 부모를 기준으로 위치와 크기 배율을 저장합니다.
        beforeParentTransform = rect.parent;
        beforeAnchoredPosition = rect.anchoredPosition3D;
        beforeLocalScale = rect.localScale;
        beforeLocalRotation = rect.localRotation;

        // 부모를 바꿀 때 현재 화면 위치를 유지합니다.
        rect.SetParent(dragPlane, true);
        rect.SetAsLastSibling();

        // 처음 집은 지점을 유지합니다.
        dragOffset = Vector3.zero;

        if (RectTransformUtility.ScreenPointToWorldPointInRectangle(dragPlane, eventData.position, eventData.pressEventCamera, out Vector3 pointerPosition))
            dragOffset = rect.position - pointerPosition;

        canvasGroup.blocksRaycasts = false;
        isDragging = true;
        dropSuccessed = false;
    }

    // 드래그 중일때 호출(이미지 위치 이동)
    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging || rect == null || dragPlane == null)
            return;

        if (RectTransformUtility.ScreenPointToWorldPointInRectangle(dragPlane, eventData.position, eventData.pressEventCamera, out Vector3 pointerPosition))
            rect.position = pointerPosition + dragOffset;
    }

    // 드래그가 종료되면 한 번 호출 (소켓에 끼워넣거나 원래 위치로 복귀)
    public void OnEndDrag(PointerEventData eventData)
    {
        if (!isDragging || rect == null)
            return;

        if (!dropSuccessed && beforeParentTransform != null)
        {
            rect.SetParent(beforeParentTransform, false);
            rect.anchoredPosition3D = beforeAnchoredPosition;
            rect.localScale = beforeLocalScale;
            rect.localRotation = beforeLocalRotation;
        }

        canvasGroup.blocksRaycasts = true;
        isDragging = false;
        dragPlane = null;
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

    // currentSocket 비우기
    public void ClearSocket(ImageSocket socket)
    {
        if (currentSocket == socket)
            currentSocket = null;
    }

    // 버튼 등록용( 소켓에서 떼어내기 )
    public void DetachSocket()
    {
        if (currentSocket == null)
            return;

        currentSocket.DetachImage(this);
        transform.SetParent(originParentTransform, false);

        SetRect();
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
