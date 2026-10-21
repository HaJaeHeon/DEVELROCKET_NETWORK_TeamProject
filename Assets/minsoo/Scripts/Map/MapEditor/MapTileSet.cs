using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(menuName = "TD/MapTileSet")]
public class MapTileSet : ScriptableObject
{
    [SerializeField] private TileBase[] logicTiles = new TileBase[5];
    [SerializeField] private TileBase[] visualTiles = new TileBase[5];
    public TileBase Logic(TileType t) => logicTiles[(int)t];
    public TileBase Visual(TileType t) => visualTiles[(int)t];
}