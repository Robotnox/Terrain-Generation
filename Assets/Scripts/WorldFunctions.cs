using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldFunctions : MonoBehaviour
{
    [SerializeField]
    [Tooltip("0 is 06:00, 900 is 12:00, 1800 is 18:00, 2400 is 00:00")]
    public float time;

    private GameObject playerSpotlight;

    // Start is called before the first frame update
    void Start()
    {
        playerSpotlight = GameObject.Find("Torchlight");

        InvokeRepeating("Tick", 1f, 1f);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void Tick()
    {
        time += 0.1f;
        if (time % 3600 == 0)
            time = 0;
        Vector3 to = new Vector3(time, 0, 0);
        this.transform.localEulerAngles = to;

        if (playerSpotlight.activeSelf && time < 1800)
            playerSpotlight.SetActive(false);
        else if (!playerSpotlight.activeSelf && time >= 1800)
            playerSpotlight.SetActive(true);
    }
}
