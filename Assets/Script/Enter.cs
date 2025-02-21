using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enter : MonoBehaviour {
    public int sceneValue;
    public int levelValue;
    int level;

    public GameObject completePannel;
    LevelLoader levelLoader;
    // Use this for initialization
    void Start () {
        level = PlayerPrefs.GetInt("level");
        levelLoader = FindObjectOfType<LevelLoader>();
    }

    void OnTriggerEnter(Collider col)
    {
        if (col.tag == "Player")
        {
            if(level< levelValue)
            {
                PlayerPrefs.SetInt("level", levelValue);
            }
            completePannel.SetActive(true);
            Debug.Log("enter");
        }
    }

    public void Loadscene()
    {
        levelLoader.LoadLevel(sceneValue);
    }
}
