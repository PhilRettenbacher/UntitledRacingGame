using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LobbyManager : MonoBehaviour
{
    GameManager gameManager;

    [SerializeField]
    private bool isTestLobby = false;

    // Start is called before the first frame update
    void Awake()
    {
        gameManager = GameObject.FindGameObjectWithTag("GameController")?.GetComponent<GameManager>();
        for (int i = 0; i < gameManager.Players.Count; i++)
        {
            gameManager.Players[i].isReady = false;
            gameManager.Players[i].inLobby = true;
            gameManager.Players[i].isInRace = false;
        }
    }

    private void OnEnable()
    {
        gameManager.OnPlayerDisconnected += OnPlayerDisconnected;
    }

    private void OnDisable()
    {
        gameManager.OnPlayerDisconnected -= OnPlayerDisconnected;
    }

    // Update is called once per frame
    void Update()
    {
        foreach (var player in gameManager.Players)
            PlayerUpdate(player);

        CheckForNewPlayers();
        CheckForInactivePlayers();

        if (!isTestLobby)
            CheckForGameStart();
    }

    void PlayerUpdate(Player player)
    {
        if (player.inputDevice.GetActionDown(InputAction.Secondary))
        {
            player.isReady = !player.isReady;
        }
        if(player.inputDevice.GetActionDown(InputAction.Primary))
        {
            //Switch Cars

            var oldInstance = player.CarInstance;

            player.carIndex = gameManager.GetNextAvailableCarIndex(player.carIndex + 1);
            InstantiatePlayerInstance(player, oldInstance.transform.position, oldInstance.transform.rotation, oldInstance.GetComponent<Rigidbody>().velocity);

            Destroy(oldInstance.gameObject);
        }
    }

    void CheckForNewPlayers()
    {
        List<IInputDevice> toAdd = gameManager.InactiveInputDevices.Where(x => x.GetActionDown(InputAction.Primary)).ToList();

        foreach (var device in toAdd)
        {
            Debug.Log(device.Name);
            gameManager.ActivateInputDevice(device);
        }
    }

    void CheckForInactivePlayers()
    {
        for (int i = 0; i < gameManager.Players.Count; i++)
        {
            var player = gameManager.Players[i];
            if (player.isActive)
                continue;

            //Activate Player
            //var carInstance = GameObject.Instantiate(gameManager.carPrefabs[player.carIndex], GetStartingPosition(i), Quaternion.identity).GetComponent<CarManager>();
            //player.SetInstance(carInstance);
            //player.inLobby = true;
            //carInstance.SetControlMode(CarControlMode.Enabled);
            InstantiatePlayerInstance(player, GetStartingPosition(i), Quaternion.identity, Vector3.zero);
        }
    }

    void InstantiatePlayerInstance(Player player, Vector3 position, Quaternion orientation, Vector3 velocity)
    {
        var carInstance = GameObject.Instantiate(gameManager.carPrefabs[player.carIndex], position, orientation).GetComponent<CarManager>();
        player.SetInstance(carInstance);
        player.inLobby = true;
        carInstance.SetControlMode(CarControlMode.Enabled);
        carInstance.GetComponent<Rigidbody>().velocity = velocity;
    }

    void CheckForGameStart()
    {
        if (gameManager.Players.Count == 0)
            return;

        if (gameManager.Players.All(x => x.isReady))
        {
            SceneManager.LoadScene("GameScene");
        }

    }

    Vector3 GetStartingPosition(int index)
    {
        float x = (index % 2 - 0.5f) * WorldConstants.TileSize * 2;
        int row = Mathf.FloorToInt(index / 2f);

        float y = row * WorldConstants.TileSize * 3 + (index % 2) * WorldConstants.TileSize;

        return new Vector3(x, 5, y);
    }

    private void OnPlayerDisconnected(Player player)
    {
        Destroy(player.CarInstance.gameObject);
    }
}
