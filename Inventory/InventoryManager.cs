using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using static UnityEngine.GraphicsBuffer;

public class InventoryManager : Singleton<InventoryManager>
{
    [Header("General")]
    public Camera mainCamera;
    
    [Header("DataInventory")]
    public DataInventory inventoryData;

    [Header("Inventory")]
    public Transform Tabs;
    public GridLayoutGroup Grid;
    public ScrollRect scrollRect;
    public Transform customCursor;
    public GameObject SlotLocked;
    public GameObject SlotFree;
    public GameObject ItemPrefab;
    public GameObject unlockNoti;
    public int inventorySize;
    public int inventoryLockedSize;
    public bool canExpand = false;
    public bool AddEmptyCells = true;


    [Header("Slots")]
    public List<GameObject> unlockedSlots = new List<GameObject>();
    public List<GameObject> lockSlots = new List<GameObject>();
    public List<EquipmentSlot> equipmentSlots = new List<EquipmentSlot>();

    public Toggle ActiveSlot;


    [Header("Item View")]
    public GameObject itemViewPanel;
    public TextMeshProUGUI itemNameUI;
    public Image itemBackgroundUI;
    public Image itemImageUI;
    public TextMeshProUGUI itemLevel;
    public TextMeshProUGUI itemRarity;
    public TextMeshProUGUI itemSpecialStat;
    public TextMeshProUGUI itemDescription;

    public GameObject verticalSlash;
    public GameObject itemViewStatGroup;
    public UIStat[] itemStats;


    public GameObject inventoryField;
    public GameObject equipField;

    public PlayerStat playerStat;

    public Sprite[] statSprites;


    [Header("InventoryTab")]
    public ToggleGroup inventoryTabPool;
    public Toggle[] inventoryTabs;

    private int[] tabParameters;

    private int currentItemType;
    private int currentRarity;

    private void Awake()
    {
        DataInventory.LoadData(inventoryData);

        //Hide all the item stats in baseStat view
        itemStats = itemViewStatGroup.GetComponentsInChildren<UIStat>();
        foreach (UIStat stat in itemStats)
        {
            stat.gameObject.SetActive(false);
        }
        SpawnSlots();
        SpawnItemsFromData();

        currentItemType = 0;
        currentRarity = 0;
        //Allocate FilterItemType() function to Toggle and filter first tab
        inventoryTabs = inventoryTabPool.GetComponentsInChildren<Toggle>();
        tabParameters = new int[] { 0, 7, 8, 9, 11};
        for(int i = 0; i < inventoryTabs.Length; i++)
        {
            Toggle tab = inventoryTabs[i];
            int tabPara = tabParameters[i];
            tab.group = inventoryTabPool;
            tab.onValueChanged.AddListener(delegate (bool isOn)
            {
                if (isOn)
                {
                    FilterItemType(tabPara);
                }
            });
            
        }
        FilterTypeAndRarity(currentItemType, currentRarity);
    }

    private void Update()
    {
        //Just check that InventorySystem is active or not => I just want to ultilize the customCursor instead of creating new parameter "GameObject inventorySystem"
        if (!customCursor.gameObject.activeInHierarchy) return;

        //Overall:
        //- If there is any slot is clicked => Display the item baseStat
        //- If users press left button to the active slot => They will take the item
        if (Grid.GetComponent<ToggleGroup>().AnyTogglesOn() == true)
        {
            ActiveSlot = Grid.GetComponent<ToggleGroup>().ActiveToggles().Single(i => i.isOn);
            DisplayItemInformation();
        }
        //----Display Item Information
        DisplayItemViewPanel();
    }
    private void SpawnSlots()
    {
        for (int i = 0; i < inventorySize; i++)
        {
            GameObject addSlot = Instantiate(SlotFree, Grid.transform);
            addSlot.transform.localScale = Vector3.one;
            unlockedSlots.Add(addSlot);
            addSlot.GetComponent<Toggle>().group = GetComponentInParent<ToggleGroup>();
            if (i == 0)
            {
                addSlot.GetComponent<Toggle>().isOn = true;
            }
        }
        for (int i = 0; i < inventoryLockedSize; i++)
        {
            GameObject addSlot = Instantiate(SlotLocked, Grid.transform);
            lockSlots.Add(addSlot);
            addSlot.GetComponent<Toggle>().group = GetComponentInParent<ToggleGroup>();
        }
    }

