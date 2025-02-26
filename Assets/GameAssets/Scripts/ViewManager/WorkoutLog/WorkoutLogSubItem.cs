using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Windows;

public class WorkoutLogSubItem : MonoBehaviour, ItemController
{
    public TMP_Text sets;
    public TMP_Text previous;
    public TMP_InputField mile;
    public TMP_InputField timerText;
    public TMP_InputField weight;
    public TMP_InputField reps;
    public TMP_Dropdown rir;
    public TMP_Dropdown rpe;
    public Toggle isComplete;
    public ExerciseType exerciseType;
    public Image previousImage,dropDownArrow,setBg;
    public ExerciseModel exerciseModel;

    //public bool timerReached; 
    private int currentTimeInSeconds; 
    private int userEnteredTimeInSeconds; 
    private float timer;
    private Coroutine timerCoroutine;
    private Action<object> callBack;
    bool shake = true;
    bool isWorkoutLog;

    public void onInit(Dictionary<string, object> data, Action<object> callback = null)
    {
        this.callBack = callback;
        exerciseModel = (ExerciseModel)data["data"];
        exerciseType = (ExerciseType)data["exerciseType"];
        HistoryExerciseModel exerciseHistory = (HistoryExerciseModel)data["exerciseHistory"];
        isWorkoutLog = (bool)data["isWorkoutLog"];
        //InputFieldManager inputFieldManager = (InputFieldManager)data["inputManager"];
        //inputFieldManager.inputFields.Add(mile);
        //inputFieldManager.inputFields.Add(timerText);
        //inputFieldManager.inputFields.Add(weight);
        //inputFieldManager.inputFields.Add(reps);
        bool il = data.TryGetValue("interruptedLoading", out var value) ? (bool)value : false;
        if (il)
        {
            previous.text = exerciseModel.previous;
            weight.text = exerciseModel.weight.ToString();
            reps.text = exerciseModel.reps.ToString();
            timerText.text = exerciseModel.time.ToString();
            mile.text = exerciseModel.mile.ToString();
            rir.value = exerciseModel.rir;
            rpe.value = exerciseModel.rpe;
            isComplete.isOn = exerciseModel.toggle;
            timerText.text = FormatTime(exerciseModel.time.ToString());
            if (exerciseModel.toggle)
            {
                OnToggleValueChange(true, false);
            }
        }
        else
        {
            ResetModel(exerciseModel);
        }
        sets.text = exerciseModel.setID.ToString();

        if (!isWorkoutLog)
        {
            OffAllInteractables();
            isComplete.transform.GetChild(1).gameObject.SetActive(true);
        }
        switch (exerciseType)
        {
            case ExerciseType.RepsOnly:
                reps.transform.parent.gameObject.SetActive(true);
                rir.transform.parent.gameObject.SetActive(true);
                if (exerciseHistory != null && isWorkoutLog)
                {
                    previous.text = $"{exerciseHistory.reps:F1} @ {exerciseHistory.rir}";
                }
                break;
            case ExerciseType.TimeBased:
                timerText.transform.parent.gameObject.SetActive(true);
                rpe.transform.parent.gameObject.SetActive(true);
                if (exerciseHistory != null && isWorkoutLog)
                {
                    int totalSeconds = exerciseHistory.time;
                    int hours = totalSeconds / 3600;          // Calculate hours
                    int minutes = (totalSeconds % 3600) / 60; // Calculate remaining minutes
                    int seconds = totalSeconds % 60;          // Calculate remaining seconds

                    if (hours > 0)
                    {
                        // Show time in hours, minutes, and seconds
                        previous.text = $"{hours:D2}:{minutes:D2}:{seconds:D2} @ {exerciseHistory.rir}";
                    }
                    else
                    {
                        // Show time in minutes and seconds
                        previous.text = $"{minutes:D2}:{seconds:D2} @ {exerciseHistory.rir}";
                    }
                }
                break;
            case ExerciseType.TimeAndMiles:
                timerText.transform.parent.gameObject.SetActive(true);
                mile.transform.parent.gameObject.SetActive(true);
                rpe.transform.parent.gameObject.SetActive(true);
                if (exerciseHistory != null && isWorkoutLog)
                {
                    int totalSeconds = exerciseHistory.time;
                    int hours = totalSeconds / 3600;          // Calculate hours
                    int minutes = (totalSeconds % 3600) / 60; // Calculate remaining minutes
                    int seconds = totalSeconds % 60;          // Calculate remaining seconds

                    if (hours > 0)
                    {
                        // Show time in hours, minutes, and seconds
                        previous.text = $"{hours:D2}:{minutes:D2}:{seconds:D2} x {exerciseHistory.mile} @ {exerciseHistory.rir}";
                    }
                    else
                    {
                        // Show time in minutes and seconds
                        previous.text = $"{minutes:D2}:{seconds:D2} x {exerciseHistory.mile} @ {exerciseHistory.rir}";
                    }
                }
                //print("Need to implement Time and Miles");
                break;
            case ExerciseType.WeightAndReps:
                timerText.transform.parent.gameObject.SetActive(false);
                weight.transform.parent.gameObject.SetActive(true);
                rir.transform.parent.gameObject.SetActive(true);
                reps.transform.parent.gameObject.SetActive(true);
                if (exerciseHistory != null && isWorkoutLog)
                {
                    switch ((WeightUnit)ApiDataHandler.Instance.GetWeightUnit())
                    {
                        case WeightUnit.kg:
                            previous.text = exerciseHistory.weight.ToString() + "kg " + "x " + exerciseHistory.reps.ToString("F1") + " @ " + exerciseHistory.rir;
                            break;
                        case WeightUnit.lbs:
                            previous.text = Mathf.Round(userSessionManager.Instance.ConvertKgToLbs(exerciseHistory.weight)).ToString() + "lbs " + "x " + exerciseHistory.reps.ToString("F1") + " @ " + exerciseHistory.rir;
                            break;
                    }
                    
                }
                break;
        }

        InitializeRirDropdown();
        InitializeRpeDropdown();

        weight.onEndEdit.AddListener(OnWeightChanged);
        rir.onValueChanged.AddListener(OnRIRChanged);
        rpe.onValueChanged.AddListener(OnRpeChanged);
        reps.onEndEdit.AddListener(OnREPSChanged);
        timerText.onEndEdit.AddListener(OnTimerInput);
        mile.onValueChanged.AddListener(OnMileChanges);
        if(isComplete!=null)
            isComplete.onValueChanged.AddListener(OnToggleValueChange);
        UpdateToggleInteractableState();
        OnRIRChanged(0);
        OnRpeChanged(0);
    }
    void OffAllInteractables()
    {
        weight.interactable = false;
        rir.interactable= false;
        reps.interactable= false;
        timerText.interactable= false;
        mile.interactable= false;
        isComplete.interactable= false;
    }
    void ResetModel(ExerciseModel model)
    {
        model.reps = 0;
        model.weight = 0;
        model.time = 0;
        model.mile= 0;
        model.toggle = false;
    }

