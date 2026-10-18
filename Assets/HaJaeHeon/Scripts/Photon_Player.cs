using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public enum MyColor
{
    Red, Green, Blue
}

public class Photon_Player : MonoBehaviourPunCallbacks
{
    private const string ColorKey = "PlayerColor";

    
    [SerializeField] private Image cursorImage;
    [SerializeField] private RectTransform myRectTransform;
    [SerializeField] private RectTransform canvasRect;
    
    
    // 카메라 캐싱
    private Camera mainCamera;
    //private SpriteRenderer targetSprite;


    public MyColor currentMyColor;

    private void Awake()
    {
        cursorImage.raycastTarget = false;
        myRectTransform = GetComponent<RectTransform>();
    }

    private void Start()
    {
        canvasRect = CursorRoot.Root;

        if( canvasRect == null)
        {
            Debug.LogError("Rect canvas is null");
            return;
        }

        transform.SetParent(canvasRect, false);
        // 플레이어 본인이 사용할 카메라 캐싱
        if (photonView.IsMine)
        {
            mainCamera = Camera.main;
            //SetMyColor(currentMyColor);
            ApplyColor(photonView.Owner.CustomProperties);
            Cursor.visible = false;
        }

        if(photonView.Owner != null)
        {
            ApplyColor(photonView.Owner.CustomProperties);
        }
    }



    private void Update()
    {
        // 플레이어 본인이 아닌 경우 , 카메라 캐싱 안되어있으면 return;
        if (!photonView.IsMine || mainCamera == null)
            return;

        //Vector3 mousePosition = Input.mousePosition;

        //mousePosition.z =
        //    mainCamera.WorldToScreenPoint(transform.position).z;

        //Vector3 target =
        //    mainCamera.ScreenToWorldPoint(mousePosition);

        //target.z = transform.position.z;

        //transform.position = target;

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect, Input.mousePosition, null, out Vector2 localMousePoint))
        {
            myRectTransform.localPosition = new Vector3(localMousePoint.x, localMousePoint.y, 0f);
        }
    }

    //private void SetMyColor(MyColor mColor)
    //{
    //    if (!photonView.IsMine)
    //        return;

    //    currentMyColor = mColor;

    //    Hashtable properties = new Hashtable
    //    {
    //        { ColorKey, (int) mColor}
    //    };

    //    PhotonNetwork.LocalPlayer.SetCustomProperties(properties);
    //}

    private void ApplyColor(Hashtable properties)
    {
        if(!properties.TryGetValue(ColorKey, out object value) || !(value is int colorValue))
        {
            return;
        }

        currentMyColor = (MyColor)colorValue;
        cursorImage.color = GetColor(currentMyColor);
    }

    private Color GetColor(MyColor mColor)
    {
        switch (mColor)
        {
            case MyColor.Red:
                return Color.red;

            case MyColor.Green:
                return Color.green;

            case MyColor.Blue:
                return Color.blue;

            default:
                return Color.white;
        }
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        base.OnPlayerPropertiesUpdate(targetPlayer, changedProps);

        if (targetPlayer.ActorNumber != photonView.OwnerActorNr)
            return;

        ApplyColor(changedProps);
    }
}
