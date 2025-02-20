using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GUISetting : MonoBehaviour
{
    public Image background;
    public Image playButton;
    public Image character;
    public Image xpBar, xpButton;
    public Image characterScreenOffOn;
    [SerializeField]
    private List<TextMeshProUGUI> primaryText = new List<TextMeshProUGUI>();
    [SerializeField]
    private List<TextMeshProUGUI> primaryBoldText = new List<TextMeshProUGUI>();
    [SerializeField]
    private List<TextMeshProUGUI> primarySemiBoldText = new List<TextMeshProUGUI>();
    [SerializeField]
    private List<TextMeshProUGUI> primaryMediumBoldText = new List<TextMeshProUGUI>();
    [SerializeField]
    private List<TextMeshProUGUI> secondaryText = new List<TextMeshProUGUI>();
    [SerializeField]
    private List<TextMeshProUGUI> historySubText = new List<TextMeshProUGUI>();
    [SerializeField]
    private List<TextMeshProUGUI> buttonText = new List<TextMeshProUGUI>();
    [SerializeField]
    private List<TMP_InputField> inputFields= new List<TMP_InputField>();
    [SerializeField]
    private List<Image> inputFieldSearchIcon= new List<Image>();
    [SerializeField]
    private List<Image> buttons = new List<Image>();
    [SerializeField]
    private List<Image> button2 = new List<Image>();
    [SerializeField]
    private List<Image> workoutLogDropdownToggle = new List<Image>();
    [SerializeField]
    private List<Image> line = new List<Image>();
    [SerializeField]
    private List<Shadow> shadow = new List<Shadow>();
    [SerializeField]
    private List<Outline> outlines = new List<Outline>();
    public List<TMP_Dropdown> dropdowns = new List<TMP_Dropdown>();
    public List<TextMeshProUGUI> achievementCountText=new List<TextMeshProUGUI>();
    public TextMeshProUGUI switchButton1,switchButton2;
    public List<TextMeshProUGUI> personlBestAndMeasurementExercise=new List<TextMeshProUGUI>();

    public Sprite darkbackground, lightbackground;
    public Sprite lightPlay, darkPlay;
    public Sprite lightCharacter, darkCharacter;
    private void OnEnable()
    {
        SetTheme();
    }
    private void Awake()
    {
        //SetTheme();
    }
    public void SetTheme()
    {
        switch (ApiDataHandler.Instance.gameTheme)
        {
            case Theme.Light:
                if (playButton != null)
                    playButton.sprite = lightPlay;
                if(character!=null)
                    character.sprite = lightCharacter;
                if (xpBar != null)
                    xpBar.color = userSessionManager.Instance.lightInputFieldColor;
                if (xpButton != null)
                    xpButton.color = userSessionManager.Instance.lightXPbutton;
                //if (characterScreenOffOn != null)
                    //characterScreenOffOn.enabled = true;

                SetShadow(true);
                SetPersonalBestAndMeasurementText(userSessionManager.Instance.lightPrimaryFontSemiBold, new Color32(92,59,28,255));
                setBackground(lightbackground);
                //SetPrimaryText(userSessionManager.Instance.lightPrimaryFont, userSessionManager.Instance.lightButtonTextColor);
                SetPrimaryBoldText(userSessionManager.Instance.lightPrimaryFontBold, userSessionManager.Instance.lightButtonTextColor);
                SetPrimaryMediumBoldText(userSessionManager.Instance.lightPrimaryFontMediumBold, userSessionManager.Instance.lightButtonTextColor);
                SetPrimarySemiBoldText(userSessionManager.Instance.lightPrimaryFontSemiBold, userSessionManager.Instance.lightButtonTextColor);
                SetSecondaryText(userSessionManager.Instance.lightSecondaryFont, userSessionManager.Instance.lightButtonTextColor);
                SetHistorySubText(userSessionManager.Instance.lightSecondaryFont, new Color32(92, 59, 28, 155));
                SetButtonColor(userSessionManager.Instance.lightButtonColor);
                SetButtonTextColor(Color.white);
                SetButton2Color(userSessionManager.Instance.lightInputFieldColor);
                SetInputFields(userSessionManager.Instance.lightPrimaryFontBold, userSessionManager.Instance.lightButtonTextColor, userSessionManager.Instance.lightPlaceholder,Color.white);
                SetDropdowns(userSessionManager.Instance.lightPrimaryFontBold, userSessionManager.Instance.lightButtonTextColor, userSessionManager.Instance.lightPlaceholder,Color.white);
                SetOutlines(userSessionManager.Instance.lightButtonColor);
                SetWorkoutLogDropdownToggle(userSessionManager.Instance.lightInputFieldColor);
                SetLineColor(userSessionManager.Instance.lineColor);
                SetSwitchColor(Color.white,userSessionManager.Instance.lightPlaceholder);
                SetAchievementText(userSessionManager.Instance.lightSecondaryFont, Color.white);
                break;
            case Theme.Dark:
                if(playButton != null)
                    playButton.sprite = darkPlay;
                if (character != null)
                    character.sprite = darkCharacter;
                if (xpBar != null)
                    xpBar.color = userSessionManager.Instance.darkButtonTextColor;
                if (xpButton != null)
                    xpButton.color = userSessionManager.Instance.darkXPbutton;
                //if (characterScreenOffOn != null)
                    //characterScreenOffOn.enabled = false;
                SetShadow(false);
                SetPersonalBestAndMeasurementText(userSessionManager.Instance.darkPrimaryFont, new Color32(186, 172, 172, 255));
                setBackground(darkbackground);
                SetPrimaryText(userSessionManager.Instance.darkPrimaryFont, Color.white);
                SetSecondaryText(userSessionManager.Instance.darkSecondaryFont,Color.white);
                SetHistorySubText(userSessionManager.Instance.darkSecondaryFont, new Color32(217, 217, 217, 127));
                SetButtonColor(userSessionManager.Instance.darkButtonColor);
                SetButtonTextColor(userSessionManager.Instance.darkButtonTextColor);
                SetButton2Color(userSessionManager.Instance.darkButtonTextColor);
                SetInputFields(userSessionManager.Instance.darkPrimaryFont, new Color32(255, 255, 255, 255), userSessionManager.Instance.darkPlaceholder,userSessionManager.Instance.darkInputFieldColor);
                SetDropdowns(userSessionManager.Instance.darkPrimaryFont, new Color32(255, 255, 255, 255), userSessionManager.Instance.darkPlaceholder,userSessionManager.Instance.darkInputFieldColor);
                SetOutlines(userSessionManager.Instance.darkButtonColor);
                SetWorkoutLogDropdownToggle(userSessionManager.Instance.darkInputFieldColor);
                SetLineColor(userSessionManager.Instance.lineColor);
                SetSwitchColor(userSessionManager.Instance.darkButtonTextColor, userSessionManager.Instance.darkSwitchTextColor);
                SetAchievementText(userSessionManager.Instance.darkSecondaryFont, new Color32(226, 136, 0, 255));
                break;
        }
    }
    public void SetPersonalBestAndMeasurementText(TMP_FontAsset font,Color col)
    {
        foreach(TextMeshProUGUI text in personlBestAndMeasurementExercise)
        {
            text.color = col;
            text.font = font;
        }
    }
    public void setBackground(Sprite sprite)
    {
        if(background != null) background.sprite = sprite;
    }
    public void SetPrimaryText(TMP_FontAsset font, Color col)
    {
        foreach (TextMeshProUGUI text in primaryText)
        {
            text.font = font;
            text.color = col;
        }
    }
    public void SetPrimaryBoldText(TMP_FontAsset font, Color col)
    {
        foreach (TextMeshProUGUI text in primaryBoldText)
        {
            text.font = font;
            text.color = col;
        }
    }
    public void SetPrimarySemiBoldText(TMP_FontAsset font, Color col)
    {
        foreach (TextMeshProUGUI text in primarySemiBoldText)
        {
            text.font = font;
            text.color = col;
        }
    }
    public void SetPrimaryMediumBoldText(TMP_FontAsset font, Color col)
    {
        foreach (TextMeshProUGUI text in primaryMediumBoldText)
        {
            text.font = font;
            text.color = col;
        }
    }
    public void SetSecondaryText(TMP_FontAsset font, Color col)
    {
        foreach (TextMeshProUGUI text in secondaryText)
        {
            text.font = font;
            text.color = col;
            //print("sec");
        }
    }
    public void SetAchievementText(TMP_FontAsset font, Color col)
    {
        foreach (TextMeshProUGUI text in achievementCountText)
        {
            text.font = font;
            text.color = col;
        }
    }
    public void SetShadow(bool enable)
    {
        foreach(Shadow sh in shadow)
        {
            sh.enabled = enable;
        }
    }
    public void SetHistorySubText(TMP_FontAsset font, Color col)
    {
        foreach (TextMeshProUGUI text in historySubText)
        {
            text.font = font;
            text.color = col;
            //print("sec");
        }
    }
    public void SetButtonTextColor(Color col)
    {
        foreach (TextMeshProUGUI text in buttonText)
        {
            text.color = col;
        }
    }
    public void SetButtonColor(Color col)
    {
        foreach(Image img in buttons)
        {
            img.color = col;
        }
    }
    public void SetButton2Color(Color col)
    {
        foreach (Image img in button2)
        {
            img.color = col;
        }
    }
    public void SetInputFields(TMP_FontAsset textFont, Color textCol, Color placeHolderCol,Color imageColor)
    {
        foreach(TMP_InputField inputField in inputFields)
        {
            inputField.GetComponent<Image>().color = imageColor;
            inputField.textComponent.font = textFont;
            inputField.textComponent.color = textCol;
            inputField.placeholder.color= placeHolderCol;
        }
        foreach(Image img in inputFieldSearchIcon)
        {
            img.color = placeHolderCol;
        }
    }
    public void SetDropdowns(TMP_FontAsset textFont, Color textCol, Color placeHolderCol, Color imageColor)
    {
        foreach (TMP_Dropdown dropdown in dropdowns)
        {
            dropdown.GetComponent<Image>().color = imageColor;
            dropdown.captionText.font = textFont;
            dropdown.captionText.color = textCol;
            dropdown.template.gameObject.SetActive(true);
            dropdown.template.GetComponent<Image>().color = imageColor;
            dropdown.template.gameObject.SetActive(false);
        }
    }
    public void SetOutlines(Color col)
    {
        foreach(Outline item in outlines)
        {
            item.effectColor = col;
        }
    }
    public void SetWorkoutLogDropdownToggle(Color col)
    {
        foreach(Image img in workoutLogDropdownToggle)
        {
            img.color = col;
        }
    }
    public void SetLineColor(Color col)
    {
        foreach(Image img in line)
        {
            img.color = col;
        }
    }
    public void SetSwitchColor(Color col1,Color col2)
    {
        if(switchButton1!=null)
            switchButton1.color= col1;
        if(switchButton2!=null)
        switchButton2.color= col2;
    }
}
