using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[SelectionBase]
public class CarBase : MonoBehaviour
{
    Player player;
    CarController controller;

    public WheelSuspension frontRight, frontLeft, rearRight, rearLeft;

    private List<WheelSuspension> _wheels;
    public List<WheelSuspension> Wheels
    {
        get
        {
            if (_wheels == null)
                _wheels = (new[] { frontRight, frontLeft, rearLeft, rearRight }).ToList();
            return _wheels;
        }
    }

    public Vector3 wheelBaseFront;
    public Vector3 wheelBaseRear;

    public CarControlMode mode { get; private set; }
    public IInputDevice inputDevice { get => player?.inputDevice; }


    // Start is called before the first frame update
    void Awake()
    {
        controller = gameObject.GetComponent<CarController>();
    }
    public void SetControlMode(CarControlMode mode)
    {
        this.mode = mode;
        controller.SetMode(mode);
    }
    public void SetPlayer(Player player)
    {
        this.player = player;
    }
}
