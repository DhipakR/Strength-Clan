using UnityEngine;
using System.Collections.Generic;
using TMPro;
using System;
using UnityEngine.UI;
using System.Linq;
using System.Globalization;
using System.IO;

public class userSessionManager : GenericSingletonClass<userSessionManager>
{
    private const float lbsToKgFactor = 0.453592f;
    private const float kgToLbsFactor = 2.20462f;
    public string mProfileUsername;
    public string mProfileID;
    public bool mSidebar = false;
    public GameObject currentScreen;
    public int weeklyGoal;
    public string joiningDate;
    public string badgeName;
    public DateTime weeklyGoalSetDate;
    public int currentCoins;
    public int userStreak;
    public int characterLevel;
    public int addedFriends;
    public int removedFriends;
    public string clotheName;
    public bool badgeChange;
    public Sprite profileSprite;
    public string profileImageUrl;
    public string gifsPath = "gifs";
    public List<AchievementTemplateDataItem> completedItemsInSingleCheck = new List<AchievementTemplateDataItem>();
    public List<string> completedItemsTitles = new List<string>();
    [Header("Theme Settings")]
    private Theme gameTheme;
    public TMP_FontAsset darkPrimaryFont, darkSecondaryFont;
    public TMP_FontAsset lightPrimaryFontBold, lightPrimaryFontMediumBold, lightPrimaryFontSemiBold, lightSecondaryFont;

    public Color darkButtonTextColor,lightButtonTextColor;
    public Color darkButtonColor,lightButtonColor;
    public Color darkPlaceholder, lightPlaceholder;
    public Color darkInputFieldColor, lightInputFieldColor;
    public Color darkLineColor;
    public Color darkSwitchTextColor;

    public Color lightXPbutton, darkXPbutton;

    public DefaultTempleteModel selectedTemplete;
    private TemplateData templateData = new TemplateData();
    private HistoryModel historyData = new HistoryModel();

    private object coinLock = new object();
    //public PersonalBest

    private void Start()
    {
        darkPrimaryFont= Resources.Load<TMP_FontAsset>("UIAssets/Shared/Font/Hoog0555/Hoog0555");
        darkSecondaryFont = Resources.Load<TMP_FontAsset>("UIAssets/Shared/Font/K2D/K2D");
        lightPrimaryFontBold = Resources.Load<TMP_FontAsset>("UIAssets/Shared/Font/Alexandria/AlexandriaBold");
        lightPrimaryFontMediumBold = Resources.Load<TMP_FontAsset>("UIAssets/Shared/Font/Alexandria/AlexandriaMediumBold");
        lightPrimaryFontSemiBold = Resources.Load<TMP_FontAsset>("UIAssets/Shared/Font/Alexandria/AlexandriaSemiBold");
        lightSecondaryFont = Resources.Load<TMP_FontAsset>("UIAssets/Shared/Font/Afacad/Afacad");
        darkButtonTextColor = new Color32(51,23,23,255);
        lightButtonTextColor = new Color32(150,0,0,255);
        darkButtonColor = new Color32(255,255,255,255);
        lightButtonColor = new Color32(218,52,52,255);
        darkPlaceholder = new Color32(127,77,77,255);
        lightPlaceholder = new Color32(92, 74, 28, 40);
        darkInputFieldColor = new Color32(81,14,14,255);
        lightInputFieldColor = new Color32(246,236,220,255);
        darkLineColor = new Color32(92, 74, 28, 40);
        darkSwitchTextColor = new Color32(171, 162, 162, 255);
        lightXPbutton = new Color32(167, 200, 33, 255);
        darkXPbutton = new Color32(241, 183, 32, 255);

    }
    public void Logout()
    {
        mProfileUsername = "";
        mProfileID = "";
        weeklyGoal = 0;
        joiningDate = "";
        badgeName = "";
        currentCoins = 0;
        userStreak = 0;
        characterLevel = 0;
        addedFriends = 0;
        removedFriends = 0;
        clotheName = "";
        profileSprite = null;
        string path = Path.Combine(Application.persistentDataPath, "lastImage.png");

        // Check if the file exists
        if (File.Exists(path))
        {
            // Delete the file
            File.Delete(path);
            Debug.Log("File deleted successfully.");
        }
    }
    public void OnInitialize(string pProfileUsername, string pProfileID)
    {
        this.mProfileID = pProfileID;
        PreferenceManager.Instance.SetString("login_username", pProfileUsername);
        mSidebar = false;

        //LoadHistory();
        //gameTheme = LoadTheme();

    }

