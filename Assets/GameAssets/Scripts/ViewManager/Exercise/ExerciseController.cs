using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;
using System.Linq;
using UnityEditor;

public class ExerciseController : MonoBehaviour, PageController
{
    public Transform content;
    public TextMeshProUGUI labelText;
    public TMP_InputField searchInputField;
    public Button addExerciseButton, backButton,bodyPartButton,addNewExercise;
    public Button alphabetic, byRank, performed;
    public Color buttonUnselectColor;
    public GameObject bodyPartPrefab;
    public Transform bodyPartsContent;

    private SearchButtonType currentButton;
    public List<string> selectedBodyParts=new List<string>();
    public List<ExerciseDataItem> selectedExercises = new List<ExerciseDataItem>();
    private List<GameObject> alphabetLabels = new List<GameObject>();
    private List<GameObject> exerciseItems = new List<GameObject>();
    private Action<List<ExerciseDataItem>> callback;
    private bool isWorkoutLog;
    private bool isHistory;
    private ExerciseAddOnPage exerciseAddOnPage;

    public HistoryModel testHistory = new HistoryModel();
    public void onInit(Dictionary<string, object> data, Action<object> callback)
    {
        this.callback = callback;
        exerciseAddOnPage = (ExerciseAddOnPage)data["ExerciseAddOnPage"];
        isWorkoutLog = (bool)data["isWorkoutLog"];
        if (data.ContainsKey("isHistory"))
        {
            isHistory = (bool)data["isHistory"];
        }
        switch (exerciseAddOnPage)
        {
            case ExerciseAddOnPage.WorkoutLogPage:
                LoadExercises();
                addExerciseButton.onClick.AddListener(() => AddExerciseToWorkoutLog());
                break;
            case ExerciseAddOnPage.CreateWorkoutPage:
                LoadExercises();
                addExerciseButton.onClick.AddListener(() => AddExerciseToCreateWorkout());
                break;
            case ExerciseAddOnPage.PersonalBestPage:
                LoadWeightAndRepsExercises();
                addExerciseButton.onClick.AddListener(() => AddExerciseToPersonalBest());
                break;
            case ExerciseAddOnPage.HistoryPage:
                LoadExercises();
                //addNewExercise.gameObject.SetActive(false);
                //bodyPartButton.GetComponent<RectTransform>().anchoredPosition = new Vector2(-40, bodyPartButton.GetComponent<RectTransform>().anchoredPosition.y);
                //searchInputField.GetComponent<RectTransform>().offsetMax = new Vector2(-80, searchInputField.GetComponent<RectTransform>().offsetMax.y);
                break;
        }
        addNewExercise.onClick.AddListener(AddNewExercise);
        addNewExercise.onClick.AddListener(AudioController.Instance.OnButtonClick);
        addExerciseButton.onClick.AddListener(AudioController.Instance.OnButtonClick);
        bodyPartButton.onClick.AddListener(AudioController.Instance.OnButtonClick);
        searchInputField.onValueChanged.AddListener(OnSearchChanged);
        StateManager.Instance.CheckTutorial("tutorial", "Choose a single exercise or multiple from our extensive list or create a custom exercise from <b>+</b> sign button!", 3);
    }

    private void OnEnable()
{
    // Clear any existing list items
    foreach (Transform child in content)
    {
        Destroy(child.gameObject);
    }

    // Optionally reset the scroll position
    if (content.parent.TryGetComponent<ScrollRect>(out ScrollRect scrollRect))
    {
        scrollRect.verticalNormalizedPosition = 1f; // Reset to top
    }

    LoadExercises("");
}


