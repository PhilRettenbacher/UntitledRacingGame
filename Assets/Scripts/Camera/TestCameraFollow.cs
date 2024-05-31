using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestCameraFollow : MonoBehaviour
{
    Transform target;

    Track track;
    public bool followTrack;
    PathTracker pathTracker;

    Vector3 currentVelocity;
    float smoothTime = 0.1f;
    public float lookAheadDistance = 5;

    // Start is called before the first frame update
    void Start()
    {
        track = GameObject.FindGameObjectWithTag("RaceManager").GetComponent<Track>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!target)
        {
            target = GameObject.FindGameObjectWithTag("Car")?.transform;

            if (target == null)
                return;

            pathTracker = target.GetComponent<PathTracker>();
        }



        Vector3 targetPosition;

        if (followTrack)
        {
            targetPosition = track.GetPositionAtDistance(pathTracker.currentDistance + lookAheadDistance);
        }
        else
        {
            targetPosition = target.position;
        }

        transform.position = Vector3.SmoothDamp(transform.position, targetPosition + transform.forward * (-40), ref currentVelocity, smoothTime);
    }
}
