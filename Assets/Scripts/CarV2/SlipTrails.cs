using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlipTrails : MonoBehaviour
{
    [SerializeField]
    private GameObject wheelTrailPrefab;

    private GameObject currentTrail;

    private Suspension suspension;
    private ParticleSystem particles;
    // Start is called before the first frame update
    void Start()
    {
        suspension = gameObject.GetComponent<Suspension>();
        particles = gameObject.GetComponentInChildren<ParticleSystem>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!suspension)
            return;

        if (!currentTrail)
        {
            if (suspension.isSlipping && suspension.onGround)
            {
                //Generate Trail
                currentTrail = Instantiate(wheelTrailPrefab, suspension.wheel.position + new Vector3(0, -0.25f, 0), Quaternion.LookRotation(suspension.wheel.up, suspension.wheel.forward), suspension.wheel);
            }
        }
        else
        {
            if (!suspension.isSlipping || !suspension.onGround)
            {
                currentTrail.transform.parent = null;
                Destroy(currentTrail, 5);
                currentTrail = null;
            }
        }

        if (suspension.isSlipping && suspension.onGround && !suspension.inReverseGear && suspension.wheelForce.magnitude > 2.5f)
        {
            particles.Play();
        }
        else
        {
            particles.Stop();
        }
    }
}
