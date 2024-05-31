using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IInputDevice
{
    public string Name { get; }
    public Vector2 GetJoystickInput();
    public bool GetAction(InputAction action);
    public bool GetActionDown(InputAction action);
    public bool GetActionUp(InputAction action);
    public void LateUpdate();
}
public enum InputAction
{
    Primary,
    Secondary
}