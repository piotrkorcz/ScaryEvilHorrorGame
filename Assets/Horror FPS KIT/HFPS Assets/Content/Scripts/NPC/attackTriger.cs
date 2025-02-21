using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class attackTriger : MonoBehaviour {
    public ZombieAI zombieai;
    // Use this for initialization
    bool enter;
	
	// Update is called once per frame
	void Update () {
		
	}
    void OnTriggerEnter(Collider col)
    {
        if (col.tag == "Player")
        {
            enter = true;
        }
    }
    void OnTriggerExit(Collider col)
    {
        if (col.tag == "Player" && enter)
        {
            zombieai.AttackPlayer();
            enter = false;
        }
    }
}
