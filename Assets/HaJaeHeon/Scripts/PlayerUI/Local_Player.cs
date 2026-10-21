using UnityEngine;
using UnityEngine.UI;

public class Local_Player : MonoBehaviour
{
    [SerializeField] private Image cursorImage;
    [SerializeField] private RectTransform myRectTransform;
    [SerializeField] private RectTransform canvasRect;

    // 카메라 캐싱
    private Camera mainCamera;

    private void Awake()
    {
        cursorImage.raycastTarget = false;
        cursorImage = GetComponentInChildren<Image>();
        myRectTransform = GetComponent<RectTransform>();
    }

    private void Start()
    {
        canvasRect = CursorRoot.Root;

        if (canvasRect == null)
        {
            Debug.LogError("Rect canvas is null");
            return;
        }

        transform.SetParent(canvasRect, false);

        mainCamera = Camera.main;
        ApplyColor();
    }

    private void Update()
    {
        // 카메라 캐싱 안되어있으면 return;
        if (mainCamera == null)
            return;


        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect, Input.mousePosition, null, out Vector2 localMousePoint))
        {
            myRectTransform.localPosition = new Vector3(localMousePoint.x, localMousePoint.y, 0f);
        }
    }

    private void ApplyColor()
    {
        cursorImage.color = Color.white;
    }
}