    /// Spawn equipped and inventory items from data when the game started.
    private void SpawnItemsFromData()
    {
        FilterItemType(0);
        InitializeEquippedItemFromData();
    }

    private void DisplayItemViewPanel()
    {
        if (ActiveSlot.GetComponentInChildren<InventoryItem>() == null) //Item Information disappear
        {
            itemViewPanel.SetActive(false);
        }
        else
        {
            itemViewPanel.SetActive(true);
        }
    }

    private void DisplayItemInformation()
    {
        InventoryItem currentItem = ActiveSlot.GetComponentInChildren<InventoryItem>();
        if (currentItem == null) return;
        //---Check special condition
        //----1. Check specialStat
        if (currentItem.data.info.prop.specialStat == "")
        {
            itemSpecialStat.gameObject.SetActive(false);
        }
        else
        {
            itemSpecialStat.gameObject.SetActive(true);
        }
        //----2. Check type of item => if (item == Potion, Book, Scroll, Material => only show Image, no level, no rarity)
        // or you can set your own type
        if (currentItem.data.info.baseStat.type == ItemType.Potion
            || currentItem.data.info.baseStat.type == ItemType.Book
            || currentItem.data.info.baseStat.type == ItemType.Scroll
            || currentItem.data.info.baseStat.type == ItemType.Material)
        {
            verticalSlash.SetActive(false);
            itemViewStatGroup.SetActive(false);
        }
        else if (currentItem.data.info.baseStat.type != ItemType.Currency) //----3. If Item is Weapon -> Ring
        {
            verticalSlash.SetActive(true);
            itemViewStatGroup.SetActive(true);
            foreach (UIStat stat in itemStats)
            {
                stat.gameObject.SetActive(false);
            }
            int statLen = currentItem.data.currentStat.Length;
            for (int i = 0; i < statLen; i++)
            {
                itemStats[i].gameObject.SetActive(true);
                itemStats[i].statImage.sprite = CheckStatImage(currentItem.data.currentStat[i]);
                itemStats[i].statText.text = currentItem.data.currentStat[i].value.ToString();
            }
        }

        //---Then, set main features
        Rarity rarity = currentItem.data.info.baseStat.rarity;
        string itemName = currentItem.data.info.prop.itemName;
        itemNameUI.text = ColoringText(rarity, itemName);

        itemBackgroundUI.sprite = currentItem.backGround.sprite;
        itemImageUI.sprite = currentItem.img.sprite;
        if (itemViewStatGroup.activeInHierarchy)
        {
            itemLevel.text = "Require Level " + currentItem.data.info.baseStat.requiredLevel.ToString();
            itemRarity.text = GetRarityText(rarity);
        }
        if (itemSpecialStat.gameObject.activeInHierarchy)
        {
            itemSpecialStat.text = currentItem.data.info.prop.specialStat;
        }
        itemDescription.text = currentItem.data.info.prop.itemDescription;
    }
    Sprite CheckStatImage(ItemInfo.ItemStat.Stat stat)
    {
        if(stat.type == StatType.Attack)
        {
            return statSprites[0];
        }
        else if (stat.type == StatType.AttackSpeed)
        {
            return statSprites[1];
        }
        else if (stat.type == StatType.AttackRange)
        {
            return statSprites[2];
        }
        else if(stat.type == StatType.Health)
        {
            return statSprites[3];
        }
        else if(stat.type == StatType.PhysicalDefense)
        {
            return statSprites[4];
        }
        else if (stat.type == StatType.MagicalDefense)
        {
            return statSprites[5];
        }
        return null;
    }
    string GetRarityText(Rarity rarity)
    {
        switch (rarity)
        {
            case Rarity.Legendary:
                return "<color=red>Legendary</color>";
            case Rarity.Mythical:
                return "<color=yellow>Mythical</color>";
            case Rarity.Epic:
                return "<color=purple>Epic</color>";
            case Rarity.Rare:
                return "<color=#04f4f4>Rare</color>";
            case Rarity.Uncommon:
                return "<color=green>Uncommon</color>";
            case Rarity.Common:
                return "<color=white>Common</color>";
        }
        return "";
    }

