/*
 * InputController.cs - script is written by ThunderWire Games
 * Script for Rebindable Inputs
 * Ver. 1.2
*/

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using System.Collections.Generic;
using ThunderWire.Configuration;
using ThunderWire.Parser;

public class InputController : MonoBehaviour {
	ConfigManager config = new ConfigManager ();
    ParsePrimitives parser = new ParsePrimitives();

	[Tooltip("Name of the config file")]
	public string ConfigName = "GameConfig";

	public bool showDebug;

	[Tooltip("In MainMenu set this to true if you want to save and load config name. In game keep this false.")]
	public bool UsePlayerPrefs;
	private bool usePP;

	[Tooltip("Set this to true if you want to save config name to PlayerPrefs.")]
	public bool SetPlayerPrefs;

    [Tooltip("All rebindable buttons must be added here")]
    public ControlsHelper controlsHelper;
    public UIOptions Options;
	private List<string> InputKeysCache = new List<string> ();

	private bool rebind;
	private Text buttonText;
	private Button button;
	private string inputName;
	private string defaultKey;

    private string configName;
    private string inputConfig;
	private string configFolder;
	private bool isExist;

    void Awake()
    {
        for (int i = 0; i < controlsHelper.InputsList.Count; i++)
        {
            controlsHelper.InputsList[i].InputButton.GetComponent<Button>().onClick.AddListener(delegate { RebindSelected(); });
        }
    }

    void Start () {
        if (PlayerPrefs.HasKey("GameConfig"))
        {
            configName = PlayerPrefs.GetString("GameConfig");
        }
		if(PlayerPrefs.HasKey("UsePlayerPrefs"))
        {
			usePP = parser.ParseType<bool>(PlayerPrefs.GetString ("UsePlayerPrefs"));
		}

        if (!controlsHelper)
        {
            Debug.LogError("InputController script needs ControlsHelper to work!");
        }
        else
        {
            if (SetPlayerPrefs)
            {
                if (config.ExistFile(ConfigName))
                {
                    config.Setup(showDebug, ConfigName);
                    isExist = true;
                }
                else
                {
                    config.Setup(showDebug, ConfigName);
                    Debug.LogError(ConfigName + " does not exist in the Data folder (Recreating Config File)");
                    isExist = false;
                }

                PlayerPrefs.SetString("GameConfig", ConfigName);
                if (UsePlayerPrefs)
                {
                    PlayerPrefs.SetString("UsePlayerPrefs", "True");
                }
                else
                {
                    PlayerPrefs.SetString("UsePlayerPrefs", "False");
                }
            }
            else
            {
                if (usePP)
                {
                    if (config.ExistFile(configName))
                    {
                        config.Setup(showDebug, configName);
                        isExist = true;
                    }
                    else
                    {
                        if (config.ExistFile(ConfigName))
                        {
                            config.Setup(showDebug, ConfigName);
                            Debug.LogWarning(configName + " does not exist in the Data folder (Using default GameConfig)");
                            isExist = true;
                        }
                        else
                        {
                            config.Setup(showDebug, ConfigName);
                            Debug.LogError(ConfigName + " does not exist in the Data folder");
                            isExist = false;
                        }
                    }
                }
                else
                {
                    if (config.ExistFile(ConfigName))
                    {
                        config.Setup(showDebug, ConfigName);
                        isExist = true;
                    }
                    else
                    {
                        config.Setup(showDebug, ConfigName);
                        isExist = false;
                    }
                }
            }

            //ExistText.SetActive(false);

            if (isExist)
            {
                Deserialize();
            }
            else
            {
                UseDefault();
            }

            LoadInputsToList();
        }
	}
	
	public void RebindSelected()
	{
        var go = EventSystem.current.currentSelectedGameObject;	
		for (int i= 0; i < controlsHelper.InputsList.Count; i++)
		{
			if (go.name == controlsHelper.InputsList[i].InputButton.name && !rebind)
			{
				buttonText = controlsHelper.InputsList[i].InputButton.transform.GetChild(0).gameObject.GetComponent<Text>();
				button = controlsHelper.InputsList[i].InputButton.GetComponent<Button>();
				defaultKey = buttonText.text;
				buttonText.text = "Press Button";
				inputName = controlsHelper.InputsList[i].Input;
				button.interactable = false;
				rebind = true;
			}
		}
	}
	
