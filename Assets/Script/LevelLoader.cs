using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelLoader : MonoBehaviour
{
    int level;

    public GameObject loadingScreen;
    public Text slider;
    public int levelValue;
    void Start()
    {
        level = PlayerPrefs.GetInt("level");
       
    }


    public void LoadLevel(int sceneIndex)
    {
        if (level < levelValue)
        {
            PlayerPrefs.SetInt("level", levelValue);
        }
        StartCoroutine(LoadAsynchronously(sceneIndex));

    }
    

    IEnumerator LoadAsynchronously(int sceneIndex)
    {

        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneIndex);

        loadingScreen.SetActive(true);

        while (!operation.isDone)
        {
            //float progress = Mathf.Clamp01((operation.progress * 100) / 0.0f);

           float progress = operation.progress;
            slider.text = (progress*100).ToString();
            Debug.Log(progress);

            yield return null;
        }
    }
}
