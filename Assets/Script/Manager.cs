using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Manager : MonoBehaviour {

    public Button level1;
    
   
    int level;

    // Use this for initialization
    void Start()
    {

        level = PlayerPrefs.GetInt("level");
        if (level == 0)
        {

            PlayerPrefs.SetInt("level", 1);
        }

    }

    // Update is called once per frame
    void Update()
    {
        level = PlayerPrefs.GetInt("level");
        {
            if (level >= 1)
            {
                level1.interactable = true;
            }
            else
            {
                level1.interactable = false;
            }

           
        }
    }

    public void Quit()
    {
    #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
    #else
		Application.Quit ();
    #endif
    }

    public void GetReview()
    {
        Application.OpenURL("");
    }

    public void MoreApp()
    {
        Application.OpenURL("");
    }
}
