using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static System.Net.Mime.MediaTypeNames;

public class CompleteWorkoutController : MonoBehaviour, IPrefabInitializer
{
    public TextMeshProUGUI workoutNameText;
    public TextMeshProUGUI dateText;
    public TextMeshProUGUI totalTimeText;
    public TextMeshProUGUI totalWeightText;
    public Transform content;
    public ParticleSystem particleComplete;
    public GameObject workoutScreen;
    public Button backButton;
    public void InitPrefab(Action<List<object>> onFinish, List<object> data)
    {
        HistoryTempleteModel historyWorkout = (HistoryTempleteModel)data[0];
        workoutScreen = (GameObject)data[1];
        workoutNameText.text=userSessionManager.Instance.FormatStringAbc(historyWorkout.templeteName);
        string savedDate = historyWorkout.dateTime;
        DateTime parsedDate = DateTime.ParseExact(savedDate, "MMM dd, yyyy hh:mm:ss tt", System.Globalization.CultureInfo.InvariantCulture);
        string formattedDate = parsedDate.ToString("dddd, dd MMMM yyyy");
        dateText.text=formattedDate;
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
                totalWeightText.text = historyWorkout.totalWeight.ToString()+" kg";
                break;
            case WeightUnit.lbs:
                totalWeightText.text = /*Mathf.RoundToInt*/(userSessionManager.Instance.ConvertKgToLbs(historyWorkout.totalWeight)).ToString("F2") +" lbs";
                break;
        }
        
        foreach(HistoryExerciseTypeModel exercise in historyWorkout.exerciseTypeModel)
        {
            GameObject exercisePrefab = Resources.Load<GameObject>("Prefabs/complete/completeScreenDataModel");
            GameObject newExerciseObject = Instantiate(exercisePrefab, content);
            newExerciseObject.transform.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>().text = userSessionManager.Instance.FormatStringAbc(exercise.exerciseName);
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
        StartCoroutine(ParticleAndCoinWait());
        //Invoke("onParticleSystem", 0.5f);
        //Destroy(workoutScreen);
    }
    
