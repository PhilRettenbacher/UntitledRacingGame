using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileMeshGenerator : MonoBehaviour
{
    public MeshFilter mf;
    public Tile tile;

    public Mesh preset;
    public float presetLength = 1;
    public float presetStartPosition = 0;
    public float presetWidth = 1;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void GenerateTileMesh()
    {
        if (tile == null)
            tile = gameObject.GetComponent<Tile>();

        if (mf.sharedMesh == null)
            mf.sharedMesh = new Mesh();

        float avgDistance = (tile.leftBorder.distance + tile.rightBorder.distance) / 2;

        int subdivisions = Mathf.Max(1, Mathf.RoundToInt(avgDistance * WorldConstants.TileSize / presetLength));


        Vector3[] verts = new Vector3[subdivisions * preset.vertexCount];
        int[] tris = new int[subdivisions * preset.triangles.Length];
        Vector2[] uvs = new Vector2[subdivisions * preset.uv.Length];

        float startDistance = 0;

        for (int i = 0; i < subdivisions; i++)
        {
            float endDistance = ((i + 1) / (float)subdivisions) * avgDistance;

            int startIdx = i * preset.vertexCount;
            int triStartIdx = i * preset.triangles.Length;

            for (int v = 0; v < preset.vertexCount; v++)
            {
                float x = preset.vertices[v].y;
                float y = preset.vertices[v].z;
                float z = preset.vertices[v].x;

                uvs[v + startIdx] = preset.uv[v];

                float currDistance = Mathf.Lerp(startDistance, endDistance, (z - presetStartPosition) / presetLength);

                Vector3 r = tile.TransformMaskPointToWorld(tile.rightBorder.GetPositionByDistance(tile.rightBorder.distance * currDistance / avgDistance));
                Vector3 l = tile.TransformMaskPointToWorld(tile.leftBorder.GetPositionByDistance(tile.leftBorder.distance * currDistance / avgDistance));

                Vector3 currPos = Vector3.Lerp(l, r, x / (presetWidth) + 0.5f) + Vector3.up * y;

                verts[startIdx + v] = currPos * tile.modelTileScale / WorldConstants.TileSize;
            }
            for (int t = 0; t < preset.triangles.Length; t++)
            {
                tris[triStartIdx + t] = preset.triangles[t] + startIdx;
            }

            startDistance = endDistance;
        }
        mf.sharedMesh.vertices = (verts);
        mf.sharedMesh.triangles = tris;
        mf.sharedMesh.uv = uvs;

        mf.sharedMesh.RecalculateNormals();
    }

    public void DeleteTileMesh()
    {

    }
}
