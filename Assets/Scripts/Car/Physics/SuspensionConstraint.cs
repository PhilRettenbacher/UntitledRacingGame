using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SuspensionConstraint : MonoBehaviour
{
    public WheelSuspension suspensionA;
    public WheelSuspension suspensionB;

    public float antirollbarStrength = 1;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        float currDifference = suspensionA.currentTravel - suspensionB.currentTravel;
        suspensionA.AddForce(-currDifference / (suspensionB.onGround ? 2 : 1) * antirollbarStrength);
        suspensionB.AddForce(currDifference / (suspensionA.onGround ? 2 : 1) * antirollbarStrength);
    }
}
