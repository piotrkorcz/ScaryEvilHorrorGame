/*
 * ConfigActionManager.cs - script is written by ThunderWire Games
 * This script reads game config
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ThunderWire.Configuration;

public class ConfigActionManager : MonoBehaviour {
	ConfigManager config = new ConfigManager();

	private List<string> ConfigKeys = new List<string> ();

	public string ConfigName = "GameConfig";
	public bool showDebug;

	private string configName;
	private bool UsePlayerPrefs;

	private bool refreshStatus = false;

    void Start()
    {
        configName = PlayerPrefs.GetString("GameConfig");
        if (PlayerPrefs.HasKey("UsePlayerPrefs"))
        {
            UsePlayerPrefs = System.Convert.ToBoolean(PlayerPrefs.GetString("UsePlayerPrefs"));
        }

        if (config.ExistFile(configName))
        {
            config.Setup(showDebug, configName);
        }
        else
        {
            config.Setup(showDebug, ConfigName);
        }

        for (int i = 0; i < config.ConfigKeysCache.Count; i++)
        {
            ConfigKeys.Add(config.ConfigKeysCache[i]);
        }
    }
		
	public string Deserialize(string Section, string Key)
	{
		return config.Deserialize (Section, Key);
	}

	public void Serialize(string Section, string Key, string Value)
	{
		config.Serialize (Section, Key, Value);
	}

	public bool ContainsSection(string Section) 
	{
		return config.ContainsSection (Section);
	}

	public bool ContainsSectionKey(string Section, string Key) {
		return config.ContainsSectionKey (Section, Key);
	}

	public bool ContainsKeyValue(string Section, string Key, string Value) {
		return config.ContainsKeyValue (Section, Key, Value);
	}

	public void RemoveSectionKey(string Section, string Key) {
		config.RemoveSectionKey (Section, Key);
	}

	public bool ExistFile(string file)
	{
		return config.ExistFile (file);
	}

	public string GetKey(int index)
	{
		return ConfigKeys [index];
	}

	public int GetKeysCount()
	{
		return config.ConfigKeysCache.Count;
	}

	public int GetKeysSectionCount(string Section)
	{
		return config.GetSectionKeys (Section);
	}

	public void Refresh()
	{
		StartCoroutine (WaitRefresh ());
	}

	IEnumerator WaitRefresh()
	{
		refreshStatus = true;
		yield return new WaitForSeconds (1f);
		refreshStatus = false;
	}

	public bool GetRefreshStatus()
	{
		return refreshStatus;
	}
}
