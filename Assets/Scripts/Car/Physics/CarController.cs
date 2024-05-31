using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public class CarController : MonoBehaviour
{
    Rigidbody rb;
    CarBase carBase;

    [SerializeField]
    public Vector3 centerOfMass;
    public DriveMode driveMode;

    bool isSetup;

    Quaternion wheelOrientation;
    Vector3 wheelNormal;
    Vector2 groundVelocity;

    bool inReverseGear = false;
    float lastYInput = 0;
    float currentSteeringAngle;

    [SerializeField]
    private float maxAcceleration = 30;
    [SerializeField]
    private float maxVelocity = 100;
    [SerializeField]
    private float brakePower = 35;
    [SerializeField]
    private float driftAcceleration = 100;
    [SerializeField]
    private float verticalInputFactor = 1.2f;
    [SerializeField]
    private float dragFactor = 0.1f;
    [SerializeField]
    private float baseDragPower = 2;
    [SerializeField]
    private float reverseSpeedThreshold = 0.5f;
    [SerializeField]
    private float wheelSidewaysDrag = 0.4f;
    [SerializeField]
    private float wheelSteeringAngle = 35;
    [SerializeField]
    private float minimumSteeringAngle = 5;
    [SerializeField]
    private float maxWheelForce = 100;

    WheelSuspension fr { get => carBase?.frontRight; }
    WheelSuspension fl { get => carBase?.frontLeft; }
    WheelSuspension rr { get => carBase?.rearRight; }
    WheelSuspension rl { get => carBase?.rearLeft; }

    public void SetMode(CarControlMode mode)
    {
        if (mode == CarControlMode.Static)
        {
            rb.isKinematic = true;
        }
        else
        {
            rb.isKinematic = false;
        }
    }

    private void Awake()
    {
        rb = gameObject.GetComponent<Rigidbody>();
        carBase = gameObject.GetComponent<CarBase>();
    }

    // Start is called before the first frame update
    void Start()
    {
        rb.centerOfMass = centerOfMass;

    }

    private void Update()
    {
        if (carBase.mode != CarControlMode.Enabled || carBase.inputDevice == null)
            return;

        float yInput = carBase.inputDevice.GetJoystickInput().y;

        if (OnGround())
        {
            if (!inReverseGear && yInput < 0 && lastYInput >= 0 && (groundVelocity.magnitude < reverseSpeedThreshold || groundVelocity.y < 0))
            {
                inReverseGear = true;
                Debug.Log("In Rerverse Gear");
            }
        }
        lastYInput = yInput;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (carBase.mode != CarControlMode.Enabled || carBase.inputDevice == null)
            return;

        Vector2 input = carBase.inputDevice.GetJoystickInput();

        CalculateWheelOrientation();
        CalculateGroundVelocity();

        //Debug.DrawLine(transform.position, transform.position + wheelOrientation * Vector3.up * 5, Color.green);
        //Debug.DrawLine(transform.position, transform.position + wheelOrientation * Vector3.forward * 5, Color.blue);
        //Debug.DrawLine(transform.position, transform.position + wheelOrientation * Vector3.right * 5, Color.red);


        if (OnGround())
        {
            Vector2 acceleration = new Vector2();
            float driftValue = Vector2.Dot(Vector2.right, groundVelocity.normalized);

            float currBrakingPower = 0;

            bool inReverse = groundVelocity.y < reverseSpeedThreshold;

            if (!inReverse)
                currentSteeringAngle = GetSteeringAngle(input.x);
            else
                currentSteeringAngle = input.x * wheelSteeringAngle;
            fr.currentSteeringAngle = fl.currentSteeringAngle = currentSteeringAngle;

            if (input.y > 0 && inReverseGear)
            {
                Debug.Log("In Gear");
                inReverseGear = false;
            }
            if ((input.y > 0 && (!inReverse || Mathf.Abs(groundVelocity.y) < reverseSpeedThreshold)) || (input.y < 0 && inReverse && inReverseGear)) //accelerate
            {
                acceleration += Vector2.up * input.y * (maxAcceleration * (1 - Mathf.Clamp01(rb.velocity.magnitude / maxVelocity)));
            }
            else if ((input.y > 0) || (input.y < 0)) //brake
            {
                currBrakingPower = Mathf.Abs(input.y);
            }

            //Drift
            //acceleration += Vector2.Perpendicular(groundVelocity.normalized) * driftValue * driftAcceleration / (1 + (verticalInputFactor - 1) * Mathf.Abs(input.y));

            float drag = baseDragPower + groundVelocity.magnitude * dragFactor;
            //float deacceleration = drag + currBrakingPower;



            var dragVec = -groundVelocity.normalized * drag;

            rb.AddForce(wheelOrientation * new Vector3(dragVec.x, 0, dragVec.y) * rb.mass);

            //CalculateWheelForce(fl.transform.position, input.x * Mathf.Deg2Rad * wheelSteeringAngle, 1);
            //CalculateWheelForce(fr.transform.position, input.x * Mathf.Deg2Rad * wheelSteeringAngle, 1);
            //CalculateWheelForce(rl.transform.position, 0, Mathf.Lerp(1, verticalInputFactor, Mathf.Abs(input.y)));
            //CalculateWheelForce(rr.transform.position, 0, Mathf.Lerp(1, verticalInputFactor, Mathf.Abs(input.y)));

            float individualAcceleration = acceleration.y / (driveMode == DriveMode.AllWheelDrive ? 4 : 2);

            CalculateWheelForce(fl, driveMode == DriveMode.RearWheelDrive ? 0 : individualAcceleration, currBrakingPower);
            CalculateWheelForce(fr, driveMode == DriveMode.RearWheelDrive ? 0 : individualAcceleration, currBrakingPower);
            CalculateWheelForce(rl, driveMode == DriveMode.FrontWheelDrive ? 0 : individualAcceleration, currBrakingPower);
            CalculateWheelForce(rr, driveMode == DriveMode.FrontWheelDrive ? 0 : individualAcceleration, currBrakingPower);

            //Debug.DrawLine(transform.position, transform.position + transform.TransformDirection(new Vector3(groundVelocity.x, 0, groundVelocity.y)));
            //Debug.DrawLine(transform.position, transform.position + transform.TransformDirection(new Vector3(acceleration.x, 0, acceleration.y)), Color.yellow);
        }
        else
        {
            currentSteeringAngle = input.x * wheelSteeringAngle;
            fr.currentSteeringAngle = fl.currentSteeringAngle = currentSteeringAngle;
        }
    }

    float GetSteeringAngle(float inputX)
    {
        if (inputX > 0)
        {
            return Mathf.Lerp(0, Mathf.Clamp(fr.neutralSlipAngle + fr.idealSlipAngleDeg + 5, minimumSteeringAngle, wheelSteeringAngle), inputX);
        }
        return Mathf.Lerp(0, Mathf.Clamp(fl.neutralSlipAngle - fl.idealSlipAngleDeg - 5, -wheelSteeringAngle, -minimumSteeringAngle), -inputX);

    }

    Vector3 GetCentralWheelPos()
    {
        Vector3 ret = Vector3.zero;
        for (int i = 0; i < carBase.Wheels.Count; i++)
        {
            ret += carBase.Wheels[i].transform.position;
        }
        ret /= carBase.Wheels.Count;
        return ret;
    }

    [Obsolete]
    void CalculateWheelForce(Vector3 wheelPos, float angle, float gripFactor)
    {
        var velocity = Vector3.ProjectOnPlane(rb.GetPointVelocity(wheelPos), wheelNormal);

        var rightVector = wheelOrientation * new Vector3(Mathf.Cos(angle), 0, -Mathf.Sin(angle));

        var rightVelocity = Vector3.Project(velocity, rightVector);

        var drag = -rightVelocity * wheelSidewaysDrag * gripFactor;

        rb.AddForceAtPosition(drag, wheelPos);

        Debug.DrawLine(wheelPos, wheelPos + rightVelocity, Color.red);
    }

    void CalculateWheelForce(WheelSuspension suspension, float accelerationForce, float brakeForce)
    {
        suspension.wheelPower = accelerationForce;
        suspension.brakePower = brakeForce;

        //var velocity = Vector3.ProjectOnPlane(rb.GetPointVelocity(suspension.transform.position), wheelNormal);

        //var rightVelocity = Vector3.Project(velocity, suspension.transform.rotation * Vector3.right);

        //Debug.DrawLine(suspension.transform.position, suspension.transform.position + rightVelocity, Color.blue);

        //Vector3 sumOfForces =
        //    -rightVelocity +
        //    suspension.transform.rotation * Vector3.forward * accelerationForce +
        //    -velocity * brakeForce;

        //var lineColor = Color.green;
        //if (sumOfForces.magnitude > maxWheelForce * suspension.currentForce)
        //{
        //    sumOfForces = sumOfForces.normalized * maxWheelForce * suspension.currentForce;
        //    lineColor = Color.red;

        //    suspension.isSlipping = true;
        //}
        //else
        //{
        //    suspension.isSlipping = false;
        //}
        //rb.AddForceAtPosition(sumOfForces, suspension.transform.position);
        //Debug.DrawLine(suspension.transform.position, suspension.transform.position + sumOfForces, lineColor);
    }

    void CalculateWheelOrientation()
    {
        Vector3 forwardDir = (fr.transform.position + fl.transform.position) - (rr.transform.position + rl.transform.position);
        Vector3 rightDir = (fr.transform.position + rr.transform.position) - (fl.transform.position + rr.transform.position);

        wheelNormal = Vector3.Cross(forwardDir, rightDir).normalized;

        wheelOrientation = Quaternion.LookRotation(forwardDir, wheelNormal);
    }

    void CalculateGroundVelocity()
    {
        Vector3 forward = Vector3.ProjectOnPlane(transform.forward, wheelNormal).normalized;
        Vector3 right = Vector3.ProjectOnPlane(transform.right, wheelNormal).normalized;

        groundVelocity = new Vector2(Vector3.Dot(rb.velocity, right), Vector3.Dot(rb.velocity, forward));
    }

    bool OnGround()
    {
        return carBase.Wheels.Any(x => x.onGround);
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(transform.TransformPoint(centerOfMass), Vector3.one / 4);
    }
}
