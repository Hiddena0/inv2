using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[System.Serializable]
public class InventoryItem : ItemBase
{
    public TextMeshProUGUI levelText;

    public override void UpdateItemImage()
    {
        base.UpdateItemImage();
        InventorySlot slot = GetComponentInParent<InventorySlot>();

        if (data.info.prop.countable)
        {
            if (slot)
            {
                slot.DisplayCountText(true, data.amount);
            }

            levelText.gameObject.SetActive(false);
        }
        else
        {
            if (slot)
            {
                slot.DisplayCountText(false, data.amount);
            }
            levelText.gameObject.SetActive(true);
            levelText.text = "+" + data.currentLevel.ToString();
        }
    }

    /// <param name="slotPos"></param> the slot that bears the item
    public void SetPosition(Transform slotPos)
    {
        transform.SetParent(slotPos);
        transform.SetSiblingIndex(4); //If it is the last => count text will be faded
        transform.localPosition = Vector3.zero;
        transform.localScale = Vector3.one;
        UpdateItemImage();
    }



}
