using System;
using UnityEngine;
using GoogleMobileAds.Api;



public class GoogleAdsManager : MonoBehaviour
{
    // Toggle this in the Inspector to decide whether to use test ads or real ads.
    public bool TestAds = false;

    // References to the BannerView and its Ad Unit ID
    private BannerView _bannerView;
    private string _bannerId;
    private InterstitialAd _interstitialAd;
    private string _interstitialId;
    private RewardedAd rewardedAd;
    private string _rewardedId;

#if UNITY_ANDROID
    private string _testBannerId = "ca-app-pub-3940256099942544/6300978111"; // Test Banner ID (Android)
    public string realBannerId = "ca-app-pub-4033162398895930/9682119757";  // Replace with your real Ad Unit ID

    
    private string _testInterstitialId = "ca-app-pub-3940256099942544/1033173712";
    public string realInterstitialId = "ca-app-pub-4033162398895930/4573263308";

    private string _testRewardedID = "ca-app-pub-3940256099942544/5224354917";
    public string realRewardedID = "ca-app-pub-4033162398895930/3260181637";

#elif UNITY_IPHONE
    private string _testBannerId = "ca-app-pub-3940256099942544/2934735716"; // Test Banner ID (iOS)
    public string realBannerId = "ca-app-pub-4033162398895930/1166201458";   // Replace with your real Ad Unit ID

    private string _testInterstitialId = "ca-app-pub-3940256099942544/1033173712";
    public string realInterstitialId = "ca-app-pub-4033162398895930/3104640096";

    private string _testRewardedID = "ca-app-pub-3940256099942544/5224354917";
    public string realRewardedID = "ca-app-pub-4033162398895930~3001456484";
#else
    private string realBannerId = "unused";
#endif

    // Singleton reference
    public static GoogleAdsManager instance { get; private set; }

    private void Awake()
    {
        // Ensure only one instance of AdManager exists.
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        // Decide which banner ID to use BEFORE initializing or requesting ads
        _bannerId = TestAds ? _testBannerId : realBannerId;
        _interstitialId = TestAds ? _testInterstitialId : realInterstitialId;

        // Initialize the Google Mobile Ads SDK
        MobileAds.Initialize(initStatus =>
        {
            Debug.Log("Mobile Ads SDK initialized.");
            RequestBannerAd();
        });
    }

    #region Banner Ad

    public void RequestBannerAd()
    {
        // If a banner already exists, destroy it before creating a new one.
        if (_bannerView != null)
        {
            _bannerView.Destroy();
            _bannerView = null;
        }

        // Create a 320x50 banner at the top of the screen.
        _bannerView = new BannerView(_bannerId, AdSize.Banner, AdPosition.Top);

        // (Optional) Register for banner ad events to help with debugging & analytics.
        ListenToBannerAdEvents();

        // Create an AdRequest (Builder pattern might be deprecated; default constructor is fine).
        AdRequest request = new AdRequest();

        // Load the banner with the request.
        _bannerView.LoadAd(request);

        Debug.Log($"Banner ad requested. Using Ad Unit ID: {_bannerId}");
    }

    private void ListenToBannerAdEvents()
    {
        _bannerView.OnBannerAdLoaded += () =>
        {
            Debug.Log("Banner loaded successfully.");
        };

        _bannerView.OnBannerAdLoadFailed += (LoadAdError error) =>
        {
            Debug.LogError("Banner failed to load: " + error);
        };

        _bannerView.OnAdPaid += (AdValue adValue) =>
        {
            Debug.Log($"Banner paid {adValue.Value} {adValue.CurrencyCode}");
        };

        _bannerView.OnAdImpressionRecorded += () =>
        {
            Debug.Log("Banner impression recorded.");
        };

        _bannerView.OnAdClicked += () =>
        {
            Debug.Log("Banner ad clicked.");
        };

        _bannerView.OnAdFullScreenContentOpened += () =>
        {
            Debug.Log("Banner opened full screen content.");
        };

        _bannerView.OnAdFullScreenContentClosed += () =>
        {
            Debug.Log("Banner closed full screen content.");
        };
    }

    /// <summary>
    /// Show the banner if it's loaded.
    /// </summary>
    public void ShowBannerView()
    {
        if (_bannerView != null)
        {
            _bannerView.Show();
            Debug.Log("Showing banner.");
        }
        else
        {
            Debug.LogWarning("No banner to show. Requesting a new banner...");
            RequestBannerAd();
        }
    }

    /// <summary>
    /// Hide the banner if it's currently shown.
    /// </summary>
    public void HideBannerAd()
    {
        if (_bannerView != null)
        {
            _bannerView.Hide();
            Debug.Log("Hiding banner.");
        }
        else
        {
            Debug.LogWarning("No banner to hide.");
        }
    }

    
    private void OnDestroy()
    {
        if (instance == this && _bannerView != null)
        {
            Debug.Log("Destroying banner from AdManager singleton.");
            _bannerView.Destroy();
            _bannerView = null;
        }
    }

    #endregion



    #region INTERSTITIAL