    public void OnResetSession()
    {
        this.mProfileUsername = null;
        this.mProfileID = null;
    }
    public void CheckAchievementStatus(List<Image> trophyImages=null, TextMeshProUGUI progressText=null, TextMeshProUGUI descriptionText = null, TextMeshProUGUI coinText=null)
    {
        foreach(AchievementTemplate _data in ApiDataHandler.Instance.getAchievementData().achievements)
        {
            switch (_data.type)
            {
                case AchievementType.BodyweightMultiplier:
                    CheckBodyWeightAchievements(_data, ApiDataHandler.Instance.getPersonalBestData(), trophyImages, progressText, descriptionText, coinText);
                    break;
                case AchievementType.WorkoutCount:
                    CheckWorkoutCountAchievements(_data, ApiDataHandler.Instance.getHistoryData().exerciseTempleteModel.Count, trophyImages, progressText, descriptionText, coinText);
                    break;
                case AchievementType.ExerciseCount:
                    CheckExerciseCountAchievements(_data, GetUniqueExerciseCount(ApiDataHandler.Instance.getHistoryData()), trophyImages, progressText, descriptionText, coinText);
                    break;
                case AchievementType.Specialist:
                    CheckSpecialistAchievements(_data, ApiDataHandler.Instance.getHistoryData(), trophyImages, progressText, descriptionText, coinText);
                    break;
                case AchievementType.CardioTime:
                    CheckCardioTimeAchievements(_data, ApiDataHandler.Instance.getHistoryData(), trophyImages, progressText, descriptionText, coinText);
                    break;
                case AchievementType.Streak:
                    CheckStreakAndLevelAchievements(_data, userStreak, trophyImages, progressText, descriptionText, coinText);
                    break;
                case AchievementType.LevelUp:
                    CheckStreakAndLevelAchievements(_data, characterLevel, trophyImages, progressText, descriptionText, coinText);
                    break;
                case AchievementType.CompleteAllAchievements:
                    CheckCompleteAllAchivements(_data, trophyImages, progressText, descriptionText, coinText);
                    break;
                case AchievementType.LongSession:
                    CheckLongSessionAchivements(_data, GetHighestCompletedTimeInHours(ApiDataHandler.Instance.getHistoryData()), trophyImages, progressText, descriptionText, coinText);
                    break;
                case AchievementType.ExercisesInSingleSession:
                    CheckAchievements(_data, GetHighestExerciseInSingleSession(ApiDataHandler.Instance.getHistoryData()), AchievementType.ExercisesInSingleSession, trophyImages, progressText, descriptionText, coinText);
                    break;
                case AchievementType.ChangeTrainingBadge:
                    ChangeTrainingBadgeAchievements(_data, badgeChange, trophyImages, progressText, descriptionText, coinText);
                    break;
                case AchievementType.AddFriends:
                    CheckAchievements(_data, addedFriends, AchievementType.AddFriends, trophyImages, progressText, descriptionText, coinText);
                    break;
                case AchievementType.RemoveFriend:
                    CheckAchievements(_data, removedFriends, AchievementType.RemoveFriend, trophyImages, progressText, descriptionText, coinText);
                    break;
                case AchievementType.AddPersonBest:
                    CheckAchievements(_data, GetExercisesWithWeightGreaterThanZero(ApiDataHandler.Instance.getPersonalBestData()), AchievementType.AddPersonBest, trophyImages, progressText, descriptionText, coinText);
                    break;
                case AchievementType.CreateWorkout:
                    CheckAchievements(_data, ApiDataHandler.Instance.GetCreatedWorkoutTempleteCount(), AchievementType.CreateWorkout, trophyImages, progressText, descriptionText, coinText);
                    break;
                case AchievementType.WorkoutInactivity:
                    CheckAchievements(_data, GetGapBetweenLatestTwoWorkoutsInDays(ApiDataHandler.Instance.getHistoryData()), AchievementType.WorkoutInactivity, trophyImages, progressText, descriptionText, coinText);
                    break;
                case AchievementType.BuyItems:
                    CheckAchievements(_data, ApiDataHandler.Instance.GetBuyedCloths(), AchievementType.BuyItems, trophyImages, progressText, descriptionText, coinText);
                    break;
                case AchievementType.WeightLifted:
                    CheckAchievements(_data, GetTotalWeightInTons(ApiDataHandler.Instance.getHistoryData()), AchievementType.WeightLifted, trophyImages, progressText, descriptionText, coinText);
                    break;
            }
        }
        //ApiDataHandler.Instance.SaveAchievementData();
        if (completedItemsInSingleCheck.Count == 1)
        {
            List<object> initialData = new List<object> { completedItemsInSingleCheck, completedItemsTitles, "SingleAchievementCompletePopup", false };
            PopupController.Instance.OpenPopup("shared", "SingleAchievementCompletePopup", null, initialData);
        }
        else if (completedItemsInSingleCheck.Count > 1)
        {
            List<object> initialData = new List<object> { completedItemsInSingleCheck, completedItemsTitles, "MultipleAchievementCompletePopup", false };
            PopupController.Instance.OpenPopup("shared", "MultipleAchievementCompletePopup", null, initialData);
        }
    }
    
    
    public void CheckIndiviualAchievementStatus(AchievementTemplate _data, List<Image> trophyImages = null, TextMeshProUGUI progressText = null, TextMeshProUGUI descriptionText = null,TextMeshProUGUI coinText=null)
    {
        switch (_data.type)
        {
            case AchievementType.BodyweightMultiplier:
                CheckBodyWeightAchievements(_data, ApiDataHandler.Instance.getPersonalBestData(), trophyImages, progressText, descriptionText, coinText);
                break;
            case AchievementType.WorkoutCount:
                CheckWorkoutCountAchievements(_data, ApiDataHandler.Instance.getHistoryData().exerciseTempleteModel.Count, trophyImages, progressText, descriptionText, coinText);
                break;
            case AchievementType.ExerciseCount:
                CheckExerciseCountAchievements(_data, GetUniqueExerciseCount(ApiDataHandler.Instance.getHistoryData()), trophyImages, progressText, descriptionText, coinText);
                break;
            case AchievementType.Specialist:
                CheckSpecialistAchievements(_data, ApiDataHandler.Instance.getHistoryData(), trophyImages, progressText, descriptionText, coinText);
                break;
            case AchievementType.CardioTime:
                CheckCardioTimeAchievements(_data, ApiDataHandler.Instance.getHistoryData(), trophyImages, progressText, descriptionText, coinText);
                break;
            case AchievementType.Streak:
                CheckStreakAndLevelAchievements(_data, userStreak, trophyImages, progressText, descriptionText, coinText);
                break;
            case AchievementType.LevelUp:
                CheckStreakAndLevelAchievements(_data, characterLevel, trophyImages, progressText, descriptionText, coinText);
                break;
            case AchievementType.CompleteAllAchievements:
                CheckCompleteAllAchivements(_data, trophyImages, progressText, descriptionText, coinText);
                break;
            case AchievementType.LongSession:
                CheckLongSessionAchivements(_data, GetHighestCompletedTimeInHours(ApiDataHandler.Instance.getHistoryData()), trophyImages, progressText, descriptionText, coinText);
                break;
            case AchievementType.ExercisesInSingleSession:
                CheckAchievements(_data, GetHighestExerciseInSingleSession(ApiDataHandler.Instance.getHistoryData()), AchievementType.ExercisesInSingleSession, trophyImages, progressText, descriptionText, coinText);
                break;
            case AchievementType.ChangeTrainingBadge:
                ChangeTrainingBadgeAchievements(_data, badgeChange, trophyImages, progressText, descriptionText, coinText);
                break;
            case AchievementType.AddFriends:
                CheckAchievements(_data, addedFriends, AchievementType.AddFriends, trophyImages, progressText, descriptionText, coinText);
                break;
            case AchievementType.RemoveFriend:
                CheckAchievements(_data, removedFriends, AchievementType.RemoveFriend, trophyImages, progressText, descriptionText, coinText);
                break;
            case AchievementType.AddPersonBest:
                CheckAchievements(_data, GetExercisesWithWeightGreaterThanZero(ApiDataHandler.Instance.getPersonalBestData()), AchievementType.AddPersonBest, trophyImages, progressText, descriptionText, coinText);
                break;
            case AchievementType.CreateWorkout:
                CheckAchievements(_data, ApiDataHandler.Instance.GetCreatedWorkoutTempleteCount(), AchievementType.CreateWorkout, trophyImages, progressText, descriptionText, coinText);
                break;
            case AchievementType.WorkoutInactivity:
                CheckAchievements(_data, GetGapBetweenLatestTwoWorkoutsInDays(ApiDataHandler.Instance.getHistoryData()), AchievementType.WorkoutInactivity, trophyImages, progressText, descriptionText, coinText);
                break;
            case AchievementType.BuyItems:
                CheckAchievements(_data, ApiDataHandler.Instance.GetBuyedCloths(), AchievementType.BuyItems, trophyImages, progressText, descriptionText, coinText);
                break;
            case AchievementType.WeightLifted:
                CheckAchievements(_data, GetTotalWeightInTons(ApiDataHandler.Instance.getHistoryData()), AchievementType.WeightLifted, trophyImages, progressText, descriptionText, coinText);
                break;
        }
    }

