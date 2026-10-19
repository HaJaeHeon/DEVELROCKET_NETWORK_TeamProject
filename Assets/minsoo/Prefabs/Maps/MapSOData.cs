using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "MapData", menuName = "SOData/MapData")]
public class MapSOData : SOData
{
    [SerializeField] private string mapName;
    public string Name => mapName;

    [SerializeField] private GameObject mapPrefab;
    public GameObject MapPrefab => mapPrefab;

    [SerializeField] private Sprite thumnail;
    public Sprite Thumnail => thumnail;

}
