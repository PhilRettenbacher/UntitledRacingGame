using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Track))]
public class RaceManager : MonoBehaviour
{
    GameManager gameManager;
    Track track;

    CameraFollow cameraFollow;

    bool setupComplete;

    List<StartingLightsDisplay> startingLightsDisplays = new List<StartingLightsDisplay>();
    List<Transform> startingPositions = new List<Transform>();

    public float yThreshold = -5;
    public float minimalCameraDistanceFromLeader = 15f;
    public float eliminationDistance = 50; //Distance to the leader to get disqualified
    public float cameraDistanceFromLeaderOnStart = -10f;


    float minimalCameraDistance = 0; //Gets set in starting process

    RaceStatus status;

    // Start is called before the first frame update
    void Awake()
    {
        gameManager = GameObject.FindGameObjectWithTag("GameController")?.GetComponent<GameManager>();
        track = gameObject.GetComponent<Track>();
    }

    // Start is called before the first frame update
    async void Start()
    {
        status = RaceStatus.WaitingForStart;

        await track.Generate();

        cameraFollow = Camera.main.gameObject.GetComponent<CameraFollow>();

        cameraFollow.enabled = true;

        InstantiateCars();

        setupComplete = true;

        StartCountdown();
    }

    private void Update()
    {
        if (!setupComplete)
            return;

        //Keep track of cars
        //Wait for Winning condition

        if (status == RaceStatus.Running)
            UpdateRace();
        else
        {
            float leaderDistance = GetLeaderDistance();

            minimalCameraDistance = leaderDistance + cameraDistanceFromLeaderOnStart;

            cameraFollow.SetTargetDistance(minimalCameraDistance);
        }
    }

    public void UpdateRace()
    {
        float leaderDistance = GetLeaderDistance();

        float averageDistance = GetAverageDistance();

        cameraFollow.SetTargetDistance(Mathf.Max(minimalCameraDistance, Mathf.Max(leaderDistance - minimalCameraDistanceFromLeader, averageDistance)));

        foreach(var player in gameManager.Players)
        {
            if(player.CarInstance.lapCount >= 3)
            {
                FinishGame();
            }

            if(player.CarInstance.distance < leaderDistance - eliminationDistance)
            {
                DisqualifyPlayer(player);
            }

            if(player.CarInstance.transform.position.y < yThreshold)
            {
                DisqualifyPlayer(player);
            }
        }


    }

    public void DisqualifyPlayer(Player player)
    {
        player.isInRace = false;
        player.CarInstance.SetControlMode(CarControlMode.Disabled);

        if(gameManager.Players.Count(x => x.isInRace) <= 1)
        {
            //Game finished
            FinishGame();
        }
    }

    public void FinishGame()
    {
        status = RaceStatus.Finished;

        StartCoroutine(LoadLobby());
    }

    public void RegisterStartingLights(StartingLightsDisplay display)
    {
        Debug.Log("Starting Lights registered");

        startingLightsDisplays.Add(display);
    }

    public void RegisterStartingPositions(List<Transform> startingPositions)
    {
        Debug.Log("Starting Position registered");

        this.startingPositions = startingPositions;
    }

    float GetLeaderDistance()
    {
        if(status == RaceStatus.Running)
            return gameManager.Players.Where(x => x.isInRace).Max(x => x.CarInstance.distance);
        else
            return gameManager.Players.Max(x => x.CarInstance.distance);
    }

    float GetAverageDistance()
    {
        return gameManager.Players.Where(x => x.isInRace).Average(x => x.CarInstance.distance);
    }

    void InstantiateCars()
    {
        for(int i = 0; i<gameManager.Players.Count; i++)
        {
            var player = gameManager.Players[i];

            var carInstance = GameObject.Instantiate(gameManager.carPrefabs[player.carIndex], startingPositions[i].position, startingPositions[i].rotation).GetComponent<CarManager>();
            carInstance.SetControlMode(CarControlMode.Static);
            player.SetInstance(carInstance);
        }
    }

    void StartCountdown()
    {
        StartCoroutine(Countdown());
    }

    IEnumerator Countdown()
    {
        SetStartingLightsDisplayValue(0);
        yield return new WaitForSeconds(2);
        SetStartingLightsDisplayValue(1);
        yield return new WaitForSeconds(1f);
        SetStartingLightsDisplayValue(2);
        yield return new WaitForSeconds(1f);
        SetStartingLightsDisplayValue(3);
        yield return new WaitForSeconds(1f);
        SetStartingLightsDisplayValue(4);
        yield return new WaitForSeconds(Random.Range(2, 3));
        Debug.Log("GO!");
        SetStartingLightsDisplayValue(0);
        StartRace();
    }

    IEnumerator LoadLobby()
    {
        yield return new WaitForSeconds(5);
        SceneManager.LoadScene("Lobby");
    }

    void SetStartingLightsDisplayValue(int value)
    {
        foreach(var display in startingLightsDisplays)
        {
            display.SetStartingLights(value);
        }
    }

    void StartRace()
    {
        foreach(var player in gameManager.Players)
        {
            player.isInRace = true;
            player.CarInstance.SetControlMode(CarControlMode.Enabled);
        }

        status = RaceStatus.Running;
    }
}
enum RaceStatus
{
    WaitingForStart,
    Running,
    Finished
}