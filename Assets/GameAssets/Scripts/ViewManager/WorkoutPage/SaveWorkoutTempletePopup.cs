using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SaveWorkoutTempletePopup : MonoBehaviour, IPrefabInitializer
{
    public TextMeshProUGUI statementText;
    public Button yesButton, noButton,continueButton, fade;
    private GameObject workoutScreen;
    private DefaultTempleteModel templeteModel;
    Action<List<object>> callback;
    public int index;
    bool isContinueButton;
    bool isEdit;
    public void InitPrefab(Action<List<object>> onFinish, List<object> data)
    {
        callback = onFinish;
        workoutScreen = (GameObject)data[0];
        templeteModel = (DefaultTempleteModel)data[1];
        isEdit = (bool)data[2];
        isContinueButton = (bool)data[3];
        statementText.text = (string)data[4];
        index=(int)data[5];
        if (isContinueButton)
        {
            continueButton.gameObject.SetActive(true);
            yesButton.gameObject.SetActive(false);
            noButton.gameObject.SetActive(false);
        }
        else
        {
            continueButton.gameObject.SetActive(false);
            yesButton.gameObject.SetActive(true);
            noButton.gameObject.SetActive(true);
        }
    }
    private void Start()
    {
        yesButton.onClick.AddListener(YesButton);
        noButton.onClick.AddListener(NoButton);
        continueButton.onClick.AddListener(NoButton);
        fade.onClick.AddListener(NoButton);
        yesButton.onClick.AddListener(AudioController.Instance.OnButtonClick);
        noButton.onClick.AddListener(AudioController.Instance.OnButtonClick);
        continueButton.onClick.AddListener(AudioController.Instance.OnButtonClick);
    }
    public void YesButton()
    {
        List<object> obj = new List<object>();
        obj.Add(templeteModel.exerciseTemplete);
        templeteModel.exerciseTemplete.RemoveAll(model => model.exerciseModel.Count == 0);
        if (isEdit)
        {
            callback?.Invoke(obj);
            //ApiDataHandler.Instance.SaveTemplateData();
            ApiDataHandler.Instance.ReplaceExerciseTemplate(templeteModel,index);
            PopupController.Instance.ClosePopup("SaveWorkoutTempletePopup");
            StateManager.Instance.OpenStaticScreen("dashboard", workoutScreen, "dashboardScreen", null);
            StateManager.Instance.OpenFooter(null, null, false);
            print("edit");
        }
        else
        {
            callback?.Invoke(obj);
            int createdWorkoutCount = ApiDataHandler.Instance.GetCreatedWorkoutTempleteCount();
            createdWorkoutCount++;
            ApiDataHandler.Instance.SetCreatedWorkoutTempleteCount(createdWorkoutCount);
            ApiDataHandler.Instance.getTemplateData().exerciseTemplete.Add(templeteModel);
            ApiDataHandler.Instance.AddExerciseTemplate(templeteModel,index);
            PopupController.Instance.ClosePopup("SaveWorkoutTempletePopup");
            StateManager.Instance.OpenStaticScreen("dashboard", workoutScreen, "dashboardScreen", null);
            StateManager.Instance.OpenFooter(null, null, false);
            print("new");
        }
    }
    public void NoButton()
    {
        PopupController.Instance.ClosePopup("SaveWorkoutTempletePopup");
    }
}