    // STEP 1: Request interstitial ad loading
    public void LoadInterstitialAd()
    {
        Debug.Log("Loading Interstitial Ad...");

        // Clear out an old ad, if any
        if (_interstitialAd != null)
        {
            _interstitialAd.Destroy();
            _interstitialAd = null;
        }

        // Create an AdRequest
        AdRequest request = new AdRequest();

        // Now use the new static Load method
        InterstitialAd.Load(_interstitialId, request,
            (InterstitialAd ad, LoadAdError loadError) =>
            {
                if (loadError != null && loadError.GetMessage() != null)
                {
                    // Failed to load
                    Debug.LogError($"Interstitial Load Failed: {loadError.GetMessage()}");
                    _interstitialAd = null;
                    return;
                }
                else
                {
                    // Save the loaded ad
                    _interstitialAd = ad;
                    Debug.Log("Interstitial Loaded!");

                    // Optional: Register for ad events
                    RegisterInterstitialCallbacks(ad);
                }
            });
    }

    // STEP 2: Show interstitial ad if loaded
    public void ShowInterstitialAd()
    {
        if (_interstitialAd != null)
        {
            Debug.Log("Showing Interstitial Ad...");
            _interstitialAd.Show();
            // After showing, the ad is consumed and can't be shown again.
            // You can call LoadInterstitialAd() again in the OnAdFullScreenContentClosed callback.
        }
        else
        {
            Debug.LogWarning("Interstitial ad not ready. Loading another one...");
            LoadInterstitialAd();
        }
    }

    private void RegisterInterstitialCallbacks(InterstitialAd ad)
    {
        // Called when the ad references full screen content.
        ad.OnAdFullScreenContentOpened += () =>
        {
            Debug.Log("Interstitial full screen content opened.");
        };
        ad.OnAdFullScreenContentClosed += () =>
        {
            Debug.Log("Interstitial full screen content closed.");
            // Often you’ll load a new ad here
            LoadInterstitialAd();
        };
        ad.OnAdImpressionRecorded += () =>
        {
            Debug.Log("Interstitial impression recorded.");
        };
        ad.OnAdClicked += () =>
        {
            Debug.Log("Interstitial was clicked.");
        };
        ad.OnAdFullScreenContentFailed += (AdError error) =>
        {
            Debug.LogError("Interstitial failed to show full screen content: " + error.GetMessage());
        };
        ad.OnAdPaid += (AdValue adValue) =>
        {
            Debug.Log($"Interstitial Ad Paid: {adValue.Value} {adValue.CurrencyCode}");
        };
    }

    #endregion


    #region Rewarded Ad

    // Call this from your GameManager.Load_RAD() method
    public void LoadRewardedAd()
    {
        Debug.Log("Loading Rewarded Ad...");

        // Destroy old references if any
        if (rewardedAd != null)
        {
            rewardedAd.Destroy();
            rewardedAd = null;
        }

        // Construct an AdRequest. (No .Build() needed in newer SDK)
        AdRequest request = new AdRequest();

        // Load the RewardedAd using the static Load() method
        // Replace "YOUR_REWARDED_AD_UNIT_ID" with your actual AdMob ID
        RewardedAd.Load("YOUR_REWARDED_AD_UNIT_ID", request, (RewardedAd ad, LoadAdError loadError) =>
        {
            if (loadError != null && loadError.GetMessage() != null)
            {
                Debug.LogError($"RewardedAd Load Failed: {loadError.GetMessage()}");
                rewardedAd = null;
                return;
            }

            // Ad loaded successfully
            rewardedAd = ad;
            Debug.Log("Rewarded Ad loaded successfully!");

            // (Optional) Register for rewarded ad events
            RegisterRewardedAdCallbacks(ad);
        });
    }

    // Call this from your GameManager.Show_RAD(RewardType _type, int _amount) method
    public void ShowRewardedAd()
    {
        if (rewardedAd != null)
        {
            // Show and pass a callback that fires when the user should be granted a reward.
            rewardedAd.Show((Reward reward) =>
            {
                Debug.Log($"User earned reward: {reward.Amount} {reward.Type}");
                // 
                // TODO: Grant the reward to the player here. For example:
                // if (rewardType == RewardType.COIN)
                // {
                //     Toolbox.Prefs.coins += rewardAmount;
                //     Toolbox.UiManager.topBarListner.UpdateCoin();
                // }
            });
        }
        else
        {
            Debug.LogWarning("Rewarded Ad not ready. Loading another one...");
            LoadRewardedAd();
        }
    }

    /// <summary>
    /// Registers optional callbacks (for debugging or analytics).
    /// </summary>
    private void RegisterRewardedAdCallbacks(RewardedAd ad)
    {
        ad.OnAdFullScreenContentOpened += () =>
        {
            Debug.Log("Rewarded Ad full screen content opened.");
        };
        ad.OnAdFullScreenContentClosed += () =>
        {
            Debug.Log("Rewarded Ad full screen content closed.");
            // Often you'll want to load another ad here so it's ready for next time:
            LoadRewardedAd();
        };
        ad.OnAdImpressionRecorded += () =>
        {
            Debug.Log("Rewarded Ad impression recorded.");
        };
        ad.OnAdClicked += () =>
        {
            Debug.Log("Rewarded Ad clicked.");
        };
        ad.OnAdFullScreenContentFailed += (AdError error) =>
        {
            Debug.LogError($"Rewarded Ad failed to show: {error.GetMessage()}");
        };
        ad.OnAdPaid += (AdValue adValue) =>
        {
            Debug.Log($"Rewarded Ad paid {adValue.Value} {adValue.CurrencyCode}");
        };
    }

    #endregion

}
