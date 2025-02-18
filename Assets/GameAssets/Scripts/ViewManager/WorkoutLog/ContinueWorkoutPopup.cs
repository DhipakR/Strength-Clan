using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ContinueWorkoutPopup : MonoBehaviour, IPrefabInitializer
{
    public Button continueButton, cancelButton, fade;
    private Action<List<object>> callback;
    private DashboardController dashboardController;
    private GameObject dashboardPage;

    public void InitPrefab(Action<List<object>> onFinish, List<object> data)
    {
        callback = onFinish;
        if (data != null && data.Count > 0)
        {
            dashboardPage = data[0] as GameObject;
            if (dashboardPage == null)
            {
                Debug.LogError("DashboardController instance not found in initialData.");
            }
            dashboardController = dashboardPage.GetComponent<DashboardController>();
        }
    }

    private void Start()
    {
        fade.onClick.AddListener(Cancel);
        continueButton.onClick.AddListener(Continue);
        continueButton.onClick.AddListener(AudioController.Instance.OnButtonClick);
        cancelButton.onClick.AddListener(Cancel);
        cancelButton.onClick.AddListener(AudioController.Instance.OnButtonClick);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cancel();
        }
    }

    void Continue()
    {

        // Load the saved workout data
        string json = PlayerPrefs.GetString("SavedOngoingWorkout");
        Debug.Log(json);
        DefaultTempleteModel workoutData = JsonUtility.FromJson<DefaultTempleteModel>(json);
        if (workoutData == null)
        {
            Debug.LogError("Failed to deserialize workout data.");
            return;
        }

        // Ensure the DashboardController instance is available
        if (dashboardController == null)
        {
            Debug.LogError("DashboardController instance is not set.");
            return;
        }

        // Prepare the data to open the workout log screen
        Dictionary<string, object> mData = new Dictionary<string, object>
    {
        { "isTemplateCreator", false },
        { "dataTemplate", workoutData }
    };

        // Use the onReloadData method from the DashboardController instance
        Action<object> onReloadDataCallback = dashboardController.onReloadData;

        // Open the workout log screen with the callback
        StateManager.Instance.OpenStaticScreen("workoutLog", dashboardPage, "workoutLogScreen", mData, true, onReloadDataCallback, true, 1);
        StateManager.Instance.CloseFooter();

        PopupController.Instance.ClosePopup("ContinueWorkoutPopup");
    }

    void Cancel()
    {
        // Delete the saved workout data
        PlayerPrefs.DeleteKey("SavedOngoingWorkout");
        PlayerPrefs.Save();

        // Close the popup
        callback?.Invoke(null);
        PopupController.Instance.ClosePopup("ContinueWorkoutPopup");
    }
}