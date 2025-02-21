using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackTrigger : MonoBehaviour {

    [HideInInspector]
    public bool PlayerInTrigger;

    void OnTriggerStay(Collider col)
    {
        if(col.tag == "Player")
        {
            PlayerInTrigger = true;
        }
    }

    void OnTriggerExit(Collider col)
    {
        if (col.tag == "Player")
        {
            PlayerInTrigger = false;
        }
    }
}