    //----------------------------------------------------------------------------------------------------------------------------------

    public void CheckBodyWeightAchievements(AchievementTemplate data, PersonalBestData personalBest,List<Image> trophyImages,TextMeshProUGUI progressText,TextMeshProUGUI descriptionText, TextMeshProUGUI coinText)
    {
        for (int i = 0; i < data.achievementData.Count; i++)
        {
            AchievementTemplateDataItem achievementDataItem = data.achievementData[i];
            if (achievementDataItem.isCompleted)
            {
                if (trophyImages != null)
                    trophyImages[i].transform.GetChild(0).gameObject.SetActive(true);
                continue; // Skip if it's already completed
            }

            // Calculate the total weight by comparing latest workout and personal best for each exercise
            int totalWeight = 0;

            foreach (string exerciseName in data.category_exercise)
            {
                // Get weight for the exercise from the latest workout
                float latestWorkoutWeight = GetPerformedWeightForExercise(ApiDataHandler.Instance.getHistoryData(), exerciseName);

                // Get weight for the exercise from personal best
                float personalBestWeight = 0;
                var matchingExercise = personalBest.exercises.Find(exercise => exercise.exerciseName.Equals(exerciseName, StringComparison.OrdinalIgnoreCase));
                if (matchingExercise != null)
                    personalBestWeight = matchingExercise.weight;

                // Add the greater weight to the total
                totalWeight += (int)Math.Max(latestWorkoutWeight, personalBestWeight);
            }

            // Calculate the required value based on the achievement multiplier
            float value = achievementDataItem.value * ApiDataHandler.Instance.getMeasurementData().weight;

            if ((float)totalWeight >= value)
            {
                achievementDataItem.isCompleted = true;
                AddCoins(achievementDataItem.coins);
                SaveCompletedAchievementToFirebase(data.id, achievementDataItem.id);

                if (trophyImages != null)
                    trophyImages[i].transform.GetChild(0).gameObject.SetActive(true);
                else
                {
                    completedItemsInSingleCheck.Add(achievementDataItem);
                    completedItemsTitles.Add(data.title);
                }
            }
            else
            {
                if (progressText != null && descriptionText != null)
                {
                    progressText.text = totalWeight.ToString() + "kg / " + value.ToString() + "kg";
                    descriptionText.text = achievementDataItem.description;
                    coinText.text = achievementDataItem.coins.ToString();
                }
                return;
            }
        }

        if (progressText != null && descriptionText != null)
        {
            progressText.gameObject.SetActive(false);
            coinText.transform.parent.gameObject.SetActive(false);
            descriptionText.text = "Congratulations! You've reached peak performance � keep the momentum going!";
        }
    }

