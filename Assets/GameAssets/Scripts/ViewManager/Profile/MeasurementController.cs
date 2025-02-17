using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MeasurementController : MonoBehaviour, PageController
{
    public Button saveButton;
    public TextMeshProUGUI messageText;
    public Button backButton;

    public TMP_InputField weight;
    public TMP_InputField bodyFat;
    public TMP_InputField chest;
    public TMP_InputField shoulder;
    public TMP_InputField hips;
    public TMP_InputField waist;
    public TMP_InputField leftThigh;
    public TMP_InputField rightThigh;
    public TMP_InputField leftBicep;
    public TMP_InputField rightBicep;
    public TMP_InputField leftForearm;
    public TMP_InputField rightForearm;
    public TMP_InputField leftCalf;
    public TMP_InputField rightCalf;
    public List<Button> buttons;

    public List<MeasurementHistoryItem> historyItems = new List<MeasurementHistoryItem>();
    public void onInit(Dictionary<string, object> data, Action<object> callback)
    {
        InitializeInputFields();
        AddListeners();
    }
    private void OnEnable()
    {
        messageText.text = "";
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Back();
        }
    }
    void AddListeners()
    {
        foreach (Button button in buttons)
        {
            TMP_InputField input = button.transform.parent.GetComponentInChildren<TMP_InputField>();
            button.onClick.AddListener(() => userSessionManager.Instance.ActiveInput(input));
        }
        saveButton.onClick.AddListener(Save);
        backButton.onClick.AddListener(Back);
        backButton.onClick.AddListener(AudioController.Instance.OnButtonClick);
        switch ((WeightUnit)ApiDataHandler.Instance.GetWeightUnit())
        {
            case WeightUnit.kg:
                weight.onEndEdit.AddListener(value => OnInputEditEnd(value, targetField: ref ApiDataHandler.Instance.getMeasurementData().weight, "kg", weight, "weight"));
                break;
            case WeightUnit.lbs:
                weight.onEndEdit.AddListener(value => 
                {
                    OnInputEditEnd(value, targetField: ref ApiDataHandler.Instance.getMeasurementData().weight, "lbs", weight, "weight");
                    weight.text = Math.Round(ApiDataHandler.Instance.getMeasurementData().weight * 2.2f) + " lbs";
                });
                break;
        }
        //weight.onEndEdit.AddListener(value => OnInputEditEnd(value, targetField: ref ApiDataHandler.Instance.getMeasurementData().weight, "kg"));
        bodyFat.onEndEdit.AddListener(value => OnInputEditEnd(value, targetField: ref ApiDataHandler.Instance.getMeasurementData().bodyFat, "%", bodyFat, "body fat"));
        chest.onEndEdit.AddListener(value => OnInputEditEnd(value, targetField: ref ApiDataHandler.Instance.getMeasurementData().chest, "cm", chest, "chest"));
        shoulder.onEndEdit.AddListener(value => OnInputEditEnd(value, targetField: ref ApiDataHandler.Instance.getMeasurementData().shoulder, "cm", shoulder, "shoulder"));
        hips.onEndEdit.AddListener(value => OnInputEditEnd(value, targetField: ref ApiDataHandler.Instance.getMeasurementData().hips, "cm", hips, "hips"));
        waist.onEndEdit.AddListener(value => OnInputEditEnd(value, targetField: ref ApiDataHandler.Instance.getMeasurementData().waist, "cm", waist, "waist"));
        leftThigh.onEndEdit.AddListener(value => OnInputEditEnd(value, targetField: ref ApiDataHandler.Instance.getMeasurementData().leftThigh, "cm", leftThigh, "left thigh"));
        rightThigh.onEndEdit.AddListener(value => OnInputEditEnd(value, targetField: ref ApiDataHandler.Instance.getMeasurementData().rightThigh, "cm", rightThigh, "right thigh"));
        leftBicep.onEndEdit.AddListener(value => OnInputEditEnd(value, targetField: ref ApiDataHandler.Instance.getMeasurementData().leftBicep, "cm", leftBicep, "left bicep"));
        rightBicep.onEndEdit.AddListener(value => OnInputEditEnd(value, targetField: ref ApiDataHandler.Instance.getMeasurementData().rightBicep, "cm", rightBicep, "right bicep"));
        leftForearm.onEndEdit.AddListener(value => OnInputEditEnd(value, targetField: ref ApiDataHandler.Instance.getMeasurementData().leftForearm, "cm", leftForearm, "left forearm"));
        rightForearm.onEndEdit.AddListener(value => OnInputEditEnd(value, targetField: ref ApiDataHandler.Instance.getMeasurementData().rightForearm, "cm", rightForearm, "right forearm"));
        leftCalf.onEndEdit.AddListener(value => OnInputEditEnd(value, targetField: ref ApiDataHandler.Instance.getMeasurementData().leftCalf, "cm", leftCalf, "left calf"));
        rightCalf.onEndEdit.AddListener(value => OnInputEditEnd(value, targetField: ref ApiDataHandler.Instance.getMeasurementData().rightCalf, "cm", rightCalf, "right calf"));
    }

    // Initialize input fields with values from the MeasurementModel and add units
    void InitializeInputFields()
    {
        switch ((WeightUnit)ApiDataHandler.Instance.GetWeightUnit())
        {
            case WeightUnit.kg:
                weight.text = ApiDataHandler.Instance.getMeasurementData().weight + " kg";
                break;
            case WeightUnit.lbs:
                weight.text = Math.Round(ApiDataHandler.Instance.getMeasurementData().weight * 2.2f) + " lbs";
                break;
        }
        //weight.text = ApiDataHandler.Instance.getMeasurementData().weight + " kg";
        bodyFat.text = ApiDataHandler.Instance.getMeasurementData().bodyFat + " %";
        chest.text = ApiDataHandler.Instance.getMeasurementData().chest + " cm";
        shoulder.text = ApiDataHandler.Instance.getMeasurementData().shoulder + " cm";
        hips.text = ApiDataHandler.Instance.getMeasurementData().hips + " cm";
        waist.text = ApiDataHandler.Instance.getMeasurementData().waist + " cm";
        leftThigh.text = ApiDataHandler.Instance.getMeasurementData().leftThigh + " cm";
        rightThigh.text = ApiDataHandler.Instance.getMeasurementData().rightThigh + " cm";
        leftBicep.text = ApiDataHandler.Instance.getMeasurementData().leftBicep + " cm";
        rightBicep.text = ApiDataHandler.Instance.getMeasurementData().rightBicep + " cm";
        leftForearm.text = ApiDataHandler.Instance.getMeasurementData().leftForearm + " cm";
        rightForearm.text = ApiDataHandler.Instance.getMeasurementData().rightForearm + " cm";
        leftCalf.text = ApiDataHandler.Instance.getMeasurementData().leftCalf + " cm";
        rightCalf.text = ApiDataHandler.Instance.getMeasurementData().rightCalf + " cm";
    }

    // Generic function to handle input editing and update the MeasurementModel
    public void OnInputEditEnd(string value, ref float targetField, string unit, TMP_InputField text, string name)
    {
        // Remove the unit from the input string before parsing
        string cleanedValue = value.Replace(unit, "").Trim();

        if (float.TryParse(cleanedValue, out float result))
        {
            if (unit == "lbs" && name == "weight")
                targetField = (float)Math.Round(result / 2.2f);
            else
                targetField = result;  // Update the corresponding field in the MeasurementModel
        }
        else
        {
            Debug.LogWarning($"Invalid input: {value}");
        }

        // Update the input field to reflect the value with the unit
        UpdateInputFieldWithUnit(targetField, unit, text);

        foreach (MeasurementHistoryItem item in historyItems)
        {
            if (item.name == name)
            {
                historyItems.Remove(item);
                break;
            }
        }
        MeasurementHistoryItem newItem = new MeasurementHistoryItem { name = name, dateTime = DateTime.Now.ToString("MMM dd, yyyy hh:mm tt"), value = result };
        historyItems.Add(newItem);
    }

    // Helper method to update the input field text with the value and unit
    private void UpdateInputFieldWithUnit(float value, string unit, TMP_InputField text)
    {
        // Check which input field needs to be updated by comparing the original text value
        if (unit.Contains("kg"))
            text.text = value + " " + unit;
        else if (unit.Contains("lbs"))
            text.text = value + " lbs";
        else if (unit.Contains("%"))
            text.text = value + " %";
        else if (unit.Contains("cm"))
            text.text = value + " cm";
    }
    public void Save()
    {
        if (historyItems.Count > 0)
        {
            int historyIndex = ApiDataHandler.Instance.getMeasurementHistory().measurmentHistory.Count;
            ApiDataHandler.Instance.SaveMeasurementData();
            foreach (MeasurementHistoryItem item in historyItems)
            {
                ApiDataHandler.Instance.SaveMeasurementHistory(item, historyIndex);
                ApiDataHandler.Instance.SetMeasurementHistory(item);
                historyIndex++;
            }
            historyItems.Clear();
            //ApiDataHandler.Instance.SaveMeasurementHistory();
            DOTween.Kill(messageText);
            GlobalAnimator.Instance.ShowTextMessage(messageText, "Saved Successfully!", 2);
        }
        else
        {
            DOTween.Kill(messageText);
            GlobalAnimator.Instance.ShowTextMessage(messageText, "Nothing new to save!", 2);
        }
        AudioController.Instance.OnButtonClick();
    }
    public void OpenHistory(string name)
    {
        Dictionary<string, object> mData = new Dictionary<string, object>
        {
            {  "name", name  }
        };
        StateManager.Instance.OpenStaticScreen("profile", null, "MeasurementHistoryScreen", mData, true);
    }
    public void Back()
    {
        StateManager.Instance.HandleBackAction(gameObject);
        StateManager.Instance.OpenFooter(null, null, false);
        //ApiDataHandler.Instance.LoadMeasurementData();
    }

}
