using UnityEngine;
using UnityStandardAssets.ImageEffects;

public class ScriptManager : MonoBehaviour {

    [Header("Script Connections")]
	public ItemSwitcher itemSwitcher;
	public InputManager inputManager;
	public HFPS_GameManager GameManager;

    [Header("Other")]
    public BlurOptimized MainCameraBlur;
    public BlurOptimized ArmsCameraBlur;

    [HideInInspector]
	public InteractManager interact;

	[HideInInspector]
	public Inventory inventory;

	[HideInInspector] public PlayerFunctions pFunctions;
    [HideInInspector] public bool SetScriptEnabledGlobal;

    private void Awake()
    {
        interact = GetComponent<InteractManager>();
        inventory = GameManager.inventoryScript;
        pFunctions = GetComponent<PlayerFunctions>();
    }

    void Start(){
        SetScriptEnabledGlobal = true;
	}
}
