using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;

public class MeasurementHistorySubItem : MonoBehaviour,ItemController
{
    public TextMeshProUGUI dateTimeText;
    public TextMeshProUGUI measurementText;
    public void onInit(Dictionary<string, object> data, Action<object> callback)
    {
        string name = (string)data["name"];
        float value = (float)data["value"];
        string dateTime = (string)data["dateTime"];
        WeightUnit savedWeightUnit = (WeightUnit)data["weightUnit"]; // Get the saved weight unit
        WeightUnit currentWeightUnit = (WeightUnit)ApiDataHandler.Instance.GetWeightUnit(); // Get the current weight unit

        // Try parsing the dateTime with both formats
        DateTime parsedDateTime;
        bool success = DateTime.TryParseExact(dateTime, "MMM dd, yyyy hh:mm:ss tt", System.Globalization.CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedDateTime);

        if (!success)
        {
            success = DateTime.TryParseExact(dateTime, "MMM dd, yyyy hh:mm tt", System.Globalization.CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedDateTime);
        }

        if (!success)
        {
            throw new FormatException($"Unable to parse dateTime: {dateTime}");
        }

        // Determine if the year should be displayed
        if (parsedDateTime.Year == DateTime.Now.Year)
        {
            dateTimeText.text = parsedDateTime.ToString("MMM dd  hh:mm tt");
        }
        else
        {
            dateTimeText.text = parsedDateTime.ToString("MMM dd, yyyy  hh:mm tt");
        }

        switch (name.ToLower())
        {
            case "weight":
                // Convert the value to the current unit if necessary
                float displayValue = value;
                if (savedWeightUnit != currentWeightUnit)
                {
                    if (savedWeightUnit == WeightUnit.kg && currentWeightUnit == WeightUnit.lbs)
                    {
                        // Convert from kg to lbs
                        displayValue = userSessionManager.Instance.ConvertKgToLbs(value);
                    }
                    else if (savedWeightUnit == WeightUnit.lbs && currentWeightUnit == WeightUnit.kg)
                    {
                        // Convert from lbs to kg
                        displayValue = userSessionManager.Instance.ConvertLbsToKg(value);
                    }
                }

                // Display the value in the current unit
                measurementText.text = displayValue.ToString("F2") + (currentWeightUnit == WeightUnit.kg ? " kg" : " lbs");
                break;
            case "body fat":
                measurementText.text = value.ToString("F2") + " %";
                break;
            default:
                measurementText.text = value.ToString("F2") + " cm";
                break;
        }
    }

}
