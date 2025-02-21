using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class rateusmanager : MonoBehaviour
{
    public string url;
    public GameObject pannel;
    public GameObject tuto;
    // Start is called before the first frame update
    void Start()
    {
        if (PlayerPrefs.GetInt("varible") == 1)
        {
            pannel.SetActive(true);
        }
        if (PlayerPrefs.GetInt("varible") == 0) 
        {

            pannel.SetActive(false);
            tuto.SetActive(true);
            PlayerPrefs.SetInt("varible", 1) ;

            StartCoroutine(LateCall());


        }
      




    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void rateus()
    {
        Application.OpenURL(url);
        PlayerPrefs.SetInt("varible", 2);
    }

    IEnumerator LateCall()
    {

        yield return new WaitForSeconds(10);

        tuto.SetActive(false);
    }
}
