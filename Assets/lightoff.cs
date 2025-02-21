using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class lightoff : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject light;
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnTriggerExit(Collider col)
    {
        if (col.tag == "Player")
        {
            light.SetActive(false);
            Debug.Log("enter");
        }

    }

    void OnTriggerEnter(Collider col)
    {

        if (col.tag == "Player")
        {
            light.SetActive(true);
            Debug.Log("enter");
        }
    }
}
