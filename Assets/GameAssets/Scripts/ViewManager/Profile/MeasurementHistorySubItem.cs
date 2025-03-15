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
        WeightUnit savedWeightUnit = (WeightUnit)data["weightUnit"];
        WeightUnit currentWeightUnit = (WeightUnit)ApiDataHandler.Instance.GetWeightUnit(); 


        string sanitizedDate = dateTime.Replace(".", "");


        string[] formats = {
        "MMM dd, yyyy hh:mm:ss tt", // Mar 14, 2025 10:19:02 PM
        "MMM dd, yyyy hh:mm tt",    // Mar 14, 2025 10:19 PM
        "MMMM dd, yyyy hh:mm:ss tt", // March 14, 2025 10:19:02 PM
        "MMMM dd, yyyy hh:mm tt",    // March 14, 2025 10:19 PM
        "dd MMM, yyyy hh:mm:ss tt", // 14 Mar, 2025 10:19:02 PM
        "dd MMM, yyyy hh:mm tt",    // 14 Mar, 2025 10:19 PM
        "yyyy-MM-dd HH:mm:ss",       // 2025-03-14 22:19:02 (ISO 8601 format)
        "yyyy/MM/dd HH:mm:ss",       // 2025/03/14 22:19:02
        "yyyy-MM-dd'T'HH:mm:ss",    // 2025-03-14T22:19:02 (UTC ISO format)
        "yyyy-MM-dd'T'HH:mm:ss'Z'", // 2025-03-14T22:19:02Z (UTC with 'Z')
        "yyyyMMdd HH:mm:ss",        // 20250314 22:19:02
        "yyyyMMdd'T'HHmmss",        // 20250314T221902 (Compact format)
    };

        // Try parsing with multiple formats
        DateTime parsedDateTime;
        bool success = DateTime.TryParseExact(sanitizedDate, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedDateTime);

        if (!success)
        {
            Debug.LogError($"❌ Unable to parse dateTime: {dateTime}");
            return;
        }

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
                float displayValue = value;
                if (savedWeightUnit != currentWeightUnit)
                {
                    if (savedWeightUnit == WeightUnit.kg && currentWeightUnit == WeightUnit.lbs)
                    {
                        displayValue = userSessionManager.Instance.ConvertKgToLbs(value);
                    }
                    else if (savedWeightUnit == WeightUnit.lbs && currentWeightUnit == WeightUnit.kg)
                    {
                        displayValue = userSessionManager.Instance.ConvertLbsToKg(value);
                    }
                }

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