    private void OnEnable()
    {
        reps.transform.parent.GetChild(1).GetComponent<Button>().onClick.AddListener(()=>userSessionManager.Instance.ActiveInput(reps));
        mile.transform.parent.GetChild(1).GetComponent<Button>().onClick.AddListener(()=> userSessionManager.Instance.ActiveInput(mile));
        weight.transform.parent.GetChild(1).GetComponent<Button>().onClick.AddListener(()=> userSessionManager.Instance.ActiveInput(weight));
        timerText.transform.parent.GetChild(1).GetComponent<Button>().onClick.AddListener(()=> userSessionManager.Instance.ActiveInput(timerText));

    }
    
    void ChangeInputFieldFount_Color(TMP_InputField inputField,TMP_FontAsset fontAsset,Color color)
    {
        TextMeshProUGUI placeholder=inputField.placeholder as TextMeshProUGUI;
        TextMeshProUGUI text = inputField.textComponent as TextMeshProUGUI;
        placeholder.font = fontAsset;
        placeholder.color = color;
        text.font = fontAsset;
        text.color = color;
    }
    void ChangeImageColorAndOutlineColor(GameObject obj,Color imgColor,Color outlineColor)
    {
        obj.GetComponent<Image>().color = imgColor;
        obj.GetComponent<Outline>().effectColor= outlineColor;
    }
    private void InitializeRirDropdown()
    {
        rir.ClearOptions();
        List<string> options = new List<string>();
        for (int i = 0; i <= 5; i++)
        {
            options.Add(i.ToString());
        }
        rir.AddOptions(options);
    }
    private void InitializeRpeDropdown()
    {
        rpe.ClearOptions();
        List<string> options = new List<string>();
        for (int i = 1; i <= 10; i++)
        {
            options.Add(i.ToString());
        }
        rpe.AddOptions(options);
    }
    private void OnMileChanges(string newMile)
    {
        if (float.TryParse(newMile, out float mileValue))
        {
            exerciseModel.mile = mileValue;
        }
        else
        {
            exerciseModel.weight = 0;
        }
        UpdateToggleInteractableState();
    }
    private void OnWeightChanged(string newWeight)
    {
        if (newWeight.Contains("."))
        {
            string[] parts = newWeight.Split('.');

            // Validate digits before the decimal point
            if (parts[0].Length > 4)
            {
                parts[0] = parts[0].Substring(0, 4);
            }

            // Validate digits after the decimal point
            if (parts.Length > 1 && parts[1].Length > 2)
            {
                parts[1] = parts[1].Substring(0, 2);
            }

            // Reassemble the input
            newWeight = string.Join(".", parts);
        }
        else
        {
            // No decimal point, limit total input to 4 characters
            if (newWeight.Length > 4)
            {
                newWeight = newWeight.Substring(0, 4);
                print("come");
            }
        }
        if (float.TryParse(newWeight, out float weightValue))
        {
            exerciseModel.weight = weightValue;
            weight.text = newWeight;
        }
        else
        {
            exerciseModel.weight = 0;
            weight.text = "";
        }
        UpdateToggleInteractableState();
    }

