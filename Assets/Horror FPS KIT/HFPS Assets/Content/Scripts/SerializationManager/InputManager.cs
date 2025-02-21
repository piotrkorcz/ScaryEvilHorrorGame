using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class InputManager : MonoBehaviour {
	private HFPS_GameManager gameManager;

	Dictionary<string,string> Inputs = new Dictionary<string,string>();

	private bool isRefreshed = false;
	private bool firstRead = false;

	void Start () {
        gameManager = GetComponent<HFPS_GameManager> ();
	}

	void Update ()
	{
		if (gameManager && !firstRead && gameManager.ContainsSection ("Input")) {
			Deserialize ();
			isRefreshed = true;
			firstRead = true;
		}
	}
	
	void Deserialize() {
		for (int i = 0; i < gameManager.GetKeysSectionCount("Input"); i++){
			string Key = gameManager.GetKey(i);
			string Value = gameManager.Deserialize("Input", Key);
			Inputs.Add(Key, Value);
		}
	}

	public void RefreshInputs()
	{
		Inputs.Clear ();
		Deserialize ();
		isRefreshed = true;
		StartCoroutine (RefreshWait ());
	}

	public bool GetRefreshStatus()
	{
		if (Inputs.Count > 0 && isRefreshed) {
			return true;
		} else {
			return false;
		}
	}
	
	public int InputsCount()
	{
		return Inputs.Count;
	}
	
	public string GetInput(string Key)
	{
		if (Inputs.ContainsKey (Key)) {
			return Inputs [Key];
		} else {
			if (gameManager.showDebug) {Debug.LogError ("No key with this name found");}
			return null;
		}
	}

	IEnumerator RefreshWait()
	{
		yield return new WaitForSeconds (2f);
		if (gameManager.showDebug) {Debug.Log("Refresh State = False");}
		isRefreshed = false;
	}
}