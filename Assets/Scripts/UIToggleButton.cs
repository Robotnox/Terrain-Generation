using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIToggleButton : MonoBehaviour
{
    [SerializeField]
    public Button button;

    [SerializeField]
    public GameObject UI;

    private bool toggleUI = true;

    // Start is called before the first frame update
    void Start()
    {
        button.onClick.AddListener(ToggleUI);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void ToggleUI()
    {
        Debug.Log("CLICKED");
        if (toggleUI)
        {
            UI.SetActive(false);
            button.GetComponentInChildren<Text>().text = "Show";
        }
        else
        {
            UI.SetActive(true);
            button.GetComponentInChildren<Text>().text = "Hide";
        }
        toggleUI = !toggleUI;
    }
}
