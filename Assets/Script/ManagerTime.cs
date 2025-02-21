using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManagerTime : MonoBehaviour {

    public float timefinish;
    public float time;
    public GameObject button;
    public GameObject player;
    public GameObject Ui;
    public GameObject camera;
    
    bool test;

    // Use this for initialization
    void Start()
    {

        player.SetActive(false);
        button.SetActive(false);
        //Ui.SetActive(false);
        
    }

    // Update is called once per frame
    void Update()
    {
        time = timefinish - Time.time;
        if (time <= 0 && test == false)
        {
            player.SetActive(true);
            button.SetActive(true);
           // Ui.SetActive(true);
            camera.SetActive(false);
            test = true;
        }
    }
}
