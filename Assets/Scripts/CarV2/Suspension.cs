using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class Suspension : MonoBehaviour
{
    public Transform wheel;
    [SerializeField]
    private LayerMask isGround;
    [SerializeField]
    private float relativeForceConstant;
    private float absoluteForceConstant;
    [SerializeField]
    private float relativeDampingCoefficient;
    private float absoluteDampingCoefficient;
    public bool isSteerable;
    public bool isPowered;
    public float maxTravel = 0.5f;
    public float wheelRadius;
    //public float relativeMaximumWheelForce = 3;

    //public float slipForceThreshhold = 2;

    //public float sidewaysWheelForceAtMaximumAngle = 1f;

    private CarSetupData sd;

    //Outside control
    public float brakePercent; //0 to 1
    public float wheelPower; //-1 to 1
    public float steeringPercent; //-1 to 1
    public bool inReverseGear;
    public bool onGround { get; private set; }

    public float currentTravelPercent { get; private set; }
    public float currentTravelVelocity { get; private set; }
    public float currentTravelDistance { get => currentTravelPercent * maxTravel; }
    public float currentSuspensionForce { get; private set; }
    public float currentVisualSteeringAngle { get => steeringPercent * 30; }
    public bool isSlipping { get; private set; }
    public Vector3 wheelForce { get; private set; }
    public Vector3 relativeVelocity { get; private set; }

    public Rigidbody rb;

    private void OnEnable()
    {
        wheel = transform.Find("Wheel");
    }
    private void OnDisable()
    {
        wheel = null;
    }
    // Start is called before the first frame update
    void Start()
    {
        rb = transform.GetComponentInParent<Rigidbody>();      
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (!Application.isPlaying)
            return;

        absoluteForceConstant = rb.mass / 4 * relativeForceConstant * Physics.gravity.magnitude;
        absoluteDampingCoefficient = rb.mass / 4 * relativeDampingCoefficient * Physics.gravity.magnitude;

        RaycastHit hit;

        const float extraRayLength = 1f;

        Ray ray = new Ray(transform.position + transform.up * extraRayLength, -transform.up);


        if (!Physics.Raycast(ray, out hit, maxTravel + extraRayLength, isGround))
        {
            //Wheel is in the air

            onGround = false;
            isSlipping = false;
            SetTravel(1);

            currentSuspensionForce = 0;
            currentTravelVelocity = 0;
            wheelForce = Vector3.zero;
            relativeVelocity = Vector3.zero;
        }
        else
        {
            //Wheel is on the ground
            onGround = true;

            isSlipping = false;

            float lastTravelPercent = currentTravelPercent;

            SetTravel((hit.distance - extraRayLength) / maxTravel);

            //positive: Spring expands, negative: Spring compresses
            currentTravelVelocity = (currentTravelPercent - lastTravelPercent) * maxTravel / Time.fixedDeltaTime;

            //only dampen the spring under compression -- maybe change so Mathf.Max surrounds whole expression
            currentSuspensionForce = Mathf.Max(-currentTravelVelocity * absoluteDampingCoefficient, 0) + absoluteForceConstant * (1 - currentTravelPercent);

            wheelForce = CalculateRelativeWheelForce(hit);

            rb.AddForceAtPosition((hit.normal + wheelForce * 9.81f / Physics.gravity.magnitude) * currentSuspensionForce, hit.point);
        }
    }

    public void SetSetupData(CarSetupData setupData)
    {
        this.sd = setupData;
    }

    Vector3 CalculateRelativeWheelForce(RaycastHit hit)
    {
        //Velocity of the vehicle relative to the hit point

        relativeVelocity = rb.GetPointVelocity(hit.point);

        var wheelGroundVelocity = Vector3.ProjectOnPlane(relativeVelocity, hit.normal);

        //relative suspension velocity
        var suspensionGroundVelocity = Vector3.ProjectOnPlane(transform.up * currentTravelVelocity, hit.normal);

        var groundVelocity = wheelGroundVelocity + suspensionGroundVelocity;

        var suspensionGroundRight = Vector3.ProjectOnPlane(transform.rotation * Vector3.right, hit.normal).normalized;
        var suspensionGroundForward = Vector3.ProjectOnPlane(transform.rotation * Vector3.forward, hit.normal).normalized;
        var forwardVelocity = Vector3.Project(groundVelocity, suspensionGroundForward);

        var vehicleGroundVelocity = Vector3.ProjectOnPlane(rb.velocity, hit.normal);

        float lowSpeedFactor = Mathf.Min(sd.lowSpeedThreshold, vehicleGroundVelocity.magnitude) / sd.lowSpeedThreshold; //Goes from 0 to 1 on slow speeds

        float slipFactor = CalculateSidewaysSlipFactor(groundVelocity, suspensionGroundForward, hit.normal);

        Vector3 driftForce = lowSpeedFactor * Mathf.Abs(Vector3.Dot(vehicleGroundVelocity.normalized, suspensionGroundRight)) * suspensionGroundForward * Mathf.Min(sd.driftForceMaxVelocity, vehicleGroundVelocity.magnitude) * sd.driftForceFactor;

        Vector3 wheelForce = Vector3.zero;

        if(!inReverseGear)
            wheelForce += driftForce;

        float actualSidewaysForce = slipFactor * lowSpeedFactor;

        if(isPowered)
        {
            float throttlePenalty = Mathf.Lerp(1, Mathf.Lerp(sd.sidewaysForceMaxThrottlePenalty, 1, forwardVelocity.magnitude / (sd.maxSpeed - 10)), wheelPower);
            float brakePenalty = Mathf.Lerp(1, sd.sidewaysForceMaxBrakePenalty, brakePercent);

            float sidewaysForceThrottlePenalty = Mathf.Min(throttlePenalty, brakePenalty);

            if (sidewaysForceThrottlePenalty < 0.8f)
                isSlipping = true;

            if(!inReverseGear)
                actualSidewaysForce *= sidewaysForceThrottlePenalty;

            float currentVelocityFactor = 1 - Mathf.Min(forwardVelocity.magnitude / sd.maxSpeed);

            if (inReverseGear)
                currentVelocityFactor *= -1;

            wheelForce += suspensionGroundForward.normalized * wheelPower * currentVelocityFactor * sd.wheelPowerFactor;// + suspensionGroundRight * sd.sidewaysWheelForceAtMaximumAngle * neutralSteeringSidewaysFactor * sidewaysForceThrottlePenalty;
        }

        if (isSteerable)
            actualSidewaysForce *= Mathf.Lerp(1, sd.steeredWheelSidewaysForceFactor, Mathf.Abs(steeringPercent));

        //Steering
        wheelForce += suspensionGroundRight * actualSidewaysForce * sd.sidewaysWheelForceAtMaximumAngle;

        //Braking:

        wheelForce -= suspensionGroundForward.normalized * brakePercent * sd.brakeFactor;

        return wheelForce;
    }

    float CalculateSidewaysSlipFactor(Vector3 groundVelocity, Vector3 suspensionGroundForward, Vector3 hitNormal)
    {
        float slipAngle = Vector3.SignedAngle(groundVelocity, suspensionGroundForward, hitNormal); //From -180 to 180  

        bool isBackwards = false;

        if(Mathf.Abs(slipAngle) > 90)
        {
            isBackwards = true;
            slipAngle = (180 * Mathf.Sign(slipAngle) - slipAngle); //Maps slipAngle to other side
        }

        float clampedSlipAngle = ClampSlipAngle(slipAngle);

        if (isSteerable)
        {
            clampedSlipAngle = Mathf.Lerp(clampedSlipAngle, ClampSlipAngle(slipAngle += steeringPercent * (isBackwards ? -1 : 1) * sd.maxSteeringAngle), Mathf.Abs(steeringPercent));

            if (Mathf.Abs(slipAngle + steeringPercent * sd.maxSteeringAngle) > 4f * sd.idealSidewaysForceAngle)
                isSlipping = true;
        }
        else
        {
            if (Mathf.Abs(slipAngle) > sd.idealSidewaysForceAngle)
                isSlipping = true;
        }
        return clampedSlipAngle / sd.idealSidewaysForceAngle;
    }

    float ClampSlipAngle(float slipAngle)
    {
        return Mathf.Clamp(slipAngle, -sd.idealSidewaysForceAngle, sd.idealSidewaysForceAngle);
    }

    void SetTravel(float travelPercent)
    {
        //travelPercent: 0% -> fully compressed, 100% -> fully decompressed

        currentTravelPercent = Mathf.Clamp01(travelPercent);

        wheel.transform.localPosition = Vector3.down * (currentTravelPercent * maxTravel - wheelRadius);
        
        if(isSteerable)
            wheel.transform.localRotation = Quaternion.Euler(0, currentVisualSteeringAngle, 0);
    }
}