    private void Start()
    {
        backButton.onClick.AddListener(AudioController.Instance.OnButtonClick);
    }
    void onParticleSystem()
    {
        AudioController.Instance.OnComplete();
        particleComplete.Play();
        userSessionManager.Instance.CheckAchievementStatus();
    }
    IEnumerator ParticleAndCoinWait()
    {
        yield return new WaitForSeconds(0.5f);
        AudioController.Instance.OnComplete();
        particleComplete.Play();
        yield return new WaitForSeconds(0.2f);
        StreakAndCharacterManager.Instance.AddVisit(DateTime.Now.ToString("yyyy-MM-dd"));
        userSessionManager.Instance.AddCoins(5);
        yield return new WaitForSeconds(0.2f);
        userSessionManager.Instance.CheckAchievementStatus();
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
        //StateManager.Instance.OpenStaticScreen("history", workoutScreen, "historyScreen", null, isfooter: true);
        StateManager.Instance.OpenFooter(null, null, false);
        StateManager.Instance.SetSpecificFooterButton(FooterButtons.History);
        PopupController.Instance.ClosePopup("completeWorkoutPopup");
    }
    void ShowOnlyReps(HistoryExerciseTypeModel exercise,GameObject parent, GameObject prefab)
    {
        ChangeTextColorAndFount(prefab.GetComponent<TextMeshProUGUI>(), userSessionManager.Instance.lightButtonTextColor, userSessionManager.Instance.lightPrimaryFontBold);
        foreach (HistoryExerciseModel data in exercise.exerciseModel)
        {
            GameObject textObj = Instantiate(prefab, parent.transform);
            TextMeshProUGUI text=textObj.GetComponent<TextMeshProUGUI>();
            ChangeTextColorAndFount(text, userSessionManager.Instance.lightButtonTextColor, userSessionManager.Instance.lightSecondaryFont);
            text.enableAutoSizing = false;
            text.fontSize = 14;
            text.text = data.reps.ToString() + " @ " + data.rir.ToString();
        }
    }
    void ShowOnlyTime(HistoryExerciseTypeModel exercise, GameObject parent, GameObject prefab)
    {
        ChangeTextColorAndFount(prefab.GetComponent<TextMeshProUGUI>(), userSessionManager.Instance.lightButtonTextColor, userSessionManager.Instance.lightPrimaryFontBold);
        foreach (HistoryExerciseModel data in exercise.exerciseModel)
        {
            GameObject textObj = Instantiate(prefab, parent.transform);
            TextMeshProUGUI text = textObj.GetComponent<TextMeshProUGUI>();
            ChangeTextColorAndFount(text, userSessionManager.Instance.lightButtonTextColor, userSessionManager.Instance.lightSecondaryFont);
            text.enableAutoSizing = false;
            text.fontSize = 14;
            if (data.time > 60)
            {
                text.text = ((int)data.time / 60).ToString() + " m"+" @ "+data.rpe.ToString();
            }
            else
            {
                text.text = data.time.ToString() + " s" + data.rpe.ToString();
            }
        }
    }
    void ShowWeightAndReps(HistoryExerciseTypeModel exercise, GameObject parent, GameObject prefab)
    {
        ChangeTextColorAndFount(prefab.GetComponent<TextMeshProUGUI>(), userSessionManager.Instance.lightButtonTextColor, userSessionManager.Instance.lightPrimaryFontBold);
        foreach (HistoryExerciseModel data in exercise.exerciseModel)
        {
            GameObject textObj = Instantiate(prefab, parent.transform);
            TextMeshProUGUI text = textObj.GetComponent<TextMeshProUGUI>();
            ChangeTextColorAndFount(text, userSessionManager.Instance.lightButtonTextColor, userSessionManager.Instance.lightSecondaryFont);
            text.enableAutoSizing = false;
            text.fontSize = 14;
            //text.text = data.weight.ToString() + " kg x " + data.reps.ToString();
            text.fontSize = 14;
            switch ((WeightUnit)ApiDataHandler.Instance.GetWeightUnit())
            {
                case WeightUnit.kg:
                    text.text = data.weight.ToString() + " kg x " + data.reps.ToString("F1")+" @ "+data.rir.ToString();
                    break;
                case WeightUnit.lbs:
                    text.text = /*Mathf.RoundToInt*/(userSessionManager.Instance.ConvertKgToLbs(data.weight)).ToString("F2") + " lbs x " + data.reps.ToString("F1") + " @ " + data.rir.ToString();
                    break;
            }
        }
    }
    void ShowTimeAndMile(HistoryExerciseTypeModel exercise, GameObject parent, GameObject prefab)
    {
        ChangeTextColorAndFount(prefab.GetComponent<TextMeshProUGUI>(), userSessionManager.Instance.lightButtonTextColor, userSessionManager.Instance.lightPrimaryFontBold);
        foreach (HistoryExerciseModel data in exercise.exerciseModel)
        {
            GameObject textObj = Instantiate(prefab, parent.transform);
            TextMeshProUGUI text = textObj.GetComponent<TextMeshProUGUI>();
            ChangeTextColorAndFount(text, userSessionManager.Instance.lightButtonTextColor, userSessionManager.Instance.lightSecondaryFont);
            text.enableAutoSizing = false;
            text.fontSize = 14;
            string time = "";
            if (data.time > 60)
            {
                time = ((int)data.time / 60).ToString() + " m";
            }
            else
            {
                time = data.time.ToString() + " s";
            }
            text.text = data.mile.ToString() + " mile x " + time + data.rpe.ToString();
            text.fontSize = 14;
        }
    }
    public void ChangeTextColorAndFount(TextMeshProUGUI text,Color col,TMP_FontAsset font)
    {
        text.color = col;
        text.font = font;
    }

}
