using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldFunctions : MonoBehaviour
{
    [SerializeField]
    [Tooltip("0 is 06:00, 900 is 12:00, 1800 is 18:00, 2400 is 00:00")]
    public float time;

    [SerializeField]
    public bool IsRaining;

    [SerializeField]
    public Material daySkybox;
    [SerializeField]
    public Material nightSkybox;

    private GameObject playerSpotlight;
    private GameObject sun, moon, weather;

    void Start()
    {
        playerSpotlight = GameObject.Find("Torchlight");
        sun = GameObject.Find("Sun");
        moon = GameObject.Find("Moon");
        weather = GameObject.Find("Rain");
        weather.GetComponent<ParticleSystem>().Stop();

        InvokeRepeating("Tick", 1f, 1f);
    }

    /*
     * 24 hours is worth 3600 ticks.
     * Rotate sun and moon per tick.
     * Turn on/off player spotlight.
     * For each new day, there is a 50% chance of rain all day.
     * TODO : Add clouds when raining
     */
    private void Tick()
    {
        time += 0.1f;
        if (time >= 3600)
        {
            time = 0;
            int v = Random.Range(0, 2);
            IsRaining = v == 1;
        }
        Vector3 sunRotation = new Vector3(time / 10, 0, 0);
        Vector3 moonRotation = new Vector3((time / 10) - 180, 0, 0);
        sun.transform.localEulerAngles = sunRotation;
        moon.transform.localEulerAngles = moonRotation;

        if (playerSpotlight != null && playerSpotlight.activeSelf && time < 1800)
        {
            playerSpotlight.SetActive(false);
            RenderSettings.skybox = daySkybox;
        }
        else if (playerSpotlight != null && !playerSpotlight.activeSelf && time >= 1800)
        {
            playerSpotlight.SetActive(true);
            RenderSettings.skybox = nightSkybox;
        }

        var weatherParticle = weather.GetComponent<ParticleSystem>();
        if (IsRaining && weatherParticle.isStopped)
        {
            weatherParticle.Play();
            sun.GetComponent<Light>().intensity = 0.35f;
        }
        else if (!IsRaining && weatherParticle.isPlaying)
        {
            weatherParticle.Stop();
            sun.GetComponent<Light>().intensity = 2f;
        }
    }
}
