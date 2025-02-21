using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class timeWaiting : MonoBehaviour {

    public float timefinish;
    public float time;
    public GameObject button;
    bool test;

	// Use this for initialization
	void Start () {

        
        button.SetActive(false);
	}
	
	// Update is called once per frame
	void Update () {
          time =timefinish - Time.time ;
          if (time <= 0 && test==false)
        {
            button.SetActive(true);
            test = true;
        }
	}
}
