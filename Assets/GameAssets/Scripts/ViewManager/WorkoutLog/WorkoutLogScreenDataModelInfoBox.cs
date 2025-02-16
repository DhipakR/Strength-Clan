using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WorkoutLogScreenDataModelInfoBox : MonoBehaviour
{
    [TextArea]
    [SerializeField] private string RPEDesc;

    [TextArea]
    [SerializeField] private string RIRDesc;

    public void InfoShowOpen()
    {
        string name = transform.parent.name;
        string give = "";
        if (name.Contains("rir"))
        {
            give = RIRDesc;
        }
        else if (name.Contains("rpe"))
        {
            give = RPEDesc;
        }
        InfoShowBoxAppear(give, name);
    }

    private void InfoShowBoxAppear(string desc, string parentName)
    {
        string title = parentName.Contains("rir") ? "RIR" : "RPE";
        List<object> initialData = new List<object> { title, desc };
        Action<List<object>> onFinish = (data) =>
        {
            Debug.Log("Popup closed with data: " + data[0]);
        };
        PopupController.Instance.OpenPopup("workoutLog", "InfoBoxPopup", onFinish, initialData);
    }
}