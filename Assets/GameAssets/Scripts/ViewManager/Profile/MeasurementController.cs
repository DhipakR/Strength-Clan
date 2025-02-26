using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
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
        // Get the current unit and saved unit
        WeightUnit currentUnit = (WeightUnit)ApiDataHandler.Instance.GetWeightUnit();
        WeightUnit savedUnit = ApiDataHandler.Instance.getMeasurementData().weightUnit;
        print(currentUnit + "-" + savedUnit);
        // Convert the weight if the saved unit differs from the current unit
        if (savedUnit != currentUnit)
        {
            if (currentUnit == WeightUnit.lbs)
            {
                // Convert from kg to lbs
                ApiDataHandler.Instance.getMeasurementData().weight = (float)Math.Round(ApiDataHandler.Instance.getMeasurementData().weight * 2.2f);
            }
            else
            {
                // Convert from lbs to kg
                ApiDataHandler.Instance.getMeasurementData().weight = (float)Math.Round(ApiDataHandler.Instance.getMeasurementData().weight / 2.2f);
            }
            ApiDataHandler.Instance.getMeasurementData().weightUnit = currentUnit; // Update the saved unit
        }

        // Display the weight in the appropriate unit
        if (currentUnit == WeightUnit.lbs)
        {
            weight.text = ApiDataHandler.Instance.getMeasurementData().weight + " lbs";
        }
        else
        {
            weight.text = ApiDataHandler.Instance.getMeasurementData().weight + " kg";
        }

        // Initialize other fields
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
            // Store the value directly (no conversion)
            targetField = result;

            // Save the current unit for weight
            if (name == "weight")
            {
                ApiDataHandler.Instance.getMeasurementData().weightUnit = (WeightUnit)ApiDataHandler.Instance.GetWeightUnit();
            }
        }
        else
        {
            Debug.LogWarning($"Invalid input: {value}");
        }

        // Update the input field to reflect the value with the unit
        UpdateInputFieldWithUnit(targetField, unit, text);

        // Update history
        foreach (MeasurementHistoryItem item in historyItems)
        {
            if (item.name == name)
            {
                historyItems.Remove(item);
                break;
            }
        }
        MeasurementHistoryItem newItem = new MeasurementHistoryItem { name = name, dateTime = DateTime.Now.ToString("MMM dd, yyyy hh:mm:ss tt"), value = result };
        historyItems.Add(newItem);
    }

    // Helper method to update the input field text with the value and unit
    private void UpdateInputFieldWithUnit(float value, string unit, TMP_InputField text)
    {
        if (unit.Contains("kg") || unit.Contains("lbs"))
        {
            WeightUnit currentUnit = (WeightUnit)ApiDataHandler.Instance.GetWeightUnit();
            if (currentUnit == WeightUnit.lbs)
            {
                text.text = value + " lbs"; // No conversion, display as pounds
            }
            else
            {
                text.text = value + " kg"; // No conversion, display as kilograms
            }
        }
        else if (unit.Contains("%"))
        {
            text.text = value + " %";
        }
        else if (unit.Contains("cm"))
        {
            text.text = value + " cm";
        }
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

    public void Save()
    {
        // Check if any input field has changed
        bool hasChanges = false;
        foreach (MeasurementHistoryItem item in historyItems)
        {
            if (HasValueChanged(ApiDataHandler.Instance.getMeasurementHistory(), item.name, item.value, ApiDataHandler.Instance.getMeasurementData().weightUnit))
            {
                hasChanges = true;
                break;
            }
        }

        if (hasChanges)
        {
            int historyIndex = ApiDataHandler.Instance.getMeasurementHistory().measurmentHistory.Count;
            ApiDataHandler.Instance.SaveMeasurementData();
            foreach (MeasurementHistoryItem item in historyItems)
            {
                // Save the value in its original unit
                item.weightUnit = ApiDataHandler.Instance.getMeasurementData().weightUnit;
                ApiDataHandler.Instance.SaveMeasurementHistory(item, historyIndex);
                ApiDataHandler.Instance.SetMeasurementHistory(item);
                historyIndex++;
            }
            historyItems.Clear();
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

    private bool HasValueChanged(MeasurementHistory history, string targetName, float newValue, WeightUnit newWeightUnit)
    {
        var filteredItems = history.measurmentHistory
            .Where(item => item.name.ToLower() == targetName.ToLower())
            .OrderByDescending(item =>
            {
                DateTime dateTime;
                bool success = DateTime.TryParseExact(item.dateTime, "MMM dd, yyyy hh:mm:ss tt", CultureInfo.InvariantCulture, DateTimeStyles.None, out dateTime);

                if (!success)
                {
                    success = DateTime.TryParseExact(item.dateTime, "MMM dd, yyyy hh:mm tt", CultureInfo.InvariantCulture, DateTimeStyles.None, out dateTime);
                }

                if (!success)
                {
                    throw new FormatException($"Unable to parse dateTime: {item.dateTime}");
                }

                return dateTime;
            })
            .ToList();

        if (filteredItems.Count() == 0)
        {
            Debug.Log("No history found. Treating as a new value.");
            return true; // No history, so treat as a change
        }

        // Get the last entry
        var lastEntry = filteredItems.First();
        float lastValue = lastEntry.value;

        // Print the comparison for debugging
        Debug.Log($"Comparing new value: {newValue} ({newWeightUnit}) with last value: {lastValue}");

        // Check if the values are different (with a small tolerance for floating-point precision)
        float tolerance = 0.01f; // Adjust as needed
        bool hasChanged = Math.Abs(newValue - lastValue) > tolerance;

        if (!hasChanged)
        {
            Debug.Log("No change detected. Nothing to save.");
        }

        return hasChanged;
    }
}
