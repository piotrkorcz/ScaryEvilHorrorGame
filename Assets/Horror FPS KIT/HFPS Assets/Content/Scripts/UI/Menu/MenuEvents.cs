﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuEvents : MonoBehaviour {

    private Color TxtNormalColor;
    private Color BtnNormalColor;

    [Header("Text Colors")]
    public Color HoverColor = Color.white;
    public Color PressedColor = Color.white;
    public Color HoldColor = Color.white;

    [Header("Button Colors")]
    public Color ButtonHover = Color.white;
    public Color ButtonPressed = Color.white;
    public Color ButtonHold = Color.white;

    [Header("Button Hold")]
    public bool isPressed;

    private bool hover;
    private bool holdColor;
    private bool pressed;

    void Start()
    {
        if (transform.childCount > 0 && transform.GetChild(0).GetComponent<Text>())
        {
            TxtNormalColor = transform.GetChild(0).GetComponent<Text>().color;
        }

        if (GetComponent<Image>())
        {
            BtnNormalColor = GetComponent<Image>().color;
        }

        if (isPressed)
        {
            ButtonHoldEvent(true);
        }
    }

    public void ButtonHoverEvent()
    {
        GetComponent<Image>().color = ButtonHover;
        transform.GetChild(0).GetComponent<Text>().color = HoverColor;
        hover = true;
    }

    public void ButtonPressedEvent()
    {
        GetComponent<Image>().color = ButtonPressed;
        transform.GetChild(0).GetComponent<Text>().color = PressedColor;
        pressed = true;
    }

    public void ButtonNormalEvent()
    {
        if (hover && !pressed)
        {
            GetComponent<Image>().color = BtnNormalColor;
            transform.GetChild(0).GetComponent<Text>().color = TxtNormalColor;
            hover = false;
        }
    }

    public void ButtonHoldEvent(bool Hold)
    {
        if (Hold)
        {
            pressed = true;
            hover = false;
            GetComponent<Image>().color = ButtonHold;
            transform.GetChild(0).GetComponent<Text>().color = HoldColor;
        }
        else
        {
            pressed = false;
            hover = false;
            GetComponent<Image>().color = BtnNormalColor;
            transform.GetChild(0).GetComponent<Text>().color = TxtNormalColor;
        }
    }

    public void ChangeTextColor(string color)
	{
        if (!holdColor)
        {
            Color col = Color.clear;
            ColorUtility.TryParseHtmlString(color, out col);
            GetComponent<Text>().color = col;
        }
	}

	public void ChangeImageColor(string color)
	{
		Color col = Color.clear;
		ColorUtility.TryParseHtmlString(color, out col);
		GetComponent<Image> ().color = col;
	}
}
