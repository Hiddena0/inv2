using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class ItemManager : Singleton<ItemManager>
{
    [System.Serializable]
    public struct rarityBackGround
    {
        public Rarity rarity;
        public Sprite background;
    }

    public rarityBackGround[] rarityBackgrounds;
    [SerializeField]
    public Dictionary<Rarity, Sprite> rarityBackgroundDict;


    [System.Serializable]
    public struct statIcon
    {
        public StatType stat;
        public Sprite icon;
    }
    public statIcon[] statIcons;
    [SerializeField]
    public Dictionary<StatType, Sprite> statIconDict;


    [System.Serializable]
    public struct frameMaterial
    {
        public Rarity rarity;
        public Material material;
    }
    public frameMaterial[] frameMaterials;
    [SerializeField]
    public Dictionary<Rarity, Material> frameMaterialDict;

    public Material shiningMaterial;

    private void Awake()
    {
        InitializeRarityBackgroundDictionary();
        InitializeStatIconDictionary();
        InitializeFrameMaterialDict();
    }

    public void InitializeRarityBackgroundDictionary()
    {
        rarityBackgroundDict = new Dictionary<Rarity, Sprite>();
        foreach (rarityBackGround rarity in rarityBackgrounds)
        {
            rarityBackgroundDict.Add(rarity.rarity, rarity.background);
        }
    }
    public void InitializeStatIconDictionary()
    {
        statIconDict = new Dictionary<StatType, Sprite>();
        foreach (statIcon icon in statIcons)
        {
            statIconDict.Add(icon.stat, icon.icon);
        }
    }

    public void InitializeFrameMaterialDict()
    {
        frameMaterialDict = new Dictionary<Rarity, Material>();
        foreach (frameMaterial frame in frameMaterials)
        {
            frameMaterialDict.Add(frame.rarity, frame.material);
        }
    }
    public Rarity GetRandomShopDailyRarity()
    {
        int rand = Random.Range(0, 101);
        if (rand <= 5) return Rarity.Epic;
        else if (rand <= 15) return Rarity.Rare;
        else if (rand <= 45) return Rarity.Uncommon;
        else return Rarity.Common;
    }
}