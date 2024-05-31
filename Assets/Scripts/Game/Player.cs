using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player
{
    public IInputDevice inputDevice { get; private set; }
    public string Name => inputDevice.Name;

    public bool inLobby;

    public bool isActive { get => CarInstance; }
    public bool isReady;
    public bool isInRace;
    public int carIndex { get; set; }
    public CarManager CarInstance { get; private set; }

    public Player(IInputDevice inputDevice, int carIndex)
    {
        this.inputDevice = inputDevice;
        this.carIndex = carIndex;
    }

    public void SetInstance(CarManager carInstance)
    {
        this.CarInstance = carInstance;
        carInstance?.SetPlayer(this);
    }
}
