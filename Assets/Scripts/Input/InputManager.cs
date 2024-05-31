using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Linq;

public class InputManager : MonoBehaviour
{
    private List<IInputDevice> InputDevices { get; set; } = new List<IInputDevice>();
    GameManager gm;

    // Start is called before the first frame update
    void Start()
    {
        gm = GameManager.Instance;

        AddDefaultKeyboardInputs();
    }

    // Update is called once per frame
    void Update()
    {
        if(CheckForGamePadChange())
        {
            gm.OnInputDeviceListChanged(InputDevices);
        }
    }
    private void LateUpdate()
    {
        foreach (var device in InputDevices)
        {
            device.LateUpdate();
        }
    }

    bool CheckForGamePadChange()
    {
        var currentGamepads = InputDevices.Where(x => x.GetType() == typeof(ControllerInputDevice));
        if (Gamepad.all.Count != currentGamepads.Count())
        {
            //Remove old gamepads
            currentGamepads.Where(x => !Gamepad.all.Any(pad => x.Name == pad.name)).ToList().ForEach(x => Remove(x));

            //Add new gamepads
            Gamepad.all.Where(x => !InputDevices.Any(device => x.name == device.Name)).ToList().ForEach(x => Add(new ControllerInputDevice(x)));

            return true;
        }
        return false;
    }

    void Add(IInputDevice device)
    {
        InputDevices.Add(device);
        Debug.Log($"Device '{device.Name}' added!");
    }

    void Remove(IInputDevice device)
    {
        InputDevices.Remove(device);
        Debug.Log($"Device '{device.Name}' removed!");
    }

    void AddDefaultKeyboardInputs()
    {
        Dictionary<InputAction, Key> actions1 = new Dictionary<InputAction, Key>();
        Dictionary<InputAction, Key> actions2 = new Dictionary<InputAction, Key>();

        actions1.Add(InputAction.Primary, Key.Digit0);
        actions1.Add(InputAction.Secondary, Key.Digit9);

        actions2.Add(InputAction.Primary, Key.E);
        actions2.Add(InputAction.Secondary, Key.Q);

        KeyMap map1 = new KeyMap()
        {
            ActionKeys = actions1,
            HorizontalPos = Key.RightArrow,
            HorizontalNeg = Key.LeftArrow,
            VerticalPos = Key.UpArrow,
            VerticalNeg = Key.DownArrow
        };

        KeyMap map2 = new KeyMap()
        {
            ActionKeys = actions2,
            HorizontalPos = Key.D,
            HorizontalNeg = Key.A,
            VerticalPos = Key.W,
            VerticalNeg = Key.S
        };

        Add(new KeyboardInputDevice(map1, "Keyboard_0"));
        Add(new KeyboardInputDevice(map2, "Keyboard_1"));

        gm.OnInputDeviceListChanged(InputDevices);
    }
}
