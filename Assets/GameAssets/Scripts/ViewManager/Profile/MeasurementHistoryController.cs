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
    }

    public void Back()
    {
        StateManager.Instance.HandleBackAction(gameObject);
    }
}