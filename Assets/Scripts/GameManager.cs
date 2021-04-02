using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour
{
    public TMP_Text scoreText;
    public TMP_Text cpsText;
    public TMP_Text tpsText;
    public Toggle particleToggle;
    
    [HideInInspector]
    public long clickAmount = 1;
    [HideInInspector]
    public int backgroundPercent;
    [HideInInspector]
    public long score;
    [HideInInspector]
    public float bgCps;

    public ParticleSystem particle1;
    public ParticleSystem particle2;
    public ParticleSystem particle3;

    private float _storedCps;
    private float _totalCps;
    private long _storedClicks;
    private long _countedClicks;

    private static readonly string[] Alphabet = {"a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m", "n", "o", "p", "q", "r", "s", "t", "u", "v", "w", "x", "y", "z"};
    private static readonly List<string> NFormat = new List<string>();

    private void Start()
    { 
        score = PlayerPrefs.GetString(nameof(score)) == "" ? 0 : Convert.ToInt64(PlayerPrefs.GetString(nameof(score)));
        scoreText.text = LargeNumberFormatting(Convert.ToDouble(score), true);
        
        clickAmount = PlayerPrefs.GetString(nameof(clickAmount)) == "" ? 1 : Convert.ToInt64(PlayerPrefs.GetString(nameof(clickAmount)));
        backgroundPercent = PlayerPrefs.GetInt(nameof(backgroundPercent));
        _storedCps = PlayerPrefs.GetFloat(nameof(_storedCps));
        bgCps = PlayerPrefs.GetFloat(nameof(bgCps));
        
        CalculateTimeDifference();

        InvokeRepeating(nameof(AddCps), 0f, 0.1f);
        InvokeRepeating(nameof(ManageCps), 0f, 1f);
    }

    private void Update()
    {
        _totalCps = _storedClicks + bgCps;
        cpsText.text = LargeNumberFormatting(Convert.ToDouble(_totalCps)) + " CPS";
        tpsText.text = LargeNumberFormatting(Convert.ToDouble(Convert.ToSingle(_storedClicks) / clickAmount)) + " TPS";
    }

    public void ButtonClick()
    {
        _countedClicks += clickAmount;
        
        IncreaseScore(clickAmount);
        if (_totalCps == 0)
        {
            EmitParticle(Convert.ToInt32(clickAmount > 20 ? 20 : clickAmount));
        }
    }

    public void RefreshScore()
    {
        scoreText.text = LargeNumberFormatting(Convert.ToDouble(score), true);
        PlayerPrefs.SetString(nameof(score), score.ToString());
    }

    private void IncreaseScore(long amount)
    {
        score += amount;
        scoreText.text = LargeNumberFormatting(Convert.ToDouble(score), true);
        
        PlayerPrefs.SetString(nameof(score), score.ToString());
    }

    private void EmitParticle(int particleAmount)
    {
        if(!particleToggle.isOn) return;
        
        var particleLayer = Random.Range(0, 3);
        var emitOverride = new ParticleSystem.EmitParams {startLifetime = 10f};

        switch (particleLayer)
        {
            case 0:
                particle1.Emit(emitOverride, particleAmount);
                break;
            case 1:
                particle2.Emit(emitOverride, particleAmount);
                break;
            case 2:
                particle3.Emit(emitOverride, particleAmount);
                break;
        }
    }

    private void AddCps()
    {
        _storedCps += bgCps / 10;
        if (!(_storedCps >= 1)) return;
        IncreaseScore(Mathf.FloorToInt(_storedCps));
        _storedCps -= Mathf.FloorToInt(_storedCps);
        
        PlayerPrefs.SetFloat(nameof(_storedCps), _storedCps);
        PlayerPrefs.SetFloat(nameof(bgCps), bgCps);
    }

    private void ManageCps()
    {
        // Count click CPS
        _storedClicks = _countedClicks;
        _countedClicks = 0;
        
        // Emit particles by CPS
        if (_totalCps <= 20)
        {
            EmitParticle(Convert.ToInt32(_totalCps / 2));
            EmitParticle(Convert.ToInt32(_totalCps / 2));
        }
        else if (_totalCps > 20 && _totalCps <= 100)
        {
            EmitParticle(_totalCps / 2 <= 20 ? 7 : Convert.ToInt32(_totalCps / 2 / 3));
            EmitParticle(_totalCps / 2 <= 20 ? 7 : Convert.ToInt32(_totalCps / 2 / 3));
            EmitParticle(_totalCps / 2 <= 20 ? 7 : Convert.ToInt32(_totalCps / 2 / 3));
        }
        else if (_totalCps > 100)
        {
            for (var i = 0; i < 8; i++)
            {
                EmitParticle(10);
            }
        }
    }

    private static void SaveCurrentTime()
    {
        PlayerPrefs.SetString("exitTime", DateTime.UtcNow.ToString("G"));
    }

    private static string LargeNumberFormatting(double value, bool isScore = false)
    {
        var num = 0;
        while (value >= 1000d)
        {
            num++;
            value /= 1000d;
        }

        if (isScore)
        {
            return num >= 1 ? value.ToString("##0.000") + NFormat[num] : value.ToString("##0.###") + NFormat[num];
        }
        return value.ToString("##0.#") + NFormat[num];
    }

    private void CalculateTimeDifference()
    {
        if(PlayerPrefs.GetString("exitTime") == "") return;
        var timeDifference = (DateTime.UtcNow - DateTime.Parse(PlayerPrefs.GetString("exitTime"))).TotalSeconds;
        if (timeDifference < 0)
        {
            FindObjectOfType<ShopManager>().GetComponent<ShopManager>().priceMultiplier *= 10f;
            FindObjectOfType<ShopManager>().GetComponent<ShopManager>().clickPriceMultiplier *= 10f;
            clickAmount = 1;
            PlayerPrefs.SetFloat("priceMultiplier", FindObjectOfType<ShopManager>().GetComponent<ShopManager>().priceMultiplier *= 10f);
            PlayerPrefs.SetFloat("clickPriceMultiplier", FindObjectOfType<ShopManager>().GetComponent<ShopManager>().clickPriceMultiplier *= 10f);
            PlayerPrefs.SetString(nameof(clickAmount), "1");
            return;
        }
        score += Convert.ToInt64(timeDifference * bgCps * (backgroundPercent / 100f));
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        if (!pauseStatus)
        {
            CalculateTimeDifference();
            return;
        }
        SaveCurrentTime();
    }

    private void OnApplicationQuit()
    {
        SaveCurrentTime();
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
