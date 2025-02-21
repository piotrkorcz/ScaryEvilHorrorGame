using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gdpr : MonoBehaviour {
    public GameObject gdprpanel;
	// Use this for initialization
	void Start ()
    {
        if (PlayerPrefs.GetInt("Gdpr") == 0)
            gdprpanel.SetActive(true);
        else
            gdprpanel.SetActive(false);


    }
	
	// Update is called once per frame
	void Update () {
		

    }

    public void Personalizedads()
    {
        PlayerPrefs.SetInt("Gdpr",1);
        gdprpanel.SetActive(false);
    }

    public void Nonpersonalizedads()
    {
        PlayerPrefs.SetInt("Gdpr", 0);
        gdprpanel.SetActive(false);
    }

    
    
}
