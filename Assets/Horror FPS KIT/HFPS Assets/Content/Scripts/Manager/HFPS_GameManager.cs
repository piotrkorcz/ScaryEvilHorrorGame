/*
 * HFPS_GameManager.cs - script is written by ThunderWire Games
 * This script controls all game actions
 * ver. 1.2
*/

using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using ThunderWire.Configuration;
using ThunderWire.Parser;

public enum spriteType
{
    Interact, Grab, Examine
}

public class HFPS_GameManager : MonoBehaviour {
    public bool mobileActive;
    ConfigManager config = new ConfigManager();
    ParsePrimitives parser = new ParsePrimitives();

    private List<string> inputKeyCache = new List<string>();
    //bool ads;
    //adsManager ads;
    AdsUnity adsUnityImplementation;
    [Header("Config Setup")]
    public string ConfigName = "GameConfig";
    public bool showDebug;

    private string configName;
    private bool UsePlayerPrefs;
    private bool refreshStatus = false;

    [Header("Main")]
    public GameObject Player;
    public InputManager inputManager;
    public Inventory inventoryScript;

    [HideInInspector]
    public ScriptManager scriptManager;

    [HideInInspector]
    public HealthManager healthManager;

    [Header("Cursor")]
    public bool m_ShowCursor = false;

    [Header("Game Panels")]
    public GameObject PauseGamePanel;
    public GameObject MainGamePanel;
    public GameObject TabButtonPanel;

    [Header("Pause UI")]
    public KeyCode ShowPauseMenuKey = KeyCode.Escape;
    public bool reallyPause = false;
    [HideInInspector] public bool isPaused = false;

    [Header("Paper UI")]
    public GameObject PaperTextUI;
    public Text PaperReadText;

    [Header("Flashlight & Battery")]
    public GameObject BatterySprites;

    [Header("Valve UI")]
    public Slider ValveSlider;

    private float slideTime;
    private float slideValue;

    [Header("Saving Notification UI")]
    public GameObject saveNotification;

    [Header("Notification UI")]
    public GameObject NotificationPanel;
    public GameObject NotificationPrefab;
    public Sprite WarningSprite;

    private List<GameObject> Notifications = new List<GameObject>();

    [Header("Hints UI")]
    public Text HintText;

    [Header("Crosshair")]
    public Image Crosshair;

    [Header("Health")]
    public Text HealthText;

    [Header("Right Buttons")]
    public bool useSprites;
    public GameObject InteractSprite;
    public GameObject InteractSprite1;

    [Header("Down Examine Buttons")]
    public GameObject DownExamineUI;
    public GameObject ExamineButton1;
    public GameObject ExamineButton2;
    public GameObject ExamineButton3;

    [Header("Down Grab Buttons")]
    public GameObject DownGrabUI;
    public GameObject GrabButton1;
    public GameObject GrabButton2;
    public GameObject GrabButton3;
    public GameObject GrabButton4;

    public Sprite DefaultSprite;

    [HideInInspector]
    public bool isHeld;

    [HideInInspector]
    public bool canGrab;

    private float fadeHint;
    private bool startFadeHint = false;

    private string GrabKey;
    private string ThrowKey;
    private string RotateKey;
    private string Inventory;
    private KeyCode InventoryKey;

    private bool isOverlapping;
    private bool isPressed;
    private bool isSet;

    void SetKeys()
    {
        if (mobileActive)
        {
            GrabKey = "E";
            ThrowKey = "I";
            RotateKey = inputManager.GetInput("Fire");
            Inventory = "tab";
        }
        else
        { 
            GrabKey = inputManager.GetInput("Pickup");
            ThrowKey = inputManager.GetInput("Throw");
            RotateKey = inputManager.GetInput("Fire");
            InventoryKey = parser.ParseType<KeyCode>(inputManager.GetInput("Inventory"));
         }
        isSet = true;

    }

    void Awake()
    {
        healthManager = Camera.main.transform.root.gameObject.GetComponent<HealthManager>();
        scriptManager = Player.transform.GetChild(0).transform.GetChild(0).GetComponent<ScriptManager>();
    }

