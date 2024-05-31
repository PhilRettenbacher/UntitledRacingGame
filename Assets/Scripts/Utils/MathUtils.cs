using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MathUtils
{
    public static Vector2 ConstraintPoint(Vector2 pos, Vector2 constraintPos, Vector2 constraintDirection)
    {
        Vector2 posDir = pos - constraintPos;

        float dot = Vector2.Dot(posDir, constraintDirection.normalized);

        return constraintPos + constraintDirection.normalized * dot;
    }

    public static Vector2 XZ(this Vector3 vec)
    {
        return new Vector2(vec.x, vec.z);
    }

    public static Vector3 X0Z(this Vector2 vec)
    {
        return new Vector3(vec.x, 0, vec.y);
    }

    public static Vector2 Normal(this Vector2 vec, bool facesRight)
    {
        if (!facesRight)
            return new Vector2(-vec.y, vec.x);
        return new Vector2(vec.y, -vec.x);
    }

    public static Vector2 ConstraintPointMulti(Vector2 cPos1, Vector2 cDir1, Vector2 cPos2, Vector2 cDir2)
    {
        Vector2 v1, v2, v3, v4;
        v1 = cPos1;
        v2 = cPos1 + cDir1;
        v3 = cPos2;
        v4 = cPos2 + cDir2;

        float denominator = ((v1.x - v2.x) * (v3.y - v4.y) - (v1.y - v2.y) * (v3.x - v4.x));

        float x = ((v1.x * v2.y - v1.y * v2.x) * (v3.x - v4.x) - (v1.x - v2.x) * (v3.x * v4.y - v3.y * v4.x)) / denominator;

        float y = ((v1.x * v2.y - v1.y * v2.x) * (v3.y - v4.y) - (v1.y - v2.y) * (v3.x * v4.y - v3.y * v4.x)) / denominator;

        return new Vector2(x, y);
    }
    public static Vector2 Rotate(this Vector2 v, float degrees) //Rotates counterclockwise
    {
        float sin = Mathf.Sin(degrees * Mathf.Deg2Rad);
        float cos = Mathf.Cos(degrees * Mathf.Deg2Rad);

        float tx = v.x;
        float ty = v.y;
        v.x = (cos * tx) - (sin * ty);
        v.y = (sin * tx) + (cos * ty);
        return v;
    }
}
