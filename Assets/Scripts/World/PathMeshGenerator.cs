using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PathMeshGenerator
{
    //static float presetLength = 6;
    //static float presetStartPosition = -3f;

    public static void GenerateMesh(Path path, Mesh presetMesh, Mesh workingMesh, float presetLength, float presetStartPosition)
    {

        int subdivisions = Mathf.Max(1, Mathf.RoundToInt(path.distance / presetLength));


        Vector3[] verts = new Vector3[subdivisions * presetMesh.vertexCount];
        int[] tris = new int[subdivisions * presetMesh.triangles.Length];
        Vector2[] uvs = new Vector2[subdivisions * presetMesh.uv.Length];

        float startDistance = 0;

        for (int i = 0; i < subdivisions; i++)
        {
            float endDistance = ((i + 1) / (float)subdivisions) * path.distance;

            int startIdx = i * presetMesh.vertexCount;
            int triStartIdx = i * presetMesh.triangles.Length;

            for (int v = 0; v < presetMesh.vertexCount; v++)
            {
                float x = presetMesh.vertices[v].y;
                float y = presetMesh.vertices[v].z;
                float z = presetMesh.vertices[v].x;

                uvs[v + startIdx] = presetMesh.uv[v];

                float currDistance = Mathf.Lerp(startDistance, endDistance, (z - presetStartPosition) / presetLength);

                Vector3 pos = path.GetPositionByDistance(currDistance).X0Z();
                Vector3 normal = path.GetNormalByDistance(currDistance).X0Z();

                Vector3 currPos = pos + x * normal + Vector3.up * y;

                verts[startIdx + v] = currPos;
            }
            for (int t = 0; t < presetMesh.triangles.Length; t++)
            {

                if (!path.normalFacesRight) //Flip tri
                {
                    tris[triStartIdx + t] = presetMesh.triangles[presetMesh.triangles.Length - t - 1] + startIdx;
                }
                else
                {
                    tris[triStartIdx + t] = presetMesh.triangles[t] + startIdx;
                }
            }

            startDistance = endDistance;
        }
        workingMesh.vertices = (verts);
        workingMesh.triangles = tris;
        workingMesh.uv = uvs;
        workingMesh.RecalculateNormals();
    }
}
