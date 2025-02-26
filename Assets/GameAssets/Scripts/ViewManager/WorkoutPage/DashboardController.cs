using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static WorkoutLogController;

public class DashboardController : MonoBehaviour, PageController, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public GameObject noWorkoutMessage,noResultMessage;
    public TMP_InputField searchInputField;
    public Transform content;
    public RectTransform switchButton;
    public TextMeshProUGUI switchWorkout, switchSplit;
    public Button createNewWorkout, startNewWorkout, workout, split;
    public ScrollRect scroll;
    List<GameObject> items = new List<GameObject>();
    bool isWorkout;
    public void onInit(Dictionary<string, object> data, Action<object> callback)
    {
        onReloadData(null);
        searchInputField.onValueChanged.AddListener(OnSearchChanged);
        Workout();
        isWorkout = true;
        createNewWorkout.onClick.AddListener(AudioController.Instance.OnButtonClick);
        startNewWorkout.onClick.AddListener(AudioController.Instance.OnButtonClick);
        workout.onClick.AddListener(AudioController.Instance.OnButtonClick);
        split.onClick.AddListener(AudioController.Instance.OnButtonClick);
        StreakAndCharacterManager.Instance.UpdateStreak();
        bool interruptCheck = StateManager.Instance.CheckTutorial("tutorial", "Press <b>+</b> sign button to create a new workout session template!", 1);
        if (interruptCheck)
        {
            CheckSavedOngoingWorkout();
        }
    }
    private void CheckSavedOngoingWorkout()
    {
        if (PlayerPrefs.HasKey("SavedOngoingWorkout"))
        {
            List<object> initialData = new List<object> { "continue", this.gameObject };
            Action<List<object>> onFinish = (data) => { };
            PopupController.Instance.OpenPopup("workoutLog", "ContinueWorkoutPopup", onFinish, initialData);
        }
    }
    private void OnEnable()
    {
        foreach(GameObject go in items)
        {
            go.SetActive(false);
            go.SetActive(true);
        }
        Workout();
        isWorkout = true;
    }
    public void Play()
    {
        if (userSessionManager.Instance.selectedTemplete != null)
        {
            AudioController.Instance.OnButtonClick();
            StartEmptyWorkoutWithTemplate(userSessionManager.Instance.selectedTemplete);
        }
    }
    public void CreateNewWorkout()
    {
        int number = content.childCount;
        string templeteName = "Workout " + number;
        while(ApiDataHandler.Instance.getTemplateData().exerciseTemplete.Any(t => t.templeteName == templeteName))
        {
            number++;
            templeteName = "Workout " + number;
        }
        Dictionary<string, object> mData = new Dictionary<string, object>
            {
                { "workoutName", templeteName },
                {"editWorkout", false}
            };
        StateManager.Instance.OpenStaticScreen("createWorkout", gameObject, "createNewWorkoutScreen", mData, true, onReloadData,true);
        StateManager.Instance.CloseFooter();
    }
    public void StartEmptyWorkout()
    {
        CheckSavedOngoingWorkout();
        DefaultTempleteModel exercise = new();
        exercise.templeteName="Workout " + content.childCount.ToString();
        Dictionary<string, object> mData = new Dictionary<string, object>
        {
            {"dataTemplate", exercise }, { "isTemplateCreator", true }
        };
        Action<object> callback = onReloadData;
        StateManager.Instance.OpenStaticScreen("workoutLog", gameObject, "workoutLogScreen", mData, true, callback, true);
        StateManager.Instance.CloseFooter();
    }
    public void onReloadData(object data)
    {
        foreach (Transform child in content)
        {
            Destroy(child.gameObject);
        }
        if (ApiDataHandler.Instance.getTemplateData().exerciseTemplete.Count == 0)
        {
            scroll.gameObject.SetActive(false);
            noWorkoutMessage.SetActive(true);
            noResultMessage.SetActive(false);
        }
        else
        {
            noWorkoutMessage.SetActive(false);
            noResultMessage.SetActive(false);
        }
        foreach (var exercise in ApiDataHandler.Instance.getTemplateData().exerciseTemplete)
        {
            DefaultTempleteModel templeteData = exercise;
            Dictionary<string, object> mData = new Dictionary<string, object>
            {
                { "data", templeteData },
                {"parent",gameObject },
                {"scroll",scroll }
            };

            GameObject exercisePrefab = Resources.Load<GameObject>("Prefabs/dashboard/dashboardDataModel");
            GameObject exerciseObject = Instantiate(exercisePrefab, content);

            DashboardItemController itemController = exerciseObject.GetComponent<DashboardItemController>();
            itemController.onInit(mData,StartEmptyWorkoutWithTemplate);
        }
    }
    public void StartEmptyWorkoutWithTemplate(object exercise)
    {
        CheckSavedOngoingWorkout();
        Dictionary<string, object> mData = new Dictionary<string, object>
        {
            { "isTemplateCreator", false },
            { "dataTemplate", exercise }
        };

        //Action<object> callback = onReloadData;

        StateManager.Instance.OpenStaticScreen("workoutLog", gameObject, "workoutLogScreen", mData,true);
    }

    void OnSearchChanged(string searchQuery)
    {
        LoadExerciseTemplates(searchQuery);
    }
    void LoadExerciseTemplates(string filter = "")
    {
        foreach(Transform child in content)
        {
            Destroy(child.gameObject) ;
        }
        TemplateData exerciseData = ApiDataHandler.Instance.getTemplateData();

        bool showAll = string.IsNullOrEmpty(filter);
        bool foundMatch = false;

        foreach (DefaultTempleteModel template in exerciseData.exerciseTemplete)
        {
            string lowerFilter = filter.ToLower();

            // Check if the templateName or any exerciseType name matches the filter
            if (!showAll &&
                !(template.templeteName.ToLower().Contains(lowerFilter) ||
                  template.exerciseTemplete.Any(e => e.name.ToLower().Contains(lowerFilter))))
            {
                continue;
            }
            foundMatch = true;
            noResultMessage.SetActive(false);
            noWorkoutMessage.SetActive(false);
            // If a match is found, spawn the dashboard prefab
            GameObject exercisePrefab = Resources.Load<GameObject>("Prefabs/dashboard/dashboardDataModel");
            GameObject newExerciseObject = Instantiate(exercisePrefab, content);

            // Initialize data for the dashboard item
            Dictionary<string, object> initData = new Dictionary<string, object>
            {
                { "data", template },
                {"parent",gameObject },
                {"scroll",scroll }
            };

            // Initialize the DashboardItemController
            DashboardItemController itemController = newExerciseObject.GetComponent<DashboardItemController>();
            itemController.onInit(initData, StartEmptyWorkoutWithTemplate);
        }
        if (!foundMatch)
        {
            noResultMessage.SetActive(true);
            noWorkoutMessage.SetActive(false);
        }
    }
 
    public void OnHistory()
    {
        StateManager.Instance.OpenStaticScreen("history", gameObject, "historyScreen", null, true, null);
    }

    public void Workout()
    {
        if (isWorkout) return;
        isWorkout = true;
        //AudioController.Instance.OnButtonClick();
        GlobalAnimator.Instance.AnimateRectTransformX(switchButton, -3, 0.25f);
        switch(ApiDataHandler.Instance.gameTheme)
        {
            case Theme.Light:
                switchWorkout.color = new Color32(255, 255, 255, 255);
                switchSplit.color = new Color32(92, 59, 28, 155);
                break;
            case Theme.Dark:
                switchWorkout.color = new Color32(51, 23, 23, 255);
                switchSplit.color = new Color32(171, 162, 162, 255);
                break;
        }
    }
    public void Splits()
    {
        if(!isWorkout) return;
        isWorkout = false;
        //AudioController.Instance.OnButtonClick();
        GlobalAnimator.Instance.AnimateRectTransformX(switchButton, 141, 0.25f);
        switch (ApiDataHandler.Instance.gameTheme)
        {
            case Theme.Light:
                switchSplit.color = new Color32(255, 255, 255, 255);
                switchWorkout.color = new Color32(92, 59, 28, 155);
                break;
            case Theme.Dark:
                switchSplit.color = new Color32(51, 23, 23, 255);
                switchWorkout.color = new Color32(171, 162, 162, 255);
                break;
        }
    }
    //--------------------------------------------------------------------------------------
    public ScrollRect scrollRect; 
    public ScrollRect currentChildScrollRect; 
    private bool isHorizontalDrag;

    public void OnBeginDrag(PointerEventData eventData)
    {
        print("drag begin");
        // Detect drag direction (horizontal or vertical)
        Vector2 delta = eventData.delta;
        isHorizontalDrag = Mathf.Abs(delta.x) > Mathf.Abs(delta.y);

        if (isHorizontalDrag)
        {
            // Forward drag event to the parent ScrollRect
            scrollRect.OnBeginDrag(eventData);
        }
        else
        {
            // Find the current child ScrollRect under the pointer
            currentChildScrollRect = FindChildScrollRect(eventData);
            if (currentChildScrollRect != null)
            {
                currentChildScrollRect.OnBeginDrag(eventData);
            }
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (isHorizontalDrag)
        {
            // Forward drag event to the parent ScrollRect
            scrollRect.OnDrag(eventData);
        }
        else if (currentChildScrollRect != null)
        {
            // Forward drag event to the active child ScrollRect
            currentChildScrollRect.OnDrag(eventData);
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (isHorizontalDrag)
        {
            // Forward end drag event to the parent ScrollRect
            scrollRect.OnEndDrag(eventData);
        }
        else if (currentChildScrollRect != null)
        {
            // Forward end drag event to the active child ScrollRect
            currentChildScrollRect.OnEndDrag(eventData);
            currentChildScrollRect = null; // Reset the active child ScrollRect
        }
    }

    private ScrollRect FindChildScrollRect(PointerEventData eventData)
    {
        // Check if the pointer is over a vertical ScrollRect
        foreach (ScrollRect scrollRect in GetComponentsInChildren<ScrollRect>())
        {
            if (scrollRect != scrollRect && RectTransformUtility.RectangleContainsScreenPoint(scrollRect.GetComponent<RectTransform>(), eventData.position))
            {
                return scrollRect; // Return the ScrollRect under the pointer
            }
        }

        return null; // No matching child ScrollRect found
    }
}
