using UnityEngine;
using UnityEngine.UI;
using ThunderWire.Parser;

public class InteractManager : MonoBehaviour {

    private ParsePrimitives parser = new ParsePrimitives();
    private InputManager inputManager;
    private HFPS_GameManager gameManager;
	private ItemSwitcher itemSelector;
	private Inventory inventory;

	[Header("Raycast")]
	public float RayLength = 3;
	public LayerMask cullLayers;
	public string InteractLayer;
	
	[Header("Crosshair Textures")]
	public Sprite defaultCrosshair;
	public Sprite interactCrosshair;
	private Sprite default_interactCrosshair;
	
	[Header("Crosshair")]
	private Image CrosshairUI;
	public int crosshairSize = 5;
	public int interactSize = 10;

	private int default_interactSize;
    private int default_crosshairSize;

    [HideInInspector]
	public bool isHeld = false;

    [HideInInspector]
    public bool inUse;

    [HideInInspector]
	public Ray playerAim;

	[HideInInspector]
	public GameObject RaycastObject;
	
	private KeyCode UseKey;
	private string PickupKey;
	
	private Camera playerCam;
	private DynamicObject dynamic;

	private GameObject LastRaycastObject;

	private string RaycastTag;

	private bool correctLayer;

	private bool isPressed;
    private bool useTexture;
	private bool isSet;

    void Awake()
    {
        inputManager = GetComponent<ScriptManager>().inputManager;
        gameManager = GetComponent<ScriptManager>().GameManager;
        itemSelector = GetComponent<ScriptManager>().itemSwitcher;
        CrosshairUI = gameManager.Crosshair;
        default_interactCrosshair = interactCrosshair;
        default_crosshairSize = crosshairSize;
        default_interactSize = interactSize;
        playerCam = Camera.main;
        RaycastObject = null;
        dynamic = null;
    }

	void SetKeys()
	{
        UseKey = parser.ParseType<KeyCode>(inputManager.GetInput("Use"));
        PickupKey = "Q";
		isSet = true;
	}
	
	void Update () {
		inventory = GetComponent<ScriptManager>().inventory;

		if(inputManager.InputsCount() > 0 && !isSet)
		{
			SetKeys();
		}

		if (inputManager.GetRefreshStatus () && isSet) {
			isSet = false;
		}

		if(ControlFreak2.CF2Input.GetKey("E") && RaycastObject && !isPressed && !isHeld && !inUse){
			Interact();
			isPressed = true;
		}

		if(ControlFreak2.CF2Input.GetKeyUp("E") && isPressed){
			isPressed = false;
		}
			
        Ray playerAim = playerCam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
		RaycastHit hit;

		if (Physics.Raycast (playerAim, out hit, RayLength, cullLayers)) {
			RaycastTag = hit.collider.gameObject.tag;
			if (hit.collider.gameObject.layer == LayerMask.NameToLayer(InteractLayer)) {
                RaycastObject = hit.collider.gameObject;
                correctLayer = true;

				if (RaycastObject.GetComponent<DynamicObject> ()) {
                    dynamic = RaycastObject.GetComponent<DynamicObject> ();
				} else {
                    dynamic = null;
				}
			
				if (RaycastObject.GetComponent<CrosshairReticle> ()) {
					CrosshairReticle ChangeReticle = RaycastObject.GetComponent<CrosshairReticle> ();
                    if (dynamic)
                    {
                        if (dynamic.useType != DynamicObject.type.Locked)
                        {
                            interactCrosshair = ChangeReticle.interactSprite;
                            interactSize = ChangeReticle.size;
                        }
                    }
                    else
                    {
                        interactCrosshair = ChangeReticle.interactSprite;
                        interactSize = ChangeReticle.size;
                    }
				}
					
				useTexture = true;

				if (LastRaycastObject) {
					if (!(LastRaycastObject == RaycastObject)) {
						ResetCrosshair ();
					}
				}
				LastRaycastObject = RaycastObject;
			
				if (!inUse) {
					if (dynamic) {
						if (dynamic.useType == DynamicObject.type.Locked) {
							if (dynamic.CheckHasKey()) {
                                gameManager.ShowInteractSprite (1, "Unlock", "E".ToString ());
							} else {
                                gameManager.ShowInteractSprite (1, "Use", "E".ToString ());
							}
						} else {
                            gameManager.ShowInteractSprite (1, "Use", "E".ToString ());
						}
					} else {
						if (!(RaycastTag == "OnlyGrab")) {
                            gameManager.ShowInteractSprite (1, "Use", "E".ToString ());
						}
					}
					if (RaycastTag == "OnlyGrab") {
                        gameManager.ShowInteractSprite (1, "Grab", PickupKey);
					} else if (RaycastTag == "Grab") {
                        gameManager.ShowInteractSprite (1, "Use", "E".ToString ());
                        gameManager.ShowInteractSprite (2, "Grab", PickupKey);
					} else if (RaycastTag == "Paper") {
                        gameManager.ShowInteractSprite (1, "Examine", PickupKey);
					}
					if (RaycastObject.GetComponent<ExamineItem> ()) {
						if (RaycastObject.GetComponent<ExamineItem> ().isUsable) {
                            gameManager.ShowInteractSprite (1, "Use", "E".ToString ());
                            gameManager.ShowInteractSprite (2, "Examine", PickupKey);
						} else {
                            gameManager.ShowInteractSprite (1, "Examine", PickupKey);
						}
					}
				}
			} else if(RaycastObject) {
				correctLayer = false;
			}
		} else if(RaycastObject) {
			correctLayer = false;
		}

		if(!correctLayer){
			ResetCrosshair ();
			useTexture = false;
			RaycastTag = null;
			RaycastObject = null;
            dynamic = null;
		}
		
		if(!RaycastObject)
		{
            gameManager.HideSprites(spriteType.Interact);
            useTexture = false;
            dynamic = null;
		}

        CrosshairUpdate();
    }

