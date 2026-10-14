using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Photon_Interact : MonoBehaviourPunCallbacks, ITileMap, IPointerClickHandler
{
    public Vector2 Pos => throw new System.NotImplementedException();

    public TileType Type => throw new System.NotImplementedException();

    private List<RaycastResult> rayResult = new();

    void Start()
    {

    }

    void Update()
    {

    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // raycast 이전 결과 삭제
        rayResult.Clear();

        EventSystem.current.RaycastAll(eventData, rayResult);

        foreach (RaycastResult result in rayResult)
        {
            // 자신은 제외
            if (result.gameObject == gameObject)
                continue;

            GameObject target = result.gameObject;

            Debug.Log($"감지된 객체: {target.name}");
            Debug.Log($"객체 위치: {target.transform.position}");
            break;
        }
    }
}