    public void CheckWorkoutCountAchievements(AchievementTemplate data, int performedWorkouts, List<Image> trophyImages, TextMeshProUGUI progressText, TextMeshProUGUI descriptionText, TextMeshProUGUI coinText)
    {
        for (int i = 0; i < data.achievementData.Count; i++)
        {
            AchievementTemplateDataItem achievementDataItem = data.achievementData[i];

            if (achievementDataItem.isCompleted)
            {
                if (trophyImages != null)
                    trophyImages[i].transform.GetChild(0).gameObject.SetActive(true);
                continue; // Skip if it's already completed
            }
            if (performedWorkouts >= achievementDataItem.value)
            {
                achievementDataItem.isCompleted = true;
                AddCoins(achievementDataItem.coins);
                SaveCompletedAchievementToFirebase(data.id, achievementDataItem.id);
                if (trophyImages != null)
                    trophyImages[i].transform.GetChild(0).gameObject.SetActive(true);
                else
                {
                    completedItemsInSingleCheck.Add(achievementDataItem);
                    completedItemsTitles.Add(data.title);
                    //List<object> initialData = new List<object> { data.title, achievementDataItem.description, false };
                    //PopupController.Instance.OpenPopup("shared", "AchievementCompletePopup", null, initialData);
                }
            }
            else
            {
                if (progressText != null && descriptionText != null)
                {
                    progressText.text = performedWorkouts.ToString() + " / " + achievementDataItem.value.ToString();
                    descriptionText.text = achievementDataItem.description;
                    coinText.text=achievementDataItem.coins.ToString();
                }
                return;
            }
        }
        if (progressText != null && descriptionText != null)
        {
            progressText.gameObject.SetActive(false);
            coinText.transform.parent.gameObject.SetActive(false);
            descriptionText.text= "Congratulations! You've reached peak performance � keep the momentum going!";
        }
    }

    public void CheckExerciseCountAchievements(AchievementTemplate data, int performedExercises, List<Image> trophyImages, TextMeshProUGUI progressText, TextMeshProUGUI descriptionText, TextMeshProUGUI coinText)
    {
        for (int i = 0; i < data.achievementData.Count; i++)
        {
            AchievementTemplateDataItem achievementDataItem = data.achievementData[i];

            if (achievementDataItem.isCompleted)
            {
                if (trophyImages != null)
                    trophyImages[i].transform.GetChild(0).gameObject.SetActive(true);
                continue; // Skip if it's already completed
            }
            if (performedExercises >= achievementDataItem.value)
            {
                achievementDataItem.isCompleted = true;
                AddCoins(achievementDataItem.coins);
                SaveCompletedAchievementToFirebase(data.id, achievementDataItem.id);
                if (trophyImages != null)
                    trophyImages[i].transform.GetChild(0).gameObject.SetActive(true);
                else
                {
                    completedItemsInSingleCheck.Add(achievementDataItem);
                    completedItemsTitles.Add(data.title);
                    //List<object> initialData = new List<object> { data.title, achievementDataItem.description, false };
                    //PopupController.Instance.OpenPopup("shared", "AchievementCompletePopup", null, initialData);
                }
            }
            else
            {
                if (progressText != null && descriptionText != null)
                {
                    progressText.text = performedExercises.ToString() + " / " + achievementDataItem.value.ToString();
                    descriptionText.text = achievementDataItem.description;
                    coinText.text=achievementDataItem.coins.ToString();
                }
                return;
            }
        }
        if (progressText != null && descriptionText != null)
        {
            progressText.gameObject.SetActive(false);
            coinText.transform.parent.gameObject.SetActive(false);
            descriptionText.text = "Congratulations! You've reached peak performance � keep the momentum going!";
        }
    }

    public void CheckSpecialistAchievements(AchievementTemplate data, HistoryModel historyModel, List<Image> trophyImages, TextMeshProUGUI progressText, TextMeshProUGUI descriptionText, TextMeshProUGUI coinText)
    {
        for (int i = 0; i < data.achievementData.Count; i++)
        {
            AchievementTemplateDataItem achievementDataItem = data.achievementData[i];
            int performed = GetPerformancedExercisesByCategory(historyModel, data.category_exercise);
            if (achievementDataItem.isCompleted)
            {
                if (trophyImages != null)
                    trophyImages[i].transform.GetChild(0).gameObject.SetActive(true);
                continue; // Skip if it's already completed
            }
            if (performed >= achievementDataItem.value)
            {
                achievementDataItem.isCompleted = true;
                AddCoins(achievementDataItem.coins);
                SaveCompletedAchievementToFirebase(data.id, achievementDataItem.id);
                if (trophyImages != null)
                    trophyImages[i].transform.GetChild(0).gameObject.SetActive(true);
                else
                {
                    completedItemsInSingleCheck.Add(achievementDataItem);
                    completedItemsTitles.Add(data.title);
                    //List<object> initialData = new List<object> { data.title, achievementDataItem.description, false };
                    //PopupController.Instance.OpenPopup("shared", "AchievementCompletePopup", null, initialData);
                }
            }
            else
            {
                if (progressText != null && descriptionText != null)
                {
                    progressText.text = performed.ToString() + " / " + achievementDataItem.value.ToString();
                    descriptionText.text = achievementDataItem.description;
                    coinText.text = achievementDataItem.coins.ToString();
                }
                return;
            }
        }
        if (progressText != null && descriptionText != null)
        {
            progressText.gameObject.SetActive(false);
            coinText.transform.parent.gameObject.SetActive(false);
            descriptionText.text = "Congratulations! You've reached peak performance � keep the momentum going!";
        }
    }