	void Update()
	{
		foreach(KeyCode kcode in Enum.GetValues(typeof(KeyCode)))
		{
			if (ControlFreak2.CF2Input.GetKeyDown (kcode) && rebind) {
				if (kcode.ToString () == defaultKey) {
					buttonText.text = defaultKey;
					buttonText = null;
					inputName = null;
					button.interactable = true;
					button = null;
					rebind = false;
				} else {
					RebindKey (kcode.ToString ());
				}
			}
		}
	}

	void RebindKey(string kcode)
	{
		if (!CheckForExistButton(kcode)) {
			buttonText.text = kcode;
			SerializeInput (inputName, kcode);
			UpdateInputCache();
			buttonText = null;
			inputName = null;
			button.interactable = true;
			button = null;
			rebind = false;
		} else {
            Options.DuplicateControl.SetActive(true);
            Options.DuplicateControl.transform.GetChild(0).GetComponent<Text>().text = "Input key \"" + kcode + "\" is already defined";
            Options.RewriteKeycode = kcode;
            Options.ApplyButton.interactable = false;
            Options.BackButton.interactable = false;
			button.interactable = true;
			button = null;
			rebind = false;
		}
	}

    public void Rewrite(string RewriteKeycode)
    {
        for (int i = 0; i < controlsHelper.InputsList.Count; i++)
        {
            Text DuplicateKeyText = controlsHelper.InputsList[i].InputButton.transform.GetChild(0).gameObject.GetComponent<Text>();
            if (DuplicateKeyText.text == RewriteKeycode)
            {
                Debug.Log("Rewrite");
                DuplicateKeyText.text = "None";
                SerializeInput(controlsHelper.InputsList[i].Input, "None"); 
            }
        }
        buttonText.text = RewriteKeycode;
        SerializeInput(inputName, RewriteKeycode);
        UpdateInputCache();
        inputName = null;
    }

    public void BackRewrite()
    {
        buttonText.text = defaultKey;
        buttonText = null;
    }

	bool CheckForExistButton(string Key)
	{
		if (InputKeysCache.Contains (Key)) {
			return true;
		} else {
			return false;
		}
	}
	
	void SerializeInput(string input, string button)
	{
		config.Serialize("Input", input, button);
	}

	void LoadInputsToList()
	{
		for (int i= 0; i < controlsHelper.InputsList.Count; i++)
		{
			string value = config.Deserialize("Input", controlsHelper.InputsList[i].Input);
			InputKeysCache.Add (value);
		}
	}

	void UpdateInputCache()
	{
		InputKeysCache.Clear ();
		for (int i= 0; i < controlsHelper.InputsList.Count; i++)
		{
			string value = config.Deserialize("Input", controlsHelper.InputsList[i].Input);
			InputKeysCache.Add (value);
		}
	}
	
	void Deserialize()
	{
		for (int i= 0; i < controlsHelper.InputsList.Count; i++)
		{
			string value = config.Deserialize("Input", controlsHelper.InputsList[i].Input);
			Text bText = controlsHelper.InputsList[i].InputButton.transform.GetChild(0).gameObject.GetComponent<Text>();
			bText.text = value;
			if(string.IsNullOrEmpty(bText.text))
			{
				SerializeInput(controlsHelper.InputsList[i].Input, controlsHelper.InputsList[i].DefaultKey.ToString());
				Deserialize();
			}
		}
	}
	
	void UseDefault()
	{
		for (int i= 0; i < controlsHelper.InputsList.Count; i++)
		{
			Text bText = controlsHelper.InputsList[i].InputButton.transform.GetChild(0).gameObject.GetComponent<Text>();
			string keycode = controlsHelper.InputsList[i].DefaultKey.ToString();
			if(!(keycode == "None"))
			{
				bText.text = keycode;
				SerializeInput(controlsHelper.InputsList[i].Input, keycode);
			}else{
				bText.text = "Set DefaultKey!";
			}
		}
	}
}
