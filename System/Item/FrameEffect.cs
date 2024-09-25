using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FrameEffect : MonoBehaviour
{
    public void SetMaterial(Material material)
    {
        Enable(true);
        GetComponent<Image>().material = material;   
    }
    public void Enable(bool isOn)
    {
        gameObject.SetActive(isOn);
    }
}
