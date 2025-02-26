using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ContinueWorkoutPopup : MonoBehaviour, IPrefabInitializer
{
    public Button continueButton, cancelButton, fade;
    public TextMeshProUGUI title; // Add this field
    public TextMeshProUGUI description; // Add this field

    private Action<List<object>> callback;
    private DashboardController dashboardController;
    private GameObject dashboardPage;
    private string mode; // Store the mode (continue or tutorial)
    private int tutorialId; // Store the tutorial ID

    public void InitPrefab(Action<List<object>> onFinish, List<object> data)
    {
        callback = onFinish;

        if (data != null && data.Count > 0)
        {
            // Check the mode (continue or tutorial)
            mode = data[0] as string;

            if (mode == "continue")
            {
                // Existing functionality for "continue"
                dashboardPage = data[1] as GameObject;
                if (dashboardPage == null)
                {
                    Debug.LogError("DashboardController instance not found in initialData.");
                }
                dashboardController = dashboardPage.GetComponent<DashboardController>();
            }
            else if (mode == "tutorial")
            {
                // New functionality for "tutorial"
                title.text = "Tutorial"; // Set the title

                // Check if data[1] contains a description
                if (data.Count > 1 && data[1] is string tutorialDescription)
                {
                    description.text = tutorialDescription; // Set the description
                }

                // Check if data[3] contains the tutorial ID
                if (data.Count > 3 && data[3] is int id)
                {
                    tutorialId = id; // Store the tutorial ID
                }

                // Update button texts
                continueButton.GetComponentInChildren<TextMeshProUGUI>().text = "Quit Tutorial";
                cancelButton.GetComponentInChildren<TextMeshProUGUI>().text = "Okay";
            }
            else
            {
                Debug.LogError("Invalid mode specified in data[0].");
            }
        }
    }

    private void Start()
    {
        fade.onClick.AddListener(Cancel);
        continueButton.onClick.AddListener(OnContinueButtonClick);
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

    private void OnContinueButtonClick()
    {
        if (mode == "continue")
        {
            Continue();
        }
        else if (mode == "tutorial")
        {
            QuitTutorial();
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

    void QuitTutorial()
    {
        // Set the PlayerPrefs flag to indicate the tutorial is finished
        PlayerPrefs.SetInt("TutorialFinished", 1);
        PlayerPrefs.Save();

        // Close the popup
        callback?.Invoke(null);
        PopupController.Instance.ClosePopup("ContinueWorkoutPopup");
    }

    void Cancel()
    {
        if (mode == "continue")
        {
            // Delete the saved workout data
            PlayerPrefs.DeleteKey("SavedOngoingWorkout");
            PlayerPrefs.Save();
        }
        else if (mode == "tutorial")
        {
            // Set the PlayerPrefs flag for this specific tutorial part
            PlayerPrefs.SetInt($"TutorialFinishedPart{tutorialId}", 1);
            PlayerPrefs.Save();
        }

        // Close the popup
        callback?.Invoke(null);
        PopupController.Instance.ClosePopup("ContinueWorkoutPopup");
    }
}