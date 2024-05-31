using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Path
{
    public List<PathPoint> points = new List<PathPoint>();
    public List<PathSubdivisionPoint> subdivisionPoints = new List<PathSubdivisionPoint>();
    public float distance { get => points[points.Count - 1].cumulativeDistance; }

    public bool normalFacesRight;

    public Path()
    {

    }
    public Path(List<PathPoint> points, bool normalFacesRight)
    {
        this.points = points;
        this.normalFacesRight = normalFacesRight;
        RecalculatePoints();
    }
    public Path(Vector2 startPosition, Vector2 startDirection, Vector2 endPosition, Vector2 endDirection)
    {
        points = new List<PathPoint>();

        startDirection = startDirection.normalized;
        endDirection = endDirection.normalized;

        bool facesSameDirection = Mathf.Approximately(Vector2.Dot(startDirection, endDirection), 1);
        bool facesOppositeDirection = Mathf.Approximately(Vector2.Dot(startDirection, endDirection), -1);

        Vector2 startToEndPoint = (endPosition - startPosition);

        bool startFacesEnd = Vector2.Dot(startDirection, startToEndPoint) > 0;
        bool endFacesStart = Vector2.Dot(endDirection, -startToEndPoint) > 0;

        bool startFacesEndExact = Mathf.Approximately(Vector2.Dot(startDirection, startToEndPoint.normalized), 1);
        bool endFacesStartExact = Mathf.Approximately(Vector2.Dot(-endDirection, -startToEndPoint.normalized), 1);

        if (facesSameDirection && startFacesEndExact)
        {
            //2 points

            points.Add(new PathPoint() { position = startPosition });
            points.Add(new PathPoint() { position = endPosition });
        }
        else if (!facesSameDirection && !facesOppositeDirection && !startFacesEndExact && !endFacesStartExact)
        {
            //3 points

            Vector2 point3Pos = MathUtils.ConstraintPointMulti(startPosition, startDirection, endPosition, endDirection);

            if (!Mathf.Approximately(Vector2.Dot((point3Pos - endPosition).normalized, endDirection), 1) && !Mathf.Approximately(Vector2.Dot((point3Pos - startPosition).normalized, -startDirection), 1))
            {

                points.Add(new PathPoint() { position = startPosition });
                points.Add(new PathPoint() { position = point3Pos, hasRadius = true, radius = 1 });
                points.Add(new PathPoint() { position = endPosition });
            }
        }

        if (points.Count == 0)
        {
            //4 points

            points.Add(new PathPoint() { position = startPosition });
            points.Add(new PathPoint() { position = startPosition + startDirection, hasRadius = true, radius = 1 });
            points.Add(new PathPoint() { position = endPosition - endDirection, hasRadius = true, radius = 1 });
            points.Add(new PathPoint() { position = endPosition });
        }

        RecalculatePoints();
    }

    public void RecalculatePoints()
    {
        for (int i = 0; i < points.Count; i++)
        {
            var point = points[i];

            if (points[i].hasRadius && i > 0 && i < points.Count - 1)
            {
                point = RecalculatePoint(point, points[i - 1], points[i + 1]);
            }
            else
            {
                point.calculatedRadius = 0;
                point.calculatedDistanceToTransition = 0;
                point.startPos = point.position;
                point.endPos = point.position;
            }

            if (i == 0)
                point.cumulativeDistance = 0;
            else
            {
                Vector2 startPos = points[i - 1].position;
                Vector2 endPos = point.position;
                float currDistance = 0;
                if (points[i - 1].hasRadius)
                {
                    startPos = points[i - 1].endPos;
                    currDistance += GetArcLength(points[i - 1]) / 2;
                }

                if (point.hasRadius)
                {
                    endPos = point.startPos;
                    currDistance += GetArcLength(point) / 2;
                }

                currDistance += Vector2.Distance(startPos, endPos);

                point.cumulativeDistance = points[i - 1].cumulativeDistance + currDistance;
            }

            points[i] = point;
        }
    }

    public void CalculateSubdivisionPoints(float minDistance)
    {
        int subdivisions = Mathf.CeilToInt(distance / minDistance);

        float segmentDistance = distance / subdivisions;
        for (int i = 0; i <= subdivisions; i++)
        {
            var point = new PathSubdivisionPoint();
            point.distance = i * segmentDistance;
            point.position = GetPositionByDistance(point.distance);

            subdivisionPoints.Add(point);
        }
    }
    public Vector2 GetPositionByDistance(float distance)
    {
        int idx = GetClosestIndexFloorByDistance(distance);

        if (idx == points.Count - 1)
            return points[idx].position;

        var point1 = points[idx];
        var point2 = points[idx + 1];

        float distanceLeft = distance - points[idx].cumulativeDistance;

        float startDistance = 0;
        float endDistance = point2.cumulativeDistance - point1.cumulativeDistance;

        Vector2 startPos = point1.position;
        Vector2 endPos = point2.position;

        if (point1.hasRadius)
        {
            float arcLength = GetArcLength(point1) / 2;
            if (arcLength > distanceLeft)
            {
                //Point is on Arc
                float angle = point1.calculatedAngle / 2 + GetArcAngleByDistance(point1, distanceLeft);
                return GetArcPosition(point1, angle);
            }
            else
            {
                startPos = point1.endPos;
                startDistance = arcLength;
            }
        }

        if (point2.hasRadius)
        {
            float arcLength = GetArcLength(point2) / 2;
            if (endDistance - arcLength < distanceLeft)
            {
                //Point is on Arc
                float angle = GetArcAngleByDistance(point2, distanceLeft + arcLength - endDistance);
                return GetArcPosition(point2, angle);
            }
            else
            {
                endPos = point2.startPos;
                endDistance -= arcLength;
            }
        }

        //Point is on Line Segment

        float t = Mathf.InverseLerp(startDistance, endDistance, distanceLeft);

        return Vector2.Lerp(startPos, endPos, t);
    }
    public Vector2 GetNormalByDistance(float distance)
    {
        int idx = GetClosestIndexFloorByDistance(distance);

        if (idx == points.Count - 1)
        {
            return GetNormalAfterPointOnLine(idx - 1);
        }

        var point1 = points[idx];
        var point2 = points[idx + 1];

        float distanceLeft = distance - points[idx].cumulativeDistance;

        float endDistance = point2.cumulativeDistance - point1.cumulativeDistance;

        if (point1.hasRadius)
        {
            float arcLength = GetArcLength(point1) / 2;
            if (arcLength > distanceLeft)
            {
                //Point is on Arc
                float angle = point1.calculatedAngle / 2 + GetArcAngleByDistance(point1, distanceLeft);
                return GetArcNormal(point1, angle);
            }
        }

        if (point2.hasRadius)
        {
            float arcLength = GetArcLength(point2) / 2;
            if (endDistance - arcLength < distanceLeft)
            {
                //Point is on Arc
                float angle = GetArcAngleByDistance(point2, distanceLeft + arcLength - endDistance);
                return GetArcNormal(point2, angle);
            }
        }

        //Point is on Line Segment

        return GetNormalAfterPointOnLine(idx);
    }

    private Vector2 GetNormalAfterPointOnLine(int idx)
    {
        if (idx < 0 || idx >= points.Count - 1)
        {
            Debug.LogError("Point not within Range!");
            return Vector2.zero;
        }

        var point1Pos = points[idx].position;
        var point2Pos = points[idx + 1].position;

        var dir = (point2Pos - point1Pos).normalized;
        return dir.Normal(normalFacesRight);
    }

    private float GetArcLength(PathPoint point)
    {
        return point.calculatedRadius * 2 * Mathf.PI * (point.calculatedAngle) / 360;
    }

    private float GetArcAngleByDistance(PathPoint point, float distance)
    {
        float angle = 360 * distance / (point.calculatedRadius * 2 * Mathf.PI);
        return angle;
    }

    private Vector2 GetArcPosition(PathPoint point, float angle)
    {
        Vector2 dir = point.startPos - point.circleCenter;

        float sign = Mathf.Sign(Vector3.Cross(dir, point.position - point.circleCenter).z);

        dir = dir.Rotate(sign * angle);
        return point.circleCenter + dir;
    }

    private Vector2 GetArcNormal(PathPoint point, float angle)
    {
        Vector2 dir = point.startPos - point.circleCenter;

        float sign = Mathf.Sign(Vector3.Cross(dir, point.position - point.circleCenter).z);

        dir = dir.Rotate(sign * angle);
        return (dir * sign * (normalFacesRight ? 1 : -1)).normalized;
    }

    public bool CanPointBeConstrained(int idx)
    {
        if (idx <= 0 || idx >= points.Count - 1)
        {
            return false;
        }
        if (idx > 1 && idx < points.Count - 2)
        {
            return false;
        }

        return true;
    }

    private int GetClosestIndexFloorByDistance(float distance)
    {
        for (int i = 0; i < points.Count; i++)
        {
            if (distance < points[i].cumulativeDistance)
            {
                return Mathf.Max(0, i - 1);
            }
        }
        return points.Count - 1;
    }

    private PathPoint RecalculatePoint(PathPoint point, PathPoint prevPoint, PathPoint nextPoint)
    {
        var entryDir = point.position - prevPoint.position;
        var exitDir = nextPoint.position - point.position;

        var angle = Vector3.Angle(-entryDir, exitDir) * Mathf.Deg2Rad; //Spanned angle

        float previousDistance = Vector3.Distance(point.position, prevPoint.position); //Distance until previous point (or previous points curvature starts)
        if (prevPoint.hasRadius)
        {
            previousDistance = previousDistance - prevPoint.calculatedDistanceToTransition;
        }

        float nextDistance = Vector3.Distance(point.position, nextPoint.position);

        //REMOVED: Always try to fully use next Distance

        //if (nextPoint.hasRadius) 
        //    nextDistance = nextDistance * point.radius / (point.radius + nextPoint.radius);

        float distanceCandidate = point.radius / Mathf.Tan(angle / 2);

        var distance = Mathf.Min(distanceCandidate, previousDistance, nextDistance);

        point.calculatedDistanceToTransition = distance;
        point.calculatedRadius = distance * Mathf.Tan(angle / 2);

        Vector2 centerDir = exitDir.normalized - entryDir.normalized;
        float centerLength = distance / Mathf.Cos(angle / 2);
        point.circleCenter = point.position + centerDir.normalized * centerLength;

        point.startPos = point.position - entryDir.normalized * point.calculatedDistanceToTransition;
        point.endPos = point.position + exitDir.normalized * point.calculatedDistanceToTransition;

        point.calculatedAngle = 180 - angle * Mathf.Rad2Deg;

        return point;
    }
}
