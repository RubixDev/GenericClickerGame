using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    public GameObject menuButton;
    public GameObject resetWarning;
    public GameObject mainCamera;
    public GameObject particle1;
    public GameObject particle2;
    public GameObject particle3;
    public Toggle particleToggle;
    public TMP_InputField colorTextField;
    public Sprite closedImage;
    public Sprite openedImage;
    
    private static readonly int IsOpen = Animator.StringToHash("isOpen");

    public void ToggleMenu()
    {
        var animator = gameObject.GetComponent<Animator>();
        var isOpen = animator.GetBool(IsOpen);
        animator.SetBool(IsOpen, isOpen == false);

        menuButton.GetComponent<Image>().sprite = isOpen ? closedImage : openedImage;
    }

    public void ToggleResetWarning()
    {
        if (!resetWarning.activeSelf)
        {
            resetWarning.SetActive(true);
            var animator = resetWarning.GetComponent<Animator>();
            var isOpen = animator.GetBool(IsOpen);
            animator.SetBool(IsOpen, isOpen == false);
        }
        else
        {
            var animator = resetWarning.GetComponent<Animator>();
            var isOpen = animator.GetBool(IsOpen);
            animator.SetBool(IsOpen, isOpen == false);
            StartCoroutine(DeactivateResetWarning());
        }
    }

    public void DeleteData()
    {
        PlayerPrefs.DeleteAll();
        ToggleResetWarning();
        SceneManager.LoadScene(0);
    }

    public void ChangeBgColor(string hexCode)
    {
        if (hexCode.Length != 6) return;
        if (hexCode == "RUBIX5")
        {
            FindObjectOfType<GameManager>().clickAmount *= 10;
            FindObjectOfType<ShopManager>().priceMultiplier = 1;
            FindObjectOfType<ShopManager>().clickPriceMultiplier = 1;
            return;
        }
        var r = Convert.ToInt32(hexCode.Substring(0, 2), 16) / 255f;
        var g = Convert.ToInt32(hexCode.Substring(2, 2), 16) / 255f;
        var b = Convert.ToInt32(hexCode.Substring(4, 2), 16) / 255f;
        mainCamera.GetComponent<Camera>().backgroundColor = new Color(r, g, b);
        PlayerPrefs.SetString("bgColor", hexCode);
        colorTextField.SetTextWithoutNotify(hexCode.ToUpper());
    }

    public void TextFieldToUpper(string hexCode)
    {
        colorTextField.SetTextWithoutNotify(hexCode.ToUpper());
    }

    public void BgColorButton()
    {
        ChangeBgColor(colorTextField.text);
    }

    public void ToggleParticles(bool isOn)
    {
        particle1.SetActive(isOn);
        particle2.SetActive(isOn);
        particle3.SetActive(isOn);
        particleToggle.isOn = isOn;
        PlayerPrefs.SetString("particlesOn", isOn.ToString());
    }

    private IEnumerator DeactivateResetWarning()
    {
        yield return new WaitForSeconds(0.5f);
        resetWarning.SetActive(false);
    }

    private void Start()
    {
        ChangeBgColor(PlayerPrefs.GetString("bgColor")  == "" ? "B4B4B4" : PlayerPrefs.GetString("bgColor"));
        
        if(PlayerPrefs.GetString("particlesOn") == "") return;
        ToggleParticles(PlayerPrefs.GetString("particlesOn").Equals("True"));
    }
}
