using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Linq;
using UnityEngine.UI;
public class UpgradeManager : MonoBehaviour
{
    [SerializeField] Toggle upgradeSystemButton;
    [SerializeField] private List<InventoryItem> equipItems;

    [SerializeField] private GameObject equipItemPool;
    [SerializeField] private GameObject currentStatPool;
    [SerializeField] private GameObject previewStatPool;

    [SerializeField] private InventoryItem upgradeItem;
    [SerializeField] private InventoryItem previewItem;

    [SerializeField] private UIStat[] currentStats;
    [SerializeField] private UIStat[] previewStats;

    private int currentIdx;

    private void Awake()
    {
        equipItems = equipItemPool.GetComponentsInChildren<InventoryItem>().ToList();
        foreach(InventoryItem item in equipItems)
        {
            item.gameObject.SetActive(false);
        }

        currentStats = currentStatPool.GetComponentsInChildren<UIStat>();
        previewStats = previewStatPool.GetComponentsInChildren<UIStat>();


        upgradeSystemButton.onValueChanged.AddListener(delegate {
            ShowEquipItem();
            DisplayUI(false);
        });

    }


    /// <summary>
    /// Display all equiped items in display-item field.
    /// </summary>
    private void ShowEquipItem()
    {
        for(int i = 0; i < 6; i++)
        {
            EquipmentSlot slot = InventoryManager.Instance.equipmentSlots[i];
            if (InventoryManager.Instance.equipmentSlots[i].isEquip)
            {
                equipItems[i].gameObject.SetActive(true);
                equipItems[i].data = slot.GetComponentInChildren<InventoryItem>().data;
            }
            else
            {
                equipItems[i].gameObject.SetActive(false);
            }
        }

        // Question: Why do we need upgradeItem.data become null when starting?
        // Answer: If you upgrade the first sword, then you swap another sword
        //         Then you go to upgrade system, but not click to any item, click to "upgrade button" => it keeps upgrading the old item
        //         Because the upgradeItem didn't reset.
        // You can see in the UpgradeItem(), the second condition is if (upgradeItem.data == null) return;
        upgradeItem.data = null;
    }


    /// <summary>
    /// Display the item that is clicked.
    /// </summary>
    public void DisplayItem(int idx)
    {
        currentIdx = idx;
        if (!equipItems[idx].gameObject.activeInHierarchy )
        {
            DisplayUI(false);
            return;
        }

        upgradeItem.gameObject.SetActive(true);
        previewItem.gameObject.SetActive(true);

        upgradeItem.data = equipItems[idx].data;

        previewItem.data.info = upgradeItem.data.info;

        DisplayUpgradeCurrentStat();
        DisplayUpgradeStatPreview();
    }
    /* 
     * Step 1: Go to Player Stat scripts and read two functions RemoveItemStat() and AddItemStat() 
     *          RemoveItemStat(): Remove current stats of current equipped item
     *          AddItemStat(): Add the stat of the equipped item
     *          => You will understand the process I indicate in step 2.
     * Step 2: Firstly, I removed all the stats of the equipped item from player stat
     *          Then, we upgrade the item => the stats of item will be increased
     *          Finally, we add the new stat to player stat.
     *
     * Question: Why don't you update the stats of equipped item?
     * Answer: It will be more complex because we need to add one more function. Just ultilize all functions we have.
     */
    public void UpgradeItem()
    {
        if (!equipItems[currentIdx].gameObject.activeInHierarchy || upgradeItem.data == null)
        {
            return;
        }

        PlayerStat.Instance.RemoveItemStat(upgradeItem);

        upgradeItem.data.currentLevel++;
        foreach(ItemInfo.ItemStat.Stat stat in upgradeItem.data.currentStat)
        {
            stat.value = stat.GetNextValue();
        }

        PlayerStat.Instance.AddItemStat(upgradeItem);
        DisplayUpgradeCurrentStat();
        DisplayUpgradeStatPreview();
    }


    /// <summary>
    /// Display description of the current stats in text.
    /// </summary>
    private void DisplayUpgradeCurrentStat()
    {
        int len = upgradeItem.data.currentStat.Length;
        for(int i = 0; i < len; i++)
        {
            currentStats[i].DisplayInfo(true);
            string statText = upgradeItem.data.currentStat[i].value.ToString();
            StatType type = upgradeItem.data.info.baseStat.stats[i].type;
            Sprite icon = ItemManager.Instance.statIconDict[type];
            currentStats[i].SetText(statText);
            currentStats[i].SetImage(icon);
        }
        for(int i = len; i < 3; i++)
        {
            currentStats[i].DisplayInfo(false);
        }
    }


    /// <summary>
    /// Display description of the previewed stats in text.
    /// </summary>
    private void DisplayUpgradeStatPreview()
    {
        previewItem.data.currentLevel = upgradeItem.data.currentLevel + 1;

        previewItem.levelText.color = Color.green;

        int len = upgradeItem.data.currentStat.Length;
        for (int i = 0; i < len; i++)
        {
            previewStats[i].DisplayInfo(true);

            string coloredStatText = "<color=green>" + upgradeItem.data.currentStat[i].GetNextValue().ToString() + "</color>";
            string normalStatText = upgradeItem.data.currentStat[i].GetNextValue().ToString();

            StatType type = upgradeItem.data.info.baseStat.stats[i].type;
            Sprite icon = ItemManager.Instance.statIconDict[type];
            previewStats[i].SetImage(icon);

            if (upgradeItem.data.currentStat[i].GetNextValue() == upgradeItem.data.currentStat[i].value)
            {
                previewStats[i].SetText(normalStatText);
            }
            else
            {
                previewStats[i].SetText(coloredStatText);
            }

        }
        for (int i = len; i < 3; i++)
        {
            previewStats[i].DisplayInfo(false);
        }
    }


    /// <summary>
    /// Hide UI of the upgrade item.
    /// </summary>
    private void DisplayUI(bool canDispplay)
    {
        upgradeItem.gameObject.SetActive(canDispplay);
        previewItem.gameObject.SetActive(canDispplay);
        foreach (UIStat stat in currentStats)
        {
            stat.DisplayInfo(canDispplay);
        }
        foreach (UIStat stat in previewStats)
        {
            stat.DisplayInfo(canDispplay);
        }
    }
}
