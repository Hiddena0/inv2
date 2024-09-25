using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Linq;

/// <summary>
/// Reference: https://www.youtube.com/watch?v=kWRyZ3hb1Vc&ab_channel=CocoCode
/// </summary>
public class DraggableItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerDownHandler
{
    public Image image;
    [HideInInspector]
    public Transform initialParent;
    public Transform parentAfterDrag;

    public InventorySlot inventorySlot;
    public EquipmentSlot equipmentSlot;

    public EquipmentSlot equipableSlot;

    private InventoryItem thisItem;

    
    private bool allowDrag = true;
    private bool allowEquip = false;
    private bool allowUnequip = false;

    private void ResetParam()
    {
        allowEquip = false;
        allowUnequip = false;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        Debug.Log("selected");
        thisItem = GetComponent<InventoryItem>();
        thisItem.gameObject.GetComponentInParent<Toggle>().isOn = true; //this is how you select an item slot
        //allowDrag = true;

    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        thisItem = GetComponent<InventoryItem>();
        if (thisItem.gameObject.GetComponentInParent<Toggle>().isOn != true)
        {
            allowDrag = false;
            return;
        }
        
        //Debug.Log("Begin drag");

        initialParent = transform.parent;
        parentAfterDrag = transform.parent;

        // Attempt to get the InventorySlot component
        inventorySlot = transform.parent.GetComponent<InventorySlot>();
        if (inventorySlot != null)
        {
            allowEquip = true;
            inventorySlot.DisplayCountText(false, 0);
            inventorySlot.isEmpty = true;
            InventoryManager.Instance.scrollRect.vertical = false;// Avoid moving inventory with mouse

            if (GetComponent<InventoryItem>().data.info.prop.countable == false)
            {
                equipableSlot = InventoryManager.Instance.equipmentSlots.Single(i => i.type == thisItem.data.info.baseStat.type); //If the item is stackable, Single() will throw exception
            }
            //Debug.Log(equipableSlot);
        }

        // Attempt to get the EquipmentSlot component
        equipmentSlot = transform.parent.GetComponent<EquipmentSlot>();
        if (equipmentSlot != null)
        {
            allowUnequip = true;
            equipmentSlot.isEquip = false;
        }

        transform.SetParent(transform.root);
        transform.SetAsLastSibling();
        image.raycastTarget = false;
    }   
    

    public void OnDrag(PointerEventData eventData)
    {
        if (!allowDrag) return;
        //Debug.Log("Dragging");

        Vector3 mousePositionScreen = Input.mousePosition;
        mousePositionScreen.z = 0; // Set the z-component to 0
        mousePositionScreen.x -= Screen.width / 2;
        mousePositionScreen.y -= Screen.height / 2;

        transform.localPosition = mousePositionScreen;

        GameObject target = eventData.pointerCurrentRaycast.gameObject;
        //Debug.Log("Target:" + target);


        if (InventoryManager.Instance.ActiveSlot.GetComponentInChildren<InventorySlot>() != null)
        {
            if (equipableSlot == null) return;
            if (allowEquip && target.CompareTag("EquipField"))
            {
                InventoryManager.Instance.equipField.SetActive(true);
                equipableSlot.ShowCanEquip();
            } 
            else
            {
                InventoryManager.Instance.equipField.SetActive(false);
                equipableSlot.ShowCannotEquip();
            }
        } 
        else 
        {
            if (allowUnequip && target.CompareTag("InventoryField"))
            {
                InventoryManager.Instance.inventoryField.SetActive(true);
            }
            else
            {
                InventoryManager.Instance.inventoryField.SetActive(false);
            }
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!allowDrag)
        {
            allowDrag = true;
            return;
        }
        thisItem.SetPosition(parentAfterDrag);
        InventoryManager.Instance.inventoryField.SetActive(false);
        InventoryManager.Instance.equipField.SetActive(false);
        InventoryManager.Instance.scrollRect.vertical = true;
        thisItem.gameObject.GetComponentInParent<Toggle>().isOn = true;
        GameObject target = eventData.pointerEnter;

        if (!thisItem.data.info.prop.countable)
        {
            if (target.CompareTag("InventoryField"))
            {
                InventoryManager.Instance.UnequipItem(thisItem);
            }
            if (target.CompareTag("EquipField"))
            {
                if (!equipableSlot.isEquip) // EquipItem
                {
                    InventoryManager.Instance.EquipItem(thisItem, equipableSlot);
                }
                else
                {
                    InventoryItem equippedItem = equipableSlot.GetComponentInChildren<InventoryItem>();
                    InventoryManager.Instance.ReplaceItem(thisItem, equippedItem);
                }
            }
        }

        InventoryItem targetItem = target.GetComponent<InventoryItem>();
        if (targetItem != null)
        {
            InventoryManager.Instance.ReplaceItem(thisItem, targetItem);
            target.gameObject.GetComponentInParent<Toggle>().isOn = true;
        }
        if (initialParent == transform.parent)
        {
            inventorySlot = GetComponentInParent<InventorySlot>();
            if (inventorySlot != null)
            {
                inventorySlot.isEmpty = false;
            }
            equipmentSlot = GetComponentInParent<EquipmentSlot>();
            if (equipmentSlot != null)
            {
                equipmentSlot.isEquip = true;
            }
        }

        ResetParam();
        image.raycastTarget = true;
    }
    
}