    void Start()
    {
        //ads = FindObjectOfType<adsManager>();
        adsUnityImplementation = FindObjectOfType<AdsUnity>();
        configName = PlayerPrefs.GetString("GameConfig");
        if (PlayerPrefs.HasKey("UsePlayerPrefs"))
        {
            UsePlayerPrefs = parser.ParseType<bool>(PlayerPrefs.GetString("UsePlayerPrefs"));
        }

        if (UsePlayerPrefs)
        {
            if (config.ExistFile(configName))
            {
                config.Setup(showDebug, configName);
            }
            else
            {
                if (config.ExistFile(ConfigName))
                {
                    config.Setup(showDebug, ConfigName);
                }
                else
                {
                    Debug.LogError("\"" + configName + "\"" + " and " + "\"" + ConfigName + "\"" + " does not exist, try launching scene from Main Menu");
                    Debug.LogError("Player will not move if GameConfig does not exist in Data folder");
                }
            }
        }
        else
        {
            if (config.ExistFile(ConfigName))
            {
                config.Setup(showDebug, ConfigName);
            }
            else
            {
                Debug.LogError("\"" + ConfigName + "\"" + " does not exist, try launching scene from Main Menu or run scene again");
                Debug.LogError("Player will not move if GameConfig does not exist in Data folder");
            }
        }

        for (int i = 0; i < config.ConfigKeysCache.Count; i++)
        {
            inputKeyCache.Add(config.ConfigKeysCache[i]);
        }

        TabButtonPanel.SetActive(false);
        //saveNotification.SetActive(false);
        HideSprites(spriteType.Interact);
        HideSprites(spriteType.Grab);
        HideSprites(spriteType.Examine);
        Unpause();

        if (m_ShowCursor) {
            ControlFreak2.CFCursor.visible = (true);
            ControlFreak2.CFCursor.lockState = CursorLockMode.None;
        } else {
            ControlFreak2.CFCursor.visible = (false);
            ControlFreak2.CFCursor.lockState = CursorLockMode.Locked;
        }

        if (ContainsSection("Game"))
        {
            float volume = float.Parse(Deserialize("Game", "Volume"));
            AudioListener.volume = volume;
        }
    }

    void Update()
    {
        HintText.gameObject.GetComponent<CanvasRenderer>().SetAlpha(fadeHint);

        if (inputManager.InputsCount() > 0 && !isSet)
        {
            SetKeys();
        }

        if (inputManager.GetRefreshStatus() && isSet) {
            isSet = false;
        }

        if (ContainsSection("Game") && GetRefreshStatus())
        {
            float volume = float.Parse(Deserialize("Game", "Volume"));
            AudioListener.volume = volume;
        }

        //Fade Out Hint
        if (fadeHint > 0 && startFadeHint)
        {
            fadeHint -= Time.deltaTime;
        }
        else
        {
            startFadeHint = false;
        }

        if (ControlFreak2.CF2Input.GetKeyDown(ShowPauseMenuKey) && !isPressed) {
            isPressed = true;
            PauseGamePanel.SetActive(!PauseGamePanel.activeSelf);
            MainGamePanel.SetActive(!MainGamePanel.activeSelf);
            isPaused = !isPaused;
        } else if (isPressed) {
            isPressed = false;
        }

        if (PauseGamePanel.activeSelf && isPaused && isPressed) {
            LockStates(true, true, true, true, 3);
            scriptManager.pFunctions.enabled = false;
            if (reallyPause)
            {
                Time.timeScale = 0;
                Debug.Log("pause");
                adsUnityImplementation.showinterUnity();
                //else if()

            }
        } else if (isPressed) {
            LockStates(false, true, true, true, 3);
            scriptManager.pFunctions.enabled = true;
            if (reallyPause)
            {
                Time.timeScale = 1;
            }
        }

        if (ControlFreak2.CF2Input.GetKeyDown("tab") && !isPressed && !isPaused && !isOverlapping) {
            isPressed = true;
            TabButtonPanel.SetActive(!TabButtonPanel.activeSelf);
        } else if (isPressed) {
            isPressed = false;
        }

        if (TabButtonPanel.activeSelf && isPressed) {
            scriptManager.pFunctions.enabled = false;
            LockStates(true, true, true, true, 0);
            HideSprites(spriteType.Interact);
            HideSprites(spriteType.Grab);
            HideSprites(spriteType.Examine);
        } else if (isPressed) {
            LockStates(false, true, true, true, 0);
            scriptManager.pFunctions.enabled = true;
        }

        if (Notifications.Count > 4) {
            Destroy(Notifications[0]);
            Notifications.RemoveAll(GameObject => GameObject == null);
            Debug.Log("Destroy");
        }
    }

