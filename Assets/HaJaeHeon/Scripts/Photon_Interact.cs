using Photon.Pun;
using UnityEngine;
using UnityEngine.EventSystems;

public class Photon_Interact : MonoBehaviourPunCallbacks, ITileMap, IPointerClickHandler
{
    public Vector2 Pos => throw new System.NotImplementedException();

    public TileType Type => throw new System.NotImplementedException();

    

    void Start()
    {
        
    }

    void Update()
    {
        
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        throw new System.NotImplementedException();

    }
}
