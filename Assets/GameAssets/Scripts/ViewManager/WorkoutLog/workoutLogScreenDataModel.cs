using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class workoutLogScreenDataModel : MonoBehaviour, ItemController
{
    public TextMeshProUGUI exerciseNameText;
    public TMP_InputField exerciseNotes;
    public List<TextMeshProUGUI> labelText = new List<TextMeshProUGUI>();
    public Button threeDots;
    public GameObject timer, mile, weight, reps, rir, rpe;
    public ExerciseTypeModel exerciseTypeModel;
    Action<object> callback;
    bool isWorkoutLog;
    public bool isTemplateCreator;
    public List<HistoryExerciseModel> exerciseHistory;
    public DefaultTempleteModel templeteModel;
    public List<WorkoutLogSubItem> workoutLogSubItems;
    //InputFieldManager inputFieldManager;
    private bool interruptedLoading;
    private void Start()
    {
        exerciseNotes.transform.GetComponentInChildren<Button>().onClick.AddListener(() => userSessionManager.Instance.ActiveInput(exerciseNotes));
        exerciseNotes.transform.GetComponentInChildren<Button>().transform.parent = exerciseNotes.transform.parent;
    }
    public void onInit(Dictionary<string, object> data, Action<object> callback)
    {
        this.callback = callback;
        this.exerciseTypeModel = (ExerciseTypeModel)data["data"];
        isWorkoutLog = (bool)data["isWorkoutLog"];
        isTemplateCreator = (bool)data["isTemplateCreator"];
        interruptedLoading = data.TryGetValue("interruptedLoading", out var value) ? (bool)value : false;

        //if (data.ContainsKey("templeteModel"))
        {
            templeteModel = (DefaultTempleteModel)data["templeteModel"];
        }
        //if (templeteModel != null)
        GetExerciseNotes(this.exerciseTypeModel);
        //if (data.ContainsKey("inputManager"))
        //    inputFieldManager = (InputFieldManager)data["inputManager"];
        //inputFieldManager.inputFields.Add(exerciseNotes);
        exerciseNameText.text = userSessionManager.Instance.FormatStringAbc(exerciseTypeModel.name);
        exerciseHistory = GetExerciseData(ApiDataHandler.Instance.getHistoryData(), exerciseTypeModel.name, exerciseTypeModel.exerciseType);
        switch (exerciseTypeModel.exerciseType)
        {
            case ExerciseType.RepsOnly:
                reps.gameObject.SetActive(true);
                rir.gameObject.SetActive(true);
                break;
            case ExerciseType.TimeBased:
                timer.gameObject.SetActive(true);
                rpe.gameObject.SetActive(true);
                break;
            case ExerciseType.TimeAndMiles:
                timer.gameObject.SetActive(true);
                mile.gameObject.SetActive(true);
                rpe.gameObject.SetActive(true);
                break;
            case ExerciseType.WeightAndReps:
                weight.gameObject.SetActive(true);
                rir.gameObject.SetActive(true);
                reps.gameObject.SetActive(true);
                break;
        }
        if (exerciseTypeModel.exerciseModel.Count > 0)
        {
            int setCount = 0;
            foreach (var exerciseModel in exerciseTypeModel.exerciseModel)
            {
                setCount++;
                exerciseModel.setID = setCount;
                AddSetFromModel(exerciseModel);
            }
        }
        else
        {
            OnAddSet(false);
        }
        //if (isTemplateCreator) threeDots.gameObject.SetActive(true);
        //else threeDots.gameObject.SetActive(false);
        exerciseNotes.text = this.exerciseTypeModel.exerciseNotes;
        exerciseNotes.onEndEdit.AddListener(OnExerciseNotesChange);

        StateManager.Instance.CheckTutorial("tutorial", "Here you can add sets, remove sets with swipe, add more exercises and remove exercises.", 4);
    }
    private void AddSetFromModel(ExerciseModel exerciseModel)
    {
        GameObject prefab;
        if (isWorkoutLog)
            prefab = Resources.Load<GameObject>("Prefabs/workoutLog/workoutLogSubItems");
        else
        {
            prefab = Resources.Load<GameObject>("Prefabs/workoutLog/workoutLogSubItems");
        }

        GameObject newSubItem = Instantiate(prefab, transform.GetChild(0));
        int childCount = transform.GetChild(0).childCount;
        newSubItem.transform.SetSiblingIndex(childCount - 3);
        WorkoutLogSubItem newSubItemScript = newSubItem.GetComponent<WorkoutLogSubItem>();
        workoutLogSubItems.Add(newSubItemScript);

        HistoryExerciseModel history = null;
        if (exerciseHistory.Count > 0)
        {
            // Match sets based on their position in the list
            int setIndex = exerciseTypeModel.exerciseModel.IndexOf(exerciseModel);
            if (setIndex < exerciseHistory.Count)
            {
                history = exerciseHistory[setIndex];
            }
        }

        Dictionary<string, object> initData = new Dictionary<string, object>
        {
        { "data", exerciseModel },
        {"exerciseType", exerciseTypeModel.exerciseType },
        {"exerciseHistory", history },
        {"isWorkoutLog", isWorkoutLog },
        {"interruptedLoading", interruptedLoading }
        };

        newSubItemScript.onInit(initData, callback);
    }

    public void OnAddSet(bool addMore)
    {
        if (isWorkoutLog && !isTemplateCreator)
        {
            //FindAnyObjectByType<WorkoutLogController>().addSets = addMore;
        }
        if (addMore)
        {
            AudioController.Instance.OnButtonClick();
        }
        ExerciseModel exerciseModel = new ExerciseModel();
        exerciseTypeModel.exerciseModel.Add(exerciseModel);
        exerciseModel.setID = exerciseTypeModel.exerciseModel.Count;
        AddSetFromModel(exerciseModel);
    }
    public void OnRemoveExercisePopup(RectTransform transform)
    {
        List<object> initialData = new List<object> { this.gameObject, this.callback, isTemplateCreator };
        PopupController.Instance.OpenSidePopup("workoutLog", "RemoveExercisePopup", OnRemoveExerciseCallBack, initialData, transform);
    }
    void OnRemoveExerciseCallBack(List<object> data)
    {
        templeteModel.exerciseTemplete.Remove(exerciseTypeModel);
        Destroy(this.gameObject);
    }
    public void onRemoveSet()
    {
        this.callback.Invoke(this.exerciseTypeModel.index);
        GameObject.Destroy(gameObject);
    }
    public void OnExerciseNotesChange(string name)
    {
        exerciseTypeModel.exerciseNotes = name.ToUpper();
    }
    public void SaveExercisePreferences(string exerciseName, HistoryExerciseModel exercisedata)
    {
        string json = JsonUtility.ToJson(exercisedata);
        PlayerPrefs.SetString((exerciseName), json);
        PlayerPrefs.Save();
    }
    public HistoryExerciseModel GetResentPerformed(string exerciseName)
    {
        if (PreferenceManager.Instance.HasKey(exerciseName))
        {
            string json = PreferenceManager.Instance.GetString(exerciseName);
            return JsonUtility.FromJson<HistoryExerciseModel>(json);
        }
        return null;
    }
    public List<HistoryExerciseModel> GetExerciseData(HistoryModel historyData, string exerciseName, ExerciseType type)
    {
        List<HistoryExerciseModel> exerciseDataList = new List<HistoryExerciseModel>();

        var sortedWorkouts = historyData.exerciseTempleteModel
            .OrderByDescending(ht => DateTime.Parse(ht.dateTime))
            .ToList();

        foreach (var workout in sortedWorkouts)
        {
            var matchingExercise = workout.exerciseTypeModel
                .FirstOrDefault(e => e.exerciseName.Equals(exerciseName, StringComparison.OrdinalIgnoreCase));

            if (matchingExercise != null)
            {
                exerciseDataList.AddRange(matchingExercise.exerciseModel);
                break;
            }
        }

        return exerciseDataList;
    }
    /*
    public List<HistoryExerciseModel> GetExerciseData(HistoryModel historyData, string exerciseName, ExerciseType type)
    {
        List<HistoryExerciseModel> exerciseDataList = new List<HistoryExerciseModel>();

        foreach (var template in historyData.exerciseTempleteModel)
        {
            foreach (var exerciseType in template.exerciseTypeModel)
            {
                if (exerciseType.exerciseName.Equals(exerciseName, StringComparison.OrdinalIgnoreCase))
                {
                    exerciseDataList.AddRange(exerciseType.exerciseModel);
                }
            }
        }
        //switch (type)
        //{
        //    case ExerciseType.RepsOnly:
        //        exerciseDataList = exerciseDataList
        //        .OrderBy(e => e.reps)
        //        .ToList();
        //        break;
        //    case ExerciseType.TimeBased:
        //        exerciseDataList = exerciseDataList
        //        .OrderBy(e => e.time)
        //        .ToList();
        //        break;
        //    case ExerciseType.TimeAndMiles:
        //        break;
        //    case ExerciseType.WeightAndReps:
        //        exerciseDataList = exerciseDataList
        //        .OrderByDescending(e => e.weight * e.reps)
        //        .ToList();
        //        break;
        //}
        return exerciseDataList;
    }*/

    public void UpdateExerciseNotes(HistoryModel historyModel, ExerciseTypeModel exerciseToCheck, string templateName)
    {
        // Step 1: Sort history templates by date and time in descending order
        var sortedHistoryTemplates = historyModel.exerciseTempleteModel
            .OrderByDescending(ht => DateTime.Parse(ht.dateTime))
            .ToList();

        // Step 2: Iterate through sorted history templates
        foreach (var historyTemplate in sortedHistoryTemplates)
        {
            print(historyTemplate.dateTime + "      " + historyTemplate.templeteName);
            // Check if the exercise exists in the current history template
            var matchingExercise = historyTemplate.exerciseTypeModel
                .FirstOrDefault(e => e.exerciseName.ToLower() == exerciseToCheck.name.ToLower());

            if (matchingExercise != null)
            {
                // If a match is found, update the notes and exit the method
                exerciseToCheck.exerciseNotes = matchingExercise.exerciseNotes;
                return;
            }
        }

        // Step 3: If no match is found, leave the notes empty
        exerciseToCheck.exerciseNotes = string.Empty;
    }
    public void GetExerciseNotes(ExerciseTypeModel exercise)
    {
        if (ApiDataHandler.Instance.getNotesHistory() == null)
            return;
        foreach(var item in ApiDataHandler.Instance.getNotesHistory().exercises)
        {
            if(item.exerciseName.ToLower() == exercise.name.ToLower())
            {
                exercise.exerciseNotes = item.notes;
                break;
            }
        }
    }
}
