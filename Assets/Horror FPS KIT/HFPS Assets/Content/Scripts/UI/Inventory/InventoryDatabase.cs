using System.Collections.Generic;
using UnityEngine;

public enum itemType { Normal, Heal, Light, Battery, Weapon, WeaponAssembly, Bullets }

[ExecuteInEditMode]
public class InventoryDatabase : MonoBehaviour {

	public List<ItemMapper> ItemDatabase = new List<ItemMapper> ();
	public List<Item> Items = new List<Item> ();

	[System.Serializable]
	public class ItemMapper{
		public string Title;

        [ReadOnly] public int ID;
        [Multiline] public string Description;

        public itemType itemIdentifier;
        public Sprite ItemSprite;
		public bool Stackable;
        public bool isUsable;
        public bool Combinable;
        public bool GetItemCombine;
        public bool CombineNoRemove;
        public bool UseItemSwitcher;

        [System.Serializable]
		public class Properties{
			public AudioClip ItemSound;
            public AudioClip CombineSound;
            public int MaxItemCount;    //0 - Default(Infinite)
            public int CombineWithID;
			public int CombinedID;

            public int ItemSwitcherID = -1;
            public int healAmount;

            [HideInInspector]
			public int weaponID;
        }

		public Properties properties = new Properties ();
	}

	void Start()
	{
		for (int i = 0; i < ItemDatabase.Count; i++) {
            Items.Add (new Item (
                i,
                ItemDatabase[i].Title,
                ItemDatabase[i].Description,
                ItemDatabase[i].ItemSprite,
                ItemDatabase[i].Stackable,
                ItemDatabase[i].isUsable,
                ItemDatabase[i].Combinable,
                ItemDatabase[i].GetItemCombine,
                ItemDatabase[i].CombineNoRemove,
                ItemDatabase[i].UseItemSwitcher,
                ItemDatabase[i].itemIdentifier,
                ItemDatabase[i].properties.ItemSound,
                ItemDatabase[i].properties.CombineSound,
                ItemDatabase[i].properties.MaxItemCount,
                ItemDatabase[i].properties.CombineWithID,
                ItemDatabase[i].properties.CombinedID,
                ItemDatabase[i].properties.ItemSwitcherID,
                ItemDatabase[i].properties.healAmount,
                ItemDatabase[i].properties.weaponID
                ));		
        }
	}

    private void Update()
    {
        for (int i = 0; i < ItemDatabase.Count; i++)
        {
            ItemDatabase[i].ID = i;
        }
    }

    public Item GetItemByID(int id)
	{
		for (int i = 0; i < Items.Count; i++)
			if (Items [i].ID == id)
				return Items[i];
		return null;
	}
}

public class Item
{
	public int ID{ get; set; }
	public string Title{ get; set; }
	public string Description{ get; set; }
	public Sprite ItemSprite{ get; set; }
	public bool Stackable{ get; set; }
    public bool IsUsable { get; set; }

    public bool Combinable{ get; set; }
    public bool GetItemCombine { get; set; }
    public bool CombineNoRemove { get; set; }
    public bool UseItemSwitcher { get; set; }

    public itemType ItemIdentifier { get; set; }

	public AudioClip ItemSound { get; set; }
    public AudioClip CombineSound { get; set; }
    public int MaxItemCount { get; set; }
    public int CombineWithID { get; set; }
	public int CombinedID{ get; set; }

    public int ItemSwitcherID { get; set; }
    public int HealAmount { get; set; }

    public int WeaponID{ get; set; }

	public Item(int id, string title, string description, Sprite itemsprite, bool stackable, bool isusable, bool combinable, bool itemcombine, bool combinenoremove, bool itemswitch, itemType identifier, AudioClip itemsound, AudioClip combinesnd, int maxcount, int idtocombine, int combinedid, int itemswitcherid, int healamount, int weaponid)
	{
		ID = id;
		Title = title;
		Description = description;
		ItemSprite = itemsprite;
		Stackable = stackable;
        IsUsable = isusable;
        Combinable = combinable;
        GetItemCombine = itemcombine;
        CombineNoRemove = combinenoremove;
        UseItemSwitcher = itemswitch;

		ItemIdentifier = identifier;

        ItemSound = itemsound;
        CombineSound = combinesnd;
        MaxItemCount = maxcount;
        CombineWithID = idtocombine;
		CombinedID = combinedid;

        ItemSwitcherID = itemswitcherid;
        HealAmount = healamount;

		WeaponID = weaponid;
	}
}
