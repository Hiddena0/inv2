using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UIStat : MonoBehaviour
{
    public TextMeshProUGUI statText;
    public Image statImage;

    public void DisplayInfo(bool canDisplay)
    {
        statText.gameObject.SetActive(canDisplay);
        statImage.gameObject.SetActive(canDisplay);
    }

    public void SetText(string text)
    {
        statText.text = text;
    }

    public void SetImage(Sprite sprite)
    {
        statImage.sprite = sprite;
    }
}
