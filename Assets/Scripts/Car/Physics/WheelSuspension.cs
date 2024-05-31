using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[SelectionBase]
public class WheelSuspension : MonoBehaviour
{
    [SerializeField]
    public float travel = 0.4f;
    [SerializeField]
    public float radius = 0.3f;
    [SerializeField]
    private LayerMask isGround;
    [SerializeField]
    private float extraRayLength = 0.5f;
    [SerializeField]
    private float forceMultiplier = 5;
    [SerializeField]
    private float damping = 2f;
    [SerializeField]
    private float maxWheelForce = 4;
    [SerializeField]
    public float idealSlipAngleDeg = 10;
    [SerializeField]
    public AnimationCurve slipCurve;
    [SerializeField]
    public AnimationCurve slipFalloffCurve;

    private Vector3 centerPosition;
    private Vector3 centerContactPosition;
    private Vector3 highestContactPosition;

    private Rigidbody rb;
    private RaycastHit lastHit;

    private float additionalForce = 0;

    public bool onGround { get; private set; }
    public float currentSteeringAngle { get; set; }
    public float currentSuspensionForce { get; private set; }
    public Vector3 currentWheelForce { get; private set; }
    public float currentMaxWheelForce { get => maxWheelForce * currentSuspensionForce; }
    public float travelVelocity { get; private set; } //negative == retracting, positive == extending
    public float currentTravel { get; private set; } //0 == fully retracted, travel == fully extended
    public float lateralSlipAngle { get; private set; }
    public float neutralSlipAngle { get; private set; } //the slip Angle if currentSteeringAngle was zero
    public bool isSlipping { get; set; }
    public float wheelPower { get; set; }
    public float wheelSpeed { get; private set; }
    public float brakePower { get; set; } //0 to 1
    public Vector3 groundVelocity { get; private set; }

    // Start is called before the first frame update
    void Start()
    {
        rb = transform.GetComponentInParent<Rigidbody>();

        centerPosition = transform.localPosition;
        centerContactPosition = centerPosition + Vector3.down * radius;
        highestContactPosition = centerContactPosition + Vector3.up * travel / 2;
    }

    private void Update()
    {
        transform.localRotation = Quaternion.Euler(0, currentSteeringAngle, 0);
    }

    private void FixedUpdate()
    {
        float force = rb.mass / 4 * Physics.gravity.magnitude * forceMultiplier;

        RaycastHit hit;

        //Highest possible wheel point (from wheel contact surface)
        Vector3 rayOrigin = transform.parent.TransformPoint(highestContactPosition);

        Ray ray = new Ray(rayOrigin + transform.up * extraRayLength, -transform.up);

        //Debug.DrawLine(rayOrigin, rayOrigin + ray.direction * travel, Color.white);

        if (Physics.Raycast(ray, out hit, travel + extraRayLength, isGround))
        {
            float lastTravel = Mathf.Max(0, currentTravel);

            onGround = true;

            var projectedNormal = Vector3.Project(transform.up, hit.normal);



            currentTravel = (hit.distance - extraRayLength);

            float travelT = currentTravel / travel;

            transform.localPosition = centerPosition + Vector3.down * (currentTravel - travel / 2);


            travelVelocity = (Mathf.Max(0, currentTravel) - lastTravel) / Time.fixedDeltaTime;



            //Vector3 relativeVelocity = rb.GetPointVelocity(transform.position);

            currentSuspensionForce = (-travelVelocity * damping + force * (travel - currentTravel) / travel);

            currentSuspensionForce = Mathf.Max(currentSuspensionForce + additionalForce, 0);

            Debug.DrawLine(hit.point, hit.point + hit.normal * currentSuspensionForce / force);
            Debug.DrawLine(hit.point + hit.normal * currentSuspensionForce / force, hit.point + hit.normal * (currentSuspensionForce - additionalForce) / force, additionalForce > 0 ? Color.green : Color.red);

            currentWheelForce = CalculateWheelForce(hit);

            rb.AddForceAtPosition(hit.normal * currentSuspensionForce + currentWheelForce, hit.point);

            if (currentTravel > 0)
            {
                //Debug.DrawLine(ray.origin + ray.direction * extraRayLength, ray.origin + ray.direction * hit.distance, Color.green);
            }
            else
            {
                //Debug.DrawLine(ray.origin + ray.direction * extraRayLength, ray.origin + ray.direction * hit.distance, Color.red);
                //rb.AddForceAtPosition(transform.up * -relativeVelocity.y / 4, transform.position, ForceMode.VelocityChange);
            }

            lastHit = hit;
        }
        else
        {
            currentTravel = travel;

            onGround = false;
            wheelSpeed = 0;
            transform.localPosition = centerPosition + Vector3.down * travel / 2;
            currentWheelForce = Vector3.zero;
        }
        additionalForce = 0;
    }

