using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct PathPoint
{
    public Vector2 position;
    public float radius;
    public bool hasRadius;
    public float calculatedRadius;
    public float calculatedDistanceToTransition;
    public float cumulativeDistance;
    public float calculatedAngle;
    public Vector2 startPos;
    public Vector2 endPos;
    public Vector2 circleCenter;
}
