using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public static class EditorUtils
{
    public static void DrawPointPath(Path path, Vector3 pos, Color primaryColor, Color secondaryColor, float scale = 1)
    {
        if (path == null || path.points.Count == 0)
            return;

        var prevColor = Handles.color;

        Handles.color = primaryColor;

        for (int i = 0; i < path.points.Count - 1; i++)
        {
            var position = path.points[i].position.X0Z() * scale + pos;

            Handles.DrawWireCube(position , Vector3.one);


            Handles.DrawLine(path.points[i].endPos.X0Z() * scale + pos , path.points[i + 1].startPos.X0Z() * scale + pos);

            if (path.points[i].hasRadius)
            {
                var circleCenter = path.points[i].circleCenter.X0Z() * scale + pos;
                var entryPoint = path.points[i].startPos.X0Z() * scale + pos;
                var exitPoint = path.points[i].endPos.X0Z() * scale + pos;

                var normal = Vector3.Cross(circleCenter - position, entryPoint - position);

                Handles.DrawWireArc(circleCenter, normal, (entryPoint - circleCenter), path.points[i].calculatedAngle, path.points[i].calculatedRadius * scale);

                Handles.color = secondaryColor;

                Handles.DrawLine(position, entryPoint);
                Handles.DrawLine(position, exitPoint);

                Handles.color = primaryColor;
            }


        }

        Handles.DrawWireCube(path.points[path.points.Count - 1].position.X0Z() * scale + pos, Vector3.one);
        Handles.color = prevColor;
    }

    public static void DrawPathSubdivisions(Path path, Vector3 pos, Color primaryColor, Color secondaryColor, float scale = 1)
    {
        if (path == null || path.subdivisionPoints.Count == 0)
            return;

        var prevColor = Handles.color;

        Handles.color = secondaryColor;

        Handles.DrawPolyLine(path.subdivisionPoints.Select(x => x.position.X0Z() * scale + pos).ToArray());

        Handles.color = primaryColor;

        for(int i = 0; i<path.subdivisionPoints.Count; i++)
        {
            Handles.DrawWireCube(path.subdivisionPoints[i].position.X0Z() * scale + pos, Vector3.one);
        }

        Handles.color = prevColor;
    }
}
