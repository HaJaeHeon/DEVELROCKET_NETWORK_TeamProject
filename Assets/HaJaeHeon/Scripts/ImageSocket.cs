using UnityEngine;
using UnityEngine.EventSystems;

public class ImageSocket : MonoBehaviour, IDropHandler
{
    public DraggableImage CurrentImage { get; private set; }

    [SerializeField] private PresetGroup group;
    [SerializeField] private RectTransform attachPoint;

    private void Awake()
    {
        if(attachPoint ==null)
            attachPoint = GetComponent<RectTransform>();
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerDrag == null)
            return;

        DraggableImage image =
            eventData.pointerDrag.GetComponent<DraggableImage>();

        if (image != null)
            TryAttach(image);
    }

    public bool TryAttach(DraggableImage image)
    {
        // 붙는 위치, 이미지, canvas group이 비어있으면 return;
        if (attachPoint == null || image == null || group == null)
            return false;

        ImageSocket sourceSocket = image.currentSocket;
        DraggableImage targetImage = CurrentImage;

        // 자기 칸에 다시 놓은 경우
        if (sourceSocket == this)
        {
            image.AttatchImage(this, attachPoint);
            return true;
        }

        // 목적지에 다른 타워가 있는 경우: 교환
        if (targetImage != null)
        {
            // 나 자신이 없는 경우
            if (sourceSocket == null)
                return false;

            // 상대방 타워도 기존 가드절을 거쳐야함
            if (sourceSocket.attachPoint == null ||sourceSocket.CurrentImage != image || sourceSocket.group == null)
                return false;

            // 양쪽 타워가 바뀔 칸에 들어 갈 수 있는지 확인
            if (!group.CanUse(image) || !sourceSocket.group.CanUse(targetImage))
                return false;


            // 자리 바꾸기
            sourceSocket.DetachImage(image);
            DetachImage(targetImage);

            sourceSocket.CurrentImage = targetImage;
            sourceSocket.group.Register(targetImage);
            targetImage.AttatchImage(
                sourceSocket, sourceSocket.attachPoint);

            CurrentImage = image;
            group.Register(image);
            image.AttatchImage(this, attachPoint);

            return true;
        }
        // 목적지가 비어 있는 경우
        // 자리를 사용할 수 없으면 false
        if (!group.CanUse(image))
            return false;

        // 프리셋 칸에서 이동시 프리셋 칸에 남아있는 나의 정보 해체
        if (sourceSocket != null)
            sourceSocket.DetachImage(image);

        // 나의 정보 등록
        CurrentImage = image;
        group.Register(image);
        image.AttatchImage(this, attachPoint);

        return true;
    }

    public void DetachImage(DraggableImage image)
    {
        if (CurrentImage != image)
            return;

        if (group != null)
            group.Unregister(image);

        CurrentImage = null;
        image.ClearSocket(this);
    }
}