    private void OnREPSChanged(string newLbs)
    {
        if (float.TryParse(newLbs, out float lbsValue))
        {
            exerciseModel.reps = lbsValue;
        }
        else
        {
            exerciseModel.reps = 0;
        }
        UpdateToggleInteractableState();
    }

    private void OnRIRChanged(int newRepsIndex)
    {
        exerciseModel.rir = newRepsIndex;
        UpdateToggleInteractableState();
    }
    private void OnRpeChanged(int newRepsIndex)
    {
        exerciseModel.rpe = newRepsIndex;
        UpdateToggleInteractableState();
    }
    private void OnTimerInput(string input)
    {
        print(input);

        // Format the input
        string formattedInput = FormatTime(input);

        // Update the timer text
        timerText.text = formattedInput;

        // Convert formatted time to total seconds
        string[] timeParts = formattedInput.Split(':');
        int hours = timeParts.Length == 3 ? int.Parse(timeParts[0]) : 0;
        int minutes = timeParts.Length >= 2 ? int.Parse(timeParts[timeParts.Length - 2]) : 0;
        int seconds = int.Parse(timeParts[timeParts.Length - 1]);
        int enteredTimeInSeconds = (hours * 3600) + (minutes * 60) + seconds;

        // Update exercise model
        exerciseModel.time = enteredTimeInSeconds;

        // Restart or adjust the timer based on entered time
        if (enteredTimeInSeconds > userEnteredTimeInSeconds)
        {
            // Restart timer if greater time is entered
            currentTimeInSeconds = enteredTimeInSeconds;
            userEnteredTimeInSeconds = enteredTimeInSeconds;

            // Stop previous coroutine and start a new one
            if (timerCoroutine != null)
            {
                StopCoroutine(timerCoroutine);
            }
            timerCoroutine = StartCoroutine(StartTimer());
        }
        else if (enteredTimeInSeconds < userEnteredTimeInSeconds)
        {
            // Update the entered time if less time is entered
            userEnteredTimeInSeconds = enteredTimeInSeconds;
        }
        else if (enteredTimeInSeconds == 0)
        {
            // Handle special case for zero time
            if (isComplete != null)
            {
                isComplete.isOn = false;
                exerciseModel.toggle = false;
            }
        }

        UpdateToggleInteractableState();
    }
    private string FormatTime(string input)
    {
        // Remove colons and trim the input
        input = input.Replace(":", "").Trim();

        // Remove all leading zeros by converting to an integer and back to string
        if (!string.IsNullOrEmpty(input))
        {
            input = int.Parse(input).ToString();
        }
        else
        {
            input = "0"; // Handle empty input
        }

        // Limit the input to 6 characters (HHMMSS format)
        if (input.Length > 6) input = input.Substring(0, 6);

        // Format the input based on its length
        string formattedInput = "";
        if (input.Length <= 2)
        {
            // SS
            formattedInput = input.PadLeft(2, '0'); // Ensure at least "00"
            formattedInput = "00:" + formattedInput;
        }
        else if (input.Length <= 4)
        {
            // MM:SS
            formattedInput = input.PadLeft(4, '0'); // Ensure at least "0000"
            formattedInput = formattedInput.Insert(2, ":");
        }
        else
        {
            // HH:MM:SS
            formattedInput = input.PadLeft(6, '0'); // Ensure at least "000000"
            formattedInput = formattedInput.Insert(2, ":").Insert(5, ":");
        }

        return formattedInput;
    }
    private IEnumerator StartTimer()
    {
        float timer = 0;

        while (timer < userEnteredTimeInSeconds)
        {
            yield return new WaitForSeconds(1); // Wait for 1 second each loop
            timer++;
        }

        // When the timer reaches the user-entered time
        //if (isComplete != null)
        //{
        //    isComplete.isOn = true;
        //    exerciseModel.toggle = true;
        //    isComplete.targetGraphic.color = new Color32(255, 182, 193, 255);

        //}
    }