    public void AddForce(float force)
    {
        additionalForce += force;
    }

    Vector3 CalculateWheelForce(RaycastHit hit)
    {
        //Vehicle velocity

        isSlipping = false;

        var vehicleVelocity = Vector3.ProjectOnPlane(rb.GetPointVelocity(hit.point), hit.normal);

        //Suspension velocity

        var suspensionVelocity = Vector3.ProjectOnPlane(transform.up * -travelVelocity, hit.normal);

        groundVelocity = vehicleVelocity + suspensionVelocity;

        var groundRight = Vector3.ProjectOnPlane(transform.rotation * Vector3.right, hit.normal);
        var groundForward = Vector3.ProjectOnPlane(transform.rotation * Vector3.forward, hit.normal);

        var rightVelocity = Vector3.Project(groundVelocity, groundRight);
        var forwardVelocity = Vector3.Project(groundVelocity, groundForward);

        wheelSpeed = forwardVelocity.magnitude;
        

        Vector3 sumOfForces;
        if (groundVelocity.magnitude < 0.1f)
        {
            lateralSlipAngle = 0;
            neutralSlipAngle = 0;
            sumOfForces = groundForward * wheelPower;
        }
        else
        {
            lateralSlipAngle = Vector3.SignedAngle(groundForward, groundVelocity, hit.normal);
            if(Mathf.Abs(lateralSlipAngle) > 90)
            {
                lateralSlipAngle = Mathf.Sign(lateralSlipAngle) * 180 - lateralSlipAngle;
            }

            neutralSlipAngle = lateralSlipAngle + currentSteeringAngle;

            float slipFactor;
            if (Mathf.Abs(lateralSlipAngle) <= idealSlipAngleDeg)
            {
                slipFactor = slipCurve.Evaluate(Mathf.Abs(lateralSlipAngle) / idealSlipAngleDeg) * Mathf.Sign(lateralSlipAngle);
                Debug.Log("SlipFactor: " + slipFactor);
            }
            else
            {
                slipFactor = slipFalloffCurve.Evaluate((Mathf.Abs(lateralSlipAngle) / idealSlipAngleDeg) - 1) * Mathf.Sign(lateralSlipAngle);
                isSlipping = true;
            }
            float lateralForce = slipFactor * maxWheelForce * currentSuspensionForce;

            sumOfForces =
                -groundRight * lateralForce +
                groundForward * wheelPower * (1 + Mathf.Min(1, Mathf.Abs(lateralSlipAngle) / idealSlipAngleDeg)) - //Test for drift force sort of thing
                forwardVelocity.normalized * (forwardVelocity.magnitude / groundVelocity.magnitude) * brakePower * currentSuspensionForce;

            if(sumOfForces.magnitude > currentMaxWheelForce)
            {
                sumOfForces = sumOfForces.normalized * currentMaxWheelForce;
                isSlipping = true;
            }

            sumOfForces += groundForward * Mathf.Abs(lateralSlipAngle) / 360 * Mathf.Max(20, forwardVelocity.magnitude) * 100;

        }
        return sumOfForces;

    }
}
