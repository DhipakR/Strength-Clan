using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
public class ExerciseHistoryController : MonoBehaviour, PageController
{
    public TextMeshProUGUI estimate1RMText;
    public TextMeshProUGUI bestSetText;
    public TextMeshProUGUI heaviestLiftedText;
    public GameObject headerObject;
    public GameObject notPerformedObject;
    public Button backButton;
    public Transform content;

    public List<string> matchedDateTimes = new List<string>();

    public void onInit(Dictionary<string, object> data, Action<object> callback)
    {
        ExerciseDataItem exercise = (ExerciseDataItem)data["data"];
        switch (exercise.exerciseType)
        {
            case ExerciseType.WeightAndReps:
                break;
            default:
                headerObject.SetActive(false);
                RectTransform rectTransform = content.parent.parent.GetComponent<RectTransform>();
                Vector2 offsetMax = rectTransform.offsetMax;
                offsetMax.y = -110; 
                rectTransform.offsetMax = offsetMax;
                break;
        }
        List<ExerciseWithDate> _exerciseHistory = _SearchExerciseByName(ApiDataHandler.Instance.getHistoryData(), exercise.exerciseName);
        _exerciseHistory.Reverse();
        if (exercise.exerciseType == ExerciseType.WeightAndReps )
        {
            headerObject.transform.parent.gameObject.SetActive(true);
            ExerciseWithDate heaviestSet = GetTopPerformance(_exerciseHistory, HistoryPerformance.HeaviestLifted);
            if (heaviestSet != null)
            {
                switch ((WeightUnit)ApiDataHandler.Instance.GetWeightUnit())
                {
                    case WeightUnit.kg:
                        heaviestLiftedText.text = heaviestSet.GetHeaviestLiftedSet().weight.ToString() + " kg";
                        break;
                    case WeightUnit.lbs:
                        heaviestLiftedText.text = Mathf.Round(userSessionManager.Instance.ConvertKgToLbs(heaviestSet.GetHeaviestLiftedSet().weight)).ToString("F1") + " lbs";
                        break;
                }
                //heaviestLiftedText.text=heaviestSet.GetHeaviestLiftedSet().weight.ToString();
                foreach (var item in _exerciseHistory)
                {
                    if (item == heaviestSet)
                    {
                        item.performance = HistoryPerformance.HeaviestLifted;
                        break;
                    }
                }
            }
            else
            {
                heaviestLiftedText.text = "-";
            }
            ExerciseWithDate bestSet = GetTopPerformance(_exerciseHistory, HistoryPerformance.BestSet);
            if(bestSet != null)
            {
                HistoryExerciseModel set = bestSet.GetBestSet();
                float rm = (int)set.reps == 1 ? set.weight : set.weight * (1 + 0.0333f * (int)set.reps);
                float roundedResult = Mathf.Round(rm * 2) / 2; // Rounds to the nearest 0.5
                switch ((WeightUnit)ApiDataHandler.Instance.GetWeightUnit())
                {
                    case WeightUnit.kg:
                        
                        bestSetText.text = bestSet.GetBestSet().weight.ToString() + " X " + userSessionManager.Instance.ShowFormattedNumber(bestSet.GetBestSet().reps);
                        estimate1RMText.text = roundedResult % 1 == 0
                    ? roundedResult.ToString("F0")
                    : roundedResult.ToString("F1");
                        break;
                    case WeightUnit.lbs:
                        bestSetText.text = Mathf.Round(userSessionManager.Instance.ConvertKgToLbs(bestSet.GetBestSet().weight)).ToString("F0") + " X " + userSessionManager.Instance.ShowFormattedNumber(bestSet.GetBestSet().reps);
                        estimate1RMText.text = roundedResult % 1 == 0
                   ? Mathf.Round(userSessionManager.Instance.ConvertKgToLbs(roundedResult)).ToString("F0")
                   : Mathf.Round(userSessionManager.Instance.ConvertKgToLbs(roundedResult)).ToString("F1");
                        break;
                }
                foreach (var item in _exerciseHistory)
                {
                    if (item == bestSet)
                    {
                        if (item.performance == HistoryPerformance.None)
                            item.performance = HistoryPerformance.BestSet;
                        else
                            item.performance |= HistoryPerformance.BestSet;
                        break;
                    }
                }
                //bestSetText.text=bestSet.GetBestSet().weight.ToString()+" X "+ bestSet.GetBestSet().reps.ToString();
                //HistoryExerciseModel set = bestSet.GetBestSet();
                //float rm = (int)set.reps == 1 ? set.weight : set.weight * (1 + 0.0333f * (int)set.reps);
                //float roundedResult = Mathf.Round(rm * 2) / 2; // Rounds to the nearest 0.5

                //switch ((WeightUnit)ApiDataHandler.Instance.GetWeightUnit())
                //{
                //    case WeightUnit.kg:
                //        estimate1RMText.text = roundedResult % 1 == 0
                //    ? roundedResult.ToString("F0")
                //    : roundedResult.ToString("F1");
                //        break;
                //    case WeightUnit.lbs:
                //        estimate1RMText.text = roundedResult % 1 == 0
                //    ? Mathf.Round(userSessionManager.Instance.ConvertKgToLbs(roundedResult)).ToString("F0")
                //    : Mathf.Round(userSessionManager.Instance.ConvertKgToLbs(roundedResult)).ToString("F1");
                //        break;
                //}
            }
            else
            {
                bestSetText.text = "-";
                estimate1RMText.text = "-";
;            }
            
        }
        else
        {
            headerObject.transform.parent.gameObject.SetActive(false);
        }
        backButton.onClick.AddListener(Back);
        backButton.onClick.AddListener(AudioController.Instance.OnButtonClick);
        AddItems(_exerciseHistory);
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Back();
        }
    }
    public void AddItems(List<ExerciseWithDate> history)
    {
        if(history.Count == 0)
        {
            notPerformedObject.SetActive(true);
            return;
        }
        notPerformedObject.SetActive(false);
        foreach(var item in history)
        {
            Dictionary<string, object> mData = new Dictionary<string, object>
                {
                    { "history", item }
                };
            GameObject exercisePrefab = Resources.Load<GameObject>("Prefabs/history/exerciseHistoryDataModel");
            GameObject exerciseObject = Instantiate(exercisePrefab, content);
            exerciseObject.GetComponent<ItemController>().onInit(mData, null);
        }
    }

    // Helper function to format time
    private string FormatTime(int timeInSeconds)
    {
        if (timeInSeconds < 60)
        {
            return $"{timeInSeconds} seconds";
        }
        else
        {
            int minutes = timeInSeconds / 60;
            int seconds = timeInSeconds % 60;
            return $"{minutes} min {seconds} sec";
        }
    }

    // Helper function to format pace (time per mile)
    private string FormatPace(float pace)
    {
        if (pace < 60)
        {
            return $"{pace:F2} seconds";
        }
        else
        {
            int minutes = (int)pace / 60;
            int seconds = (int)pace % 60;
            return $"{minutes} min {seconds} sec";
        }
    }
    public void Back()
    {
        StateManager.Instance.HandleBackAction(gameObject);
    }

    public List<HistoryExerciseModel> SearchExerciseByName(HistoryModel history, string exerciseName)
    {
        List<HistoryExerciseModel> matchedExercises = new List<HistoryExerciseModel>();
        matchedDateTimes.Clear();
        foreach (var template in history.exerciseTempleteModel)
        {
            foreach (var exerciseType in template.exerciseTypeModel)
            {
                if (exerciseType.exerciseName.ToUpper() == exerciseName.ToUpper())
                {
                    foreach (var exercise in exerciseType.exerciseModel)
                    {
                        matchedExercises.Add(exercise);
                        matchedDateTimes.Add(template.dateTime);
                    }
                }
            }
        }
        return matchedExercises;
    }

    public List<ExerciseWithDate> _SearchExerciseByName(HistoryModel history, string exerciseName)
    {
        List<ExerciseWithDate> matchedExercises = new List<ExerciseWithDate>();

        foreach (var template in history.exerciseTempleteModel)
        {
            foreach (var exerciseType in template.exerciseTypeModel)
            {
                if (exerciseType.exerciseName.Equals(exerciseName, StringComparison.OrdinalIgnoreCase))
                {
                    var exerciseTemplate = new ExerciseWithDate
                    {
                        dateTime = template.dateTime,
                        exerciseType = exerciseType.exerciseType
                    };
                    foreach (var exercise in exerciseType.exerciseModel)
                    {
                        exerciseTemplate.exercise.Add(exercise);
                    }
                    matchedExercises.Add((exerciseTemplate));
                }
            }
        }

        return matchedExercises;
    }

    public ExerciseWithDate GetTopPerformance(List<ExerciseWithDate> exercises, HistoryPerformance performanceType)
    {
        ExerciseWithDate topPerformance = null;

        switch (performanceType)
        {
            case HistoryPerformance.HeaviestLifted:
                topPerformance = exercises
                    .OrderByDescending(e => e.GetHeaviestLiftedSet()?.weight ?? 0)
                    .FirstOrDefault();
                break;

            case HistoryPerformance.BestSet:
                topPerformance = exercises
                    .OrderByDescending(e =>
                    {
                        var bestSet = e.GetBestSet();
                        if (bestSet == null) return 0; // Handle null case for GetBestSet
                        return (int)bestSet.reps == 1 ? bestSet.weight : bestSet.weight * (1 + 0.0333f * (int)bestSet.reps);
                    })
                    .FirstOrDefault();
                break;

                // Add cases for other performance types if needed
        }

        return topPerformance;
    }

}
