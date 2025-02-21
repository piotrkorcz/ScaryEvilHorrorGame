using UnityEngine;

using System.Collections;

using UnityEngine.Advertisements;




public class AdsUnity : MonoBehaviour, IUnityAdsInitializationListener
{

    string gameID = "3807597";
    public bool testMode = false;
    //Show an inter no more than every 60s
    public float time = 60f;

    void Start()
    {

        Advertisement.Initialize(gameID, testMode, this);
        //StartCoroutine(ShowBannerWhenInitialized());

    }

    public void OnInitializationComplete()
    {
        Debug.Log("Unity Ads initialization complete.");
        //load ads
        Advertisement.Banner.SetPosition(BannerPosition.TOP_CENTER);
        // Set up options to notify the SDK of load events:
        BannerLoadOptions options = new BannerLoadOptions
        {
            loadCallback = OnBannerLoaded,
            errorCallback = OnBannerError
        };

        // Load the Ad Unit with banner content:
        Advertisement.Banner.Load("bannerPlacement", options);
        Advertisement.Load("interstitialPlacement");
        Advertisement.Load("rewardedVideo");
    }

    public void OnInitializationFailed(UnityAdsInitializationError error, string message)
    {
        Debug.Log($"Unity Ads Initialization Failed: {error.ToString()} - {message}");
    }

    //BANNER

    void OnBannerLoaded()
    {
        Debug.Log("Banner loaded");
        // Set up options to notify the SDK of show events:
        BannerOptions options = new BannerOptions
        {
            clickCallback = OnBannerClicked,
            hideCallback = OnBannerHidden,
            showCallback = OnBannerShown
        };

        // Show the loaded Banner Ad Unit:
        Advertisement.Banner.Show("bannerPlacement", options);
    }

    void OnBannerError(string message)
    {
        Debug.Log($"Banner Error: {message}");
        // Optionally execute additional code, such as attempting to load another ad.
    }

    void OnBannerClicked() { }
    void OnBannerShown() { }
    void OnBannerHidden() { }

    
    //INTER and REWARDED

    void Update()
    {
        if (time >= 0)
        {
            time -= Time.deltaTime;
        }

    }
    public void ShowRewardedAd()
    {

        Advertisement.Show("rewardedVideo");
        //if (Advertisement.IsReady("rewardedVideo"))
        //{

        //    var options = new ShowOptions { resultCallback = HandleShowResult };
        //    Advertisement.Show("rewardedVideo", options);
        //}
    }



    //private void HandleShowResult(ShowResult result)
    //{
    //    switch (result)
    //    {
    //        case ShowResult.Finished:
    //            Debug.Log("The ad was successfully shown.");

    //            // YOUR CODE TO REWARD THE GAMER
    //            // Give coins etc.
    //            break;
    //        case ShowResult.Skipped:
    //            Debug.Log("The ad was skipped before reaching the end.");

    //            break;
    //        case ShowResult.Failed:
    //            Debug.LogError("The ad failed to be shown.");
    //            break;

    //    }
    //}

    public void ShowInterstitialAd()
    {

        Advertisement.Show("interstitialPlacement");
        // Check if UnityAds ready before calling Show method:
        //if (Advertisement.IsReady("interstitialPlacement"))
        //{
        //    Advertisement.Show("interstitialPlacement");
        //}
        //else
        //{
        //    Debug.Log("Interstitial ad not ready at the moment! Please try again later!");
        //}
    }

    //banner
    //IEnumerator ShowBannerWhenInitialized()
    //{

    //    //while (!Advertisement.isInitialized)
    //    //{
    //    //    yield return new WaitForSeconds(0.5f);
    //    //}
    //    //Advertisement.Banner.SetPosition(BannerPosition.TOP_CENTER);
    //    //Advertisement.Banner.Show("bannerPlacement");
    //}



    public void showinterUnity()
    {
        if (time <= 0)
        {
            ShowInterstitialAd();
            time = 60f;
            Debug.Log("showinte");
        }


    }


}