    public void CheckCardioTimeAchievements(AchievementTemplate data, HistoryModel historyModel, List<Image> trophyImages, TextMeshProUGUI progressText, TextMeshProUGUI descriptionText, TextMeshProUGUI coinText)
    {
        for (int i = 0; i < data.achievementData.Count; i++)
        {
            AchievementTemplateDataItem achievementDataItem = data.achievementData[i];
            int performed = GetTotalPerformancedExerciseTime(historyModel, data.category_exercise);
            print(performed);
            float performedTime = (float)performed / 3600f;
            print(performedTime);
            if (achievementDataItem.isCompleted)
            {
                if (trophyImages != null)
                    trophyImages[i].transform.GetChild(0).gameObject.SetActive(true);
                continue; // Skip if it's already completed
            }
            if (performedTime >= (float)achievementDataItem.value / 60)
            {
                achievementDataItem.isCompleted = true;
                AddCoins(achievementDataItem.coins);
                SaveCompletedAchievementToFirebase(data.id, achievementDataItem.id);
                if (trophyImages != null)
                    trophyImages[i].transform.GetChild(0).gameObject.SetActive(true);
                else
                {
                    completedItemsInSingleCheck.Add(achievementDataItem);
                    completedItemsTitles.Add(data.title);
                    //List<object> initialData = new List<object> { data.title, achievementDataItem.description, false };
                    //PopupController.Instance.OpenPopup("shared", "AchievementCompletePopup", null, initialData);
                }
            }
            else
            {
                if (progressText != null && descriptionText != null)
                {
                    progressText.text = performedTime % 1 == 0 ? 
                        performedTime.ToString("0") + " / " + ((float)achievementDataItem.value / 60).ToString() : 
                        performedTime.ToString("0.##") + " / " + ((float)achievementDataItem.value / 60).ToString();
                    //progressText.text = performedTime.ToString("F2") + " / " + ((float)achievementDataItem.value / 60).ToString();
                    descriptionText.text = achievementDataItem.description;
                    coinText.text=achievementDataItem.coins.ToString();
                }
                return;
            }
        }
        if (progressText != null && descriptionText != null)
        {
            progressText.gameObject.SetActive(false);
            coinText.transform.parent.gameObject.SetActive(false);
            descriptionText.text = "Congratulations! You've reached peak performance � keep the momentum going!";
        }
        //descriptionText.text = _data.achievementData[_data.achievementData.Count - 1].description;
    }
    public void CheckStreakAndLevelAchievements(AchievementTemplate data, int streak, List<Image> trophyImages, TextMeshProUGUI progressText, TextMeshProUGUI descriptionText, TextMeshProUGUI coinText)
    {
        for (int i = 0; i < data.achievementData.Count; i++)
        {
            AchievementTemplateDataItem achievementDataItem = data.achievementData[i];
            if (achievementDataItem.isCompleted)
            {
                if (trophyImages != null)
                    trophyImages[i].transform.GetChild(0).gameObject.SetActive(true);
                continue; // Skip if it's already completed
            }
            if (streak >= achievementDataItem.value)
            {
                achievementDataItem.isCompleted = true;
                AddCoins(achievementDataItem.coins);
                SaveCompletedAchievementToFirebase(data.id, achievementDataItem.id);
                if (trophyImages != null)
                    trophyImages[i].transform.GetChild(0).gameObject.SetActive(true);
                else
                {
                    completedItemsInSingleCheck.Add(achievementDataItem);
                    completedItemsTitles.Add(data.title);
                    //List<object> initialData = new List<object> { data.title, achievementDataItem.description, false };
                    //PopupController.Instance.OpenPopup("shared", "AchievementCompletePopup", null, initialData);
                }
            }
            else
            {
                if (progressText != null && descriptionText != null)
                {
                    progressText.text = streak.ToString() + " / " + (achievementDataItem.value).ToString();
                    descriptionText.text = achievementDataItem.description;
                    coinText.text = achievementDataItem.coins.ToString();
                }
                return;
            }
        }
        if (progressText != null && descriptionText != null)
        {
            progressText.gameObject.SetActive(false);
            coinText.transform.parent.gameObject.SetActive(false);
            descriptionText.text = "Congratulations! You've reached peak performance � keep the momentum going!";
        }
    }
    public void CheckCompleteAllAchivements(AchievementTemplate data, List<Image> trophyImages, TextMeshProUGUI progressText, TextMeshProUGUI descriptionText, TextMeshProUGUI coinText)
    {
        for (int i = 0; i < data.achievementData.Count; i++)
        {
            AchievementTemplateDataItem achievementDataItem = data.achievementData[i];
            if (achievementDataItem.isCompleted)
            {
                if (trophyImages != null)
                    trophyImages[i].transform.GetChild(0).gameObject.SetActive(true);
                continue; // Skip if it's already completed
            }
            int totalAchievements = ApiDataHandler.Instance.GetTotalAchievements() - 1;
            int completeAchievements = ApiDataHandler.Instance.GetCompletedAchievements(ApiDataHandler.Instance.getAchievementData());
            if (completeAchievements==totalAchievements)
            {
                achievementDataItem.isCompleted = true;
                AddCoins(achievementDataItem.coins);
                SaveCompletedAchievementToFirebase(data.id, achievementDataItem.id);
                if (trophyImages != null)
                    trophyImages[i].transform.GetChild(0).gameObject.SetActive(true);
                else
                {
                    completedItemsInSingleCheck.Add(achievementDataItem);
                    completedItemsTitles.Add(data.title);
                    //List<object> initialData = new List<object> { data.title, achievementDataItem.description, false };
                    //PopupController.Instance.OpenPopup("shared", "AchievementCompletePopup", null, initialData);
                }
            }
            else
            {
                if (progressText != null && descriptionText != null)
                {
                    progressText.text = completeAchievements + " / " + totalAchievements.ToString();
                    descriptionText.text = achievementDataItem.description;
                    coinText.text = achievementDataItem.coins.ToString();
                }
                return;
            }
        }
        if (progressText != null && descriptionText != null)
        {
            progressText.gameObject.SetActive(false);
            coinText.transform.parent.gameObject.SetActive(false);
            descriptionText.text = "Congratulations! You've reached peak performance � keep the momentum going!";
        }
    }

