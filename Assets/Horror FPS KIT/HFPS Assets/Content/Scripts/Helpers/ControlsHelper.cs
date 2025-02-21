﻿using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ThunderWire.Parser;

[ExecuteInEditMode]
public class ControlsHelper : MonoBehaviour {

    public List<InputItem> InputsList = new List<InputItem>();

    private List<GameObject> InputsCache = new List<GameObject>();
    private ParsePrimitives parser = new ParsePrimitives();


    void Start () {
        if (!(InputsList.Count >= transform.childCount))
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                InputsCache.Add(transform.GetChild(i).gameObject);
            }

            for (int i = 0; i < InputsCache.Count; i++)
            {
                KeyCode key = KeyCode.None;
                try
                {
                    key = parser.ParseType<KeyCode>(InputsCache[i].transform.GetChild(1).GetChild(0).GetComponent<Text>().text);
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }

                InputsList.Add(new InputItem(InputsCache[i].name, key, InputsCache[i].transform.GetChild(1).gameObject));
            }
        }
    }
}

[Serializable]
public class InputItem
{
    public string Input;
    public KeyCode DefaultKey;
    public GameObject InputButton;

    public InputItem(string input, KeyCode key, GameObject btn)
    {
        Input = input;
        DefaultKey = key;
        InputButton = btn;
    }
}
