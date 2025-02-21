using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneLoader : MonoBehaviour {

    [System.Serializable]
    public class loaderClass
    {
        public Sprite LevelWallpaper;
        [Tooltip("Scene load name.")]
        public string SceneName;
    }

    public enum priority { Low, BelowNormal, Normal, High }

    public loaderClass[] sceneWallapaper;
    [Space(7)]
    public GameObject MenuUI;
    public GameObject LoadUI;
    public Animation CircleLoad;
    [Space(7)]
    public Image WallpaperImage;
    public Text sceneName;
    [Space(7)]
    [Tooltip("Background Loading Priority")]
	public priority LoadingPriority = priority.High;

	void Start()
	{
        StopAllCoroutines();
        StopCoroutine(loadScene(""));
        switch (LoadingPriority) {
		case priority.Low:
			Application.backgroundLoadingPriority = ThreadPriority.Low;
			break;
		case priority.BelowNormal:
			Application.backgroundLoadingPriority = ThreadPriority.BelowNormal;
			break;
		case priority.Normal:
			Application.backgroundLoadingPriority = ThreadPriority.Normal;
			break;
		case priority.High:
			Application.backgroundLoadingPriority = ThreadPriority.High;
			break;
		}
	}

	public void LoadScene (string scene) 
	{
		SceneManager.LoadScene(scene);
	}

	public void QuitApplication ()
	{
		Application.Quit ();
	}

	public void LoadLevelAsync(string scene)
	{
        CircleLoad.Play();
        MenuUI.SetActive(false);
        LoadUI.SetActive(true);
        sceneName.text = scene;

        for(int i = 0; i < sceneWallapaper.Length; i++)
        {
            if(scene == sceneWallapaper[i].SceneName)
            {
                WallpaperImage.sprite = sceneWallapaper[i].LevelWallpaper;
            }
        }

        Debug.Log("Set Load");
        PlayerPrefs.SetInt("LoadGame", 1);
        StartCoroutine (loadScene (scene));
	}

    public void NewGameAsync(string scene)
    {
        CircleLoad.Play();
        MenuUI.SetActive(false);
        LoadUI.SetActive(true);
        sceneName.text = scene;

        for (int i = 0; i < sceneWallapaper.Length; i++)
        {
            if (scene == sceneWallapaper[i].SceneName)
            {
                WallpaperImage.sprite = sceneWallapaper[i].LevelWallpaper;
            }
        }

        Debug.Log("Set New Game");
        PlayerPrefs.SetInt("LoadGame", 0);
        StartCoroutine(loadScene(scene));
    }

    IEnumerator loadScene(string scene){
        Debug.Log("Load Phase 1");
        //yield return new WaitForSeconds (1f);
        Debug.Log("Load Phase 2");
        AsyncOperation async = SceneManager.LoadSceneAsync(scene, LoadSceneMode.Single);
		async.allowSceneActivation = false;
		while (!async.isDone) {
			if (async.progress == 0.9f) {
				async.allowSceneActivation = true;
				break;
			}
			yield return null; 
		}
	}
}