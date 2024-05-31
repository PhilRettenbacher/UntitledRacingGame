using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Tileset", menuName = "TileSet/TileSet", order = 1)]
public class TileSet : ScriptableObject
{
    public List<Tile> tiles = new List<Tile>();
    public Tile startTile;
}
