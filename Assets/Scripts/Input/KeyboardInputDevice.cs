using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class KeyboardInputDevice : IInputDevice
{
    public string Name => name;

    private string name;

    private KeyMap keyMap;
    private Keyboard keyboard => Keyboard.current;
    public KeyboardInputDevice(KeyMap keyMap, string name)
    {
        this.keyMap = keyMap;
        this.name = name;
    }

    public bool GetAction(InputAction action) => keyboard[keyMap.ActionKeys[action]].isPressed;
    public bool GetActionDown(InputAction action) => keyboard[keyMap.ActionKeys[action]].wasPressedThisFrame;
    public bool GetActionUp(InputAction action) => keyboard[keyMap.ActionKeys[action]].wasReleasedThisFrame;

    public Vector2 GetJoystickInput()
    {
        float vertical = 0, horizontal = 0;

        if (keyboard[keyMap.VerticalNeg].isPressed)
            vertical = -1;
        else if (keyboard[keyMap.VerticalPos].isPressed)
            vertical = 1;
        
        if(keyboard[keyMap.HorizontalPos].isPressed)
        {
            horizontal++;
        }
        else if(keyboard[keyMap.HorizontalNeg].isPressed)
        {
            horizontal--;
        }

        return new Vector2(horizontal, vertical);
    }

    public void LateUpdate()
    {

    }
}

public struct KeyMap
{
    public Key HorizontalPos;
    public Key HorizontalNeg;
    public Key VerticalPos;
    public Key VerticalNeg;
    public Dictionary<InputAction, Key> ActionKeys;
}