    public void Unpause()
    {
        
        LockStates(false, true, true, true, 3);
        scriptManager.pFunctions.enabled = true;
        PauseGamePanel.SetActive(false);
        MainGamePanel.SetActive(true);
        isPaused = false;
        if (reallyPause)
        {
            Time.timeScale = 1;
        }
    }

    public void ChangeScene(string SceneName)
    {
        adsUnityImplementation.showinterUnity();
        SceneManager.LoadScene(SceneName);
    }

    public void LockStates(bool LockState, bool Interact, bool Controller, bool CursorVisible, int BlurLevel) {
        switch (LockState) {
            case true:
                Player.transform.GetChild(0).GetChild(0).GetComponent<MouseLook>().enabled = false;
                if (Interact) {
                    Player.transform.GetChild(0).GetChild(0).GetComponent<InteractManager>().inUse = true;
                }
                if (Controller) {
                    Player.GetComponent<PlayerController>().controllable = false;
                }
                if (BlurLevel > 0) {
                    if (BlurLevel == 1) { scriptManager.MainCameraBlur.enabled = true; }
                    if (BlurLevel == 2) { scriptManager.ArmsCameraBlur.enabled = true; }
                    if (BlurLevel == 3)
                    {
                        scriptManager.MainCameraBlur.enabled = true;
                        scriptManager.ArmsCameraBlur.enabled = true;
                    }
                }
                if (CursorVisible) {
                    ShowCursor(true);
                }
                break;
            case false:
                Player.transform.GetChild(0).GetChild(0).GetComponent<MouseLook>().enabled = true;
                if (Interact) {
                    Player.transform.GetChild(0).GetChild(0).GetComponent<InteractManager>().inUse = false;
                }
                if (Controller) {
                    Player.GetComponent<PlayerController>().controllable = true;
                }
                if (BlurLevel > 0) {
                    if (BlurLevel == 1) { scriptManager.MainCameraBlur.enabled = false; }
                    if (BlurLevel == 2) { scriptManager.ArmsCameraBlur.enabled = false; }
                    if (BlurLevel == 3)
                    {
                        scriptManager.MainCameraBlur.enabled = false;
                        scriptManager.ArmsCameraBlur.enabled = false;
                    }
                }
                if (CursorVisible) {
                    ShowCursor(false);
                }
                break;
        }
    }

    public void UIPreventOverlap(bool State)
    {
        isOverlapping = State;
    }

    public void MouseLookState(bool State)
    {
        switch (State) {
            case true:
                Player.transform.GetChild(0).GetChild(0).GetComponent<MouseLook>().enabled = true;
                break;
            case false:
                Player.transform.GetChild(0).GetChild(0).GetComponent<MouseLook>().enabled = false;
                break;
        }
    }

    public void ShowCursor(bool state)
    {
        switch (state) {
            case true:
                ControlFreak2.CFCursor.visible = (true);
                ControlFreak2.CFCursor.lockState = CursorLockMode.None;
                break;
            case false:
                ControlFreak2.CFCursor.visible = (false);
                ControlFreak2.CFCursor.lockState = CursorLockMode.Locked;
                break;
        }
    }

    public void AddPickupMessage(string itemName)
    {
        GameObject PickupMessage = Instantiate(NotificationPrefab);
        Notifications.Add(PickupMessage);
        PickupMessage.transform.SetParent(NotificationPanel.transform);
        PickupMessage.GetComponent<ItemPickupNotification>().SetPickupNotification(itemName);
    }

