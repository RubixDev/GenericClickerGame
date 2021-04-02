using System;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;

public class ShopManager : MonoBehaviour
{
    public GameObject shopPanel;
    public GameObject gameManager;
    public GameObject closeButton;
    public GameObject menuButton;
    public TMP_Text moreClicksAmountText;
    public TMP_Text backgroundPercentText;
    public int clickMultiplier = 2;
    public float priceMultiplier = 1.103f;
    public float clickPriceMultiplier = 2.193f;

    public ShopItems[] shopItems;
    
    private static readonly int Open = Animator.StringToHash("open");
    private static readonly int ShopOpen = Animator.StringToHash("shopOpen");
    private static readonly string[] Alphabet = {"a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m", "n", "o", "p", "q", "r", "s", "t", "u", "v", "w", "x", "y", "z"};
    private static readonly List<string> NFormat = new List<string>();

    public void ToggleCloseButton()
    {
        closeButton.SetActive(!closeButton.activeSelf);
    }
    
    public void ToggleShop()
    {
        var animator = shopPanel.GetComponent<Animator>();
        var isOpen = animator.GetBool(Open);
        animator.SetBool(Open, isOpen == false);

        var animator2 = menuButton.GetComponent<Animator>();
        var shopOpen = animator2.GetBool(ShopOpen);
        animator2.SetBool(ShopOpen, shopOpen == false);
    }

    public void PurchaseItem(string itemName)
    {
        var i = Array.Find(shopItems, shopItem => shopItem.name == itemName);

        if (gameManager.GetComponent<GameManager>().score >= i.price)
        {
            if(itemName == "BackgroundClicking" && gameManager.GetComponent<GameManager>().backgroundPercent >= 100) return;
            gameManager.GetComponent<GameManager>().score -= i.price;
            FindObjectOfType<AudioManager>().Play("PurchaseItem");
            i.level++;
            
            switch (itemName)
            {
                case "MoreClicks":
                    gameManager.GetComponent<GameManager>().clickAmount *= clickMultiplier;
                    i.price = Convert.ToInt64(Math.Round(i.price * clickPriceMultiplier));
                    moreClicksAmountText.text = "x" + LargeNumberFormatting(Convert.ToDouble(Mathf.Pow(2f, i.level)));
                    break;
                case "BackgroundClicking":
                    gameManager.GetComponent<GameManager>().backgroundPercent += 5;
                    i.price = Convert.ToInt64(Math.Round(i.price * clickPriceMultiplier));
                    backgroundPercentText.text = gameManager.GetComponent<GameManager>().backgroundPercent + "%";
                    break;
                default:
                    i.price = Convert.ToInt64(Math.Round(i.price * priceMultiplier));
                    gameManager.GetComponent<GameManager>().bgCps += i.cps;
                    break;
            }
        }

        i.priceText.text = "Price: " + LargeNumberFormatting(Convert.ToDouble(i.price));
        i.levelText.text = "Level: " + i.level;
        PlayerPrefs.SetString(i.name + "Price", i.price.ToString());
        PlayerPrefs.SetInt(i.name + "Level", i.level);
        PlayerPrefs.SetString("clickAmount", gameManager.GetComponent<GameManager>().clickAmount.ToString());
        PlayerPrefs.SetInt("backgroundPercent", gameManager.GetComponent<GameManager>().backgroundPercent);
        
        gameManager.GetComponent<GameManager>().RefreshScore();
        GreyOutItems();
    }

    private void Start()
    {
        priceMultiplier = PlayerPrefs.GetInt(nameof(priceMultiplier)) == 0 ? priceMultiplier : PlayerPrefs.GetInt(nameof(priceMultiplier));
        clickPriceMultiplier = PlayerPrefs.GetInt(nameof(clickPriceMultiplier)) == 0 ? clickPriceMultiplier : PlayerPrefs.GetInt(nameof(clickPriceMultiplier));
        
        foreach (var shopItem in shopItems)
        {
            shopItem.price = PlayerPrefs.GetString(shopItem.name + "Price") == "" ? shopItem.price : Convert.ToInt64(PlayerPrefs.GetString(shopItem.name + "Price"));
            shopItem.level = PlayerPrefs.GetInt(shopItem.name + "Level");
            shopItem.priceText.text = "Price: " + LargeNumberFormatting(Convert.ToDouble(shopItem.price));
            shopItem.levelText.text = "Level: " + shopItem.level;
            shopItem.cpsText.text = "+" + shopItem.cps.ToString(CultureInfo.CurrentCulture) + " CPS";

            switch (shopItem.name)
            {
                case "MoreClicks":
                    moreClicksAmountText.text = "x" + LargeNumberFormatting(Convert.ToDouble(Mathf.Pow(2f, shopItem.level)));
                    break;
                case "BackgroundClicking":
                    backgroundPercentText.text = gameManager.GetComponent<GameManager>().backgroundPercent + "%";
                    break;
            }
        }
        
        InvokeRepeating(nameof(GreyOutItems), 0f, 0.2f);
    }

    private void GreyOutItems()
    {
        if (!closeButton.activeSelf) return;
        foreach (var shopItem in shopItems)
        {
            shopItem.button.GetComponent<CanvasGroup>().alpha = gameManager.GetComponent<GameManager>().score >= shopItem.price ? 1f : 0.7f;
            if (shopItem.name == "BackgroundClicking" && gameManager.GetComponent<GameManager>().backgroundPercent >= 100)
            {
                shopItem.button.GetComponent<CanvasGroup>().alpha = 0.7f;
            }
        }
    }
    
    private static string LargeNumberFormatting(double value, bool isScore = false)
    {
        var num = 0;
        while (value >= 1000d)
        {
            num++;
            value /= 1000d;
        }
        return isScore ? value.ToString("##0.###") + NFormat[num] : value.ToString("##0.#") + NFormat[num];
    }
    
    private void Awake()
    {
        NFormat.Add("");
        NFormat.Add("K");
        NFormat.Add("M");
        NFormat.Add("B");
        NFormat.Add("T");
        
        for (var i = 0; i < 26; i++)
        {
            for (var j = 0; j < 26; j++)
            {
                NFormat.Add(Alphabet[i] + Alphabet[j]);
            }
        }
    }
}
