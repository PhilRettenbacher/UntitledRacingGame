using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WheelTrails : MonoBehaviour
{
    [SerializeField]
    private GameObject wheelTrailPrefab;

    private GameObject currentTrail;

    private WheelSuspension suspension;

    // Start is called before the first frame update
    void Start()
    {
        suspension = gameObject.GetComponent<WheelSuspension>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!suspension)
            return;

        if(!currentTrail)
        {
            if(suspension.isSlipping && suspension.onGround)
            {
                //Generate Trail
                currentTrail = Instantiate(wheelTrailPrefab, transform);
            }
        }
        else
        {
            if(!suspension.isSlipping || !suspension.onGround)
            {
                currentTrail.transform.parent = null;
                Destroy(currentTrail, 5);
                currentTrail = null;
            }
        }
    }
}
