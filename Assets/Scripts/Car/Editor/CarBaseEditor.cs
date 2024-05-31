using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CarBase))]
public class CarBaseEditor : Editor
{
    public override void OnInspectorGUI()
    {
        CarBase car = target as CarBase;
     
        base.OnInspectorGUI();

        if(GUILayout.Button("Apply Wheelbase"))
        {
            Undo.RecordObjects(car.Wheels.ToArray(), "Set Wheelbase");
            car.frontRight.transform.position = new Vector3(car.wheelBaseFront.x / 2, car.wheelBaseFront.y, car.wheelBaseFront.z / 2);
            car.frontLeft.transform.position = new Vector3(car.wheelBaseFront.x / -2, car.wheelBaseFront.y, car.wheelBaseFront.z / 2);

            car.rearRight.transform.position = new Vector3(car.wheelBaseRear.x / 2, car.wheelBaseRear.y, car.wheelBaseRear.z / 2);
            car.rearLeft.transform.position = new Vector3(car.wheelBaseRear.x / -2, car.wheelBaseRear.y, car.wheelBaseRear.z / 2);

        }
    }

    private void OnSceneGUI()
    {
        CarBase car = target as CarBase;

        foreach(var suspension in car.Wheels)
        {
            if (!suspension)
                continue;
            DrawSuspensionSceneGUI(suspension);
            DrawSuspensionWheelForce(suspension, car);
        }

        Handles.DrawPolyLine(car.Wheels.Select(x => x.transform.position).ToArray());
        Handles.DrawLine(car.Wheels[0].transform.position, car.Wheels[car.Wheels.Count - 1].transform.position);
    }

    void DrawSuspensionSceneGUI(WheelSuspension suspension)
    {
        Handles.DrawWireArc(suspension.transform.position, suspension.transform.right, suspension.transform.forward, 360, suspension.radius); 

        var color = Handles.color;
        
        Handles.color = Color.green;
        Handles.DrawLine(suspension.transform.position, suspension.transform.position + Vector3.up * suspension.travel / 2);
        Handles.color = Color.red;
        Handles.DrawLine(suspension.transform.position, suspension.transform.position - Vector3.up * suspension.travel / 2);
        Handles.color = color;
    }
    void DrawSuspensionWheelForce(WheelSuspension suspension, CarBase car)
    {
        Vector3 circleCenter = suspension.transform.position + car.transform.right * 0.5f * Mathf.Sign(suspension.transform.localPosition.x);

        Handles.DrawWireArc(circleCenter, car.transform.up, car.transform.forward, 360, suspension.radius);

        var color = Handles.color;

        if (suspension.currentWheelForce.magnitude > suspension.currentMaxWheelForce)
            Handles.color = Color.red;
        else
            Handles.color = Color.green;

        Handles.DrawLine(circleCenter, circleCenter + (suspension.currentWheelForce / suspension.currentMaxWheelForce) * suspension.radius);

        Handles.color = color;
    }
}
