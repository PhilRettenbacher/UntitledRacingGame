using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartPositionRegistration : MonoBehaviour
{
    public List<Transform> startingPositions;

    // Start is called before the first frame update
    void Start()
    {
        GameObject.FindGameObjectWithTag("RaceManager")?.GetComponent<RaceManager>()?.RegisterStartingPositions(startingPositions);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
