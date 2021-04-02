using System;
using TMPro;
using UnityEngine;

[Serializable]
public class ShopItems
{
    public string name;

    public GameObject button;

    public TMP_Text priceText;
    public TMP_Text levelText;
    public TMP_Text cpsText;

    public long price;
    public float cps;
    [HideInInspector]
    public int level;
}