    public void OnToggleValueChange(bool value, bool playSound = true)
    {
        exerciseModel.toggle = value;

        if (value)
        {
            if (playSound)
            {
                AudioController.Instance.OnSetComplete();
            }

            isComplete.targetGraphic.color = new Color32(255, 182, 193, 255);
            setBg.color = new Color32(255, 127, 127, 255);
        }
        else
        {
            isComplete.targetGraphic.color = new Color32(246, 236, 220, 255);
            setBg.color = Color.white;
        }
    }

    public void OnToggleValueChange(bool value)
    {
        //if (exerciseModel.weight > 0 && exerciseModel.reps > 0)
        //{
            exerciseModel.toggle=value;
        //this.callBack?.Invoke(value);
            if (value)
            {
                AudioController.Instance.OnSetComplete();
                isComplete.targetGraphic.color = new Color32(255, 182, 193, 255);
                setBg.color= new Color32(255, 127, 127, 255);
        }
            else
            {
                isComplete.targetGraphic.color = new Color32(246, 236, 220, 255);
            setBg.color = Color.white;
        }
        //}
    }
    private void UpdateToggleInteractableState()
    {
        if (isComplete != null)
        {
            switch (exerciseType)
            {
                case ExerciseType.RepsOnly:
                    isComplete.interactable = (exerciseModel.reps > 0);
                    break;
                case ExerciseType.TimeBased:
                    isComplete.interactable = (exerciseModel.time > 0);
                    break;
                case ExerciseType.TimeAndMiles:
                    isComplete.interactable = (exerciseModel.time > 0 && exerciseModel.mile > 0);
                    break;
                case ExerciseType.WeightAndReps:
                    isComplete.interactable = (/*exerciseModel.weight > 0 &&*/ exerciseModel.reps > 0);
                    break;
            }
            //if (exerciseType == ExerciseType.WeightAndReps)
            //    isComplete.interactable = (exerciseModel.weight > 0 && exerciseModel.reps > 0);
            //else
            //    isComplete.interactable = false;
        }
    }
    public void ToggleShake()
    {
        if (!isComplete.interactable && shake&&isWorkoutLog)
        {
            //shake = false;
            AudioController.Instance.OnError();
            callBack?.Invoke(null);
            //GlobalAnimator.Instance.ApplyShakeEffect(isComplete.gameObject.GetComponent<RectTransform>(), () => shake = true);
        }
    }

}
