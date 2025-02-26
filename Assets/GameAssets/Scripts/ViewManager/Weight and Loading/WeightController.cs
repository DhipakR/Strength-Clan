using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Windows;

public class WeightController : MonoBehaviour,PageController
{
    public TMP_InputField weightInput;
    public TMP_Dropdown dropdown;
    public TextMeshProUGUI messageText;
    public WeightUnit weightUnit;
    public Button backButton;
    public void onInit(Dictionary<string, object> data, Action<object> callback)
    {
        backButton.onClick.AddListener(() => StateManager.Instance.Backer());
        StateManager.Instance.ShiftStep(AccountCreationStep.Weight);

    }
    private void Start()
    {
        weightUnit = (WeightUnit)ApiDataHandler.Instance.GetWeightUnit();
        switch (weightUnit)
        {
            case WeightUnit.kg:
                dropdown.value = 0;
                break;
            case WeightUnit.lbs:
                dropdown.value = 1;
                break;
        }
        weightInput.onValueChanged.AddListener(LimitInput);
    }
    void LimitInput(string input)
    {
        if (input.Contains("."))
        {
            // Split the input into whole and decimal parts
            string[] parts = input.Split('.');

            // Limit whole part to 3 characters and decimal part to 2 characters
            string wholePart = parts[0].Length > 3 ? parts[0].Substring(0, 3) : parts[0];
            string decimalPart = parts[1].Length > 2 ? parts[1].Substring(0, 2) : parts[1];

            // Combine the parts back and set the text
            weightInput.text = wholePart + "." + decimalPart;
        }
        else
        {
            // If there's no decimal point, limit input to 3 characters
            weightInput.text = input.Length > 3 ? input.Substring(0, 3) : input;
        }
    }
    public void OnDropdownClick()
    {
        AudioController.Instance.OnButtonClick();
        if (dropdown.value == 0)
            weightUnit=WeightUnit.kg;
        else if (dropdown.value == 1)
            weightUnit = WeightUnit.lbs;
    }
    public void Next()
    {
        bool save=false;
        AudioController.Instance.OnButtonClick();
        if (float.TryParse(weightInput.text, out float result))
        {
            print(result);
            switch (weightUnit)
            {
                case WeightUnit.kg:
                    if (result > 10&&result<150)
                    {
                        save = true;
                        ApiDataHandler.Instance.getMeasurementData().weight = Mathf.RoundToInt(result);
                        ApiDataHandler.Instance.SaveMeasurementData();
                        MeasurementHistoryItem item = new MeasurementHistoryItem
                        {
                            name = "weight",
                            dateTime = DateTime.Now.ToString("MMM dd, yyyy hh:mm tt"),
                            value = Mathf.RoundToInt(result)
                        };
                        ApiDataHandler.Instance.SaveMeasurementHistory(item, ApiDataHandler.Instance.getMeasurementHistory().measurmentHistory.Count);
                        ApiDataHandler.Instance.SetMeasurementHistory(item);
                        //ApiDataHandler.Instance.SaveWeight(Mathf.RoundToInt(result));
                    }
                    break;
                case WeightUnit.lbs:
                    if (result > 22  && result<331)
                    {
                        save = true;
                        ApiDataHandler.Instance.getMeasurementData().weight = Mathf.RoundToInt(userSessionManager.Instance.ConvertKgToLbs(result));
                        ApiDataHandler.Instance.SaveMeasurementData();
                        MeasurementHistoryItem item = new MeasurementHistoryItem
                        {
                            name = "weight",
                            dateTime = DateTime.Now.ToString("MMM dd, yyyy hh:mm tt"),
                            value = Mathf.RoundToInt(userSessionManager.Instance.ConvertLbsToKg(result))
                        };
                        ApiDataHandler.Instance.SaveMeasurementHistory(item,ApiDataHandler.Instance.getMeasurementHistory().measurmentHistory.Count);
                        ApiDataHandler.Instance.SetMeasurementHistory(item);
                        //ApiDataHandler.Instance.SaveWeight(Mathf.RoundToInt(result));
                    }
                    break;
            }
            
        }
        if (save)
        {
            ApiDataHandler.Instance.SetWeightUnit((int)weightUnit);
            StateManager.Instance.OpenStaticScreen("date", gameObject, "DateScreen", null);
        }
        else
        {
            switch (weightUnit)
            {
                case WeightUnit.kg:
                    GlobalAnimator.Instance.ShowTextMessage(messageText, "Weight must be between 10kg to 150kg", 2);
                    break;
                case WeightUnit.lbs:
                    GlobalAnimator.Instance.ShowTextMessage(messageText, "Weight must be between 22lbs to 331lbs", 2);
                    break;
            }
            //GlobalAnimator.Instance.ShowTextMessage(messageText, "Weight must be between 10kg / 22lbs to 150kg / 331lbs", 2);
        }
    }

}
