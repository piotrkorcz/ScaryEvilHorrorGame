using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine;
using UnityEngine.UI;

public class Inventory : MonoBehaviour, IPointerClickHandler {

	private InventoryDatabase inventoryData;
	private HFPS_GameManager gameManager;
	private ItemSwitcher switcher;

	[Header("Main")]
	public GameObject SlotsPanel;
	public GameObject UseButton;
	public GameObject CombineButton;
	public Text ItemLabel;
	public Text ItemDescription;

	[Header("Inventory Sprites")]
	public Sprite inventorySlotFilled;
	public GameObject inventorySlot;
	public GameObject inventoryItem;

	[Header("Inventory Items")]
	public int slotAmout;
	public int maxSlots = 16;

	[HideInInspector]
	public int selectedID;

    [HideInInspector]
    public int selectedSwitcherID = -1;

    public List<Item> items = new List<Item> ();
	public List<GameObject> slots = new List<GameObject> ();

	[Header("Inventory Settings")]
	public Color normalColor = Color.white;
	public Color selectedColor = Color.white;
	public Color combineColor = Color.white;

	void Start () {
		inventoryData = GetComponent<InventoryDatabase> ();
        gameManager = GetComponent<HFPS_GameManager>();

		for (int i = 0; i < slotAmout; i++) {
			slots.Add(Instantiate(inventorySlot));
			slots [i].GetComponent<InventorySlot> ().id = i;
			slots [i].transform.SetParent (SlotsPanel.transform);
		}

		ItemLabel.text = "";
		ItemDescription.text = "";

        selectedID = -1;
	}

	void Update()
	{
		if (!switcher) {
			switcher = gameManager.scriptManager.itemSwitcher;
        }
        else
        {
            selectedSwitcherID = switcher.currentItem;
        }

		if (!gameManager.TabButtonPanel.activeSelf) {
			CombineButton.SetActive (false);
			UseButton.SetActive (false);
			for (int i = 0; i < slots.Count; i++) {
				slots [i].GetComponent<InventorySlot> ().isCombining = false;
				slots [i].GetComponent<InventorySlot> ().isCombinable = false;
			}
		}
	}

    public void AddSlotItem(int slotID, int id, int amount)
    {
        Item itemToAdd = inventoryData.GetItemByID(id);
        if (CheckInventorySpace())
        {
            for (int i = 0; i < slots.Count; i++)
            {
                if (i == slotID)
                {
                    items.Add(itemToAdd);
                    GameObject item = Instantiate(inventoryItem);
                    item.GetComponent<InventoryItemData>().item = itemToAdd;
                    item.GetComponent<InventoryItemData>().amount = amount;
                    item.GetComponent<InventoryItemData>().slotID = i;
                    item.transform.SetParent(slots[i].transform);
                    item.GetComponent<Image>().sprite = itemToAdd.ItemSprite;
                    item.GetComponent<RectTransform>().position = Vector2.zero;
                    item.name = itemToAdd.Title;
                    break;
                }
            }
        }
    }

    public void AddItemToSlot(int id, int amount)
	{
		Item itemToAdd = inventoryData.GetItemByID (id);
		if (CheckInventorySpace()) {
			if (itemToAdd.Stackable && CheckItemInventory (itemToAdd) && GetSlotByItem (itemToAdd) != -1) {
				InventoryItemData itemData = slots [GetSlotByItem (itemToAdd)].transform.GetChild (0).GetComponent<InventoryItemData> ();
				itemData.amount = itemData.amount + amount;
			} else {
				for (int i = 0; i < slots.Count; i++) {
					if (slots [i].transform.childCount < 1) {
						items.Add (itemToAdd);
						GameObject item = Instantiate (inventoryItem);
						item.GetComponent<InventoryItemData> ().item = itemToAdd;
						item.GetComponent<InventoryItemData> ().amount = amount;
						item.GetComponent<InventoryItemData> ().slotID = i;
						item.transform.SetParent (slots [i].transform);         
						item.GetComponent<Image> ().sprite = itemToAdd.ItemSprite;
						item.GetComponent<RectTransform> ().position = Vector2.zero;
						item.name = itemToAdd.Title;
						break;
					}
				}
			}
		}
	}

	public void SetWeaponID(int id, int weaponID){
		Item item = inventoryData.GetItemByID (id);
		item.WeaponID = weaponID;
	}

	int GetSlotByItem(Item item)
	{
		for (int i = 0; i < slots.Count; i++) {
			if (slots [i].transform.childCount > 0 && slots [i].transform.GetChild (0).GetComponent<InventoryItemData> ().item == item)
				return i;
		}
		return -1;
	}

