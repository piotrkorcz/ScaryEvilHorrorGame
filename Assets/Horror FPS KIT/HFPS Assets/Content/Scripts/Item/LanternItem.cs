using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LanternItem : MonoBehaviour {

    [Header("Setup")]
    public ScriptManager scriptManager;
    private InputManager inputManager;
    private Inventory inventory;

    [Header("Candle Properties")]
    public float oilTimeInSec = 300f;
    public float oilPercentage = 100;

    [Header("Inventory Settings")]
    public int itemSwitcherID;
    public int LanternInventoryID;
    public int LanternOilID;

    [Header("Candle Animations")]
    public GameObject LanternGO;
    public string DrawAnimation;
    public string HideAnimation;

    public float DrawSpeed = 1f;
    public float HideSpeed = 1f;

    public KeyCode OilReloadKey;
    public KeyCode UseLantern;

    private bool isSelected;
    private bool IsPressed;
    private bool playAnim;

    void Start()
    {
        inputManager = scriptManager.inputManager;
    }

    public void Select()
    {
        isSelected = true;
        LanternGO.SetActive(true);
        LanternGO.GetComponent<Animation>().Play(DrawAnimation);
    }

    public void Deselect()
    {
        if (LanternGO.activeSelf)
        {
            StartCoroutine(DeselectCoroutine());
        }
    }

    IEnumerator DeselectCoroutine()
    {
        LanternGO.GetComponent<Animation>().Play(HideAnimation);
        IsPressed = true;

        yield return new WaitUntil(() => !LanternGO.GetComponent<Animation>().isPlaying);

        isSelected = false;
        LanternGO.SetActive(false);
    }

    void Update()
    {
        if (!inventory)
        {
            inventory = scriptManager.inventory;
        }

        if (inputManager.InputsCount() > 0)
        {
            UseLantern = (KeyCode)System.Enum.Parse(typeof(KeyCode), inputManager.GetInput("Flashlight"));
            OilReloadKey = (KeyCode)System.Enum.Parse(typeof(KeyCode), inputManager.GetInput("Reload"));
        }

        if (inventory.CheckItemIDInventory(LanternInventoryID))
        {
            if(ControlFreak2.CF2Input.GetKeyDown(UseLantern) && !IsPressed)
            {
                IsPressed = true;
                if (playAnim && !LanternGO.GetComponent<Animation>().isPlaying)
                {
                    if (!isSelected)
                    {
                        Select();
                        playAnim = false;
                    }
                    else
                    {
                        Deselect();
                        playAnim = false;
                    }
                }
            }else if (IsPressed){
                IsPressed = false;
                playAnim = true;
            }
        }
    }
}
