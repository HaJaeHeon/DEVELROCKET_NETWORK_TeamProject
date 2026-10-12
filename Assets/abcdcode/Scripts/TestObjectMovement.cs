using Photon.Pun;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PhotonView))]
public class TestObjectMovement : MonoBehaviourPun, IPunObservable
{
    [SerializeField] private float moveSpeed = 5f;

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        throw new System.NotImplementedException();
    }


    private void Update()
    {
        // 소유자만 입력을 처리하고, 위치는 PhotonTransformView가 다른 클라이언트로 동기화한다.
        if (!photonView.IsMine)
            return;

        Keyboard keyboard = Keyboard.current;
        if (keyboard == null)
            return;

        Vector2 input = Vector2.zero;
        if (keyboard.wKey.isPressed) input.y += 1f;
        if (keyboard.sKey.isPressed) input.y -= 1f;
        if (keyboard.dKey.isPressed) input.x += 1f;
        if (keyboard.aKey.isPressed) input.x -= 1f;

        if (input.sqrMagnitude > 1f)
            input.Normalize();

        transform.Translate(input * (moveSpeed * Time.deltaTime), Space.World);
    }
}
