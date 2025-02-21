/*
 * UIOptions.cs - script is written by ThunderWire Games
 * This script control In Game Options
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class QualityLevels
{
    public string QualityName;

    public enum antialiasing { Disabled, m_2x, m_4x, m_8x }
    public antialiasing Antialiasing = antialiasing.Disabled;

    public enum anisotropic { Disabled, PerTexture, ForcedOn }
    public anisotropic Anisotropic = anisotropic.Disabled;

    public enum textureQuality { FullRes, HalfRes, QuarterRes, EightRes }
    public textureQuality TextureQuality = textureQuality.EightRes;

    public enum blendWeight { m_1Bone, m_2Bones, m_4Bones }
    public blendWeight BlendWeight = blendWeight.m_1Bone;

    public enum vSync { DontSync, EveryVBank, EverySecondVBank }
    public vSync VSync = vSync.DontSync;

    public enum shadowResolution { Low, Medium, High, VeryHigh }
    public shadowResolution ShadowResolution = shadowResolution.Low;

    public enum shadowCascades { NoCascades, TwoCascades, FourCascades }
    public shadowCascades ShadowCascades = shadowCascades.NoCascades;

    public enum shadowProjecion { CloseFit, StableFit }
    public shadowProjecion ShadowProjecion = shadowProjecion.CloseFit;

    [Range(0, 150)]
    public int ShadowDistance;
}

public class UIOptions : MonoBehaviour {

	private ConfigActionManager configActions;
    private HFPS_GameManager gameManager;
    private InputController InputControl;

    [Header("Main")]
    public List<QualityLevels> qualityLevels = new List<QualityLevels>();
    public bool isMainMenu;

    [Header("Objects")]
    public GameObject OptionsTab;
    public GameObject DuplicateControl;
    public Button ApplyButton;
    public Button BackButton;

    [Header("General")]
	public Slider volumeSlider;
	public Slider sensitivitySlider;

	[Header("Advanced Graphic")]
	public Dropdown resolution;
	public Dropdown quality;
	public Dropdown antialiasing;
	public Dropdown anisotropic;
	public Dropdown textureQuality;
	public Dropdown blendWeight;
	public Dropdown vSync;
	public Dropdown shadowResolution;
	public Dropdown shadowCascades;
	public Dropdown shadowProjecion;
	public Slider shadowDistance;
	public Toggle fullscreen;
	public Text shadowDistanceText;

    private int qualityLevel = -1;
    private int defaultQuality;

    private int resolutionLevel = -1;
    private int antiaLevel = -1;
    private int anisoLevel = -1;
    private int textureQualityLevel = -1;
    private int blendWeightLevel = -1;
    private int vSyncLevel = -1;
    private int shadowResLevel = -1;
    private int shadowCasLevel = -1;
    private int shadowProjLevel = -1;

    public bool setQuality;
    private bool m_quality = false;
    private bool m_resolution = false;
    private bool m_fullscreen = false;
    private bool m_shadowDistanceText = false;

    private bool m_antialiasing = false;
    private bool m_anisotropic = false;
    private bool m_textureQuality = false;
    private bool m_blendWeight = false;
    private bool m_vSync = false;
    private bool m_shadowResolution = false;
    private bool m_shadowCascades = false;
    private bool m_shadowProjecion = false;

    private bool sectionExist;
    private bool updateByConfig = true;
    private bool firstLoad = true;

    [HideInInspector] public string RewriteKeycode;

    Resolution[] resolutions;

    void Start()
	{
        resolutions = Screen.resolutions;
        resolution.options.Clear();

        updateByConfig = true;
        firstLoad = true;
        GameObject gameManagerGO = null;

        if (GameObject.Find("GAMEMANAGER"))
        {
            gameManagerGO = GameObject.Find("GAMEMANAGER");
        }
        else
        {
            gameManagerGO = GameObject.Find("MENUMANAGER");
        }
        if (gameManagerGO)
        {
            InputControl = gameManagerGO.GetComponent<InputController>();
            if (gameManagerGO.GetComponent<ConfigActionManager>())
            {
                configActions = gameManagerGO.GetComponent<ConfigActionManager>();
                gameManager = null;
            }
            else if (gameManagerGO.GetComponent<HFPS_GameManager>())
            {
                gameManager = gameManagerGO.GetComponent<HFPS_GameManager>();
                configActions = null;
            }
        }else{
            Debug.LogError("Cannot find GAMEMANAGER GameObject!");
        }

        GetMonitorResolutions();
        SetDropdownListeners();
        UpdateSettings ();
	}

    void Update()
    {
        shadowDistanceText.text = shadowDistance.value.ToString();

        if (!m_quality && !firstLoad)
        {
            if (m_anisotropic || m_antialiasing || m_blendWeight || m_shadowCascades || m_shadowProjecion || m_textureQuality || m_vSync)
            {
                quality.value = 3;
                qualityLevel = 3;
            }
        }
    }

    private void GetMonitorResolutions()
    {
        for(int i = resolutions.Length - 1; i >= 0; i--) {
            AddResolution(string.Format("{0}x{1}", resolutions[i].width, resolutions[i].height));
        }
        resolution.value = GetCurrentResolution();
    }

    private void AddResolution(string Resolution)
    {
        if(!resolution.options.Contains(new Dropdown.OptionData(Resolution)))
        {
            resolution.options.Add(new Dropdown.OptionData(Resolution));
            resolution.RefreshShownValue();
        }
    }

    private void SetDropdownListeners()
    {
        resolution.onValueChanged.AddListener(delegate { OnResolutionChange(resolution); });
        quality.onValueChanged.AddListener(delegate { OnQualityChange(quality); });
        antialiasing.onValueChanged.AddListener(delegate { OnAntialiasingChange(antialiasing); });
        anisotropic.onValueChanged.AddListener(delegate { OnAnisotropicChange(anisotropic); });
        textureQuality.onValueChanged.AddListener(delegate { OnTextureQualityChange(textureQuality); });
        blendWeight.onValueChanged.AddListener(delegate { OnBlendWeightChange(blendWeight); });
        vSync.onValueChanged.AddListener(delegate { OnVSyncChange(vSync); });
        shadowResolution.onValueChanged.AddListener(delegate { OnShadowResolutionChange(shadowResolution); });
        shadowCascades.onValueChanged.AddListener(delegate { OnShadowCascadesChange(shadowCascades); });
        shadowProjecion.onValueChanged.AddListener(delegate { OnShadowProjectionChange(shadowProjecion); });
        fullscreen.onValueChanged.AddListener(delegate { OnFullscreenChange(fullscreen); });
    }

	void UpdateSettings()
	{
        if (isMainMenu)
        {
            if (configActions)
            {
                if (updateByConfig)
                {
                    if (configActions.ContainsSection("Graphic"))
                    {
                        Debug.Log("Contains");
                        DeserializeByConfigAction();
                        updateByConfig = false;
                        sectionExist = true;
                    }
                    else
                    {
                        Debug.Log("Contains false");
                        updateByConfig = false;
                    }
                }
            }

            if (gameManager)
            {
                if (updateByConfig)
                {
                    if (gameManager.ContainsSection("Graphic"))
                    {
                        DeserializeByGameManager();
                        updateByConfig = false;
                        sectionExist = true;
                    }
                    else
                    {
                        updateByConfig = false;
                    }
                }
            }
        }
        else
        {
            sectionExist = false;
        }

        if (!updateByConfig && !sectionExist)
        {
            if (quality.value != 3)
            {
                defaultQuality = QualitySettings.GetQualityLevel();
                quality.value = defaultQuality;
            }

            resolution.value = GetCurrentResolution();
            antialiasing.value = DDAntialiasingLevel(QualitySettings.antiAliasing);
            shadowCascades.value = DDSCascadesLevel(QualitySettings.shadowCascades);
            anisotropic.value = (int)QualitySettings.anisotropicFiltering;
            textureQuality.value = QualitySettings.globalTextureMipmapLimit;
            blendWeight.value = (int)QualitySettings.skinWeights;
            vSync.value = QualitySettings.vSyncCount;
            shadowResolution.value = (int)QualitySettings.shadowResolution;
            shadowProjecion.value = (int)QualitySettings.shadowProjection;
            shadowDistance.value = QualitySettings.shadowDistance;
            fullscreen.isOn = ControlFreak2.CFScreen.fullScreen;
        }

        if (configActions)
        {
            if (configActions.ContainsSection("Game"))
            {
                volumeSlider.value = float.Parse(configActions.Deserialize("Game", "Volume"));
                sensitivitySlider.value = float.Parse(configActions.Deserialize("Game", "Sensitivity"));
            }
        }
        else if (gameManager)
        {
            if (gameManager.ContainsSection("Game"))
            {
                volumeSlider.value = float.Parse(gameManager.Deserialize("Game", "Volume"));
                sensitivitySlider.value = float.Parse(gameManager.Deserialize("Game", "Sensitivity"));
            }
        }

        StartCoroutine(WaitForActivation());
    }

    IEnumerator WaitForActivation()
    {
        yield return new WaitUntil(() => OptionsTab.activeSelf == true);
        OnOptionsEnabled();
    }

    private void OnOptionsEnabled()
    {
        m_antialiasing = false;
        m_anisotropic = false;
        m_textureQuality = false;
        m_blendWeight = false;
        m_vSync = false;
        m_shadowResolution = false;
        m_shadowCascades = false;
        m_shadowProjecion = false;
        m_quality = false;
        m_fullscreen = false;
        firstLoad = false;
    }

    int GetCurrentResolution()
    {
        for(int i = 0; i < resolution.options.Count; i++)
        {
            string[] ress = resolution.options[i].text.Split(new char[] { 'x' });
            if (Screen.width.ToString() == ress[0] && Screen.height.ToString() == ress[1])
            {
                return i;
            }
        }
        return 0;
    }

    int DDAntialiasingLevel(int Level)
    {
        switch (Level)
        {
            case 0:
                return 0;
            case 2:
                return 1;
            case 4:
                return 2;
            case 8:
                return 3;
        }
        return 0;
    }

    int DDSCascadesLevel(int Level)
    {
        switch (Level)
        {
            case 0:
                return 0;
            case 2:
                return 1;
            case 4:
                return 2;
        }
        return 0;
    }

    public void OnFullscreenChange(Toggle tg)
    {
        m_fullscreen = true;
    }

    public void OnResolutionChange(Dropdown dd)
	{
        if (resolutionLevel != -1)
        {
            m_resolution = true;
        }
        resolutionLevel = dd.value;
    }

    public void OnQualityClick()
    {
        setQuality = true;
    }

    public void OnQualityChange(Dropdown dd)
	{
        if (qualityLevel != -1 && dd.value != 3)
        {
            m_quality = true;
        }

        if(dd.options[dd.value].text != "Custom") {
            defaultQuality = dd.value;
        }

        SetGraphicSettings(dd.value);

        qualityLevel = dd.value;    
	}

	public void OnAntialiasingChange(Dropdown dd)
	{
        if (antiaLevel != -1)
        {
            m_antialiasing = true;
        }
        switch (dd.value) {
		case 0:
			antiaLevel = 0;
			break;
		case 1:
			antiaLevel = 2;
			break;
		case 2:
			antiaLevel = 4;
			break;
		case 3:
			antiaLevel = 8;
			break;
		}
    }

    public void OnAntialiasingChange(int value)
    {
        switch (value)
        {
            case 0:
                antiaLevel = 0;
                break;
            case 1:
                antiaLevel = 2;
                break;
            case 2:
                antiaLevel = 4;
                break;
            case 3:
                antiaLevel = 8;
                break;
        }
        m_antialiasing = true;
    }

    public void OnAnisotropicChange(Dropdown dd)
	{
        if (anisoLevel != -1)
        {
            m_anisotropic = true;
        }
        anisoLevel = dd.value;
    }

	public void OnTextureQualityChange(Dropdown dd)
	{
        if (textureQualityLevel != -1)
        {
            m_textureQuality = true;
        }
        textureQualityLevel = dd.value;        
    }

	public void OnBlendWeightChange(Dropdown dd)
	{
        if (blendWeightLevel != -1)
        {
            m_blendWeight = true;
        }
        blendWeightLevel = dd.value;
    }

	public void OnVSyncChange(Dropdown dd)
	{
        if (vSyncLevel != -1)
        {
            m_vSync = true;
        }
        vSyncLevel = dd.value;
    }

	public void OnShadowResolutionChange(Dropdown dd)
	{
        if (shadowResLevel != -1)
        {
            m_shadowResolution = true;
        }
        shadowResLevel = dd.value;
    }

	public void OnShadowCascadesChange(Dropdown dd)
	{
        if (shadowCasLevel != -1)
        {
            m_shadowCascades = true;
        }
        switch (dd.value) {
		case 0:
			shadowCasLevel = 0;
			break;
		case 1:
			shadowCasLevel = 2;
			break;
		case 2:
			shadowCasLevel = 4;
			break;
		}
    }

    public void OnShadowCascadesChange(int value)
    {
        switch (value)
        {
            case 0:
                shadowCasLevel = 0;
                break;
            case 1:
                shadowCasLevel = 2;
                break;
            case 2:
                shadowCasLevel = 4;
                break;
        }
    }

    public void OnShadowProjectionChange(Dropdown dd)
	{
        if (shadowProjLevel != -1)
        {
            m_shadowProjecion = true;
        }
        shadowProjLevel = dd.value;
    }

	void OnDestroy()
	{
		resolution.onValueChanged.RemoveAllListeners();
		quality.onValueChanged.RemoveAllListeners();
		antialiasing.onValueChanged.RemoveAllListeners();
		anisotropic.onValueChanged.RemoveAllListeners();
		textureQuality.onValueChanged.RemoveAllListeners();
		blendWeight.onValueChanged.RemoveAllListeners();
		vSync.onValueChanged.RemoveAllListeners();
		shadowResolution.onValueChanged.RemoveAllListeners();
		shadowCascades.onValueChanged.RemoveAllListeners();
		shadowProjecion.onValueChanged.RemoveAllListeners();
	}

	public void ApplyAllSettings () {
        ApplyGeneral();
        ApplyGraphic();
        ApplyControls();
	}

    public void Rewrite()
    {
        InputControl.Rewrite(RewriteKeycode);
        DuplicateControl.SetActive(false);
        ApplyButton.interactable = true;
        BackButton.interactable = true;
    }

    public void BackRewrite()
    {
        InputControl.BackRewrite();
        DuplicateControl.SetActive(false);
        ApplyButton.interactable = true;
        BackButton.interactable = true;
    }

    private void ApplyControls()
    {
        if (gameManager)
        {
            gameManager.gameObject.GetComponent<InputManager>().RefreshInputs();
        }
    }

    private void ApplyGeneral()
    {
        if (configActions)
        {
            configActions.Serialize("Game", "Volume", volumeSlider.value.ToString());
            configActions.Serialize("Game", "Sensitivity", sensitivitySlider.value.ToString());
            configActions.Refresh();
        }
        else if (gameManager)
        {
            gameManager.Serialize("Game", "Volume", volumeSlider.value.ToString());
            gameManager.Serialize("Game", "Sensitivity", sensitivitySlider.value.ToString());
            gameManager.Refresh();
        }
    }

    private void ApplyGraphic()
    {
        if (m_fullscreen && !m_resolution)
        {
            Debug.Log("fullscreen Set");
            ControlFreak2.CFScreen.fullScreen = fullscreen.isOn;
            m_fullscreen = false;
        }

        if (m_resolution)
        {
            Debug.Log("resolution Set");
            Resolution res = resolutions[resolutionLevel];
            ControlFreak2.CFScreen.SetResolution(res.width, res.height, fullscreen.isOn);
            m_resolution = false;
        }

        if (setQuality && m_quality)
        {
            QualitySettings.SetQualityLevel(qualityLevel);

            m_antialiasing = false;
            m_anisotropic = false;
            m_textureQuality = false;
            m_blendWeight = false;
            m_vSync = false;
            m_shadowResolution = false;
            m_shadowProjecion = false;
            m_shadowCascades = false;
            m_quality = false;
            setQuality = false;
        }
        else if(!m_quality)
        {
            if (m_anisotropic)
            {
                switch (anisoLevel)
                {
                    case 0:
                        QualitySettings.anisotropicFiltering = AnisotropicFiltering.ForceEnable;
                        break;
                    case 1:
                        QualitySettings.anisotropicFiltering = AnisotropicFiltering.Enable;
                        break;
                    case 2:
                        QualitySettings.anisotropicFiltering = AnisotropicFiltering.Disable;
                        break;
                }
                m_anisotropic = false;
            }
            if (m_blendWeight)
            {
                switch (blendWeightLevel)
                {
                    case 0:
                        QualitySettings.skinWeights = SkinWeights.OneBone;
                        break;
                    case 1:
                        QualitySettings.skinWeights = SkinWeights.TwoBones;
                        break;
                    case 2:
                        QualitySettings.skinWeights = SkinWeights.FourBones;
                        break;
                }
                m_blendWeight = false;
            }
            if (m_shadowResolution)
            {
                switch (shadowResLevel)
                {
                    case 0:
                        QualitySettings.shadowResolution = ShadowResolution.Low;
                        break;
                    case 1:
                        QualitySettings.shadowResolution = ShadowResolution.Medium;
                        break;
                    case 2:
                        QualitySettings.shadowResolution = ShadowResolution.High;
                        break;
                    case 3:
                        QualitySettings.shadowResolution = ShadowResolution.VeryHigh;
                        break;
                }
                m_shadowResolution = false;
            }
            if (m_shadowProjecion)
            {
                switch (shadowProjLevel)
                {
                    case 0:
                        QualitySettings.shadowProjection = ShadowProjection.CloseFit;
                        break;
                    case 1:
                        QualitySettings.shadowProjection = ShadowProjection.StableFit;
                        break;
                }
                m_shadowProjecion = false;
            }
            if (m_antialiasing)
            {
                QualitySettings.antiAliasing = antiaLevel;
                m_antialiasing = false;
            }
            if (m_textureQuality)
            {
                QualitySettings.globalTextureMipmapLimit = textureQualityLevel;
                m_textureQuality = false;
            }
            if (m_vSync)
            {
                QualitySettings.vSyncCount = vSyncLevel;
                m_vSync = false;
            }
            if (m_shadowCascades)
            {
                QualitySettings.shadowCascades = shadowCasLevel;
                m_shadowCascades = false;
            }

            QualitySettings.shadowDistance = shadowDistance.value;
        }

        if (configActions)
        {
            SerializeByConfigAction();
        }
        else if (gameManager)
        {
            SerializeByGameManager();
        }
    }

    public void SetGraphicSettings(int GraphicLevel)
    {
        string defaultQualityName = quality.options[GraphicLevel].text;
        foreach(var q in qualityLevels)
        {
            if(q.QualityName == defaultQualityName)
            {
                qualityLevel = GraphicLevel;
                antialiasing.value = (int)q.Antialiasing;
                anisotropic.value = (int)q.Anisotropic;
                textureQuality.value = (int)q.TextureQuality;
                blendWeight.value = (int)q.BlendWeight;
                vSync.value = (int)q.VSync;
                shadowResolution.value = (int)q.ShadowResolution;
                shadowCascades.value = (int)q.ShadowCascades;
                shadowProjecion.value = (int)q.ShadowProjecion;
                shadowDistance.value = q.ShadowDistance;
            }
        }
    }

    private void SerializeByConfigAction()
    {
        configActions.Serialize("Graphic", "GraphicQuality", qualityLevel.ToString());
        configActions.Serialize("Graphic", "Antialiasing", antiaLevel.ToString());
        configActions.Serialize("Graphic", "Anisotropic", anisoLevel.ToString());
        configActions.Serialize("Graphic", "TextureQuality", textureQualityLevel.ToString());
        configActions.Serialize("Graphic", "BlendWeight", blendWeightLevel.ToString());
        configActions.Serialize("Graphic", "VSync", vSyncLevel.ToString());
        configActions.Serialize("Graphic", "ShadowResolution", shadowResLevel.ToString());
        configActions.Serialize("Graphic", "ShadowCascades", shadowCasLevel.ToString());
        configActions.Serialize("Graphic", "ShadowProjecion", shadowProjLevel.ToString());
        configActions.Serialize("Graphic", "ShadowDistance", shadowDistance.value.ToString());
    }

    private void DeserializeByConfigAction()
    {
        int q = int.Parse(configActions.Deserialize("Graphic", "GraphicQuality"));
        if (q != 3)
        {
            quality.value = q;
            defaultQuality = q;
            SetGraphicSettings(q);
            Debug.Log("Set Graphic Quality");
        }
        else
        {
            quality.value = 3;
            antialiasing.value = int.Parse(configActions.Deserialize("Graphic", "Antialiasing"));
            anisotropic.value = int.Parse(configActions.Deserialize("Graphic", "Anisotropic"));
            textureQuality.value = int.Parse(configActions.Deserialize("Graphic", "TextureQuality"));
            blendWeight.value = int.Parse(configActions.Deserialize("Graphic", "BlendWeight"));
            vSync.value = int.Parse(configActions.Deserialize("Graphic", "VSync"));
            shadowResolution.value = int.Parse(configActions.Deserialize("Graphic", "ShadowResolution"));
            shadowCascades.value = int.Parse(configActions.Deserialize("Graphic", "ShadowCascades"));
            shadowProjecion.value = int.Parse(configActions.Deserialize("Graphic", "ShadowProjecion"));
            shadowDistance.value = int.Parse(configActions.Deserialize("Graphic", "ShadowDistance"));
            ApplyGraphic();
            Debug.Log("Set Values Quality");
        }
        firstLoad = false;
    }

    private void SerializeByGameManager()
    {
        gameManager.Serialize("Graphic", "GraphicQuality", qualityLevel.ToString());
        gameManager.Serialize("Graphic", "Antialiasing", antiaLevel.ToString());
        gameManager.Serialize("Graphic", "Anisotropic", anisoLevel.ToString());
        gameManager.Serialize("Graphic", "TextureQuality", textureQualityLevel.ToString());
        gameManager.Serialize("Graphic", "BlendWeight", blendWeightLevel.ToString());
        gameManager.Serialize("Graphic", "VSync", vSyncLevel.ToString());
        gameManager.Serialize("Graphic", "ShadowResolution", shadowResLevel.ToString());
        gameManager.Serialize("Graphic", "ShadowCascades", shadowCasLevel.ToString());
        gameManager.Serialize("Graphic", "ShadowProjecion", shadowProjLevel.ToString());
        gameManager.Serialize("Graphic", "ShadowDistance", shadowDistance.value.ToString());
    }

    private void DeserializeByGameManager()
    {
        int q = int.Parse(gameManager.Deserialize("Graphic", "GraphicQuality"));
        if (q != 3)
        {
            quality.value = q;
            defaultQuality = q;
            SetGraphicSettings(q);
        }
        else
        {
            quality.value = 3;
            antialiasing.value = int.Parse(gameManager.Deserialize("Graphic", "Antialiasing"));
            anisotropic.value = int.Parse(gameManager.Deserialize("Graphic", "Anisotropic"));
            textureQuality.value = int.Parse(gameManager.Deserialize("Graphic", "TextureQuality"));
            blendWeight.value = int.Parse(gameManager.Deserialize("Graphic", "BlendWeight"));
            vSync.value = int.Parse(gameManager.Deserialize("Graphic", "VSync"));
            shadowResolution.value = int.Parse(gameManager.Deserialize("Graphic", "ShadowResolution"));
            shadowCascades.value = int.Parse(gameManager.Deserialize("Graphic", "ShadowCascades"));
            shadowProjecion.value = int.Parse(gameManager.Deserialize("Graphic", "ShadowProjecion"));
            shadowDistance.value = int.Parse(gameManager.Deserialize("Graphic", "ShadowDistance"));
            ApplyGraphic();
        }
        firstLoad = false;
    }
}