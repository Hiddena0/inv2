using UnityEngine.UI;
using UnityEngine;
using TMPro;

public class DropdownRarity : MonoBehaviour
{
    private TMP_Dropdown dropdown;
    
    private void Awake()
    {
        dropdown = GetComponent<TMP_Dropdown>();
        dropdown.value = 0;
        dropdown.onValueChanged.AddListener(delegate
        {
            InventoryManager.Instance.FilterRarity(dropdown.value);
        });
    }

    private void OnDestroy()
    {
        dropdown.onValueChanged.RemoveAllListeners();
    }
}
