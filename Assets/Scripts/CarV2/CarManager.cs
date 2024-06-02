using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[SelectionBase]
public class CarManager : MonoBehaviour
{
    public List<Suspension> Suspensions = new List<Suspension>();

    public Vector3 centerOfMass;

    IInputDevice inputDevice;

    Rigidbody rb;
    CarControlMode mode;
    public Vector3 velocity { get => rb?.velocity ?? Vector3.zero; }

    public CarSetupData setupData;

    private PathTracker pathTracker;

    private Player player;
    public Suspension frontLeft { get => Suspensions[0]; }
    public Suspension frontRight { get => Suspensions[1]; }
    public Suspension rearLeft { get => Suspensions[2]; }
    public Suspension rearRight { get => Suspensions[3]; }
    [SerializeField]
    private float reverseSpeedThreshold = 0.5f;
    [SerializeField]
    private float forwardsSpeedThreshold = 2f; //At this forwards speed, reverse gets switched off automatically

    private float lastYInput = 0;

    public bool inReverseGear { get; private set; }
    public bool onGround { get; private set; }
    public Vector2 groundVelocity { get; set; }

    public float distance { get => pathTracker?.currentDistance ?? -1; }
    public int lapCount { get => pathTracker?.lapCount ?? -1; }

    // Start is called before the first frame update
    void Awake()
    {
        rb = gameObject.GetComponent<Rigidbody>();
        pathTracker = gameObject.GetComponent<PathTracker>();
    }

    private void Start()
    {
        rb.centerOfMass = centerOfMass;

        foreach(var suspension in Suspensions)
        {
            suspension.SetSetupData(setupData);
        }
    }

    private void Update()
    {
        if (mode == CarControlMode.Static)
            return;

        Vector2 joystickInput = mode == CarControlMode.Enabled ? inputDevice.GetJoystickInput() : Vector2.zero;

        if (!inReverseGear && joystickInput.y < 0 && (groundVelocity.magnitude < reverseSpeedThreshold || groundVelocity.y < 0))
            inReverseGear = true;

        lastYInput = joystickInput.y;
    }

    void FixedUpdate()
    {
        if (mode == CarControlMode.Static)
            return;

        Vector2 joystickInput = mode == CarControlMode.Enabled ? inputDevice.GetJoystickInput() : Vector2.zero;


        //Gear
        if (inReverseGear && (joystickInput.y > 0 || groundVelocity.y > forwardsSpeedThreshold))
            inReverseGear = false;

        onGround = false;
        foreach (var suspension in Suspensions)
        {
            if (inReverseGear)
            {
                suspension.brakePercent = Mathf.Max(0, joystickInput.y);
                suspension.wheelPower = -Mathf.Min(0, joystickInput.y);
            }
            else
            {
                suspension.brakePercent = -Mathf.Min(0, joystickInput.y);
                suspension.wheelPower = Mathf.Max(0, joystickInput.y);
            }

            suspension.inReverseGear = inReverseGear;

            suspension.steeringPercent = joystickInput.x;

            if (suspension.onGround)
                onGround = true;
        }
        CalculateGroundVelocity();
    }

    public void SetControlMode(CarControlMode mode)
    {
        this.mode = mode;

        rb.isKinematic = (mode == CarControlMode.Static);
    }
    public void SetPlayer(Player player)
    {
        this.player = player;
        this.inputDevice = player.inputDevice;
    }

    private void CalculateGroundVelocity()
    {
        Vector3 forwardDir = (frontRight.transform.position + frontLeft.transform.position) - (rearRight.transform.position + rearLeft.transform.position);
        Vector3 rightDir = (frontRight.transform.position + rearRight.transform.position) - (frontLeft.transform.position + rearLeft.transform.position);

        var wheelNormal = Vector3.Cross(forwardDir, rightDir).normalized;

        Vector3 forward = Vector3.ProjectOnPlane(transform.forward, wheelNormal).normalized;
        Vector3 right = Vector3.ProjectOnPlane(transform.right, wheelNormal).normalized;

        groundVelocity = new Vector2(Vector3.Dot(rb.velocity, right), Vector3.Dot(rb.velocity, forward));
    }
}
