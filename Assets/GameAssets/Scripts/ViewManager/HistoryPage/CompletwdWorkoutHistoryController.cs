using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CompletwdWorkoutHistoryController : MonoBehaviour, PageController
{
    public TextMeshProUGUI workoutNameText;
    public TextMeshProUGUI dateText;
    public TextMeshProUGUI totalTimeText;
    public TextMeshProUGUI totalWeightText;
    public Transform content;
    public Button backButton;


    void PageController.onInit(Dictionary<string, object> data, Action<object> callback)
    {
        HistoryTempleteModel historyWorkout = (HistoryTempleteModel)data["workout"];
        workoutNameText.text = userSessionManager.Instance.FormatStringAbc(historyWorkout.templeteName);
        string savedDate = historyWorkout.dateTime;
        DateTime parsedDate = DateTime.ParseExact(savedDate, "MMM dd, yyyy hh:mm:ss tt", System.Globalization.CultureInfo.InvariantCulture);
        string formattedDate = parsedDate.ToString("dddd, dd MMMM yyyy");
        dateText.text = formattedDate;
        if (historyWorkout.completedTime > 60)
        {
            totalTimeText.text = ((int)historyWorkout.completedTime / 60).ToString() + "m";
        }
        else
        {
            totalTimeText.text = historyWorkout.completedTime.ToString() + "s";
        }
        switch ((WeightUnit)ApiDataHandler.Instance.GetWeightUnit())
        {
            case WeightUnit.kg:
                totalWeightText.text = (historyWorkout.totalWeight).ToString()+" kg";
                break;
            case WeightUnit.lbs:
                totalWeightText.text = (userSessionManager.Instance.ConvertKgToLbs(historyWorkout.totalWeight)).ToString() + " lbs";
                break;
        }
        //totalWeightText.text = historyWorkout.totalWeight.ToString();
        foreach (HistoryExerciseTypeModel exercise in historyWorkout.exerciseTypeModel)
        {
            GameObject exercisePrefab = Resources.Load<GameObject>("Prefabs/history/completeWorkoutHistoryScreenDataModel");
            GameObject newExerciseObject = Instantiate(exercisePrefab, content);
            newExerciseObject.transform.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>().text = exercise.exerciseName.ToUpper();
            switch (exercise.exerciseType)
            {
                case ExerciseType.RepsOnly:
                    ShowOnlyReps(exercise, newExerciseObject, newExerciseObject.transform.GetChild(0).gameObject);
                    break;
                case ExerciseType.TimeBased:
                    ShowOnlyTime(exercise, newExerciseObject, newExerciseObject.transform.GetChild(0).gameObject);
                    break;
                case ExerciseType.TimeAndMiles:
                    ShowTimeAndMile(exercise, newExerciseObject, newExerciseObject.transform.GetChild(0).gameObject);
                    break;
                case ExerciseType.WeightAndReps:
                    ShowWeightAndReps(exercise, newExerciseObject, newExerciseObject.transform.GetChild(0).gameObject);
                    break;
            }
        }
    }

    private void Start()
    {
        backButton.onClick.AddListener(AudioController.Instance.OnButtonClick);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Back();
        }
    }
    public void Back()
    {
        StateManager.Instance.HandleBackAction(gameObject);
        StateManager.Instance.OpenFooter(null, null, false);
    }
    void ShowOnlyReps(HistoryExerciseTypeModel exercise, GameObject parent, GameObject prefab)
    {
        foreach (HistoryExerciseModel data in exercise.exerciseModel)
        {
            GameObject textObj = Instantiate(prefab, parent.transform);
            TextMeshProUGUI text = textObj.GetComponent<TextMeshProUGUI>();
            text.text = data.reps.ToString("F1") + " @ " + data.rir.ToString();
            text.fontSize = 14;
            SetFontAndColor(text);
        }
    }
    void ShowOnlyTime(HistoryExerciseTypeModel exercise, GameObject parent, GameObject prefab)
    {
        foreach (HistoryExerciseModel data in exercise.exerciseModel)
        {
            GameObject textObj = Instantiate(prefab, parent.transform);
            TextMeshProUGUI text = textObj.GetComponent<TextMeshProUGUI>();
            text.fontSize = 14;
            if (data.time > 60)
            {
                text.text = ((int)data.time / 60).ToString() + " m" + " @ " + data.rpe.ToString();
            }
            else
            {
                text.text = data.time.ToString() + " s" + " @ " + data.rpe.ToString();
            }
            SetFontAndColor(text);
        }
    }
    void ShowWeightAndReps(HistoryExerciseTypeModel exercise, GameObject parent, GameObject prefab)
    {
        foreach (HistoryExerciseModel data in exercise.exerciseModel)
        {
            GameObject textObj = Instantiate(prefab, parent.transform);
            TextMeshProUGUI text = textObj.GetComponent<TextMeshProUGUI>();
            switch ((WeightUnit)ApiDataHandler.Instance.GetWeightUnit())
            {
                case WeightUnit.kg:
                    text.text = data.weight.ToString() + " kg x " + data.reps.ToString() + " @ " + data.rir.ToString();
                    break;
                case WeightUnit.lbs:
                    text.text = (userSessionManager.Instance.ConvertKgToLbs(data.weight)).ToString("f0") + " lbs x " + data.reps.ToString("F1") + " @ " + data.rir.ToString();
                    break;
            }
            //text.text = data.weight.ToString() + " kg x " + data.reps.ToString();
            text.fontSize = 14;
            SetFontAndColor(text);
        }
    }
    void ShowTimeAndMile(HistoryExerciseTypeModel exercise, GameObject parent, GameObject prefab)
    {
        foreach (HistoryExerciseModel data in exercise.exerciseModel)
        {
            GameObject textObj = Instantiate(prefab, parent.transform);
            TextMeshProUGUI text = textObj.GetComponent<TextMeshProUGUI>();
            string time = "";
            if (data.time > 60)
            {
                time = ((int)data.time / 60).ToString() + " m";
            }
            else
            {
                time = data.time.ToString() + " s";
            }
            text.text = data.mile.ToString() + " mile x " + time + " @ " + data.rpe.ToString();
            text.fontSize = 14;
            SetFontAndColor(text);
        }
    }
    void SetFontAndColor(TextMeshProUGUI text)
    {
        switch (ApiDataHandler.Instance.gameTheme)
        {
            case Theme.Light:
                text.font = userSessionManager.Instance.lightSecondaryFont;
                text.color = userSessionManager.Instance.lightButtonColor;
                break;
            case Theme.Dark:
                text.font=userSessionManager.Instance.darkSecondaryFont;
                text.color = Color.white;
                break;
        }
    }

}