    public void CheckLongSessionAchivements(AchievementTemplate data, int longestSession, List<Image> trophyImages, TextMeshProUGUI progressText, TextMeshProUGUI descriptionText, TextMeshProUGUI coinText)
    {
        for (int i = 0; i < data.achievementData.Count; i++)
        {
            AchievementTemplateDataItem achievementDataItem = data.achievementData[i];
            if (achievementDataItem.isCompleted)
            {
                if (trophyImages != null)
                    trophyImages[i].transform.GetChild(0).gameObject.SetActive(true);
                continue; // Skip if it's already completed
            }
            if (longestSession >= achievementDataItem.value)
            {
                achievementDataItem.isCompleted = true;
                AddCoins(achievementDataItem.coins);
                SaveCompletedAchievementToFirebase(data.id, achievementDataItem.id);
                if (trophyImages != null)
                    trophyImages[i].transform.GetChild(0).gameObject.SetActive(true);
                else
                {
                    completedItemsInSingleCheck.Add(achievementDataItem);
                    completedItemsTitles.Add(data.title);
                    //List<object> initialData = new List<object> { data.title, achievementDataItem.description, false };
                    //PopupController.Instance.OpenPopup("shared", "AchievementCompletePopup", null, initialData);
                }
            }
            else
            {
                if (progressText != null && descriptionText != null)
                {
                    progressText.text = longestSession + "H / " + achievementDataItem.value.ToString()+"H";
                    descriptionText.text = achievementDataItem.description;
                    coinText.text = achievementDataItem.coins.ToString();
                }
                return;
            }
        }
        if (progressText != null && descriptionText != null)
        {
            progressText.gameObject.SetActive(false);
            coinText.transform.parent.gameObject.SetActive(false);
            descriptionText.text = "Congratulations! You've reached peak performance � keep the momentum going!";
        }
    }
    public void ChangeTrainingBadgeAchievements(AchievementTemplate data, bool badgeChange, List<Image> trophyImages, TextMeshProUGUI progressText, TextMeshProUGUI descriptionText, TextMeshProUGUI coinText)
    {
        for (int i = 0; i < data.achievementData.Count; i++)
        {
            AchievementTemplateDataItem achievementDataItem = data.achievementData[i];
            if (achievementDataItem.isCompleted)
            {
                if (trophyImages != null)
                    trophyImages[i].transform.GetChild(0).gameObject.SetActive(true);
                continue; // Skip if it's already completed
            }
            if (badgeChange)
            {
                achievementDataItem.isCompleted = true;
                AddCoins(achievementDataItem.coins);
                SaveCompletedAchievementToFirebase(data.id, achievementDataItem.id);
                if (trophyImages != null)
                    trophyImages[i].transform.GetChild(0).gameObject.SetActive(true);
                else
                {
                    completedItemsInSingleCheck.Add(achievementDataItem);
                    completedItemsTitles.Add(data.title);
                    //List<object> initialData = new List<object> { data.title, achievementDataItem.description, false };
                    //PopupController.Instance.OpenPopup("shared", "AchievementCompletePopup", null, initialData);
                }
            }
            else
            {
                if (progressText != null && descriptionText != null)
                {
                    progressText.text = "0 / " + achievementDataItem.value.ToString();
                    descriptionText.text = achievementDataItem.description;
                    coinText.text = achievementDataItem.coins.ToString();
                }
                return;
            }
        }
        if (progressText != null && descriptionText != null)
        {
            progressText.gameObject.SetActive(false);
            coinText.transform.parent.gameObject.SetActive(false);
            descriptionText.text = "Congratulations! You've reached peak performance � keep the momentum going!";
        }
    }
    public void CheckAchievements(AchievementTemplate data, int performedExercises, AchievementType type, List<Image> trophyImages, TextMeshProUGUI progressText, TextMeshProUGUI descriptionText, TextMeshProUGUI coinText)
    {
        for (int i = 0; i < data.achievementData.Count; i++)
        {
            AchievementTemplateDataItem achievementDataItem = data.achievementData[i];

            if (achievementDataItem.isCompleted)
            {
                if (trophyImages != null)
                    trophyImages[i].transform.GetChild(0).gameObject.SetActive(true);
                continue; // Skip if it's already completed
            }
            if (performedExercises >= achievementDataItem.value)
            {
                achievementDataItem.isCompleted = true;
                AddCoins(achievementDataItem.coins);
                SaveCompletedAchievementToFirebase(data.id, achievementDataItem.id);
                if (trophyImages != null)
                    trophyImages[i].transform.GetChild(0).gameObject.SetActive(true);
                else
                {
                    completedItemsInSingleCheck.Add(achievementDataItem);
                    completedItemsTitles.Add(data.title);
                    //List<object> initialData = new List<object> { data.title, achievementDataItem.description, false };
                    //PopupController.Instance.OpenPopup("shared", "AchievementCompletePopup", null, initialData);
                }
            }
            else
            {
                if (progressText != null && descriptionText != null)
                {
                    switch (type)
                    {
                        case AchievementType.WeightLifted:
                            progressText.text = performedExercises.ToString() + "T / " + achievementDataItem.value.ToString()+"T";                          
                            break;
                        default:
                            progressText.text = performedExercises.ToString() + " / " + achievementDataItem.value.ToString();                         
                            break;
                    }
                    descriptionText.text = achievementDataItem.description;
                    coinText.text = achievementDataItem.coins.ToString();
                }
                return;
            }
        }
        if (progressText != null && descriptionText != null)
        {
            progressText.gameObject.SetActive(false);
            coinText.transform.parent.gameObject.SetActive(false);
            descriptionText.text = "Congratulations! You've reached peak performance � keep the momentum going!";
        }
    }

