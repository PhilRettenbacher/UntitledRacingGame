using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartingLightsDisplay : MonoBehaviour
{
    public List<GameObject> startingLightsOff;
    public List<GameObject> startingLightsOn;

    private void Start()
    {
        GameObject.FindGameObjectWithTag("RaceManager")?.GetComponent<RaceManager>()?.RegisterStartingLights(this);
    }

    public void SetStartingLights(int count)
    {
        for (int i = 0; i < startingLightsOff.Count; i++)
        {
            bool isActive = i < count;

            startingLightsOff[i].SetActive(!isActive);
            startingLightsOn[i].SetActive(isActive);
        }
    }
}
