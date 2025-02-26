using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Windows;

public class DateController : MonoBehaviour,PageController
{
    public TextMeshProUGUI messageText;
    public TMP_InputField monthInput;
    public TMP_InputField yearInput;
    public Button continueButton;
    int month;
    int year=DateTime.Now.Year;
    public Button backButton;
    bool firstTime;
    public void onInit(Dictionary<string, object> data, Action<object> callback)
    {
        StateManager.Instance.ShiftStep(AccountCreationStep.JoiningDate);

        firstTime = true;
        try
        {
            firstTime = (bool)data["data"];
        }
        catch { }
        monthInput.onEndEdit.AddListener(OnMonthInputEditEnd);
        yearInput.onEndEdit.AddListener(OnYearInputEditEnd);
        continueButton.onClick.AddListener(ContinueButton);
        if (firstTime)
        {
            backButton.onClick.AddListener(() => StateManager.Instance.Backer(gameObject));
        }
        else
        {
            backButton.onClick.AddListener(() => StateManager.Instance.HandleBackAction(gameObject));
        }
    }
    public void OnMonthInputEditEnd(string input)
    {
        if (int.TryParse(input, out int month))
        {
            this.month = month;
            Debug.Log($"Valid Month: {this.month}");
        }
        else
        {
            monthInput.text = month.ToString(); // Revert to last valid value
            Debug.LogError("Invalid month. Please enter a value between 1 and 12.");
        }
    }
    public void OnYearInputEditEnd(string input)
    {
        if (int.TryParse(input, out int year))
        {
            this.year = year;
            Debug.Log($"Valid Year: {this.year}");
        }
        else
        {
            yearInput.text = year.ToString(); // Revert to last valid value
            Debug.LogError($"Invalid year. Please enter a value greater than 1980 and less than or equal to {DateTime.Now.Year}.");
        }
    }
    public void ContinueButton()
    {
        if ( month >= 1 && month <= 12 && year > 1980 && year <= DateTime.Now.Year)
        {
            DateTime date=new DateTime(year,month,1);
            if (date <= DateTime.Now)
            {
                ApiDataHandler.Instance.SetJoiningDate(date);
                userSessionManager.Instance.joiningDate= date.ToString("MMM / yyyy");
                if (firstTime)
                {
                    StateManager.Instance.OpenStaticScreen("profile", gameObject, "ChangeBadgeScreen", null);
                }
                else
                {
                    StateManager.Instance.HandleBackAction(gameObject);
                }
            }
            else
            {
                GlobalAnimator.Instance.ShowTextMessage(messageText, "Enter valid date", 2);
            }
           
        }
        else
        {
            GlobalAnimator.Instance.ShowTextMessage(messageText, "Enter valid date", 2);
        }
    }
}