    //------------------------------------------------------Helper Functions----------------------------------------------------------------------------------


    public float GetPerformedWeightForExercise(HistoryModel historyModel, string exerciseName)
    {
        float maxWeight = 0;

        // Iterate through all templates and check for the maximum weight of the exercise
        foreach (var template in historyModel.exerciseTempleteModel)
        {
            var exerciseType = template.exerciseTypeModel
                .FirstOrDefault(type => type.exerciseName.Equals(exerciseName, StringComparison.OrdinalIgnoreCase));

            if (exerciseType != null && exerciseType.exerciseModel.Any())
            {
                // Get the maximum weight for the sets of this exercise
                float currentMaxWeight = exerciseType.exerciseModel.Max(set => set.weight);

                // Keep track of the overall maximum weight
                maxWeight = Math.Max(maxWeight, currentMaxWeight);
            }
        }

        return maxWeight; // Return the maximum weight found
    }

    public int GetUniqueExerciseCount(HistoryModel historyModel)
    {
        HashSet<string> uniqueExercises = new HashSet<string>();
        foreach (var template in historyModel.exerciseTempleteModel)
        {
            foreach (var exerciseType in template.exerciseTypeModel)
            {
                uniqueExercises.Add(exerciseType.exerciseName);
            }
        }
        return uniqueExercises.Count;
    }

    public int GetPerformancedExercisesByCategory(HistoryModel myHistory, List<string> categoryNames)
    {
        int totalCount = 0;
        List<string> lowerCaseCategories = categoryNames.Select(c => c.ToLower()).ToList();

        foreach (HistoryTempleteModel historyTemplate in myHistory.exerciseTempleteModel)
        {
            foreach (HistoryExerciseTypeModel exerciseType in historyTemplate.exerciseTypeModel)
            {
                if (lowerCaseCategories.Contains(SplitString(exerciseType.categoryName).ToLower()))
                {
                    totalCount++;
                }
            }
        }

        return totalCount;
    }

    public int GetTotalPerformancedExerciseTime(HistoryModel myHistory, List<string> categoryNames)
    {
        int totalTime = 0;
        List<string> lowerCaseCategories = categoryNames.Select(c => c.ToLower()).ToList();
        
        // Loop through each exercise template in the history
        foreach (HistoryTempleteModel historyTemplate in myHistory.exerciseTempleteModel)
        {
            // Loop through each exercise type in the template
            foreach (HistoryExerciseTypeModel exerciseType in historyTemplate.exerciseTypeModel)
            {
                // Check if the exercise category matches any category in the achievement template
                if (lowerCaseCategories.Contains(exerciseType.categoryName.ToLower()))
                {
                    // Loop through each exercise model and sum its time
                    foreach (HistoryExerciseModel exercise in exerciseType.exerciseModel)
                    {
                        totalTime += exercise.time;
                    }
                }
            }
        }

        return totalTime;
    }
    public string SplitString(string inputList)
    {
        string resultList;

        if (inputList.Contains('/'))
        {
            // Split the string and add the parts to the result list
            string[] splitParts = inputList.Split('/');
            resultList = splitParts[0];
            return resultList;
        }
        else
        {
            resultList = inputList;
        }

        return resultList;
    }

