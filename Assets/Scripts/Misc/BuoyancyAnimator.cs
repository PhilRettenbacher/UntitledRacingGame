using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuoyancyAnimator : MonoBehaviour
{
    public float amplitude = 1f;
    public float amplitudePeriod = 1f;
    public float xPeriod = 1f;
    public float rotationZ = 5;
    public float zPeriod = 1f;
    public float rotationX = 5;

    Vector3 defaultPosition;
    Quaternion defaultRotation;

    // Start is called before the first frame update
    void Start()
    {
        defaultPosition = transform.localPosition;
        defaultRotation = transform.localRotation;
    }

    // Update is called once per frame
    void Update()
    {
        transform.localPosition = defaultPosition + Vector3.up * Mathf.Sin(Time.time / amplitudePeriod * 2 * Mathf.PI) * amplitude;
        transform.localRotation = Quaternion.Euler(Mathf.Sin(Time.time / xPeriod * 2 * Mathf.PI) * rotationX, 0, Mathf.Sin(Time.time / zPeriod * 2 * Mathf.PI) * rotationZ) * defaultRotation;
    }
}
