using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class adsManager : MonoBehaviour
{
    public float time;
    bool next;
   // GoogleMobileAdsDemoScript admob;

    // Start is called before the first frame update
    void Start()
    {
       
      //  admob = FindObjectOfType<GoogleMobileAdsDemoScript>();
        
    }

    // Update is called once per frame
    void Update()
    {
        if (time >= 0)
        {
            time -= Time.deltaTime;
        }
    }
    public void showinte()
    {
       if(time<= 0)
        {
   //         admob.ShowInterstitial();
            time = 60f;
            Debug.Log("showinte");
        }
            
           
       
    }


}
