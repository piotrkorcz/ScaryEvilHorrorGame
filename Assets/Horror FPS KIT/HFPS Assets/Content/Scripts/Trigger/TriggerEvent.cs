using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class TriggerEvent : MonoBehaviour {

    public UnityEvent triggerEvent;
    private bool isPlayed;

    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player" && !isPlayed)
        {
            triggerEvent.Invoke();
            isPlayed = true;
        }
    }
}
