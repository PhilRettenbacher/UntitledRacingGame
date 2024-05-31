using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

public class ControllerInputDevice : IInputDevice
{
    Gamepad gamepad;

    public string Name => name;

    private string name;

    public ControllerInputDevice(Gamepad gamepad)
    {
        this.gamepad = gamepad;
        name = gamepad.name;
    }

    public bool GetAction(InputAction action) => GetByAction(action).isPressed;
    public bool GetActionDown(InputAction action) => GetByAction(action).wasPressedThisFrame;
    public bool GetActionUp(InputAction action) => GetByAction(action).wasReleasedThisFrame;

    public Vector2 GetJoystickInput()
    {
        float yValue = -gamepad.leftTrigger.ReadValue();
        if (yValue >= 0)
            yValue = gamepad.rightTrigger.ReadValue();

        return new Vector2(gamepad.leftStick.ReadValue().x, yValue);
    }
    public void LateUpdate()
    {

    }
    private ButtonControl GetByAction(InputAction action)
    {
        switch (action)
        {
            case InputAction.Primary:
                return gamepad.rightShoulder;
            case InputAction.Secondary:
                return gamepad.leftShoulder;
        }
        return null;
    }
}
