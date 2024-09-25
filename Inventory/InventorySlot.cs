using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class InventorySlot : MonoBehaviour, IDropHandler
{
    public bool isEmpty;
    public bool isLocked;
    public GameObject lockObject;
    public Toggle Toggle;
    public TextMeshProUGUI countText;
    private void Start()
    {
        countText.gameObject.SetActive(false);
        if (!isLocked)
        {
            lockObject.SetActive(false);
        }
        else
        {
            lockObject.SetActive(true);
        } 
        
        if (Toggle)
        {
            Toggle.group = GetComponentInParent<ToggleGroup>();
        }
    }

    public void DisplayCountText(bool bl, int amount)
    {
        countText.gameObject.SetActive(bl);
        countText.text = amount.ToString();
    }

    public InventoryItem GetItemType()
    {
        return GetComponentInChildren<InventoryItem>();
    }

    public void DestroyItem()
    {
        countText.gameObject.SetActive(false);
        Destroy(GetItemType().gameObject);
        isEmpty = true;
    }


    public void OnDrop(PointerEventData eventData)
    {
        if (isEmpty)
        {
            GameObject dropped = eventData.pointerDrag;
            DraggableItem draggableItem = dropped.GetComponent<DraggableItem>();
            if (draggableItem != null)
            {
                draggableItem.parentAfterDrag = this.transform;
                draggableItem.GetComponent<InventoryItem>().UpdateItemImage();
                isEmpty = false;
            }
        }
    }
}
