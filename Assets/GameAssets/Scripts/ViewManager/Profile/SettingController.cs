using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Mail;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SettingController : MonoBehaviour, PageController
{
    public TMP_InputField searchBar;
    public TextMeshProUGUI messageText;
    public Button backButton;
    public GameObject[] items;

    public RectTransform changeThemeToggle;

    Theme gameTheme;
    [SerializeField]
    private bool back;
    private bool onMessage;
    public void onInit(Dictionary<string, object> data, Action<object> callback)
    {
        
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)&&back)
        {
            Back();
        }
    }
    void Start()
    {
        back=true;
        searchBar.onValueChanged.AddListener(SearchItems);
        backButton.onClick.AddListener(Back);
        backButton.onClick.AddListener(AudioController.Instance.OnButtonClick);
        changeThemeToggle.gameObject.GetComponent<Button>().onClick.AddListener(ChangeGameTheme);
        gameTheme=ApiDataHandler.Instance.LoadTheme();
        switch (gameTheme)
        {
            case Theme.Dark:
                GlobalAnimator.Instance.AnimateRectTransformX(changeThemeToggle, -13, 0.25f);
                changeThemeToggle.parent.GetComponent<Image>().color = new Color32(81, 14, 14, 255);
                changeThemeToggle.GetComponent<Image>().color = Color.white;
                break;
            case Theme.Light:
                GlobalAnimator.Instance.AnimateRectTransformX(changeThemeToggle, 13, 0.25f);
                changeThemeToggle.parent.GetComponent<Image>().color = new Color32(236, 219, 190, 255);
                changeThemeToggle.GetComponent<Image>().color = new Color32(218, 52, 52, 255);
                break;
        }
    }
    void SearchItems(string searchTerm)
    {
        if (string.IsNullOrEmpty(searchTerm))
        {
            foreach (var item in items)
            {
                item.SetActive(true);
            }
            return;
        }

        searchTerm = searchTerm.ToLower();

        foreach (var item in items)
        {
            TextMeshProUGUI[] texts = item.GetComponentsInChildren<TextMeshProUGUI>();
            bool matchFound = false;

            foreach (var text in texts)
            {
                string itemText = text.text.ToLower();
                if (itemText.Contains(searchTerm))
                {
                    matchFound = true;
                    break;
                }
            }
            item.SetActive(matchFound);
        }
    }
    public void Back()
    {
        StateManager.Instance.HandleBackAction(gameObject);
        StateManager.Instance.OpenFooter(null, null, false);
    }
    public void OnBackCheck(List<object> list)
    {
        back = true;
    }
    public void ChangeGameTheme()
    {
        AudioController.Instance.OnButtonClick();
        switch (gameTheme)
        {
            case Theme.Light:
                GlobalAnimator.Instance.AnimateRectTransformX(changeThemeToggle, -13, 0.25f);
                changeThemeToggle.parent.GetComponent<Image>().color = new Color32(81,14,14,255);
                changeThemeToggle.GetComponent<Image>().color = Color.white;
                ApiDataHandler.Instance.SaveTheme(Theme.Dark);
                gameTheme= Theme.Dark;
                break;
            case Theme.Dark:
                GlobalAnimator.Instance.AnimateRectTransformX(changeThemeToggle, 13, 0.25f);
                changeThemeToggle.parent.GetComponent<Image>().color = new Color32(236, 219, 190, 255);
                changeThemeToggle.GetComponent<Image>().color = new Color32(218, 52, 52, 255);
                ApiDataHandler.Instance.SaveTheme(Theme.Light);
                gameTheme= Theme.Light;
                break;
        }
        GUISetting[] scripts = FindObjectsOfType<GUISetting>();
        foreach (GUISetting script in scripts)
        {
            script.SetTheme();
        }
    }
    public void ChangeWeightUnit()
    {
        back = false;
        PopupController.Instance.OpenPopup("profile", "ChangeUnitPopup", OnBackCheck, null);
    }


    public void ChangeBadge()
    {
        //PopupController.Instance.OpenPopup("profile", "ChangeBadgePopup", null, null);
        Dictionary<string, object> mData = new Dictionary<string, object> { { "data", false } };
        StateManager.Instance.OpenStaticScreen("profile", gameObject, "ChangeBadgeScreen", mData, true);
    }
    public bool testing = false;
    public void ChangeWeeklyGoal()
    {
        AudioController.Instance.OnButtonClick();
        DateTime now = DateTime.Now;
        //print(now + "    " + ApiDataHandler.Instance.GetCurrentWeekStartDate());
        TimeSpan timeDifference = now - userSessionManager.Instance.weeklyGoalSetDate;
        if (testing || timeDifference.TotalDays >= 14 || userSessionManager.Instance.weeklyGoal == 0)
        {
            Dictionary<string, object> mData = new Dictionary<string, object> { { "data", false } };
            StateManager.Instance.OpenStaticScreen("profile", gameObject, "weeklyGoalScreen", mData, true);
            StateManager.Instance.CloseFooter();
        }
        else
        {
            if (!onMessage)
            {
                print(timeDifference.TotalDays);
                onMessage = true;
                GlobalAnimator.Instance.ShowTextMessage(messageText, "This can only be changed once every 2 weeks.", 2f);
                StartCoroutine(OffMessageText(2));
            }
        }


    }
    public void ContactUs()
    {
        StateManager.Instance.OpenStaticScreen("profile", gameObject, "ContactUsScreen", null, true);
        //string mailto = $"mailto:{"stregnthclanfitness@gmail.com"}";
        //Application.OpenURL(mailto);
    }
    public void RPE_RIR_Popup()
    {
        back = false;
        PopupController.Instance.OpenPopup("profile", "RPE_RIR_Popup", OnBackCheck, null);
    }
    public void LogOut()
    {
        FirebaseManager.Instance.OnLogout();
        ApiDataHandler.Instance.LogOut();
        userSessionManager.Instance.Logout();
        PlayerPrefs.DeleteAll();
        Dictionary<string, object> mData = new Dictionary<string, object>
            {
                { AuthKey.sAuthType, AuthConstant.sAuthTypeLogin}
            };
        StateManager.Instance.DeleteFooter();
        StateManager.Instance.OpenStaticScreen("auth", this.gameObject, "authScreen", mData);
    }
    public void Subscrition()
    {
        IAPManager.Instance.Subscription_Btn_Pressed();
    }
    public void TermsAndConditions()
    {
        StateManager.Instance.OpenStaticScreen("profile", gameObject, "TermsAndConditions", null, true);
    }
    public void PrivacyPolicy()
    {
        StateManager.Instance.OpenStaticScreen("profile", gameObject, "PrivacyPolicy", null, true);
    }
    IEnumerator OffMessageText(float time)
    {
        yield return new WaitForSeconds(time);
        onMessage = false;
    }
    public void OnButtonClick()
    {
        AudioController.Instance.OnButtonClick();
    }
}
