using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CarManager))]
public class CarManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        CarManager car = target as CarManager;

        base.OnInspectorGUI();

    }

    private void OnSceneGUI()
    {
        CarManager car = target as CarManager;

        Vector3 com = car.transform.TransformPoint(car.centerOfMass);

        Handles.CubeHandleCap(-1, com, Quaternion.identity, 0.1f, Event.current.type);

        Handles.DrawLine(car.transform.position, car.transform.position + car.velocity.normalized * 2);

        foreach (var suspension in car.Suspensions)
        {
            if (!suspension)
                continue;
            DrawSuspensionSceneGUI(suspension);
        }
    }

    void DrawSuspensionSceneGUI(Suspension suspension)
    {
        //Handles.DrawWireArc(suspension.transform.position, suspension.transform.right, suspension.transform.forward, 360, suspension.radius);

        var color = Handles.color;

        //Draw max Suspension Travel:

        Vector3 suspensionStartingPoint = suspension.transform.position + suspension.transform.up * suspension.wheelRadius;
        Vector3 suspensionDirection = suspension.transform.up;
        Vector3 currentSuspensionPosition = suspensionStartingPoint - suspensionDirection * suspension.currentTravelDistance;

        Handles.DrawLine(suspensionStartingPoint, suspension.transform.position - suspensionDirection * (suspension.maxTravel - suspension.wheelRadius));

        Handles.color = Color.green;
        Handles.DrawWireDisc(suspensionStartingPoint, suspensionDirection, 0.1f);
        Handles.DrawWireDisc(currentSuspensionPosition, suspensionDirection, 0.1f);
        Handles.DrawLine(suspensionStartingPoint, currentSuspensionPosition);
        //Handles.color = Color.red;
        //Handles.DrawLine(suspension.transform.position, suspension.transform.position - Vector3.up * suspension.travel / 2);

        //Suspension Force
        float xPositionSign = Mathf.Sign(suspension.transform.localPosition.x);

        Handles.color = Color.white;

        Vector3 forceCircleCenter = suspension.transform.position + suspension.transform.right * xPositionSign * 0.5f;

        Handles.DrawWireDisc(forceCircleCenter, suspensionDirection, 0.5f);
        Handles.color = Color.green;
        Handles.DrawLine(forceCircleCenter, forceCircleCenter + suspension.wheelForce * 0.25f);
        Handles.color = Color.yellow;
        Handles.DrawLine(forceCircleCenter, forceCircleCenter + suspension.relativeVelocity.normalized * 0.5f);
        Handles.color = color;
    }
}
