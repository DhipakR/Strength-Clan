
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CreateNewWorkoutController : MonoBehaviour,PageController
{
    public TextMeshProUGUI labelText;
    public TMP_InputField workoutName;
    public Transform content;
    public Button addExercise;
    public Button saveTemplate;
    public Button backButton;
    public Button deleteButton;

    private int exerciseCounter = 0;
    public List<ExerciseDataItem> exerciseDataItems = new List<ExerciseDataItem>();
    public DefaultTempleteModel templeteModel = new DefaultTempleteModel();
    [SerializeField]
    private DefaultTempleteModel orignalTempleteModel = new DefaultTempleteModel();
    int index;
    bool editWorkout;
    Action<object> callback;
    bool interruptedLoading;
    public void onInit(Dictionary<string, object> data, Action<object> callback)
    {
        interruptedLoading = data.TryGetValue("interruptedLoading", out var value) ? (bool)value : false;
        editWorkout = (bool)data["editWorkout"];
        if(editWorkout)
        {
            orignalTempleteModel = (DefaultTempleteModel)data["templeteModel"];
            index=ApiDataHandler.Instance.getTemplateData().exerciseTemplete.IndexOf(orignalTempleteModel);
            print(index);
            templeteModel = DeepCopy(orignalTempleteModel);
            deleteButton.gameObject.SetActive(true);
            deleteButton.onClick.AddListener(DeleteWorkout);
            deleteButton.onClick.AddListener(AudioController.Instance.OnButtonClick);
            Vector2 offsetMin = workoutName.GetComponent<RectTransform>().offsetMin;
            offsetMin.x = 60; // Set new left value
            workoutName.GetComponent<RectTransform>().offsetMin = offsetMin;
           
            labelText.text = "EDIT WORKOUT";
            workoutName.text = templeteModel.templeteName;
            
            List<ExerciseTypeModel> list = new List<ExerciseTypeModel>();
            foreach (var exerciseType in templeteModel.exerciseTemplete)
            {
                list.Add(exerciseType);
            }
            OnExerciseAdd(list);
        }
        else
        {
            labelText.text = "NEW WORKOUT";
            workoutName.text = (string)data["workoutName"];
            templeteModel.templeteName= (string)data["workoutName"];
            index = ApiDataHandler.Instance.getTemplateData().exerciseTemplete.Count;
        }
        this.callback = callback;
        //saveTemplate.interactable = false;
        workoutName.onEndEdit.AddListener(OnTempleteNameChange);
        addExercise.onClick.AddListener(() => AddExerciseButton());
        saveTemplate.onClick.AddListener(() => SaveNewWorkout()); 
        addExercise.onClick.AddListener(() => AudioController.Instance.OnButtonClick());
        saveTemplate.onClick.AddListener(() => AudioController.Instance.OnButtonClick());
        backButton.onClick.AddListener(() => AudioController.Instance.OnButtonClick());
        StateManager.Instance.CheckTutorial("tutorial", "Press <b>+</b> sign button to add single or multiple exercises to your workout session template!", 2);
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            OnClose();
        }
    }
    public void OnClose()
    {
        StateManager.Instance.HandleBackAction(gameObject);
        StateManager.Instance.OpenFooter(null, null, false);
    }
    public void AddExerciseButton()
    {
        Dictionary<string, object> mData = new Dictionary<string, object>
        {
            { "isWorkoutLog", false }, {"ExerciseAddOnPage",ExerciseAddOnPage.CreateWorkoutPage}
        };
        StateManager.Instance.OpenStaticScreen("exercise", gameObject, "exerciseScreen", mData, true, OnExerciseAdd,true);
        StateManager.Instance.CloseFooter();
    }
    public void OnExerciseAdd(object data)
    {
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
                            { "data", typeModel },
                            { "isWorkoutLog", false },
                            { "isTemplateCreator", true },
                            {"templeteModel", templeteModel},
                            {"interruptedLoading", interruptedLoading }
                        };

                        GameObject exercisePrefab = Resources.Load<GameObject>("Prefabs/workoutLog/workoutLogScreenDataModel");
                        //GameObject exercisePrefab = Resources.Load<GameObject>("Prefabs/createWorkout/createNewWorkoutDataModel");
                        GameObject exerciseObject = Instantiate(exercisePrefab, content);
                        exerciseObject.transform.SetSiblingIndex(content.childCount - 2);
                        exerciseObject.GetComponent<workoutLogScreenDataModel>().onInit(mData, OnRemoveIndex);
                        saveTemplate.interactable = true;
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
                //templeteModel.exerciseTemplete.Add(typeModel);

                Dictionary<string, object> mData = new Dictionary<string, object>
                {
                    { "data", typeModel },
                    { "isWorkoutLog", false },
                    {"isTemplateCreator",true },
                    {"templeteModel", templeteModel},
                    { "interruptedLoading", interruptedLoading }
            };

                GameObject exercisePrefab = Resources.Load<GameObject>("Prefabs/workoutLog/workoutLogScreenDataModel");
                GameObject exerciseObject = Instantiate(exercisePrefab, content);
                exerciseObject.GetComponent<workoutLogScreenDataModel>().onInit(mData, OnRemoveIndex);
            }
            saveTemplate.interactable = true;
        }
    }
    private void OnRemoveIndex(object data)
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
    private void OnTempleteNameChange(string name)
    {
        templeteModel.templeteName = name;
    }
    private void DeleteWorkout()
    {
        List<object> initialData = new List<object> { this.gameObject, orignalTempleteModel,index };
        PopupController.Instance.OpenPopup("createWorkout", "DeleteWorkoutPopup", null, initialData);
    }
    public void SaveNewWorkout()
    {
        if (editWorkout)
        {
            CopyAndPast(templeteModel, orignalTempleteModel);
            ValidateTemplate(templeteModel);
            //orignalTempleteModel = templeteModel;
        }
        else 
        {
            ValidateTemplate(templeteModel);
            //templeteModel.templeteName = workoutName.text;
            //ApiDataHandler.Instance.getTemplateData().exerciseTemplete.Add(templeteModel);
            
        }
        //ApiDataHandler.Instance.SaveTemplateData();
        //StateManager.Instance.OpenStaticScreen("dashboard", gameObject, "dashboardScreen", null);
        //StateManager.Instance.OpenFooter(null, null, false);
    }
    public void ValidateTemplate(DefaultTempleteModel template)
    {
        // Check if the exercise template list has any items
        if (template.exerciseTemplete.Count == 0)
        {
            string message = "Attention: You cannot save an empty workout template.";
            List<object> initialData = new List<object> { this.gameObject, template, editWorkout,true,message,index};
            PopupController.Instance.OpenPopup("createWorkout", "SaveWorkoutTempletePopup", null, initialData);
            //Debug.Log("The exercise template list is empty.");
            return;
        }

        // Check if all exercise templates have at least one exercise model
        bool allHaveExerciseModels = template.exerciseTemplete
            .All(et => et.exerciseModel.Count > 0);
        int exerciseHaveSet = template.exerciseTemplete.Count(model => model.exerciseModel.Count > 0);
        int totaltExercise = template.exerciseTemplete.Count;
        
        print(exerciseHaveSet +" "+totaltExercise);

        if (exerciseHaveSet== totaltExercise)
        {
            string message = "Are you sure you want to save workout templete.";
            List<object> initialData = new List<object> { this.gameObject, template, editWorkout, false, message, index };
            PopupController.Instance.OpenPopup("createWorkout", "SaveWorkoutTempletePopup", SaveExerciseNotesHistory, initialData);
            //Debug.Log("All exercise templates have at least one exercise model.");
        }
        else if(exerciseHaveSet>0)
        {
            string message = "Exercises with 0 sets will be discard. Are you sure you want to save it.";
            List<object> initialData = new List<object> { this.gameObject, template, editWorkout, false, message, index };
            PopupController.Instance.OpenPopup("createWorkout", "SaveWorkoutTempletePopup", SaveExerciseNotesHistory, initialData);
            Debug.Log("Some exercise templates have models, and some do not.");
        }
        else if (exerciseHaveSet == 0)
        {
            string message = "Attention: You cannot save all exercise with 0 set.";
            List<object> initialData = new List<object> { this.gameObject, template, editWorkout, true, message, index };
            PopupController.Instance.OpenPopup("createWorkout", "SaveWorkoutTempletePopup", null, initialData);
        }
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
            exerciseCopy.categoryName = exercise.categoryName;
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

    public void CopyAndPast(DefaultTempleteModel _new, DefaultTempleteModel _old)
    {
        //DefaultTempleteModel copy = new DefaultTempleteModel();
        _old.templeteName = _new.templeteName;
        _old.templeteNotes = _new.templeteNotes;
        _old.exerciseTemplete.Clear();

        // Copy each exercise template
        foreach (var exercise in _new.exerciseTemplete)
        {
            ExerciseTypeModel exerciseCopy = new ExerciseTypeModel();
            exerciseCopy.index = exercise.index;
            exerciseCopy.name = exercise.name;
            exerciseCopy.categoryName = exercise.categoryName;
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
            if(exerciseCopy.exerciseModel.Count>0)
                _old.exerciseTemplete.Add(exerciseCopy);
        }
    }
    public void SaveExerciseNotesHistory(List<object> data)
    {
        if (data[0] is List<ExerciseTypeModel> templeteModel)
        {
            foreach (object item in templeteModel)
            {
                ExerciseTypeModel exerciseType = (ExerciseTypeModel)item;
                if (exerciseType.exerciseNotes != string.Empty)
                {
                    bool exerciseExist = false;
                    var exercises = ApiDataHandler.Instance.getNotesHistory().exercises;
                    for (int index = 0; index < exercises.Count; index++)
                    {
                        ExerciseNotesHistoryItem exercise = exercises[index];
                        if (exercise.exerciseName.ToLower() == exerciseType.name.ToLower())
                        {
                            exercise.notes = exerciseType.exerciseNotes;
                            ApiDataHandler.Instance.SaveNotesHistory(exercise, index);
                            exerciseExist = true;
                            Debug.Log("Item found at index: " + index);
                            break;
                        }
                    }
                    if (!exerciseExist)
                    {
                        var newExercise = new ExerciseNotesHistoryItem
                        {
                            exerciseName = exerciseType.name,
                            notes = exerciseType.exerciseNotes
                        };
                        ApiDataHandler.Instance.SaveNotesHistory(newExercise, ApiDataHandler.Instance.getNotesHistory().exercises.Count);
                        ApiDataHandler.Instance.getNotesHistory().exercises.Add(newExercise);

                    }
                }
            }
        }
    }
}