    string ColoringText(Rarity rarity, string text)
    {
        switch (rarity)
        {
            case Rarity.Legendary:
                return "<color=red>" + text + "</color>";
            case Rarity.Mythical:
                return "<color=yellow>" + text + "</color>";
            case Rarity.Epic:
                return "<color=purple>" + text + "</color>";
            case Rarity.Rare:
                return "<color=#04f4f4>" + text + "</color>";
            case Rarity.Uncommon:
                return "<color=green>" + text + "</color>";
            case Rarity.Common:
                return "<color=white>" + text + "</color>";
        }
        return "";
    }

    public void EquipItem(InventoryItem thisItem, EquipmentSlot equipableSlot)
    {
        if (ActiveSlot.GetComponentInChildren<InventorySlot>() == null) return;

        thisItem.SetPosition(equipableSlot.transform);

        equipableSlot.ShowCannotEquip(); // Undisplay the red frame
        equipableSlot.isEquip = true;

        inventoryData.RemoveInventoryData(thisItem.data.ID);
        inventoryData.AddEquipmentData(thisItem.data);

        playerStat.AddItemStat(thisItem);
    }

    public void UnequipItem(InventoryItem thisItem)
    {
        if (ActiveSlot.GetComponentInChildren<EquipmentSlot>() == null) return;

        foreach (GameObject unlockedSlot in unlockedSlots)
        {
            InventorySlot slot = unlockedSlot.GetComponent<InventorySlot>();
            if (slot.isEmpty)
            {
                thisItem.SetPosition(slot.transform);
                slot.isEmpty = false;
                ActiveSlot.GetComponent<EquipmentSlot>().isEquip = false;
                ActiveSlot.GetComponent<EquipmentSlot>().ShowCannotEquip();

                inventoryData.RemoveEquipmentData(thisItem.data.ID);
                inventoryData.AddInventoryData(thisItem.data);

                playerStat.RemoveItemStat(thisItem);

                break;
            }
        }
    }

    public void ReplaceItem(InventoryItem thisItem, InventoryItem targetItem)
    {
        EquipmentSlot equipmentSlot = targetItem.GetComponentInParent<EquipmentSlot>();
        if (equipmentSlot != null) // Swap with equipped item
        {
            thisItem.SetPosition(equipmentSlot.transform);
            equipmentSlot.ShowCannotEquip();
            equipmentSlot.isEquip = true;

            targetItem.SetPosition(ActiveSlot.transform);
            ActiveSlot.GetComponent<InventorySlot>().isEmpty = false;
            
            //Update the database
            inventoryData.RemoveEquipmentData(targetItem.data.ID);
            inventoryData.AddInventoryData(targetItem.data);

            inventoryData.RemoveInventoryData(thisItem.data.ID);
            inventoryData.AddEquipmentData(thisItem.data);

            playerStat.RemoveItemStat(targetItem);
            playerStat.AddItemStat(thisItem);
            return;
        }

        InventorySlot inventorySlot = targetItem.GetComponentInParent<InventorySlot>();
        if (inventorySlot != null) // Swap with another item in inventory
        {
            targetItem.SetPosition(ActiveSlot.transform);
            thisItem.SetPosition(inventorySlot.transform);
            ActiveSlot.GetComponent<InventorySlot>().isEmpty = false;
        }
    }