    public void AddMessage(string message)
    {
        GameObject Message = Instantiate(NotificationPrefab);
        Notifications.Add(Message);
        Message.transform.SetParent(NotificationPanel.transform);
        Message.GetComponent<ItemPickupNotification>().SetNotification(message);
    }

    public void WarningMessage(string warning)
    {
        GameObject Message = Instantiate(NotificationPrefab);
        Notifications.Add(Message);
        Message.transform.SetParent(NotificationPanel.transform);
        Message.GetComponent<ItemPickupNotification>().SetNotificationIcon(warning, WarningSprite);
    }

    public void ShowHint(string hint)
    {
        StopCoroutine(FadeWaitHint());
        fadeHint = 1f;
        startFadeHint = false;
        HintText.gameObject.SetActive(true);
        HintText.text = hint;
        HintText.color = Color.white;
        StartCoroutine(FadeWaitHint());
    }

    IEnumerator FadeWaitHint()
    {
        yield return new WaitForSeconds(3f);
        startFadeHint = true;
    }

    public void NewValveSlider(float start, float time)
    {
        ValveSlider.gameObject.SetActive(true);
        StartCoroutine(MoveValveSlide(start, 10f, time));
    }

    public void DisableValveSlider()
    {
        ValveSlider.gameObject.SetActive(false);
        StopCoroutine(MoveValveSlide(0,0,0));
    }

    public IEnumerator MoveValveSlide(float start, float end, float time)
    {
        var currentValue = start;
        var t = 0f;
        while (t < 1)
        {
            t += Time.deltaTime / (time * 10);
            ValveSlider.value = Mathf.Lerp(currentValue, end, t);
            yield return null;
        }
    }

    public void ShowSaveNotification(float time)
    {
        StartCoroutine(FadeInSave(time));
    }

    IEnumerator FadeInSave(float t)
    {
        saveNotification.SetActive(true);
        yield return new WaitForSeconds(t);
        StartCoroutine(FadeOutSave());
    }

    IEnumerator FadeOutSave()
    {
        saveNotification.GetComponent<Image>().CrossFadeAlpha(0.1f, 0.5f, true); ;
        saveNotification.transform.GetChild(0).GetComponent<Text>().CrossFadeAlpha(0.1f, 0.5f, true);
        yield return new WaitForSeconds(0.5f);
        saveNotification.SetActive(false);
    }

    public bool CheckController()
	{
		return Player.GetComponent<PlayerController> ().controllable;
	}

    public void ShowInteractSprite(int num, string name, string Key)
    {
		if (!isHeld) {
			switch (num) {
				case 1:
					InteractSprite.SetActive (true);
					Image bg = InteractSprite.transform.GetChild (0).GetComponent<Image> ();
					Text buttonKey = InteractSprite.transform.GetChild (1).gameObject.GetComponent<Text> ();
					Text txt = InteractSprite.gameObject.GetComponent<Text> ();
					buttonKey.text = Key;
					txt.text = name;
					if (Key == "Mouse0" || Key == "Mouse1" || Key == "Mouse2") {
						bg.sprite = GetKeySprite (Key);
						buttonKey.gameObject.SetActive (false);
					} else {
						bg.sprite = DefaultSprite;
						buttonKey.gameObject.SetActive (true);
					}
				break;
				case 2:
					InteractSprite1.SetActive (true);
					Image bg1 = InteractSprite1.transform.GetChild (0).GetComponent<Image> ();
					Text buttonKey1 = InteractSprite1.transform.GetChild (1).gameObject.GetComponent<Text> ();
					Text txt1 = InteractSprite1.gameObject.GetComponent<Text> ();
					buttonKey1.text = Key;
					txt1.text = name;
					if (Key == "Mouse0" || Key == "Mouse1" || Key == "Mouse2") {
						bg1.sprite = GetKeySprite (Key);
						buttonKey1.gameObject.SetActive (false);
					} else {
						bg1.sprite = DefaultSprite;
						buttonKey1.gameObject.SetActive (true);
					}
				break;
			}
		}
    }