    public int GetHighestCompletedTimeInHours(HistoryModel history)
    {
        if (history.exerciseTempleteModel.Count < 2)
            return 0;
        int highestCompletedTimeInHours = history.exerciseTempleteModel
            .Max(template => template.completedTime / 3600); // Divide seconds by 3600 to get hours

        return highestCompletedTimeInHours;
    }
    public int GetHighestExerciseInSingleSession(HistoryModel history)
    {
        if (history.exerciseTempleteModel.Count < 1)
            return 0;
        // Get the highest count of exerciseTypeModel in the list
        int highestCount = history.exerciseTempleteModel
            .Max(template => template.exerciseTypeModel.Count);

        return highestCount;
    }
    public int GetTotalWeightInTons(HistoryModel history)
    {
        float totalWeightInTons = history.exerciseTempleteModel
            .Sum(template => template.totalWeight) / 1000f;

        return (int)totalWeightInTons;
    }
    public int GetGapBetweenLatestTwoWorkoutsInDays(HistoryModel history)
    {
        try
        {
            // Parse and sort the dateTime strings into DateTime objects
            var sortedDates = history.exerciseTempleteModel
                .Where(template => DateTime.TryParse(template.dateTime, out _))
                .Select(template => DateTime.Parse(template.dateTime))
                .OrderByDescending(date => date)
                .ToList();

            // Ensure there are at least two valid dates
            if (sortedDates.Count < 2)
            {
                Debug.LogWarning("Not enough valid dates to calculate the gap.");
                return 0;
            }

            // Calculate the gap in days between the two latest dates
            int gapInDays = (sortedDates[0] - sortedDates[1]).Days;

            if (gapInDays >= 14)
                return gapInDays;
            else
                return 0;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error calculating gap: {ex.Message}");
            return 0; // Return -1 to indicate an error
        }
    }
    public int GetExercisesWithWeightGreaterThanZero(PersonalBestData personalBestData)
    {
        if(personalBestData.exercises.Count>0)
            return personalBestData.exercises.Count(exercise => exercise.weight > 0);
        else 
            return 0;
    }
    public void AddCoins(int coins)
    {
        lock (coinLock)
        {
            this.currentCoins += coins;
            print("set coins: " + currentCoins);
            ApiDataHandler.Instance.SetCoinsToFirebase(currentCoins);
        }
    }
    public void SaveCompletedAchievementToFirebase(string achievementId,string itemId)
    {
        ApiDataHandler.Instance.SaveUserAchievementData(achievementId, itemId);
    }

    public float ConvertLbsToKg(float pounds)
    {
        return pounds * lbsToKgFactor;
    }
    public float ConvertKgToLbs(float kilograms)
    {
        return kilograms * kgToLbsFactor;
    }
    public void ActiveInput(TMP_InputField input)
    {
        input.Select();
        input.ActivateInputField();
    }
    public string FormatStringAbc(string input)
    {
        TextInfo textInfo = CultureInfo.InvariantCulture.TextInfo;

        string normalizedInput = input.Replace("(", " (").Replace(")", ") ");

        string[] words = normalizedInput.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        for (int i = 0; i < words.Length; i++)
        {
            words[i] = textInfo.ToTitleCase(words[i].ToLower());
        }
        string result = string.Join(" ", words).Trim();
        return result.Replace(" (", " (").Replace(") ", ")");
    }
    public string ShowFormattedNumber(float number)
    {
        // Format the number
        string formattedNumber = (number % 1 == 0) ? number.ToString("0") : number.ToString("0.##");

        return formattedNumber;
    }
    public string GetGifFolder(int level)
    {
        int characterIndex = level / 4;

        // Ensure the index is within the bounds of the array
        characterIndex = Mathf.Clamp(characterIndex, 0, 1);
        return characterIndex.ToString()+ "/";
    }

    public void FitImage(Image targetImage, RectTransform mask)
    {
        if (targetImage == null || mask == null) return;

        targetImage.SetNativeSize();
        // Get the dimensions of the image and the mask
        float imageWidth = targetImage.sprite.texture.width;
        float imageHeight = targetImage.sprite.texture.height;

        // Get the dimensions of the mask
        float maskWidth = mask.rect.width;
        float maskHeight = mask.rect.height;

        // Calculate aspect ratios
        float imageAspectRatio = imageWidth / imageHeight;
        float maskAspectRatio = maskWidth / maskHeight;

        // Scale uniformly to ensure the image fills the mask
        if (imageAspectRatio > maskAspectRatio)
        {
            // Image is wider than the mask; scale height to fill
            float scaleFactor = maskHeight / imageHeight;
            targetImage.rectTransform.sizeDelta = new Vector2(imageWidth * scaleFactor, maskHeight);
        }
        else
        {
            // Image is taller than the mask; scale width to fill
            float scaleFactor = maskWidth / imageWidth;
            targetImage.rectTransform.sizeDelta = new Vector2(maskWidth, imageHeight * scaleFactor);
        }

        // Ensure the image stays centered within the mask
        targetImage.rectTransform.anchoredPosition = Vector2.zero;
    }
}
