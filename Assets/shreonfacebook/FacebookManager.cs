using System.Collections;

using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
//using GameAnalyticsSDK;

public class FacebookManager : MonoBehaviour {
    public Text userIdText;
    public string message;
    public string URL;
    public string privacypolicy;

    /*
    private void Awake()
    {

        if (!FB.IsInitialized)
        {
            FB.Init();
        }
        else
        {
            FB.ActivateApp();
        }
    }*/
	/*
    public void LogIn()
    {
        FB.LogInWithReadPermissions(callback:OnLogIn);
    }

    private void OnLogIn(ILoginResult result)
    {
        if (FB.IsLoggedIn)
        {
            AccessToken token = AccessToken.CurrentAccessToken;
            userIdText.text = token.UserId;
        }
        else
            Debug.Log("Conceled Login");
    }
    public void Share()
    {
        FB.ShareLink(contentTitle: message, contentURL: new System.Uri(URL),
            contentDescription:"Here is a link to my game",
            callback:OnShare);
    }

    private void OnShare(IShareResult result)
    {
        if (result.Cancelled || !string.IsNullOrEmpty(result.Error))
        {
            Debug.Log("ShareLink errer:" + result.Error);
        }
        else if (!string.IsNullOrEmpty(result.PostId))
        {
            Debug.Log(result.PostId);
        }
        else
            Debug.Log("Share succeed");
    }
    */
    public void sharelink()
    {
        Application.OpenURL(URL);
    }
    public void Privacy()
    {
        Application.OpenURL(privacypolicy);
    }
}
