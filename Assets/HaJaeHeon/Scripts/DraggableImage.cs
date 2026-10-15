using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class DraggableImage : MonoBehaviour, IDragHandler, IEndDragHandler, IBeginDragHandler
{
    public string imageId;
    public ImageSocket currentSocket {  get; private set; }
    
    [SerializeField] private Canvas canvas;
    [SerializeField] private GameObject rootPanel;
    
    private CanvasGroup canvasGroup;
    private Button imageButton;

    private RectTransform rect;

    private Transform originParentTransform;

    private Transform beforeParentTransform;
    private Vector3 beforePosition;

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
        if (rect == null || canvas == null || rootPanel == null)
            return;

        beforeParentTransform = rect.parent;
        beforePosition = rect.position;

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

        if(!dropSuccessed)
        {
            rect.SetParent(beforeParentTransform);
            rect.position = beforePosition;
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