    public void ShowExamineSprites(string UseKey, string ExamineKey)
    {
        SetKeyCodeSprite(ExamineButton1.transform, UseKey);
        SetKeyCodeSprite(ExamineButton2.transform, RotateKey);
        SetKeyCodeSprite(ExamineButton3.transform, ExamineKey);
        DownExamineUI.SetActive(true);
    }

    public void ShowGrabSprites()
    {
        SetKeyCodeSprite(GrabButton1.transform, GrabKey);
        SetKeyCodeSprite(GrabButton2.transform, RotateKey);
        SetKeyCodeSprite(GrabButton3.transform, ThrowKey);
        GrabButton4.SetActive(true); //ZoomKey
        DownGrabUI.SetActive(true);
    }

    private void SetKeyCodeSprite(Transform Button, string Key)
    {
        if (Key == "Mouse0" || Key == "Mouse1" || Key == "Mouse2")
        {
            Button.GetChild(1).GetComponent<Text>().text = Key;
            Button.GetChild(0).GetComponent<Image>().sprite = GetKeySprite(Key);
            Button.GetChild(0).GetComponent<Image>().rectTransform.sizeDelta = new Vector2(25, 25);
            Button.GetChild(0).GetComponent<Image>().type = Image.Type.Simple;
            Button.GetChild(1).gameObject.SetActive(false);
        }
        else
        {
            Button.GetChild(1).GetComponent<Text>().text = Key;
            Button.GetChild(0).GetComponent<Image>().sprite = DefaultSprite;
            Button.GetChild(0).GetComponent<Image>().rectTransform.sizeDelta = new Vector2(34, 34);
            Button.GetChild(0).GetComponent<Image>().type = Image.Type.Sliced;
            Button.GetChild(1).gameObject.SetActive(true);
        }
        if(Key == "None")
        {
            Button.GetChild(1).GetComponent<Text>().text = "NA";
            Button.GetChild(0).GetComponent<Image>().sprite = DefaultSprite;
            Button.GetChild(0).GetComponent<Image>().rectTransform.sizeDelta = new Vector2(34, 34);
            Button.GetChild(0).GetComponent<Image>().type = Image.Type.Sliced;
            Button.GetChild(1).gameObject.SetActive(true);
        }
    }

	public void HideSprites(spriteType type)
	{
		switch (type) {
            case spriteType.Interact:
			InteractSprite.SetActive (false);
			InteractSprite1.SetActive (false);
			break;
            case spriteType.Grab:
            DownGrabUI.SetActive(false);
			break;
            case spriteType.Examine:
            DownExamineUI.SetActive(false);		
			break;
		}
	}

	public Sprite GetKeySprite(string Key)
	{
		return Resources.Load<Sprite>(Key);
	}

    public string Deserialize(string Section, string Key)
    {
        return config.Deserialize(Section, Key);
    }

    public void Serialize(string Section, string Key, string Value)
    {
        config.Serialize(Section, Key, Value);
    }

    public bool ContainsSection(string Section)
    {
        return config.ContainsSection(Section);
    }

    public bool ContainsSectionKey(string Section, string Key)
    {
        return config.ContainsSectionKey(Section, Key);
    }

    public bool ContainsKeyValue(string Section, string Key, string Value)
    {
        return config.ContainsKeyValue(Section, Key, Value);
    }

    public void RemoveSectionKey(string Section, string Key)
    {
        config.RemoveSectionKey(Section, Key);
    }

    public bool ExistFile(string file)
    {
        return config.ExistFile(file);
    }

    public string GetKey(int index)
    {
        return inputKeyCache[index];
    }

    public int GetKeysCount()
    {
        return config.ConfigKeysCache.Count;
    }

    public int GetKeysSectionCount(string Section)
    {
        return config.GetSectionKeys(Section);
    }

    public void Refresh()
    {
        StartCoroutine(WaitRefresh());
    }

    IEnumerator WaitRefresh()
    {
        refreshStatus = true;
        yield return new WaitForSeconds(1f);
        refreshStatus = false;
    }

    public bool GetRefreshStatus()
    {
        return refreshStatus;
    }
}