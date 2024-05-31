using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    List<Player> players { get; set; } = new List<Player>();

    public List<GameObject> carPrefabs = new List<GameObject>();

    public IReadOnlyList<Player> Players { get => players; }

    List<IInputDevice> inactiveInputDevices { get; set; } = new List<IInputDevice>();
    public IReadOnlyList<IInputDevice> InactiveInputDevices { get => inactiveInputDevices; }

    public Action<Player> OnPlayerDisconnected; 

    // Start is called before the first frame update
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        DontDestroyOnLoad(gameObject);
    }

    public void OnInputDeviceListChanged(List<IInputDevice> devices)
    {
        //Remove old Players
        var playersToDelete = players.Where(x => !devices.Any(device => device.Name == x.Name));

        foreach(var player in playersToDelete)
        {
            OnPlayerDisconnected?.Invoke(player);
            RemovePlayer(player);
        }

        var inactiveDevicesToDelete = inactiveInputDevices.Where(x => !devices.Contains(x));

        foreach(var device in inactiveDevicesToDelete)
        {
            inactiveInputDevices.Remove(device);
        }

        //Add new Inactive Devices

        var toAdd = devices.Where(x => !players.Any(player => player.Name == x.Name) && !inactiveInputDevices.Contains(x));

        foreach (var device in toAdd)
        {
            inactiveInputDevices.Add(device);
        }
    }

    public Player ActivateInputDevice(IInputDevice inputDevice)
    {
        if(!inactiveInputDevices.Contains(inputDevice))
        {
            Debug.LogError($"InputDevice '{inputDevice.Name}' is already active!");
            return null;
        }

        var newPlayer = new Player(inputDevice, GetNextAvailableCarIndex());

        players.Add(newPlayer);
        inactiveInputDevices.Remove(inputDevice);
        Debug.Log($"Added Player '{inputDevice.Name}'");
        return newPlayer;
    }
    void RemovePlayer(Player player)
    {
        if (players.Contains(player))
        {
            players.Remove(player);
            Debug.Log($"Removed Player '{player.Name}'");
        }
        else
        {
            Debug.LogError($"Player '{player.Name}' doesn't exist, so they can't be removed!");
        }
    }

    public int GetNextAvailableCarIndex(int startIndex = -1)
    {
        if(startIndex == -1)
            startIndex = UnityEngine.Random.Range(0, carPrefabs.Count);
        for(int i = 0; i<carPrefabs.Count; i++)
        {
            int index = (i + startIndex) % carPrefabs.Count;
            if (players.Any(x => x.carIndex == index))
                continue;
            else
                return index;
        }
        return startIndex;
    }
}
