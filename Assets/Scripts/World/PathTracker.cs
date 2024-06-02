using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathTracker : MonoBehaviour
{
    public Track track;

    public int closestPoint = -1;
    public int secondClosestPoint = -1;

    public float currentChunkDistance = -1;
    public float currentDistance = -1;
    public int lapCount { get; private set; } = 0;

    public int currentChunkId = -1;

    // Start is called before the first frame update
    void Start()
    {
        track = GameObject.FindGameObjectWithTag("RaceManager")?.GetComponent<Track>();
    }

    // Update is called once per frame
    void Update()
    {
        if (track == null || !track.IsGenerated)
            return;

        int lastChunk = currentChunkId;

        currentChunkId = track.GetChunkIdOfPosition(transform.position);
        if (currentChunkId == -1)
            return;

        if(currentChunkId == 0 && lastChunk == track.chunks.Count - 1)
        {
            lapCount ++;
        }
        else if(currentChunkId == track.chunks.Count - 1 && lastChunk == 0)
        {
            Debug.Log(track.chunks.Count);
            lapCount --;
        }

        var chunk = track.chunks[currentChunkId];

        closestPoint = FindClosestPoint(chunk.centerPath, chunk.transform.position.XZ());
        secondClosestPoint = FindSecondClosestPoint(chunk.centerPath, chunk.transform.position.XZ(), closestPoint);

        Debug.DrawLine(transform.position, (chunk.centerPath.subdivisionPoints[closestPoint].position + chunk.transform.position.XZ()).X0Z());
        Debug.DrawLine(transform.position, (chunk.centerPath.subdivisionPoints[secondClosestPoint].position + chunk.transform.position.XZ()).X0Z());

        Vector2 distancePoints = chunk.centerPath.subdivisionPoints[secondClosestPoint].position - chunk.centerPath.subdivisionPoints[closestPoint].position;
        Vector2 distanceToClosestPoint = transform.position.XZ() - (chunk.centerPath.subdivisionPoints[closestPoint].position + chunk.transform.position.XZ());

        float angle = Vector2.Angle(distancePoints, distanceToClosestPoint);
        float distanceOnLineT = Mathf.Cos(angle * Mathf.Deg2Rad) * distanceToClosestPoint.magnitude / distancePoints.magnitude;

        currentChunkDistance = Mathf.Lerp(chunk.centerPath.subdivisionPoints[closestPoint].distance, chunk.centerPath.subdivisionPoints[secondClosestPoint].distance, distanceOnLineT);
        currentDistance = chunk.centerPathStartingDistance + currentChunkDistance + lapCount * track.Distance;

        Vector2 pointOnLine = Vector2.Lerp(chunk.centerPath.subdivisionPoints[closestPoint].position, chunk.centerPath.subdivisionPoints[secondClosestPoint].position, distanceOnLineT);
        Debug.DrawLine(transform.position, (pointOnLine + chunk.transform.position.XZ()).X0Z());

    }

    int FindClosestPoint(Path path, Vector2 offset)
    {
        int closestPoint = -1;
        float closestSquaredDistance = 0;
        for(int i = 0; i<path.subdivisionPoints.Count; i++)
        {
            var squaredDistance = ((path.subdivisionPoints[i].position + offset) - transform.position.XZ()).sqrMagnitude;

            if(closestPoint == -1 || closestSquaredDistance > squaredDistance)
            {
                closestPoint = i;
                closestSquaredDistance = squaredDistance;
            }
        }

        return closestPoint;
    }
    int FindSecondClosestPoint(Path path, Vector2 offset, int closestPoint)
    {
        if (closestPoint == 0)
            return 1;
        if (closestPoint == path.subdivisionPoints.Count - 1)
            return closestPoint - 1;

        var pointBehindDistance = ((path.subdivisionPoints[closestPoint - 1].position + offset) - transform.position.XZ()).sqrMagnitude;
        var pointAheadDistance = ((path.subdivisionPoints[closestPoint + 1].position + offset) - transform.position.XZ()).sqrMagnitude;

        if (pointAheadDistance > pointBehindDistance)
            return closestPoint - 1;
        return closestPoint + 1;
    }
}
