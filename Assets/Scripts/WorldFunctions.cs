using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldFunctions : MonoBehaviour
{
    [SerializeField]
    [Tooltip("0 is 06:00, 900 is 12:00, 1800 is 18:00, 2400 is 00:00")]
    public float time;

    private GameObject playerSpotlight;
    private GameObject sun, moon;


    // Start is called before the first frame update
    void Start()
    {
        playerSpotlight = GameObject.Find("Torchlight");
        sun = GameObject.Find("Sun");
        moon = GameObject.Find("Moon");

        InvokeRepeating("Tick", 1f, 1f);
    }

    private void Tick()
    {
        time += 0.1f;
        if (time % 3600 == 0)
            time = 0;
        Vector3 sunRotation = new Vector3(time / 10, 0, 0);
        Vector3 moonRotation = new Vector3((time / 10) - 180, 0, 0);
        sun.transform.localEulerAngles = sunRotation;
        moon.transform.localEulerAngles = moonRotation;

        if (playerSpotlight != null && playerSpotlight.activeSelf && time < 1800)
            playerSpotlight.SetActive(false);
        else if (playerSpotlight != null && !playerSpotlight.activeSelf && time >= 1800)
            playerSpotlight.SetActive(true);
    }
}
