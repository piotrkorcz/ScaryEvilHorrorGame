using UnityEngine;
using System.Collections;

public class KeyHintManager : MonoBehaviour {

	private HFPS_GameManager gameManager;
	private InputManager inputManager;

	[Header("Main Setup")]
	public ScriptManager scriptManager;
	public string InputKey;
	
	[Header("Text")]
	public string  HintText;
	
	private KeyCode Key;

	void Start()
	{
        gameManager = scriptManager.GameManager;
		inputManager = scriptManager.inputManager;
	}

	void Update()
	{
		if(inputManager.InputsCount() > 0)
		{
			Key = (KeyCode)System.Enum.Parse(typeof(KeyCode), inputManager.GetInput(InputKey));
		}
	}
	
	public void UseObject()
	{
        gameManager.ShowHint("Press " + "\"" + Key.ToString() + "\" " + HintText);
	}
}
