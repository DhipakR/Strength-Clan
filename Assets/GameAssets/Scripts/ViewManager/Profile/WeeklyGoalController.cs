using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class WeeklyGoalController : MonoBehaviour, PageController
{
    public TMP_Dropdown dropDown;
    public TextMeshProUGUI messageText;
    public Button continueButton;
    public Button infoButton;
    TextMeshProUGUI goalText = null;
    public Button backButton;
    bool firstTime;
    public void onInit(Dictionary<string, object> data, Action<object> callback)
    {
        StateManager.Instance.ShiftStep(AccountCreationStep.WeeklyGoal);

        firstTime = true;
        if (data != null)
        {
            firstTime = (bool)data["data"];
        }
        if (data != null && data.ContainsKey("text"))
        {
            goalText = (TextMeshProUGUI)data["text"];
        }
        continueButton.onClick.AddListener(Continue);
        continueButton.onClick.AddListener(AudioController.Instance.OnButtonClick);
        //dropDown.gameObject.AddComponent<Button>();
        //dropDown.gameObject.GetComponent<Button>().onClick.AddListener(AudioController.Instance.OnButtonClick);
        infoButton.onClick.AddListener(AudioController.Instance.OnButtonClick);
        if (firstTime)
        {
            backButton.onClick.AddListener(() => StateManager.Instance.Backer(gameObject));
        }
        else
        {
            backButton.gameObject.SetActive(true);
            backButton.onClick.AddListener(() => StateManager.Instance.HandleBackAction(gameObject));
        }
    }
    public void OnDropdownClick()
    {
        AudioController.Instance.OnButtonClick();
    }
    public void Continue()
    {
        ApiDataHandler.Instance.SetWeeklyGoal(dropDown.value + 2);
        ApiDataHandler.Instance.WeeklyGoalSetDate(DateTime.Now);
        ApiDataHandler.Instance.SetCurrentWeekStartDate(DateTime.Now);
        if (goalText != null) { goalText.text = userSessionManager.Instance.weeklyGoal.ToString(); }
        PreferenceManager.Instance.SetBool("FirstTimePlanInitialized_" /*+ userSessionManager.Instance.mProfileUsername*/, false);
        //userSessionManager.Instance.AddGymVisit();
        if (firstTime)
        {
            StateManager.Instance.OpenStaticScreen("weight", gameObject, "weightScreen", null);
        }
        else
        {
            StateManager.Instance.HandleBackAction(gameObject);
        }
    }

    public void ShowDetails()
    {
        GlobalAnimator.Instance.ShowTextMessage(messageText, "This can only be changed once every 2  weeks.",2f);
    }
}
