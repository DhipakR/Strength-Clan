using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MeasurementHistoryController : MonoBehaviour, PageController
{
    public TextMeshProUGUI labelText;
    public GameObject noHistory;
    public Button backButton;
    public Transform content;

    void PageController.onInit(Dictionary<string, object> data, Action<object> callback)
    {
        string name = (string)data["name"];
        labelText.text = name;
        List<MeasurementHistoryItem> items = GetFilteredAndSortedItems(ApiDataHandler.Instance.getMeasurementHistory(), name);

        if (items.Count == 0)
        {
            noHistory.SetActive(true);
        }
        else
        {
            noHistory.SetActive(false);
            foreach (MeasurementHistoryItem item in items)
            {
                AddItems(item, name);
            }
        }

        backButton.onClick.AddListener(Back);
        backButton.onClick.AddListener(AudioController.Instance.OnButtonClick);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Back();
        }
    }

    private void AddItems(MeasurementHistoryItem historyItem, string name)
    {
        GameObject prefab = Resources.Load<GameObject>("Prefabs/profile/measurementHistoryItem");
        GameObject newItem = Instantiate(prefab, content);
        MeasurementHistorySubItem itemScript = newItem.GetComponent<MeasurementHistorySubItem>();

        Dictionary<string, object> initData = new Dictionary<string, object>
    {
        { "dateTime", historyItem.dateTime },
        { "value", historyItem.value },
        { "name", name },
        { "weightUnit", historyItem.weightUnit } // Pass the saved weight unit
    };

        itemScript.onInit(initData, null);
    }
    public List<MeasurementHistoryItem> GetFilteredAndSortedItems(MeasurementHistory history, string targetName)
    {
        return history.measurmentHistory
            .Where(item => item.name.ToLower() == targetName.ToLower())
            .OrderByDescending(item =>
            {
                DateTime dateTime;

                string sanitizedDate = item.dateTime.Replace(".", "");

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

                bool success = DateTime.TryParseExact(sanitizedDate, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out dateTime);

                if (!success)
                {
                    Debug.LogError($"❌ Unable to parse dateTime: {item.dateTime}");
                    return DateTime.MinValue;
                }

                return dateTime;
            })
            .ToList();
    }



    public void Back()
    {
        StateManager.Instance.HandleBackAction(gameObject);
    }
}