    void CrosshairUpdate()
    {
        if(useTexture)
        {
			CrosshairUI.rectTransform.sizeDelta = new Vector2(interactSize, interactSize);
            CrosshairUI.sprite = interactCrosshair;
        }
        else
        {
			CrosshairUI.rectTransform.sizeDelta = new Vector2(crosshairSize, crosshairSize);
            CrosshairUI.sprite = defaultCrosshair;
        }
    }

	private void ResetCrosshair(){
		crosshairSize = default_crosshairSize;
		interactSize = default_interactSize;
		interactCrosshair = default_interactCrosshair;
	}

	public void CrosshairVisible(bool state)
	{
		switch (state) 
		{
		case true:
			CrosshairUI.enabled = true;
			break;
		case false:
			CrosshairUI.enabled = false;
			break;
		}
	}

	public bool GetInteractBool()
	{
		if (RaycastObject) {
			return true;
		} else {
			return false;
		}
	}

	void Interact(){
		if (RaycastObject.GetComponent<ItemID> ()) {
			ItemID iID = RaycastObject.GetComponent<ItemID> ();
            Item item = inventory.GetItem(iID.InventoryID);

            if (iID.ItemType == ItemID.Type.BackpackExpand) {
				inventory.ExpandSlots (iID.BackpackExpand);
				Pickup ();
			}

			if (inventory.CheckInventorySpace ()) {
                if (inventory.GetItemAmount(item.ID) < item.MaxItemCount || item.MaxItemCount == 0)
                {
                    if (iID.ItemType == ItemID.Type.NoInventoryItem)
                    {
                        itemSelector.selectItem(iID.WeaponID);
                    }
                    else if (iID.ItemType == ItemID.Type.InventoryItem)
                    {
                        inventory.AddItemToSlot(iID.InventoryID, iID.Amount);
                    }
                    else if (iID.ItemType == ItemID.Type.WeaponItem)
                    {
                        if (iID.weaponType == ItemID.WeaponType.Weapon)
                        {
                            inventory.AddItemToSlot(iID.InventoryID, iID.Amount);
                            inventory.SetWeaponID(iID.InventoryID, iID.WeaponID);
                            itemSelector.selectItem(iID.WeaponID);
                        }
                        else if (iID.weaponType == ItemID.WeaponType.Ammo)
                        {
                            inventory.AddItemToSlot(iID.InventoryID, iID.Amount);
                        }
                    }

                    if(iID.messageType == ItemID.MessageType.Hint)
                    {
                        gameManager.ShowHint(iID.message);
                    }
                    if (iID.messageType == ItemID.MessageType.Message)
                    {
                        gameManager.AddMessage(iID.message);
                    }
                    if (iID.messageType == ItemID.MessageType.ItemName)
                    {
                        gameManager.AddPickupMessage(iID.message);
                    }

                    Pickup();
                }
                else if(inventory.GetItemAmount(item.ID) >= item.MaxItemCount)
                {
                    gameManager.ShowHint("You cannot carry more " + item.Title);
                }
			} else {
                gameManager.ShowHint ("No Inventory Space!");
			}
		} else {
			Pickup ();
		}
	}

	void Pickup()
	{
        gameManager.HideSprites (spriteType.Interact);
		RaycastObject.SendMessage ("UseObject", SendMessageOptions.DontRequireReceiver);
	}
}
