using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MapDBData", menuName = "SOData/MapDBData")]
public class MapDB : ScriptableObject
{
    [SerializeField] private List<MapSOData> mapDatas;
    public List<MapSOData> MapDatas => mapDatas;
}