	public void RemoveItem (int id)
	{
		Item itemToRemove = inventoryData.GetItemByID (id);
		if (itemToRemove.Stackable && CheckItemInventory (itemToRemove)) {
			InventoryItemData data = slots [GetSlotByItem (itemToRemove)].transform.GetChild (0).GetComponent<InventoryItemData> ();
			data.amount--;
			data.transform.GetChild (0).GetComponent<Text> ().text = data.amount.ToString ();
			if (data.amount == 0) {
				Destroy (slots [GetSlotByItem (itemToRemove)].transform.GetChild (0).gameObject);
				items.Remove (itemToRemove);
			}
			if (data.amount == 1) {
				slots [GetSlotByItem (itemToRemove)].transform.GetChild (0).transform.GetChild (0).GetComponent<Text> ().text = "";
			}
		} else {
			Destroy (slots [GetSlotByItem (itemToRemove)].transform.GetChild (0).gameObject);
			items.Remove (itemToRemove);
		}
	}

	public void RemoveItemAmount (int id, int amount)
	{
		Item itemToRemove = inventoryData.GetItemByID (id);
		if (CheckItemInventory (itemToRemove)) {
			InventoryItemData data = slots [GetSlotByItem (itemToRemove)].transform.GetChild (0).GetComponent<InventoryItemData> ();
			data.amount = data.amount - amount;
			data.transform.GetChild (0).GetComponent<Text> ().text = data.amount.ToString ();
			if (data.amount <= 0 && itemToRemove.ItemIdentifier != itemType.Weapon) {
				Destroy (slots [GetSlotByItem (itemToRemove)].transform.GetChild (0).gameObject);
				items.Remove (itemToRemove);
			}
		}
	}

	public void ExpandSlots(int slotsAmount)
	{
		int extendedSlots = slotAmout + slotsAmount;
		if (extendedSlots > maxSlots) {
            gameManager.WarningMessage ("Cannot carry more backpacks");
			return;
		}
		for (int i = slotAmout; i < extendedSlots; i++) {
			slots.Add(Instantiate(inventorySlot));
			slots [i].GetComponent<InventorySlot> ().id = i;
			slots [i].transform.SetParent (SlotsPanel.transform);
		}
		slotAmout = extendedSlots;
	}

	public bool CheckInventorySpace()
	{
		for (int i = 0; i < slots.Count; i++) {
			if (slots [i].transform.childCount < 1)
				return true;
		}
		return false;
	}

	bool CheckItemInventory(Item item)
	{
		for (int i = 0; i < items.Count; i++) {
			if (items [i].ID == item.ID)
				return true;
		}
		return false;
	}

	public bool CheckItemIDInventory(int ItemID)
	{
		for (int i = 0; i < items.Count; i++) {
			if (items [i].ID == ItemID)
				return true;
		}
		return false;
	}

	int GetSlotId(int itemID)
	{
		for (int i = 0; i < slots.Count; i++) {
			if (slots [i].transform.childCount > 0)
				if (slots [i].transform.GetChild (0).GetComponent<InventoryItemData> ().item.ID == itemID)
					return i;
		}
		return -1;
	}

    public Item GetItem(int itemID)
    {
        return inventoryData.Items[itemID];
    }

    public int GetItemAmount(int itemID)
	{
		for (int i = 0; i < slots.Count; i++) {
			if (slots [i].transform.childCount > 0)
			if (slots [i].transform.GetChild (0).GetComponent<InventoryItemData> ().item.ID == itemID)
				return slots [i].transform.GetChild (0).GetComponent<InventoryItemData> ().amount;
		}
		return -1;
	}

	public void SetItemAmount(int itemID, int amount)
	{
		for (int i = 0; i < slots.Count; i++) {
			if (slots [i].transform.childCount > 0)
			if (slots [i].transform.GetChild (0).GetComponent<InventoryItemData> ().item.ID == itemID)
				slots [i].transform.GetChild (0).GetComponent<InventoryItemData> ().amount = amount;
		}
	}

	public void UseItem()
	{
		Item usableItem = slots [selectedID].transform.GetChild (0).GetComponent<InventoryItemData> ().item;

		if (usableItem.ItemIdentifier == itemType.Heal) {
            gameManager.healthManager.ApplyHeal (usableItem.HealAmount);
			if (!gameManager.healthManager.isMaximum) {
				if (usableItem.ItemSound) {
					AudioSource.PlayClipAtPoint (usableItem.ItemSound, Camera.main.transform.position, 1.0f);
				}
				RemoveItem (usableItem.ID);
			}
		}
        else if (usableItem.ItemIdentifier == itemType.Weapon) {
			switcher.selectItem (usableItem.ItemSwitcherID);
        }
        else if(usableItem.ItemIdentifier == itemType.Light) {
            switcher.selectItem(usableItem.ItemSwitcherID);
        }

		CombineButton.SetActive (false);
		UseButton.SetActive (false);
		UseButton.transform.GetChild (0).GetComponent<MenuEvents> ().ChangeTextColor ("Black");
	}

	public void CombineItem()
	{
		int combineWithID = slots [selectedID].transform.GetChild (0).GetComponent<InventoryItemData> ().item.CombineWithID;
		for (int i = 0; i < slots.Count; i++) {
			slots [i].GetComponent<InventorySlot> ().isCombining = true;
			if (slots [i] != slots [GetSlotId (combineWithID)]) {
				slots [i].GetComponent<Image> ().color = combineColor;
				slots [i].GetComponent<InventorySlot> ().isCombinable = false;
			} else {
				slots [i].GetComponent<InventorySlot> ().isCombinable = true;
			}
		}
	}

