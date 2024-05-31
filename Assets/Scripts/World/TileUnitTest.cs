using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileUnitTest : MonoBehaviour
{
    public List<Tile> tiles;

    // Start is called before the first frame update
    void Start()
    {
        TilePosition globalPos = new TilePosition(new Vector2Int(2, 3), TileDirection.East);
        TilePosition localPos = new TilePosition(new Vector2Int(2, 0), TileDirection.North);

        var trans = new TilePositionTransformation(globalPos, localPos);

        Debug.Log(trans.TransformTilePosition(new TilePosition(new Vector2Int(1, 1), TileDirection.West)));

        Debug.Log(trans.InverseTransformTilePosition(new TilePosition(new Vector2Int(-1, 3), TileDirection.North)));

        Debug.Log(trans.InverseTransformTilePosition(new TilePosition(new Vector2Int(1, 2), TileDirection.North)));

        //var gen = new TrackGenerator(50, 3, 200, tiles);
        //var res = gen.GenerateTrack(new TilePosition(new Vector2Int(30, 0), TileDirection.North), new TileDirection[] { TileDirection.North, TileDirection.East, TileDirection.West });

        //if(res != null)
        //{
        //    for(int i= 0; i<res.Count; i++)
        //    {
        //        Instantiate(res[i].tile, new Vector3(res[i].pos.position.x, 0, res[i].pos.position.y) * WorldConstants.TileSize - Quaternion.Euler(0, res[i].pos.direction * 90, 0) * res[i].tile.TransformMaskPointToLocal(res[i].tile.mask.entryPosition.position), Quaternion.Euler(0, res[i].pos.direction * 90, 0));
        //    }
        //}

        //if (trans.TransformPosition(localPos) != globalPos)
        //{
        //    Debug.LogError(trans.TransformPosition(localPos) + " does not equal " + globalPos);
        //}
        //else
        //{
        //    Debug.Log(trans.TransformPosition(localPos) + " equals " + globalPos);
        //}
    }

    // Update is called once per frame
    void Update()
    {

    }
}