    public void AddAmountOfItem(ItemBase.ItemData data, int amount)
    {
        bool newItem = false;
        if (data.info.prop.countable)
        {
            if (inventoryData.GetAmount(data.info) == 0)
            {
                newItem = true;
            }
            else
            {
                foreach (GameObject unlockedSlot in unlockedSlots)
                {
                    InventorySlot slot = unlockedSlot.GetComponent<InventorySlot>();
                    if (!slot.isEmpty)
                    {
                        InventoryItem item = unlockedSlot.GetComponentInChildren<InventoryItem>();
                        if (item.data.info == data.info)
                        {
                            item.data.amount += amount;
                            //Add data to database
                            inventoryData.AddInventoryData(item.data);

                            slot.countText.text = item.data.amount.ToString();
                            break;
                        }
                    }
                }
            }
        }
        if (!data.info.prop.countable || newItem == true)
        {
            foreach (GameObject unlockedSlot in unlockedSlots)
            {
                InventorySlot slot = unlockedSlot.GetComponent<InventorySlot>();
                if (slot.isEmpty)
                {
                    GameObject itemAdd = Instantiate(ItemPrefab, transform.position, Quaternion.identity);

                    itemAdd.transform.SetParent(unlockedSlot.transform);
                    itemAdd.transform.SetSiblingIndex(4);   
                    itemAdd.transform.localPosition = new Vector3(0, 1, 0);
                    itemAdd.transform.localScale = new Vector3(1, 1, 1);
                    slot.isEmpty = false;

                    //Initialize data for new item => Fix in the future because it should be init in shop system/craft/gacha.
                    //Means it should be initialized when the item was created, not when add to inventory.
                    InventoryItem item = itemAdd.GetComponent<InventoryItem>();
                    item.data.ID = GenerateID();
                    item.data.info = data.info;
                    item.data.amount += amount;
                    item.data.currentStat = data.info.baseStat.stats;

                    //Add data to database
                    inventoryData.AddInventoryData(item.data);

                    if (data.info.prop.countable)
                    {
                        slot.countText.text = item.data.amount.ToString();
                    }
                    break;
                }
            }
        }
    }
    public void RemoveAmountOfItem(ItemInfo info, int amount)
    {
        foreach (GameObject unlockedSlot in unlockedSlots)
        {
            InventorySlot slot = unlockedSlot.GetComponent<InventorySlot>();
            InventoryItem item = unlockedSlot.GetComponentInChildren<InventoryItem>();
            if (item != null)
            {
                if (item.data.info.prop.itemName == info.prop.itemName)
                {
                    item.data.amount -= amount;

                    if (item.data.amount == 0)
                    {
                        inventoryData.RemoveInventoryData(item.data.ID);
                        slot.DestroyItem();
                    }
                    else if (item.data.amount > 0)
                    {
                        //Update to database
                        inventoryData.AddInventoryData(item.data);
                        slot.countText.text = item.data.amount.ToString();
                    }
                    break;
                }
            }
        }
    }

    public void DeleteItem()
    {
        InventorySlot activeSlot = ActiveSlot.GetComponent<InventorySlot>();
        if (!activeSlot.isEmpty)
        {
            inventoryData.RemoveInventoryData(activeSlot.GetComponentInChildren<InventoryItem>().data.ID);
            activeSlot.DestroyItem();
        }

    }
    public int GenerateID()
    {
        int randNum;
        do
        {
            randNum = Random.Range(1000, 10000);
        } while (inventoryData.inventoryData.Exists(x => x.ID == randNum)); 
        /* Exists is similar to Single:
         * - Exists used for List
         * - Single used for array
         */
        return randNum;
    }