    void Start()
    {
        backButton.onClick.AddListener(AudioController.Instance.OnButtonClick);

    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            OnClose();
        }
    }
    public void SortBodyParts()
    {
        Dictionary<string, object> mData = new Dictionary<string, object>
            {
                { "controller", this.GetComponent<ExerciseController>() }
            };
        StateManager.Instance.OpenStaticScreen("exercise", gameObject, "bodyPartsScreen", mData, true, null);
    }
    void AddAlphabeticLabels()
    {
        for (char letter = 'A'; letter <= 'Z'; letter++)
        {
            GameObject labelPrefab = Resources.Load<GameObject>("Prefabs/exercise/exerciseLabel");
            GameObject textLabelObject = Instantiate(labelPrefab, content);
            TextMeshProUGUI textMeshPro = textLabelObject.GetComponentInChildren<TextMeshProUGUI>();
            textLabelObject.name = $"Label_{letter}";
            textMeshPro.text = letter.ToString();
            alphabetLabels.Add(textLabelObject.gameObject);
            Image line = textLabelObject.GetComponentInChildren<Image>();
            switch (ApiDataHandler.Instance.gameTheme)
            {
                case Theme.Light:
                    textMeshPro.font = userSessionManager.Instance.lightPrimaryFontBold;
                    textMeshPro.color = userSessionManager.Instance.lightButtonTextColor;
                    line.color = userSessionManager.Instance.lightPlaceholder;
                    break;
                case Theme.Dark:
                    textMeshPro.font = userSessionManager.Instance.darkPrimaryFont;
                    textMeshPro.color = Color.white;
                    line.color = userSessionManager.Instance.darkLineColor;
                    break;
            }
        }
    }
    public static List<string> GetUniqueExercises(HistoryModel historyData)
    {
        // Create a HashSet to store unique exercise names
        HashSet<string> uniqueExercises = new HashSet<string>();

        // Iterate over each HistoryTempleteModel in the historyData
        foreach (var template in historyData.exerciseTempleteModel)
        {
            // Iterate over each HistoryExerciseTypeModel in the current HistoryTempleteModel
            foreach (var exerciseType in template.exerciseTypeModel)
            {
                // Add each exercise name to the HashSet
                uniqueExercises.Add(exerciseType.exerciseName);
            }
        }

        // Convert HashSet to List and return it
        return uniqueExercises.ToList();
    }

    
    void PerformedExercises(string filter)
    {
        //addExerciseButton.gameObject.SetActive(false);
        //selectedExercises.Clear();
        //currentButton = SearchButtonType.Performed;
        //SetSelectedButton();

        //if (alphabetLabels != null)
        //{
        //    foreach (GameObject label in alphabetLabels)
        //    {
        //        if (label != null)
        //        {
        //            Destroy(label);
        //        }
        //    }
        //    alphabetLabels.Clear();
        //}

        //foreach (GameObject item in exerciseItems)
        //{
        //    Destroy(item);
        //}
        //exerciseItems.Clear();

        //ExerciseData exerciseData = ApiDataHandler.Instance.getExerciseData();
        //HistoryModel historyData = ApiDataHandler.Instance.getHistoryData();
        //List<string> filterExercises = GetUniqueExercises(historyData);
        //foreach(string name in filterExercises)
        //{
        //    print(name);
        //}
        //string lowerFilter = filter.ToLower(); // Convert filter to lowercase for case-insensitive comparison

        //foreach (ExerciseDataItem exercise in exerciseData.exercises)
        //{
        //    // Check if the exercise name or category should be filtered out
        //    if (!string.IsNullOrEmpty(lowerFilter) &&
        //        !(exercise.exerciseName.ToLower().Contains(lowerFilter) ||
        //          exercise.category.ToLower().Contains(lowerFilter)))
        //    {
        //        continue;
        //    }
        //    // Check if the exercise name is in the list of filterExercises
        //    if (filterExercises != null && !filterExercises.Contains(exercise.exerciseName))
        //    {
        //        continue;
        //    }

        //    GameObject exercisePrefab = Resources.Load<GameObject>("Prefabs/exercise/exerciseScreenDataModel");
        //    GameObject newExerciseObject = Instantiate(exercisePrefab, content);

        //    ExerciseItem newExerciseItem = newExerciseObject.GetComponent<ExerciseItem>();

        //    Dictionary<string, object> initData = new Dictionary<string, object>
        //    {
        //        { "data", exercise },
        //    };

        //    newExerciseItem.onInit(initData);

        //    Button button = newExerciseObject.GetComponent<Button>();
        //    if (button != null)
        //    {
        //        button.onClick.AddListener(() =>
        //        {
        //            //callback?.Invoke(exercise);
        //            SelectAndDeselectExercise(newExerciseObject, exercise);
        //            //OnClose();
        //        });
        //    }

        //    exerciseItems.Add(newExerciseObject);
        //}
    }
    void SelectAndDeselectExercise(GameObject obj, ExerciseDataItem exercise)
    {
        AudioController.Instance.OnButtonClick();
        if (selectedExercises.Contains(exercise))
        {
            selectedExercises.Remove(exercise);
            Color col = obj.GetComponent<Image>().color;
            col.a = 0;
            obj.GetComponent<Image>().color = col;
            //obj.GetComponent<ExerciseItem>().selected.SetActive(false);
            if (selectedExercises.Count <= 0)
            {
                addExerciseButton.gameObject.SetActive(false);
            }
        }
        else
        {
            selectedExercises.Add(exercise);
            Color col = new Color32();
            switch (ApiDataHandler.Instance.gameTheme)
            {
                case Theme.Light: col = new Color32(226,136,0,255); break;
                case Theme.Dark: col = new Color32(132, 0, 0, 255); break;
            }
            col.a = 1;
            obj.GetComponent<Image>().color = col;
            //obj.GetComponent<ExerciseItem>().selected.SetActive(true);
            if (selectedExercises.Count > 0)
            {
                addExerciseButton.gameObject.SetActive(true);
            }
        }
    }
    void SetSelectedButton()
    {
        if (currentButton == SearchButtonType.Alphabetic)
        {
            alphabetic.gameObject.GetComponent<Image>().color = Color.blue;
            byRank.gameObject.GetComponent<Image>().color = buttonUnselectColor;
            performed.gameObject.GetComponent<Image>().color = buttonUnselectColor;
        }
        else if(currentButton == SearchButtonType.ByRank)
        {
            alphabetic.gameObject.GetComponent<Image>().color = buttonUnselectColor;
            byRank.gameObject.GetComponent<Image>().color = Color.blue;
            performed.gameObject.GetComponent<Image>().color = buttonUnselectColor;
        }
        else if (currentButton == SearchButtonType.Performed)
        {
            alphabetic.gameObject.GetComponent<Image>().color = buttonUnselectColor;
            byRank.gameObject.GetComponent<Image>().color = buttonUnselectColor;
            performed.gameObject.GetComponent<Image>().color = Color.blue;
        }
    }
    void LoadExercises(string filter = "")
    {
        addExerciseButton.gameObject.SetActive(false);
        selectedExercises.Clear();
        currentButton = SearchButtonType.Alphabetic;
        SetSelectedButton();
        foreach (GameObject item in exerciseItems)
        {
            Destroy(item);
        }
        exerciseItems.Clear();
        if (alphabetLabels != null)
        {
            foreach (GameObject label in alphabetLabels)
            {
                if (label != null)
                {
                    Destroy(label);
                }
            }
            alphabetLabels.Clear();
        }

        AddAlphabeticLabels();

        ExerciseData exerciseData = ApiDataHandler.Instance.getExerciseData();

        List<GameObject> relevantLabels = new List<GameObject>();

        bool showAll = string.IsNullOrEmpty(filter);

        foreach (ExerciseDataItem exercise in exerciseData.exercises)
        {
            try{
                string lowerFilter = filter.Replace(" ", "").ToLower();

                if (!showAll &&
                    !(exercise.exerciseName.Replace(" ", "").ToLower().Contains(lowerFilter) ||
                    exercise.category.ToLower().Contains(lowerFilter)))
                {
                    continue;
                }

                char firstLetter = char.ToUpper(exercise.exerciseName[0]);
                GameObject targetLabel = alphabetLabels.Find(label => label.name == $"Label_{firstLetter}");

                if (targetLabel != null)
                {
                    if (!relevantLabels.Contains(targetLabel))
                    {
                        relevantLabels.Add(targetLabel);
                    }

                    GameObject exercisePrefab = Resources.Load<GameObject>("Prefabs/exercise/exerciseScreenDataModel");
                    GameObject newExerciseObject = Instantiate(exercisePrefab, content);

                    int labelIndex = targetLabel.transform.GetSiblingIndex();
                    newExerciseObject.transform.SetSiblingIndex(labelIndex + 1);

                    ExerciseItem newExerciseItem = newExerciseObject.GetComponent<ExerciseItem>();

                    Dictionary<string, object> initData = new Dictionary<string, object>
                    {
                        { "data", exercise },
                    };

                    newExerciseItem.onInit(initData);

                    Button button = newExerciseObject.GetComponent<Button>();
                    if (button != null)
                    {
                        if (exerciseAddOnPage == ExerciseAddOnPage.HistoryPage)
                        {
                            button.onClick.AddListener(() => {
                                ShowExerciseHistory(exercise);
                                AudioController.Instance.OnButtonClick();
                            });
                        }
                        else
                        {
                            button.onClick.AddListener(() =>{
                                SelectAndDeselectExercise(newExerciseObject, exercise);
                                AudioController.Instance.OnButtonClick();
                            });
                        }
                    }

                    exerciseItems.Add(newExerciseObject);
                }
            }catch(Exception es){
                continue;
            }
        }
        foreach (GameObject label in alphabetLabels)
        {
            if (showAll)
            {
                label.SetActive(true);
            }
            else if (relevantLabels.Contains(label))
            {
                label.SetActive(true);
            }
            else
            {
                label.SetActive(false);
            }
        }
    }
    void LoadWeightAndRepsExercises(string filter = "")
    {
        addExerciseButton.gameObject.SetActive(false);
        selectedExercises.Clear();
        currentButton = SearchButtonType.Alphabetic;
        SetSelectedButton();

        // Clear the current exercise items
        foreach (GameObject item in exerciseItems)
        {
            Destroy(item);
        }
        exerciseItems.Clear();

        // Clear alphabet labels if they exist
        if (alphabetLabels != null)
        {
            foreach (GameObject label in alphabetLabels)
            {
                if (label != null)
                {
                    Destroy(label);
                }
            }
            alphabetLabels.Clear();
        }

        AddAlphabeticLabels();

        ExerciseData exerciseData = ApiDataHandler.Instance.getExerciseData();
        List<GameObject> relevantLabels = new List<GameObject>();
        bool showAll = string.IsNullOrEmpty(filter);

        foreach (ExerciseDataItem exercise in exerciseData.exercises)
        {
            // Check for WeightAndReps exercise type
            if (exercise.exerciseType != ExerciseType.WeightAndReps)
            {
                continue;
            }

            string lowerFilter = filter.ToLower();

            // Check if the exercise name or category contains the filter
            if (!showAll &&
                !(exercise.exerciseName.ToLower().Contains(lowerFilter) ||
                  exercise.category.ToLower().Contains(lowerFilter)))
            {
                continue;
            }

            char firstLetter = char.ToUpper(exercise.exerciseName[0]);
            GameObject targetLabel = alphabetLabels.Find(label => label.name == $"Label_{firstLetter}");

            if (targetLabel != null)
            {
                if (!relevantLabels.Contains(targetLabel))
                {
                    relevantLabels.Add(targetLabel);
                }

                GameObject exercisePrefab = Resources.Load<GameObject>("Prefabs/exercise/exerciseScreenDataModel");
                GameObject newExerciseObject = Instantiate(exercisePrefab, content);

                int labelIndex = targetLabel.transform.GetSiblingIndex();
                newExerciseObject.transform.SetSiblingIndex(labelIndex + 1);

                ExerciseItem newExerciseItem = newExerciseObject.GetComponent<ExerciseItem>();

                Dictionary<string, object> initData = new Dictionary<string, object>
            {
                { "data", exercise },
            };

                newExerciseItem.onInit(initData);

                Button button = newExerciseObject.GetComponent<Button>();
                if (button != null)
                {
                    button.onClick.AddListener(() =>
                    {
                        SelectAndDeselectExercise(newExerciseObject, exercise);
                    });
                }

                exerciseItems.Add(newExerciseObject);
            }
        }

        // Handle the visibility of alphabet labels
        foreach (GameObject label in alphabetLabels)
        {
            if (showAll)
            {
                label.SetActive(true);
            }
            else if (relevantLabels.Contains(label))
            {
                label.SetActive(true);
            }
            else
            {
                label.SetActive(false);
            }
        }
    }

    public void LoadExercisesByBodyParts()
    { 
        addExerciseButton.gameObject.SetActive(false);
        selectedExercises.Clear();
        currentButton = SearchButtonType.Alphabetic;
        SetSelectedButton();
        foreach (GameObject item in exerciseItems)
        {
            Destroy(item);
        }
        exerciseItems.Clear();
        if (alphabetLabels != null)
        {
            foreach (GameObject label in alphabetLabels)
            {
                if (label != null)
                {
                    Destroy(label);
                }
            }
            alphabetLabels.Clear();
        }

        AddAlphabeticLabels();

        ExerciseData exerciseData = ApiDataHandler.Instance.getExerciseData();

        List<GameObject> relevantLabels = new List<GameObject>();

        foreach (ExerciseDataItem exercise in exerciseData.exercises)
        {
            // Check if the exercise category is in the provided list
            if (!selectedBodyParts.Contains(exercise.category))
            {
                continue;
            }

            char firstLetter = char.ToUpper(exercise.exerciseName[0]);
            GameObject targetLabel = alphabetLabels.Find(label => label.name == $"Label_{firstLetter}");

            if (targetLabel != null)
            {
                if (!relevantLabels.Contains(targetLabel))
                {
                    relevantLabels.Add(targetLabel);
                }

                GameObject exercisePrefab = Resources.Load<GameObject>("Prefabs/exercise/exerciseScreenDataModel");
                GameObject newExerciseObject = Instantiate(exercisePrefab, content);

                int labelIndex = targetLabel.transform.GetSiblingIndex();
                newExerciseObject.transform.SetSiblingIndex(labelIndex + 1);

                ExerciseItem newExerciseItem = newExerciseObject.GetComponent<ExerciseItem>();

                Dictionary<string, object> initData = new Dictionary<string, object>
                {
                    { "data", exercise },
                };

                newExerciseItem.onInit(initData);

                Button button = newExerciseObject.GetComponent<Button>();
                if (button != null)
                {
                    if (exerciseAddOnPage == ExerciseAddOnPage.HistoryPage)
                    {
                        button.onClick.AddListener(() => { 
                            ShowExerciseHistory(exercise);
                            AudioController.Instance.OnButtonClick();
                        });
                    }
                    else
                    {
                        button.onClick.AddListener(() =>{
                            SelectAndDeselectExercise(newExerciseObject, exercise);
                            AudioController.Instance.OnButtonClick();
                        });
                    }
                }

                exerciseItems.Add(newExerciseObject);
            }
        }
        foreach (GameObject label in alphabetLabels)
        {
            if (relevantLabels.Contains(label))
            {
                label.SetActive(true);
            }
            else
            {
                label.SetActive(false);
            }
        }
    }
    void ShowExerciseHistory(ExerciseDataItem exercise)
    {
        Dictionary<string, object> initData = new Dictionary<string, object>
        {
                { "data", exercise },
        };
        StateManager.Instance.OpenStaticScreen("history", gameObject, "exerciseHistoryScreen", initData, keepState: true);
        StateManager.Instance.CloseFooter();
    }
    public void CreateBodyPartChecks()
    {
        foreach (Transform child in bodyPartsContent)
        {
            Destroy(child.gameObject);
        }
        foreach (string text in selectedBodyParts)
        {
            // Instantiate text prefab
            GameObject newTextObj = Instantiate(bodyPartPrefab, bodyPartsContent);
            newTextObj.transform.GetChild(1).gameObject.SetActive(true);
            newTextObj.GetComponent<Image>().color = new Color32(237, 231, 211, 255);
            TextMeshProUGUI textComponent = newTextObj.GetComponentInChildren<TextMeshProUGUI>();
            textComponent.text = text;
            textComponent.color = new Color32(51, 23, 23, 255);
            newTextObj.GetComponent<Button>().onClick.AddListener(() => UnSelectBodypart(newTextObj,text));
            //newTextObj.GetComponent<Image>().color = itemColor;
            LayoutRebuilder.ForceRebuildLayoutImmediate(textComponent.rectTransform);
            float textWidth = textComponent.preferredWidth + 15;
            RectTransform textRect = newTextObj.GetComponent<RectTransform>();
            RectTransform objRect = newTextObj.GetComponent<RectTransform>();
            textRect.sizeDelta = new Vector2(textWidth, textRect.sizeDelta.y);
            objRect.sizeDelta = new Vector2(textWidth, objRect.sizeDelta.y);
        }
    }
    public void UnSelectBodypart(GameObject obj,string text)
    {
        Destroy(obj);
        selectedBodyParts.Remove(text);
        if(selectedBodyParts.Count > 0)
        {
            LoadExercisesByBodyParts();
        }
        else
        {
            LoadExercises();
        }
    }
    
    void OnSearchChanged(string searchQuery)
    {
        switch (exerciseAddOnPage)
        {
            case ExerciseAddOnPage.CreateWorkoutPage:
                LoadExercises(searchQuery);
                break;
            case ExerciseAddOnPage.WorkoutLogPage:
                LoadExercises(searchQuery);
                break;
            case ExerciseAddOnPage.PersonalBestPage:
                LoadWeightAndRepsExercises(searchQuery);
                break;
            case ExerciseAddOnPage.HistoryPage:
                LoadExercises(searchQuery);
                break;
                //case SearchButtonType.ByRank:
                //    ByRankExercises(searchQuery);
                //    break;
                //case SearchButtonType.Performed:
                //    PerformedExercises(searchQuery);
                //    break;
        }

    }

    public void OnClose()
    {
        StateManager.Instance.HandleBackAction(gameObject);
        if (isHistory)
        {
            StateManager.Instance.OpenFooter(null, null, false);
        }
    }
    public void AddNewExercise()
    {
        StateManager.Instance.OpenStaticScreen("exercise", gameObject, "addNewExerciseScreen", null,true,OnAddExercise);
    }
    void OnAddExercise(object data)
    {
    }

    public void AddExerciseToWorkoutLog()
    {
        callback?.Invoke(selectedExercises);
        OnClose();
    }
    public void AddExerciseToCreateWorkout()
    {
        callback?.Invoke(selectedExercises);
        OnClose();
    }
    public void AddExerciseToPersonalBest()
    {
        callback?.Invoke(selectedExercises);
        OnClose();
    }

    void ClearSearchBar()
    {
        searchInputField.text = "";
    }
    public void SaveTestHistory()
    {
        // test code for history
        if (!PreferenceManager.Instance.HasKey("historyData"))
        {
            string json = JsonUtility.ToJson(testHistory);
            PreferenceManager.Instance.SetString("historyData", json);
            PreferenceManager.Instance.Save();
            print("save");
            print(json);
        }
        ApiDataHandler.Instance.LoadHistory();
    }
}
