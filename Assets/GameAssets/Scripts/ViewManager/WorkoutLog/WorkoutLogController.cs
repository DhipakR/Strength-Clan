using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class WorkoutLogController : MonoBehaviour, PageController
{
    public TextMeshProUGUI workoutNameText;
    //public TMP_InputField workoutNotes;
    public TMP_InputField editWorkoutName;
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI messageText;
    public Button editWorkoutButton;
    public Button cancelWorkoutButton;
    public Button finishWorkoutButton;
    public Button cancelWorkoutButton2;
    public Button addExerciseButton;
    public Button timerButton;
    public Transform content;


    public bool addSets;
    private int exerciseCounter = 0;
    public List<ExerciseInformationModel> exercises = new List<ExerciseInformationModel>();
    private List<ExerciseDataItem> exerciseDataItems = new List<ExerciseDataItem>();
    public DefaultTempleteModel templeteModel = new DefaultTempleteModel();
    //public DefaultTempleteModel orignalModel = new DefaultTempleteModel();

    private bool isTemplateCreator;
    private bool isTimerRunning = false;
    private float elapsedTime = 0f;
    private Coroutine timerCoroutine;
    private Color enabledColor = Color.white;
    private Color disabledColor = Color.gray;
    bool back;
    Action<object> callback;
    public void onInit(Dictionary<string, object> data, Action<object> callback)
    {
        this.callback = callback;
        isTemplateCreator = (bool)data["isTemplateCreator"];

        if (data.ContainsKey("dataTemplate"))
        {

            DefaultTempleteModel dataTemplate = DeepCopy((DefaultTempleteModel)data["dataTemplate"]);
            templeteModel.templeteName= dataTemplate.templeteName;
            //workoutNotes.text = dataTemplate.templeteNotes;
            List<ExerciseTypeModel> list = new List<ExerciseTypeModel>();
            foreach (var exerciseType in dataTemplate.exerciseTemplete)
            {
                list.Add(exerciseType);
            }
            //UpdateExerciseNotesFromHistory(ApiDataHandler.Instance.getHistoryData(), (DefaultTempleteModel)data["dataTemplate"]);
            OnExerciseAdd(list);
            if (workoutNameText != null)
            {
                workoutNameText.text = dataTemplate.templeteName;
                editWorkoutName.textComponent.text = dataTemplate.templeteNotes; 
                float textWidth = workoutNameText.preferredWidth;
                workoutNameText.transform.GetComponent<RectTransform>().sizeDelta=new Vector2(textWidth, workoutNameText.transform.GetComponent<RectTransform>().sizeDelta.y);
            }
            //workoutNotes.onValueChanged.AddListener(OnNotesChange);
        }
        editWorkoutName.onEndEdit.AddListener(OnNameChanged);
        editWorkoutButton.onClick.AddListener(EditWorkoutName);
        editWorkoutButton.onClick.AddListener(AudioController.Instance.OnButtonClick);
        cancelWorkoutButton.onClick.AddListener(AudioController.Instance.OnButtonClick);
        finishWorkoutButton.onClick.AddListener(AudioController.Instance.OnButtonClick);
        cancelWorkoutButton2.onClick.AddListener(AudioController.Instance.OnButtonClick);
        addExerciseButton.onClick.AddListener(AudioController.Instance.OnButtonClick);
        timerButton.onClick.AddListener(AudioController.Instance.OnButtonClick);
        timerButton.onClick.AddListener(()=>OnToggleWorkout(data: null));

        //saveButton.interactable = false;
        OnToggleWorkout(null);
        back = true;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && back)
        {
            OnBack();
        }
    }
    void OnBackCheck(List<object> list)
    {
        back = true;
    }
    void EditWorkoutName()
    {
        workoutNameText.gameObject.SetActive(false);
        editWorkoutName.gameObject.SetActive(true);
        editWorkoutName.text=workoutNameText.text;
        editWorkoutName.ActivateInputField();
    }
    public void OnToggleWorkout(List<object> data)
    {
        //if (templeteModel.exerciseTemplete.Count == 0)
        //{
        //    return;
        //}
        isTimerRunning = !isTimerRunning;

        if (isTimerRunning)
        {
            //timerText.color = enabledColor;
            switch (ApiDataHandler.Instance.gameTheme)
            {
                case Theme.Light:
                    timerText.color = userSessionManager.Instance.lightButtonTextColor;
                    break;
                case Theme.Dark:
                    timerText.color = Color.white;
                    break;
            }

            if (timerCoroutine == null)
            {
                timerCoroutine = StartCoroutine(TimerCoroutine());
            }
        }
        else
        {
            timerText.color = disabledColor;

            if (timerCoroutine != null)
            {
                StopCoroutine(timerCoroutine);
                timerCoroutine = null;
            }
        }
    }

    private IEnumerator TimerCoroutine()
    {
        while (isTimerRunning)
        {
            elapsedTime += Time.deltaTime;
            int minutes = Mathf.FloorToInt(elapsedTime / 60);
            int seconds = Mathf.FloorToInt(elapsedTime % 60);
            timerText.text = /*"Timer: " +*/ string.Format("{0:00}:{1:00}", minutes, seconds);
            yield return null;
        }
    }

    public void OnNameChanged(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            workoutNameText.gameObject.SetActive(true);
            editWorkoutName.gameObject.SetActive(false);
        }
        else
        {
            templeteModel.templeteName = name.ToUpper();
            workoutNameText.text = name.ToUpper();
            float textWidth = workoutNameText.preferredWidth;
            workoutNameText.transform.GetComponent<RectTransform>().sizeDelta = new Vector2(textWidth, workoutNameText.transform.GetComponent<RectTransform>().sizeDelta.y);
            workoutNameText.gameObject.SetActive(true);
            editWorkoutName.gameObject.SetActive(false);
        }

    }

    public void AddExerciseButton()
    {
        back = false;
        Dictionary<string, object> mData = new Dictionary<string, object>
        {
            { "isWorkoutLog", true }, {"ExerciseAddOnPage",ExerciseAddOnPage.WorkoutLogPage}
        };
        StateManager.Instance.OpenStaticScreen("exercise", null, "exerciseScreen", mData, true, OnExerciseAdd);
    }



    public void OnExerciseAdd(object data)
    {
        OnBackCheck(null);
        //List<object> dataList = data as List<object>;
        if (data == null)
        {
            print("data null");
        }

        if (data is List<ExerciseDataItem> dataList)
        {
            foreach (object item in dataList)
            {
                ExerciseTypeModel typeModel;

                if (item is ExerciseDataItem dataItem)
                {
                    bool exerciseExists = templeteModel.exerciseTemplete
                       .Any(exercise => exercise.name.ToLower() == dataItem.exerciseName.ToLower());

                    if (!exerciseExists)
                    {
                        typeModel = new ExerciseTypeModel
                        {
                            name = dataItem.exerciseName,
                            categoryName = dataItem.category,
                            exerciseModel = new List<ExerciseModel>(),
                            index = exerciseCounter++,
                            exerciseType = dataItem.exerciseType
                        };

                        templeteModel.exerciseTemplete.Add(typeModel);

                        exerciseDataItems.Add(dataItem);

                        Dictionary<string, object> mData = new Dictionary<string, object>
                        {
                            { "data", typeModel }, { "isWorkoutLog", true },{ "isTemplateCreator", isTemplateCreator },
                            {"templeteModel",templeteModel},{"inputManager",this.GetComponent<ScrollRect>()}
                        };

                        GameObject exercisePrefab = Resources.Load<GameObject>("Prefabs/workoutLog/workoutLogScreenDataModel");
                        GameObject exerciseObject = Instantiate(exercisePrefab, content);
                        int childCount = content.childCount;
                        //exerciseObject.transform.SetSiblingIndex(childCount - 2);
                        exerciseObject.GetComponent<workoutLogScreenDataModel>().onInit(mData, SaveButtonInteractable);
                        print("1");
                    }
                }
            }
        }
        else if (data is List<ExerciseTypeModel> dataList2)
        {
            foreach (object item in dataList2)
            {
                ExerciseTypeModel typeModel;

                typeModel = (ExerciseTypeModel)item;
                templeteModel.exerciseTemplete.Add(typeModel);

                Dictionary<string, object> mData = new Dictionary<string, object>
                {
                    { "data", typeModel },
                    { "isWorkoutLog", true },
                    {"isTemplateCreator",isTemplateCreator },
                    {"templeteModel",templeteModel}
                    //{"inputManager",this.GetComponent<InputFieldManager>()}
                };

                GameObject exercisePrefab = Resources.Load<GameObject>("Prefabs/workoutLog/workoutLogScreenDataModel");
                GameObject exerciseObject = Instantiate(exercisePrefab, content);
                exerciseObject.GetComponent<workoutLogScreenDataModel>().onInit(mData, SaveButtonInteractable);
                print("2");
            }
        }
        else { print("null"); }
    }

    private void SaveButtonInteractable(object data)
    {
        //bool check = (bool)data;
        //if (check) { saveButton.interactable = true; }
        //else
        //{
        //    foreach (var exerciseType in templeteModel.exerciseTemplete)
        //    {
        //        foreach (var exercise in exerciseType.exerciseModel)
        //        {
        //            if (exercise.toggle)
        //            {
        //                saveButton.interactable = true;
        //                return;
        //            }
        //        }
        //    }
        //    saveButton.interactable = false;
        //}
    }
    private void OnRemoveIndex(object data)
    {
        if (isTemplateCreator)
        {
            int index = (int)data;

            for (int i = 0; i < templeteModel.exerciseTemplete.Count; i++)
            {
                if (templeteModel.exerciseTemplete[i].index == index)
                {
                    templeteModel.exerciseTemplete.RemoveAt(i);
                    break;
                }
            }
        }
    }

    public (int,int) CheckAllSetsComplete()
    {
        int totalSets=0;
        int completedSets=0;
        foreach (var exerciseType in templeteModel.exerciseTemplete)
        {
            foreach (var exercise in exerciseType.exerciseModel)
            {
                totalSets++;
                if (exercise.toggle)
                {
                    completedSets++;
                }
            }
        }
        return (totalSets, completedSets);
    }
    public void OnFinishPopupOrMessage()
    {
        (int totalSets, int completedSets) = CheckAllSetsComplete();
        if (completedSets == 0)
        {
            GlobalAnimator.Instance.ShowTextMessage(messageText, "Please Complete some sets before Finishing", 2f);
            return;
        }
        //OnToggleWorkout(null);
       
        if (completedSets > 0 && completedSets != totalSets)
        {
            string message = "All invalid/empty sets will be discarded. All sets with valid data will be automatically marked as completed.";
            List<object> initialData = new List<object> { this.gameObject, templeteModel, this.callback,message, isTemplateCreator, SetDataForHistory() };
            //Action<List<object>> onFinish = OnToggleWorkout;
            Action<List<object>> onFinish = OnBackCheck;
            PopupController.Instance.OpenPopup("workoutLog", "FinishWorkoutPopup", onFinish, initialData);
            return;
        }
        if (completedSets == totalSets)
        {
            string message = "Are you sure you want to finish this workout.";
            List<object> initialData = new List<object> { this.gameObject, templeteModel, this.callback,message, isTemplateCreator, SetDataForHistory() };
            //Action<List<object>> onFinish = OnToggleWorkout;
            Action<List<object>> onFinish = OnBackCheck;
            PopupController.Instance.OpenPopup("workoutLog", "FinishWorkoutPopup", onFinish, initialData);
            return;
        }
    }

    public HistoryTempleteModel SetDataForHistory()
    {
        DateTime currentDateTime = DateTime.Now;
        float totalWeightInKgs = 0;
        switch ((WeightUnit)ApiDataHandler.Instance.GetWeightUnit())
        {
            case WeightUnit.kg:
                totalWeightInKgs = CalculateTotalWeight(templeteModel);
                break;
            case WeightUnit.lbs:
                totalWeightInKgs = /*Mathf.RoundToInt*/(userSessionManager.Instance.ConvertLbsToKg(CalculateTotalWeight(templeteModel)));
                break;
        }
        var historyTemplate = new HistoryTempleteModel
        {
            templeteName = templeteModel.templeteName,
            dateTime = currentDateTime.ToString("MMM dd, yyyy"),
            completedTime = (int)elapsedTime,
            totalWeight = totalWeightInKgs,
            prs = 0 // Assuming PRs are not tracked here. Adjust as needed.
        };
        // Populate HistoryExerciseTypeModel list
        foreach (var exerciseType in templeteModel.exerciseTemplete)
        {
            var historyExerciseType = new HistoryExerciseTypeModel
            {
                exerciseName = exerciseType.name,
                categoryName = exerciseType.categoryName,
                exerciseNotes= exerciseType.exerciseNotes,
                index = exerciseType.index,
                exerciseType = exerciseType.exerciseType,
                exerciseModel = new List<HistoryExerciseModel>()
            };
            //print(historyExerciseType.categoryName + "/" + exerciseType.categoryName);
            // Populate HistoryExerciseModel list but only add exercises where toggle is true
            foreach (var exercise in exerciseType.exerciseModel)
            {
                //print("bool " + exercise.toggle);
                if (exercise.toggle) // Only add exercise if toggle is true
                {
                    float weightInKgs = 0;
                    switch ((WeightUnit)ApiDataHandler.Instance.GetWeightUnit())
                    {
                        case WeightUnit.kg:
                            weightInKgs = exercise.weight;
                            break;
                        case WeightUnit.lbs:
                            weightInKgs = /*Mathf.RoundToInt*/(userSessionManager.Instance.ConvertLbsToKg(exercise.weight));
                            break;
                    }
                    var historyExercise = new HistoryExerciseModel
                    {
                        weight = weightInKgs,
                        reps = exercise.reps,
                        time = exercise.time
                    };

                    historyExerciseType.exerciseModel.Add(historyExercise);
                }
            }

            // Only add the exerciseType if it has any exercises with toggle true
            if (historyExerciseType.exerciseModel.Count > 0)
            {
                historyTemplate.exerciseTypeModel.Add(historyExerciseType);
            }
        }
        return historyTemplate;
    }
    public void Finish()
    {

        isTimerRunning = false;
        DateTime currentDateTime = DateTime.Now;
        var historyTemplate = new HistoryTempleteModel
        {
            templeteName = templeteModel.templeteName,
            dateTime = currentDateTime.ToString("MMM dd, yyyy"),
            completedTime = (int)elapsedTime,
            totalWeight = CalculateTotalWeight(templeteModel),
            prs = 0 // Assuming PRs are not tracked here. Adjust as needed.
        };
        // Populate HistoryExerciseTypeModel list
        foreach (var exerciseType in templeteModel.exerciseTemplete)
        {
            var historyExerciseType = new HistoryExerciseTypeModel
            {
                exerciseName = exerciseType.name,
                categoryName = exerciseType.categoryName,
                index = exerciseType.index,
                exerciseType = exerciseType.exerciseType,
                exerciseModel = new List<HistoryExerciseModel>()
            };
            print(historyExerciseType.categoryName + "/" + exerciseType.categoryName);
            // Populate HistoryExerciseModel list but only add exercises where toggle is true
            foreach (var exercise in exerciseType.exerciseModel)
            {
                print("bool "+exercise.toggle);
                if (exercise.toggle) // Only add exercise if toggle is true
                {
                    var historyExercise = new HistoryExerciseModel
                    {
                        weight = exercise.weight,
                        reps = exercise.reps,
                        time = exercise.time
                    };

                    historyExerciseType.exerciseModel.Add(historyExercise);
                }
            }

            // Only add the exerciseType if it has any exercises with toggle true
            if (historyExerciseType.exerciseModel.Count > 0)
            {
                historyTemplate.exerciseTypeModel.Add(historyExerciseType);
            }
        }
        if (historyTemplate.exerciseTypeModel.Count > 0)
        {
            ApiDataHandler.Instance.AddItemToHistoryData(historyTemplate);
            ApiDataHandler.Instance.SaveHistory();
        }

        if (isTemplateCreator)
        {
            if (templeteModel.exerciseTemplete.Count > 0)
            {
                ApiDataHandler.Instance.AddItemToTemplateData(templeteModel);
                ApiDataHandler.Instance.SaveTemplateData();
            }
            StateManager.Instance.OpenStaticScreen("dashboard", gameObject, "dashboardScreen", null);
            StateManager.Instance.OpenFooter(null, null, false);
            return;
        }

        if (addSets)
        {
            List<object> initialData = new List<object> { this.gameObject, templeteModel, this.callback };
            PopupController.Instance.OpenPopup("workoutLog", "FinishWorkoutPopup", null, initialData);
        }
        else
        {
            //StateManager.Instance.HandleBackAction(gameObject);
            StateManager.Instance.OpenStaticScreen("dashboard", gameObject, "dashboardScreen", null);
            StateManager.Instance.OpenFooter(null, null, false);
        }

        
        
        //}
        //OnBack();
    }

    public void OnBack()
    {
        //OnToggleWorkout(null);
        //StateManager.Instance.HandleBackAction(gameObject);
        List<object> initialData = new List<object> { this.gameObject };
        //Action<List<object>> onFinish = OnToggleWorkout;
        Action<List<object>> onFinish = OnBackCheck;
        PopupController.Instance.OpenPopup("workoutLog", "CancelWorkoutPopup", onFinish, initialData);
    }
    private float CalculateTotalWeight(DefaultTempleteModel defaultTemplate)
    {
        float totalWeight = 0;

        foreach (var exerciseType in defaultTemplate.exerciseTemplete)
        {
            if (exerciseType.exerciseType == ExerciseType.WeightAndReps)
            {
                foreach (var exercise in exerciseType.exerciseModel)
                {
                    totalWeight += exercise.weight * exercise.reps;
                }
            }
        }

        return totalWeight;
    }
    public int GetIndexByTempleteName(string name)
    {
        TemplateData templateData = ApiDataHandler.Instance.getTemplateData();
        return templateData.exerciseTemplete.FindIndex(t => t.templeteName == name);
    }
    public DefaultTempleteModel DeepCopy(DefaultTempleteModel original)
    {
        DefaultTempleteModel copy = new DefaultTempleteModel();
        copy.templeteName = original.templeteName;
        copy.templeteNotes = original.templeteNotes;

        // Copy each exercise template
        foreach (var exercise in original.exerciseTemplete)
        {
            ExerciseTypeModel exerciseCopy = new ExerciseTypeModel();
            exerciseCopy.index = exercise.index;
            exerciseCopy.name = exercise.name;
            exerciseCopy.categoryName= exercise.categoryName;
            exerciseCopy.exerciseType = exercise.exerciseType;

            // Copy each exercise model in the template
            foreach (var exerciseModel in exercise.exerciseModel)
            {
                ExerciseModel exerciseModelCopy = new ExerciseModel();
                exerciseModelCopy.setID = exerciseModel.setID;
                exerciseModelCopy.previous = exerciseModel.previous;
                exerciseModelCopy.weight = exerciseModel.weight;
                exerciseModelCopy.rir = exerciseModel.rir;
                exerciseModelCopy.reps = exerciseModel.reps;
                exerciseModelCopy.toggle = exerciseModel.toggle;
                exerciseModelCopy.time = exerciseModel.time;
                exerciseModelCopy.mile = exerciseModel.mile;

                exerciseCopy.exerciseModel.Add(exerciseModelCopy);
            }
            copy.exerciseTemplete.Add(exerciseCopy);
        }

        return copy;
    }
    public void UpdateExerciseNotesFromHistory(HistoryModel historyModel, DefaultTempleteModel defaultTemplateModel)
    {
        // Step 1: Filter history models with matching template names
        var matchingHistoryTemplates = historyModel.exerciseTempleteModel
            .Where(ht => ht.templeteName == defaultTemplateModel.templeteName)
            .OrderByDescending(ht => DateTime.Parse(ht.dateTime)) // Sort by latest date first
            .ToList();

        // Step 2: Iterate through the exercise templates in the default template model
        foreach (var exercise in defaultTemplateModel.exerciseTemplete)
        {
            bool noteAssigned = false;

            // Step 3: Check through each matching history template
            foreach (var historyTemplate in matchingHistoryTemplates)
            {
                // Check if the exercise name exists in the history template
                var matchingExercise = historyTemplate.exerciseTypeModel
                    .FirstOrDefault(e => e.exerciseName == exercise.name);

                if (matchingExercise != null)
                {
                    // Assign the notes from the history template to the default template
                    exercise.exerciseNotes = matchingExercise.exerciseNotes;
                    noteAssigned = true;
                    break; // Exit loop once note is assigned
                }
            }

            // If no matching exercise found in any history template, the notes remain unchanged
            if (!noteAssigned)
            {
                exercise.exerciseNotes = string.Empty; // Optional: Reset to empty if needed
            }
        }
    }
}
