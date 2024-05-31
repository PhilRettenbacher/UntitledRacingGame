using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SetupData", menuName = "CarV2/CarSetupData", order = 1)]
public class CarSetupData : ScriptableObject
{
    public float sidewaysWheelForceAtMaximumAngle = 2f;
    public float lowSpeedThreshold = 10f;
    public float driftForceMaxVelocity = 20;
    public float driftForceFactor = 0.1f;
    public float maxSpeed = 40;
    public float sidewaysForceMaxThrottlePenalty = 0.5f;
    public float sidewaysForceMaxBrakePenalty = 0.3f;
    public float wheelPowerFactor = 3;
    public float brakeFactor = 3;
    public float idealSidewaysForceAngle = 90; //Degrees
    public float steeredWheelSidewaysForceFactor = 1.2f;
    public float maxSteeringAngle = 45;
}