    public int GetAmountOfItem(ItemInfo info)
    {
        foreach (GameObject unlockedSlot in unlockedSlots)
        {
            InventoryItem item = unlockedSlot.GetComponentInChildren<InventoryItem>();
            if (item != null)
            {
                if (item.data.info.prop.itemName == info.prop.itemName)
                {
                    return item.data.amount;
                }
            }
        }
        return 0;
    }

    public void FilterRarity(int rarity)
    {
        currentRarity = rarity;
        FilterTypeAndRarity(currentItemType, currentRarity);
    }

    // itemType == 0 => All, itemType == 7 => Filter with type < 7, which means Weapon, Armor, ... Necklace.
    public void FilterItemType(int itemType) 
    {
        currentItemType = itemType;
        FilterTypeAndRarity(currentItemType, currentRarity);
    }

    public void FilterTypeAndRarity(int itemType, int rarity)
    {
        if (inventoryData.inventoryData.Count == 0)
        {
            return;
        }
        RemoveAllItem();
        foreach (ItemBase.ItemData data in inventoryData.inventoryData)
        {
            if (itemType == 0
                || itemType == 7 && (int)data.info.baseStat.type < itemType
                || (int)data.info.baseStat.type == itemType)
            {
                if (rarity == 0 ||
                (int)data.info.baseStat.rarity == rarity)
                {
                    InitializeInventoryItemFromData(data);
                }
            }
        }
    }
    public void InitializeInventoryItemFromData(ItemBase.ItemData data) //I ultilize this function for filter item type too
    {
        foreach (GameObject unlockedSlot in unlockedSlots)
        {
            InventorySlot slot = unlockedSlot.GetComponent<InventorySlot>();
            if (slot.isEmpty)
            {
                GameObject itemAdd = Instantiate(ItemPrefab, transform.position, Quaternion.identity);

                itemAdd.transform.SetParent(unlockedSlot.transform);
                itemAdd.transform.SetSiblingIndex(4); //If it is the last => count text will be faded
                itemAdd.transform.localPosition = new Vector3(0, 1, 0); // Not Vector3.zero because the slot asset is 32x33 pixel while the item asset is 32x32 pixel.
                itemAdd.transform.localScale = new Vector3(1, 1, 1);
                slot.isEmpty = false;

                InventoryItem item = itemAdd.GetComponent<InventoryItem>();
                item.data = data;


                if (data.info.prop.countable)
                {
                    slot.countText.text = inventoryData.GetAmount(data.info).ToString();
                }
                break;
            }

        }
    }

    public void InitializeEquippedItemFromData()
    {
        if (inventoryData.equipmentData.Count == 0)
        {
            return;
        }

        foreach (ItemBase.ItemData data in inventoryData.equipmentData)
        {
            foreach (EquipmentSlot slot in equipmentSlots)
            {
                if (slot.type == data.info.baseStat.type)
                {
                    GameObject itemAdd = Instantiate(ItemPrefab, transform.position, Quaternion.identity);
                    itemAdd.transform.SetParent(slot.transform);
                    itemAdd.transform.localPosition = Vector3.zero;
                    itemAdd.transform.localScale = new Vector3(1, 1, 1);

                    InventoryItem item = itemAdd.GetComponent<InventoryItem>();
                    item.data = data;

                    slot.isEquip = true;
                }
            }
        }
    }

    /// <summary>
    /// Just remove all items from the inventory, but not remove it from data.
    /// </summary>
    public void RemoveAllItem()
    {
        foreach (GameObject unlockedSlot in unlockedSlots)
        {
            InventorySlot slot = unlockedSlot.GetComponent<InventorySlot>();
            if (!slot.isEmpty)
            {
                slot.DestroyItem();
            }
        }
    }


    /// <summary>
    /// Rearrange the inventory.
    /// </summary>
    public void RearrangeItem()
    {
        //Update in the future
    }


    /// <summary>
    /// Exit the game.
    /// </summary>
    public void QuitGame()
    {
        Application.Quit();
    }


    public void OnApplicationQuit()
    {
        DataInventory.SaveData(inventoryData);
    }
}