    public void CombineWith(Item SecondItem, int id)
    {
        if (id != selectedID)
        {
            int CombinedItemID = SecondItem.CombinedID;
            Item SelectedItem = slots[selectedID].transform.GetChild(0).GetComponent<InventoryItemData>().item;

            for (int i = 0; i < slots.Count; i++)
            {
                slots[i].GetComponent<InventorySlot>().isCombining = false;
                slots[i].GetComponent<Image>().color = normalColor;
            }

            CombineButton.SetActive(false);
            CombineButton.transform.GetChild(0).GetComponent<MenuEvents>().ChangeTextColor("Black");
            UseButton.SetActive(false);

            if (SelectedItem.CombineSound)
            {
                AudioSource.PlayClipAtPoint(SelectedItem.CombineSound, Camera.main.transform.position, 1.0f);
            }
            else
            {
                if (SecondItem.CombineSound)
                {
                    AudioSource.PlayClipAtPoint(SecondItem.CombineSound, Camera.main.transform.position, 1.0f);
                }
            }

            if(SelectedItem.ItemIdentifier == itemType.Light && SelectedItem.Combinable)
            {
                switcher.selectItem(SelectedItem.ItemSwitcherID);
            }

            if (SelectedItem.ItemIdentifier == itemType.WeaponAssembly)
            {
                switcher.selectItem(SelectedItem.WeaponID);
            }

            if (SelectedItem.ItemIdentifier == itemType.Battery)
            {
                FlashlightScript flashlight = switcher.ItemList[SelectedItem.ItemSwitcherID].GetComponent<FlashlightScript>();
                if (flashlight && flashlight.canReload)
                {
                    flashlight.ReloadBattery();
                    RemoveItem(SelectedItem.ID);
                }
            }
            else
            {
                if (SelectedItem.GetItemCombine)
                {
                    int a_count = GetItemAmount(SelectedItem.ID);
                    int b_count = GetItemAmount(SecondItem.ID);

                    if (a_count < 2 && b_count >= 2)
                    {
                        if (!SelectedItem.CombineNoRemove)
                        {
                            StartCoroutine(WaitForRemoveAddItem(SelectedItem, CombinedItemID));
                        }
                        else
                        {
                            AddItemToSlot(CombinedItemID, 1);
                        }
                    }
                    if (a_count >= 2 && b_count < 2)
                    {
                        if (!SecondItem.CombineNoRemove)
                        {
                            StartCoroutine(WaitForRemoveAddItem(SecondItem, CombinedItemID));
                        }
                        else
                        {
                            AddItemToSlot(CombinedItemID, 1);
                        }
                    }
                    if (a_count < 2 && b_count < 2)
                    {
                        if (!SelectedItem.CombineNoRemove)
                        {
                            StartCoroutine(WaitForRemoveAddItem(SelectedItem, CombinedItemID));
                        }
                        else
                        {
                            AddItemToSlot(CombinedItemID, 1);
                        }
                    }
                    if (a_count >= 2 && b_count >= 2)
                    {
                        AddItemToSlot(CombinedItemID, 1);
                    }
                }

                if (!SelectedItem.CombineNoRemove)
                {
                    RemoveItem(SelectedItem.ID);
                }
                if (!SecondItem.CombineNoRemove)
                {
                    RemoveItem(SecondItem.ID);
                }
            }
        }
    }

	IEnumerator WaitForRemoveAddItem(Item item, int combinedID)
	{
		yield return new WaitUntil (() => !CheckItemInventory(item));
		AddItemToSlot (combinedID, 1);
	}

	public void Deselect(int id){
		slots [id].GetComponent<Image> ().color = normalColor;
		slots [id].transform.GetChild (0).GetComponent<InventoryItemData> ().selected = false;
		ItemLabel.text = "";
		ItemDescription.text = "";
		selectedID = -1;
	}

   void DeselectSelected()
   {
		if (selectedID != -1) {
            slots[selectedID].GetComponent<Image>().color = normalColor;
            slots[selectedID].transform.GetChild(0).GetComponent<InventoryItemData>().selected = false;
            CombineButton.SetActive(false);
            UseButton.SetActive(false);
            ItemLabel.text = "";
            ItemDescription.text = "";
            selectedID = -1;
        }
    }

    public void OnPointerClick (PointerEventData eventData)
	{
		if (gameManager.TabButtonPanel.activeSelf) {
			for (int i = 0; i < slots.Count; i++) {
				slots [i].GetComponent<InventorySlot> ().isCombining = false;
				slots [i].GetComponent<InventorySlot> ().isCombinable = false;
			}
			if (selectedID != -1) {
				slots [selectedID].GetComponent<Image> ().color = normalColor;
				slots [selectedID].transform.GetChild (0).GetComponent<InventoryItemData> ().selected = false;
				CombineButton.SetActive (false);
				UseButton.SetActive (false);
				ItemLabel.text = "";
				ItemDescription.text = "";
				selectedID = -1;
			}
		}
	}
}
