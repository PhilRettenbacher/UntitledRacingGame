using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public float targetDistance;

    public float distanceToTarget = 40;
    public float yOffset = -5;

    Track track;

    Vector3 currentVelocity;
    float smoothTime = 0.15f;
    // Start is called before the first frame update
    void Start()
    {
        track = GameObject.FindGameObjectWithTag("RaceManager").GetComponent<Track>();
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 targetPosition = track.GetPositionAtDistance(targetDistance) + Vector3.up * yOffset;

        transform.position = Vector3.SmoothDamp(transform.position, targetPosition + transform.forward * (-distanceToTarget), ref currentVelocity, smoothTime);
    }

    public void SetTargetDistance(float distance)
    {
        targetDistance = distance;
    }
}
