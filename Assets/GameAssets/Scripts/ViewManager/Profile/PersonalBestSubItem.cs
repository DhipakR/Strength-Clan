using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PersonalBestSubItem : MonoBehaviour, ItemController
{
    public TextMeshProUGUI exerciseName;
    public TMP_InputField weight;
    public TMP_InputField rep;
    public PersonalBestDataItem _data;
    private bool isSocial = false;

    public void onInit(Dictionary<string, object> data, Action<object> callback)
    {
        _data = (PersonalBestDataItem)data["data"];
        if (data.ContainsKey("social"))
            isSocial = (bool)data["social"];

        exerciseName.text = userSessionManager.Instance.FormatStringAbc(_data.exerciseName);
        rep.text = _data.rep.ToString();

        // Check if the saved unit matches the current unit
        WeightUnit currentUnit = (WeightUnit)ApiDataHandler.Instance.GetWeightUnit();
        if (_data.savedUnit != currentUnit)
        {
            // Convert the weight to the current unit
            if (currentUnit == WeightUnit.lbs)
            {
                // Convert from kg to lbs
                _data.weight = (int)Math.Round(_data.weight * 2.2f);
            }
            else
            {
                // Convert from lbs to kg
                _data.weight = (int)Math.Round(_data.weight / 2.2f);
            }
            _data.savedUnit = currentUnit; // Update the saved unit
        }

        // Display the weight in the appropriate unit
        UpdateWeightDisplay();

        weight.onEndEdit.AddListener(WeightValueChange);
        rep.onEndEdit.AddListener(RepValueChange);

        if (new[] { "bench press (barbell)", "squat (barbell)", "deadlifts (barbell)" }.Contains(_data.exerciseName.ToLower()))
        {
            transform.SetAsFirstSibling();
        }
        if (isSocial)
        {
            weight.interactable = false;
            rep.interactable = false;
        }
        weight.transform.parent.GetChild(1).GetComponent<Button>().onClick.AddListener(() => userSessionManager.Instance.ActiveInput(weight));
        rep.transform.parent.GetChild(1).GetComponent<Button>().onClick.AddListener(() => userSessionManager.Instance.ActiveInput(rep));
    }

    void RepValueChange(string value)
    {
        if (int.TryParse(value, out int parsedWeight))
        {
            rep.text = parsedWeight.ToString();
            _data.rep = parsedWeight; // Update weight only if parsing succeeds
        }
        ApiDataHandler.Instance.SavePersonalBestData();
    }

    void WeightValueChange(string value)
    {
        // Remove non-digit characters from the input
        string numericValue = Regex.Replace(value, @"\D", "");

        if (int.TryParse(numericValue, out int parsedWeight))
        {
            // Determine the current unit setting
            WeightUnit currentUnit = (WeightUnit)ApiDataHandler.Instance.GetWeightUnit();

            // Store the weight and unit
            _data.weight = parsedWeight;
            _data.savedUnit = currentUnit;

            // Update the display
            UpdateWeightDisplay();
        }
        else
        {
            Debug.Log("Invalid input: Please enter a valid integer.");
        }

        ApiDataHandler.Instance.SavePersonalBestData();
        userSessionManager.Instance.CheckAchievementStatus();
    }

    void UpdateWeightDisplay()
    {
        // Determine the current unit setting
        WeightUnit currentUnit = (WeightUnit)ApiDataHandler.Instance.GetWeightUnit();

        // Check if the saved unit matches the current unit
        if (_data.savedUnit == currentUnit)
        {
            // No conversion needed, display the weight as is
            if (currentUnit == WeightUnit.lbs)
            {
                weight.text = _data.weight.ToString() + " lbs";
            }
            else
            {
                weight.text = _data.weight.ToString() + " kg";
            }
        }
        else
        {
            // Conversion is needed
            if (currentUnit == WeightUnit.lbs)
            {
                // Convert from kilograms to pounds
                int weightInLbs = (int)Math.Round(_data.weight * 2.2f);
                weight.text = weightInLbs.ToString() + " lbs";
            }
            else
            {
                // Convert from pounds to kilograms
                int weightInKg = (int)Math.Round(_data.weight / 2.2f);
                weight.text = weightInKg.ToString() + " kg";
            }
        }
    }
}