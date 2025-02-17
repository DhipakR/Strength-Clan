using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class historyScreenDataModel : MonoBehaviour, ItemController
{
    public TextMeshProUGUI workoutNameText;
    public TextMeshProUGUI timeText;
    public TextMeshProUGUI dateText;
    public Image line;

    public HistoryTempleteModel historyWorkout;
    private GameObject mainParent;
    public void onInit(Dictionary<string, object> data, Action<object> callback)
    {
        this.historyWorkout = (HistoryTempleteModel)data["data"];
        mainParent = (GameObject)data["mainParent"];
        workoutNameText.text = userSessionManager.Instance.FormatStringAbc(historyWorkout.templeteName);
        int completeTime = historyWorkout.completedTime;
        if(completeTime > 60) 
        { 
            timeText.text=((int)completeTime/60).ToString()+"m";
        }
        else
        {
            timeText.text=completeTime.ToString()+"s";
        }
        DateTime parsedDateTime = DateTime.Parse(historyWorkout.dateTime);
        string formattedDate = parsedDateTime.ToString("MMM dd, yyyy");
        dateText.text = formattedDate;
        //if (historyWorkout.totalWeight > 0) { dateText.text = historyWorkout.totalWeight.ToString() + " kg"; }
        //else { dateText.text = "-"; }
        if (historyWorkout.exerciseTypeModel.Count > 0)
        {
            int totalExercises = 0;
            foreach (var exerciseModel in historyWorkout.exerciseTypeModel)
            {
                AddSetFromModel(exerciseModel);
                totalExercises++;
            }
            float imageY = line.GetComponent<RectTransform>().sizeDelta.y;
            Vector2 newSize = new Vector2(line.GetComponent<RectTransform>().sizeDelta.x, (imageY) * totalExercises*1.1f);
            line.GetComponent<RectTransform>().sizeDelta = newSize;
        }

        switch (ApiDataHandler.Instance.gameTheme)
        {
            case Theme.Light:
                line.color = new Color32(92, 59, 28, 155);
                break;
            case Theme.Dark:
                line.color = new Color32(217, 217, 217, 127);
                break;
        }
    }
    private void AddSetFromModel(HistoryExerciseTypeModel exerciseModel)
    {
        GameObject prefab = Resources.Load<GameObject>("Prefabs/history/historySubItem");

        GameObject newSubItem = Instantiate(prefab, transform);
        int childCount = transform.childCount;
        //newSubItem.transform.SetSiblingIndex(childCount - 2);
        HistorySubItem newSubItemScript = newSubItem.GetComponent<HistorySubItem>();

        //HistoryExerciseModel history = null;
        //if (exerciseHistory.Count > 0)
        //{
        //    history = exerciseHistory[0];
        //    exerciseHistory.RemoveAt(0);
        //}
        Dictionary<string, object> initData = new Dictionary<string, object>
        {
            {  "data", exerciseModel   }
        };
        newSubItemScript.onInit(initData,null);
    }
    public void OpenDetails()
    {
        Dictionary<string, object> mData = new Dictionary<string, object>
            {
            { "workout",historyWorkout }
            };
        print(mData);
        StateManager.Instance.OpenStaticScreen("history", mainParent, "completeWorkoutHistoryScreen", mData,true);
        StateManager.Instance.CloseFooter();
    }
}
