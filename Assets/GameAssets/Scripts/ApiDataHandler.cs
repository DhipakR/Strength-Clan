using System.Linq;
using UnityEngine;
using System.Collections.Generic;
using System;
using Firebase.Database;
using System.Collections;
using UnityEngine.Networking;
using SDev.GiphyAPI;
using System.Runtime.InteropServices.ComTypes;
using System.Threading.Tasks;
using System.Security.Policy;
using UnityEngine.UI;

public class ApiDataHandler : GenericSingletonClass<ApiDataHandler>
{
    [SerializeField]
    private ExerciseData exerciseData = new ExerciseData(); // firebase
    [SerializeField]
    private AchievementData achievementData = new AchievementData();    //firebase
    [SerializeField]
    private PersonalBestData personalBestData = new PersonalBestData(); //firebase
    [SerializeField]
    private TemplateData templateData = new TemplateData(); //firebase
    [SerializeField]
    private HistoryModel historyData = new HistoryModel();  //firebase
    [SerializeField]
    private MeasurementModel measurementData = new MeasurementModel();  //firebase
    [SerializeField]
    private MeasurementHistory measurementHistory = new MeasurementHistory();   //firebase
    [SerializeField]
    private ExerciseNotesHistory notesHistory = new ExerciseNotesHistory(); //firebase
    [SerializeField]
    private ShopModel shopData = new ShopModel(); 
    [SerializeField]
    private Dictionary<string, string> friendsData = new Dictionary<string, string>();
    [SerializeField]
    private FriendModel friendDataModel = new FriendModel();
    [SerializeField]
    private string userName;

    [Header("Theme Settings")]
    public Theme gameTheme;


    public bool isSignUp;

    public void LogOut()
    {
        exerciseData = new ExerciseData();
        achievementData = new AchievementData();
        personalBestData = new PersonalBestData();
        templateData= new TemplateData();
        historyData= new HistoryModel();
        measurementData=new MeasurementModel();
        measurementHistory = new MeasurementHistory();
        notesHistory = new ExerciseNotesHistory();
        shopData=new ShopModel();
        friendsData=new Dictionary<string, string>();
        friendDataModel=new FriendModel();
    }
    public void SaveTheme(Theme theme)
    {
        PreferenceManager.Instance.SetInt("SelectedTheme", (int)theme);
        gameTheme = theme;
        PreferenceManager.Instance.Save();
    }
    public Theme LoadTheme()
    {
        int savedTheme = PlayerPrefs.GetInt("SelectedTheme", (int)Theme.Light);
        return (Theme)savedTheme;
    }
    public void SaveWeight(int weight)
    {
        PreferenceManager.Instance.SetInt("Weight", weight);
        PreferenceManager.Instance.Save();
    }
    public int GetWeight()
    {
         return PlayerPrefs.GetInt("Weight", 0);
    }

    public void SaveTemplateData()
    {
        string json = JsonUtility.ToJson(templateData);
        FirebaseManager.Instance.databaseReference.Child("users").Child(FirebaseManager.Instance.user.UserId)
            .Child("workoutTempletes").SetRawJsonValueAsync(json).ContinueWith(task =>
            {
                if (task.IsCompleted)
                {
                    Debug.Log("workoutTempletes added");
                }
                else
                {
                    Debug.LogError("Failed to add exercise: " + task.Exception);
                }
            });
    }
    public void ReplaceExerciseTemplate(DefaultTempleteModel newTemplate, int templateIndex)
    {

        // Convert the new template to JSON
        string jsonUpdatedTemplate = JsonUtility.ToJson(newTemplate);
        // Update the specific template node in Firebase
        FirebaseManager.Instance.databaseReference.Child("users")
            .Child(FirebaseManager.Instance.user.UserId).Child("workoutTempletes").Child("exerciseTemplete")
            .Child(templateIndex.ToString()).SetRawJsonValueAsync(jsonUpdatedTemplate).ContinueWith(task =>
            {
                if (task.IsCompleted)
                {
                    Debug.Log("Workout template replaced successfully.");
                }
                else
                {
                    Debug.LogError("Failed to replace workout template: " + task.Exception);
                }
            });
    }
    public void AddExerciseTemplate(DefaultTempleteModel template,int index)
    {
        string json = JsonUtility.ToJson(template);
        FirebaseManager.Instance.databaseReference.Child("users")
           .Child(FirebaseManager.Instance.user.UserId).Child("workoutTempletes").Child("exerciseTemplete")
           .Child(index.ToString()).SetRawJsonValueAsync(json).ContinueWith(task =>
           {
               if (task.IsCompleted)
               {
                   Debug.Log("Workout template added.");
               }
               else
               {
                   Debug.LogError("Failed to add workout template: " + task.Exception);
               }
           });
    }
    public void DeleteExerciseTemplate(int templateIndex)
    {
        // Reference to the workoutTempletes node in Firebase
        var reference = FirebaseManager.Instance.databaseReference
            .Child("users")
            .Child(FirebaseManager.Instance.user.UserId)
            .Child("workoutTempletes").Child("exerciseTemplete");

        // Remove the template at the specific index in Firebase
        reference.Child(templateIndex.ToString())  // Use the index as the key
            .RemoveValueAsync().ContinueWith(task =>
            {
                if (task.IsCompleted)
                {
                    Debug.Log("Workout template deleted successfully.");
                }
                else
                {
                    Debug.LogError("Failed to delete workout template: " + task.Exception);
                }
            });
    }
    public void LoadFriendData()
    {
        FirebaseManager.Instance.GetDataFromFirebase("/users/" + FirebaseManager.Instance.user.UserId + "/friend", data =>
        {
            DataSnapshot snapshot = data;
            foreach (var child in snapshot.Children)
            {
                // Add the key (user identifier) and value (user data) to the dictionary
                friendsData.Add(child.Key, child.Value.ToString());
            }
        });
    }
    public void LoadTemplateData()
    {
        FirebaseManager.Instance.CheckIfLocationExists("/users/" + FirebaseManager.Instance.user.UserId + "/workoutTempletes", result =>
        {
            if (result)
            {
                FirebaseManager.Instance.GetDataFromFirebase("/users/" + FirebaseManager.Instance.user.UserId + "/workoutTempletes", _data =>
                {
                    if (_data != null)
                    {
                        string jsonData = _data.GetRawJsonValue();
                        templateData = (TemplateData)LoadData(jsonData, typeof(TemplateData));
                        print("from firebase");
                    }
                });

                Debug.Log("Data exists at the path.");
            }
            else
            {
                //CreateRandomDefaultEntry();

                Debug.Log("No data found at the path.");
            }
        });


    }

    public void LoadExercises()
    {
        FirebaseManager.Instance.CheckIfLocationExists("/users/" + FirebaseManager.Instance.user.UserId + "/exercises", result =>
        {
            if (result)
            {
                FirebaseManager.Instance.GetDataFromFirebase("/users/" + FirebaseManager.Instance.user.UserId, _data =>
                {
                    if (_data != null)
                    {
                        string jsonData = _data.GetRawJsonValue();
                        exerciseData = (ExerciseData)LoadData(jsonData, typeof(ExerciseData));
                        print("from firebase");
                    }
                });

                Debug.Log("Data exists at the path.");
            }
            else
            {
                TextAsset exerciseJsonFile = Resources.Load<TextAsset>("data/exercise");
                string exerciseJson = exerciseJsonFile.text;
                this.exerciseData = JsonUtility.FromJson<ExerciseData>(exerciseJson);
                FirebaseManager.Instance.databaseReference.Child("users").Child(FirebaseManager.Instance.user.UserId).Child("exercises")
                .SetRawJsonValueAsync(exerciseJson).ContinueWith(task =>
                {
                    if (task.IsCompleted)
                    {
                        Debug.Log("exercise added.");
                    }
                    else
                    {
                        Debug.LogError("Failed to add exercises: " + task.Exception);
                    }
                });
            }
        });


    }

    public void SaveExerciseData(ExerciseDataItem exercise, int index, Action onSuccess = null)
    {
        string json = JsonUtility.ToJson(exercise);

        var exercisesRef = FirebaseManager.Instance.databaseReference
            .Child("users")
            .Child(FirebaseManager.Instance.user.UserId)
            .Child("exercises")
            .Child("exercises")
            .Child(index.ToString());

        exercisesRef.SetRawJsonValueAsync(json).ContinueWith(async addTask =>
        {
            if (addTask.IsCompleted && addTask.Exception == null)
            {
                Debug.Log("Exercise added at index: " + index);
                this.exerciseData.exercises.Add(exercise);
                onSuccess?.Invoke();
            }
            else
            {
                Debug.LogError("Failed to add exercise: " + addTask.Exception);
            }
        });
    }

    public object LoadData(string jsonData, Type targetType)
    {
        return JsonUtility.FromJson(jsonData, targetType);
    }
    
    

    public void CreateRandomDefaultEntry()
    {
        ExerciseTypeModel back1 = new ExerciseTypeModel
        {
            index = 0,
            name = "Deadlifts (Barbell)",
            categoryName = "Glutes",
            exerciseType = ExerciseType.WeightAndReps,
            exerciseModel = new List<ExerciseModel>()
        };
        ExerciseTypeModel back2 = new ExerciseTypeModel
        {
            index = 0,
            name = "Seated row - narrow grip (cable)",
            categoryName = "Lats",
            exerciseType = ExerciseType.WeightAndReps,
            exerciseModel = new List<ExerciseModel>()
        };
        ExerciseTypeModel chest = new ExerciseTypeModel
        {
            index = 0,
            name = "Bench Press (Barbell)",
            categoryName = "Chest",
            exerciseType = ExerciseType.WeightAndReps,
            exerciseModel = new List<ExerciseModel>()
        };
        ExerciseTypeModel running = new ExerciseTypeModel
        {
            index = 0,
            name = "Running",
            categoryName = "Cardio",
            exerciseType = ExerciseType.TimeAndMiles,
            exerciseModel = new List<ExerciseModel>()
        };
        ExerciseTypeModel jumpRope = new ExerciseTypeModel
        {
            index = 0,
            name = "Jump Rope",
            categoryName = "Cardio",
            exerciseType = ExerciseType.RepsOnly,
            exerciseModel = new List<ExerciseModel>()
        };
        ExerciseTypeModel spiderCurls = new ExerciseTypeModel
        {
            index = 0,
            name = "Spider Curls",
            categoryName = "Biceps",
            exerciseType = ExerciseType.WeightAndReps,
            exerciseModel = new List<ExerciseModel>()
        };
        ExerciseTypeModel bicepCulDumbbell = new ExerciseTypeModel
        {
            index = 0,
            name = "Bicep Curl (Dumbbell)",
            categoryName = "Biceps",
            exerciseType = ExerciseType.WeightAndReps,
            exerciseModel = new List<ExerciseModel>()
        };
        ExerciseTypeModel bicepCurlMachine = new ExerciseTypeModel
        {
            index = 0,
            name = "Bicep Curl (Machine)",
            categoryName = "Biceps",
            exerciseType = ExerciseType.WeightAndReps,
            exerciseModel = new List<ExerciseModel>()
        };
        ExerciseModel defaultExerciseModel1 = new ExerciseModel
        {
            setID = 1,
            previous = "-",
            weight = 0,
            rir = 0,
            reps = 0
        };
        back1.exerciseModel.Add(defaultExerciseModel1);
        back1.exerciseModel.Add(defaultExerciseModel1);
        back1.exerciseModel.Add(defaultExerciseModel1);
        back2.exerciseModel.Add(defaultExerciseModel1);
        chest.exerciseModel.Add(defaultExerciseModel1);
        running.exerciseModel.Add(defaultExerciseModel1);
        running.exerciseModel.Add(defaultExerciseModel1);
        jumpRope.exerciseModel.Add(defaultExerciseModel1);
        jumpRope.exerciseModel.Add(defaultExerciseModel1);
        jumpRope.exerciseModel.Add(defaultExerciseModel1);
        spiderCurls.exerciseModel.Add(defaultExerciseModel1);
        spiderCurls.exerciseModel.Add(defaultExerciseModel1);
        bicepCulDumbbell.exerciseModel.Add(defaultExerciseModel1);
        bicepCulDumbbell.exerciseModel.Add(defaultExerciseModel1);
        bicepCurlMachine.exerciseModel.Add(defaultExerciseModel1);
        bicepCurlMachine.exerciseModel.Add(defaultExerciseModel1);


        DefaultTempleteModel chestAndBack = new DefaultTempleteModel
        {
            templeteName = "Chest And Back",
            exerciseTemplete = new List<ExerciseTypeModel> { back1, back2, chest }
        };
        DefaultTempleteModel runingAndJumpRope = new DefaultTempleteModel
        {
            templeteName = "Runing And Jump Rope",
            exerciseTemplete = new List<ExerciseTypeModel> { running, jumpRope }
        };
        DefaultTempleteModel bicep = new DefaultTempleteModel
        {
            templeteName = "Biceps",
            exerciseTemplete = new List<ExerciseTypeModel> { bicepCulDumbbell, bicepCurlMachine, spiderCurls }
        };
        templateData.exerciseTemplete.Clear();
        templateData.exerciseTemplete.Add(chestAndBack);
        templateData.exerciseTemplete.Add(runingAndJumpRope);
        templateData.exerciseTemplete.Add(bicep);
        print("create templete");
        SaveTemplateData();
    }
    public void LoadMeasurementData()
    {
        FirebaseManager.Instance.CheckIfLocationExists("/users/" + FirebaseManager.Instance.user.UserId + "/measurements", result =>
        {
            if (result)
            {
                FirebaseManager.Instance.GetDataFromFirebase("/users/" + FirebaseManager.Instance.user.UserId + "/measurements", _data =>
                {
                    if (_data != null)
                    {
                        string jsonData = _data.GetRawJsonValue();
                        measurementData = (MeasurementModel)LoadData(jsonData, typeof(MeasurementModel));
                        print("from firebase");
                    }
                });

                Debug.Log("Data exists at the path.");
            }

        });
    }
    public void LoadMeasurementHistory()
    {
        FirebaseManager.Instance.CheckIfLocationExists("/users/" + FirebaseManager.Instance.user.UserId + "/measurementHistory", result =>
        {
            if (result)
            {
                FirebaseManager.Instance.GetDataFromFirebase("/users/" + FirebaseManager.Instance.user.UserId + "/measurementHistory", _data =>
                {
                    if (_data != null)
                    {
                        string jsonData = _data.GetRawJsonValue();
                        measurementHistory = (MeasurementHistory)LoadData(jsonData, typeof(MeasurementHistory));
                        print("from firebase");
                    }
                });

                Debug.Log("Data exists at the path.");
            }

        });
    }
    public void SaveMeasurementData()
    {
        string json = JsonUtility.ToJson(measurementData);
        var path = "/users/" + FirebaseManager.Instance.user.UserId + "/measurements";

        FirebaseManager.Instance.databaseReference.Child(path).SetRawJsonValueAsync(json).ContinueWith(task =>
        {
            if (task.IsCompleted)
            {
                Debug.Log("Workout template added.");
            }
            else
            {
                Debug.LogError("Failed to add workout template: " + task.Exception);
            }
        });
    }
    public void SaveMeasurementHistory(MeasurementHistoryItem item,int index)
    {
        string json = JsonUtility.ToJson(item);
        var path = "/users/" + FirebaseManager.Instance.user.UserId + "/measurementHistory/measurmentHistory";

        FirebaseManager.Instance.databaseReference.Child(path).Child(index.ToString()).SetRawJsonValueAsync(json).ContinueWith(task =>
        {
            if (task.IsCompleted)
            {
                Debug.Log("Workout template added.");
            }
            else
            {
                Debug.LogError("Failed to add workout template: " + task.Exception);
            }
        });
    }
   
    public void SetMeasurementHistory(MeasurementHistoryItem item)
    {
        measurementHistory.measurmentHistory.Add(item);
    }
    public MeasurementModel getMeasurementData()
    {
        return measurementData;
    }
    public MeasurementHistory getMeasurementHistory()
    {
        return measurementHistory;
    }
    
    public void LoadHistory()
    {
        FirebaseManager.Instance.CheckIfLocationExists("/users/" + FirebaseManager.Instance.user.UserId + "/workoutHistory", result =>
        {
            if (result)
            {
                FirebaseManager.Instance.GetDataFromFirebase("/users/" + FirebaseManager.Instance.user.UserId + "/workoutHistory", _data =>
                {
                    if (_data != null)
                    {
                        string jsonData = _data.GetRawJsonValue();
                        historyData = (HistoryModel)LoadData(jsonData, typeof(HistoryModel));
                        print("from firebase");
                    }
                });

                Debug.Log("Data exists at the path.");
            }

        });
    }
    public void SaveHistory(HistoryTempleteModel item,int index)
    {
        string json = JsonUtility.ToJson(item);
        var path = "/users/" + FirebaseManager.Instance.user.UserId + "/workoutHistory/exerciseTempleteModel";

        FirebaseManager.Instance.databaseReference.Child(path).Child(index.ToString()).SetRawJsonValueAsync(json).ContinueWith(task =>
        {
            if (task.IsCompleted)
            {
                Debug.Log("Workout template added.");
            }
            else
            {
                Debug.LogError("Failed to add workout template: " + task.Exception);
            }
        });


    }
    public void LoadNotesHistory()
    {
        FirebaseManager.Instance.CheckIfLocationExists("/users/" + FirebaseManager.Instance.user.UserId + "/exerciseNotes", result =>
        {
            if (result)
            {
                FirebaseManager.Instance.GetDataFromFirebase("/users/" + FirebaseManager.Instance.user.UserId + "/exerciseNotes", _data =>
                {
                    if (_data != null)
                    {
                        string jsonData = _data.GetRawJsonValue();
                        notesHistory = (ExerciseNotesHistory)LoadData(jsonData, typeof(ExerciseNotesHistory));
                        print("from firebase");
                    }
                });

                Debug.Log("Data exists at the path.");
            }

        });
    }
    public void SaveNotesHistory(ExerciseNotesHistoryItem item, int index)
    {
        string json = JsonUtility.ToJson(item);
        var path = "/users/" + FirebaseManager.Instance.user.UserId + "/exerciseNotes/exercises";

        FirebaseManager.Instance.databaseReference.Child(path).Child(index.ToString()).SetRawJsonValueAsync(json).ContinueWith(task =>
        {
            if (task.IsCompleted)
            {
                Debug.Log("notes history added.");
            }
            else
            {
                Debug.LogError("Failed to add workout template: " + task.Exception);
            }
        });
    }
    public ExerciseNotesHistory getNotesHistory()
    {
        if (notesHistory == null)
        {
            notesHistory = new ExerciseNotesHistory();
        }
        return notesHistory;
    }
    
    //public void LoadAchievements()
    //{
    //    FirebaseManager.Instance.CheckIfLocationExists("/users/" + FirebaseManager.Instance.user.UserId + "/achievements", result =>
    //    {
    //        if (result)
    //        {
    //            FirebaseManager.Instance.GetDataFromFirebase("/users/" + FirebaseManager.Instance.user.UserId + "/achievements", _data =>
    //            {
    //                if (_data != null)
    //                {
    //                    string jsonData = _data.GetRawJsonValue();
    //                    achievementData = (AchievementData)LoadData(jsonData, typeof(AchievementData));
    //                    print("from firebase");
    //                }
    //            });

    //            Debug.Log("Data exists at the path.");
    //        }
    //        else
    //        {
    //            TextAsset achievementJsonFile = Resources.Load<TextAsset>("data/achievement");
    //            string achievementJson = achievementJsonFile.text;
    //            this.achievementData = JsonUtility.FromJson<AchievementData>(achievementJson);

    //            FirebaseManager.Instance.databaseReference.Child("users").Child(FirebaseManager.Instance.user.UserId).Child("achievements")
    //            .SetRawJsonValueAsync(achievementJson).ContinueWith(task =>
    //            {
    //                if (task.IsCompleted)
    //                {
    //                    Debug.Log("exercise added.");
    //                }
    //                else
    //                {
    //                    Debug.LogError("Failed to add exercises: " + task.Exception);
    //                }
    //            });
    //        }

    //    });
    //}
    public void LoadAchievements()
    {
        TextAsset achievementJsonFile = Resources.Load<TextAsset>("data/achievement");
        string achievementJson = achievementJsonFile.text;
        this.achievementData = JsonUtility.FromJson<AchievementData>(achievementJson);
    }
    public void CheckCompletedAchievements(DataSnapshot completedData,AchievementData achievements)
    {
        foreach (var achievementSnapshot in completedData.Children)
        {
            string achievementId = (string)achievementSnapshot.Key;  // Get the achievementId
            print(achievementId);
            AchievementTemplate achievementTemplate = GetAchievementTemplate(achievementId, achievements);  // Retrieve the template locally

            if (achievementTemplate != null)
            {
                print("not null");
                foreach (var dataItem in achievementTemplate.achievementData)
                {
                    print("for");
                    if (achievementSnapshot.Child("completedItemIds").HasChild(dataItem.id))
                    {
                        dataItem.isCompleted = true;  // Mark as completed
                        print(true);
                    }
                }
            }
        }

    }
    AchievementTemplate GetAchievementTemplate(string achievementId,AchievementData data)
    {
        // Assume 'achievementData' is an instance of AchievementData that contains all the achievement templates
        foreach (AchievementTemplate template in data.achievements)
        {
            if (template.id == achievementId)
            {
                return template;
            }
        }
        // Return null if no matching achievement is found
        return null;
    }

    public void SaveUserAchievementData(string achievementId, string completedItemId)
    {
        // Get a reference to the user's achievement data
        DatabaseReference reference = FirebaseDatabase.DefaultInstance.RootReference
            .Child("users")
            .Child(FirebaseManager.Instance.user.UserId)
            .Child("achievements")
            .Child(achievementId)
            .Child("completedItemIds");

        // Save the completed item ID as a key with 'true' as the value
        reference.Child(completedItemId).SetValueAsync(true).ContinueWith(task =>
        {
            if (task.IsCompleted)
            {
                Debug.Log($"Achievement item {completedItemId} marked as completed in achievement {achievementId}");
            }
            else if (task.IsFaulted)
            {
                Debug.LogError($"Failed to save completed item {completedItemId}: {task.Exception}");
            }
        });
    }

    public void SaveAchievementData()
    {
        string json = JsonUtility.ToJson(achievementData);
        FirebaseManager.Instance.databaseReference.Child("users").Child(FirebaseManager.Instance.user.UserId).Child("achievements")
               .SetRawJsonValueAsync(json).ContinueWith(task =>
               {
                   if (task.IsCompleted)
                   {
                       Debug.Log("exercise added.");
                   }
                   else
                   {
                       Debug.LogError("Failed to add exercises: " + task.Exception);
                   }
               });
    }
    public void LoadPersonalBest()
    {
        FirebaseManager.Instance.CheckIfLocationExists("/users/" + FirebaseManager.Instance.user.UserId + "/personalBest", result =>
        {
            if (result)
            {
                FirebaseManager.Instance.GetDataFromFirebase("/users/" + FirebaseManager.Instance.user.UserId + "/personalBest", _data =>
                {
                    if (_data != null)
                    {
                        string jsonData = _data.GetRawJsonValue();
                        personalBestData = (PersonalBestData)LoadData(jsonData, typeof(PersonalBestData));
                        print("from firebase");
                    }
                });

                Debug.Log("Data exists at the path.");
            }
            else
            {
                TextAsset achievementJsonFile = Resources.Load<TextAsset>("data/personalBest");
                string achievementJson = achievementJsonFile.text;
                this.personalBestData = JsonUtility.FromJson<PersonalBestData>(achievementJson);

                FirebaseManager.Instance.databaseReference.Child("users").Child(FirebaseManager.Instance.user.UserId).Child("personalBest")
                .SetRawJsonValueAsync(achievementJson).ContinueWith(task =>
                {
                    if (task.IsCompleted)
                    {
                        Debug.Log("exercise added.");
                    }
                    else
                    {
                        Debug.LogError("Failed to add exercises: " + task.Exception);
                    }
                });
            }

        });
    }

    public void SavePersonalBestData()
    {
        string json = JsonUtility.ToJson(personalBestData);
        var path = "/users/" + FirebaseManager.Instance.user.UserId + "/personalBest";

        FirebaseManager.Instance.databaseReference.Child(path).SetRawJsonValueAsync(json).ContinueWith(task =>
        {
            if (task.IsCompleted)
            {
                Debug.Log("Workout template added.");
            }
            else
            {
                Debug.LogError("Failed to add workout template: " + task.Exception);
            }
        });
    }

    public void LoadShopData()
    {
        TextAsset shopJsonFile = Resources.Load<TextAsset>("data/shopData");
        string shopJson = shopJsonFile.text;
        this.shopData = JsonUtility.FromJson<ShopModel>(shopJson);

        //FirebaseManager.Instance.CheckIfLocationExists("/users/" + FirebaseManager.Instance.user.UserId + "/shop", result =>
        //{
        //    if (result)
        //    {
        //        FirebaseManager.Instance.GetDataFromFirebase("/users/" + FirebaseManager.Instance.user.UserId+"/shop", _data =>
        //        {
        //            if (_data != null)
        //            {
        //                string jsonData = _data.GetRawJsonValue();
        //                shopData = (ShopModel)LoadData(jsonData, typeof(ShopModel));
        //                print("from firebase");
        //            }
        //        });

        //        Debug.Log("Data exists at the path.");
        //    }
        //    else
        //    {
        //        TextAsset shopJsonFile = Resources.Load<TextAsset>("data/shopData");
        //        string shopJson = shopJsonFile.text;
        //        this.shopData = JsonUtility.FromJson<ShopModel>(shopJson);
        //        SetShopDataToFirebase(shopJson);
        //    }
        //});
    }
    public void SetShopDataToFirebase(string shopJson)
    {
        FirebaseManager.Instance.databaseReference.Child("users").Child(FirebaseManager.Instance.user.UserId).Child("shop")
               .SetRawJsonValueAsync(shopJson).ContinueWith(task =>
               {
                   if (task.IsCompleted)
                   {
                       Debug.Log("shop added.");
                   }
                   else
                   {
                       Debug.LogError("Failed to add shop: " + task.Exception);
                   }
               });
    }
    public void CheckPurchaseItems(DataSnapshot purchaseData, ShopModel shopData)
    {
        foreach (var item in purchaseData.Children)
        {
            string itemID = (string)item.Key;
            ShopItem shopItem = GetItemFromShop(itemID, shopData);

            if (shopItem != null)
            {
                print("not null");
                shopItem.buyed = true;
            }
        }

    }
    ShopItem GetItemFromShop(string itemID, ShopModel shopData)
    {
        foreach (ShopItem item in shopData.items)
        {
            if (item.id == itemID)
            {
                return item;
            }
        }
        return null;
    }
    public void SaveUserPurchaseData(string itemID)
    {
        // Get a reference to the user's achievement data
        DatabaseReference reference = FirebaseDatabase.DefaultInstance.RootReference
            .Child("users")
            .Child(FirebaseManager.Instance.user.UserId)
            .Child("shop");

        // Save the completed item ID as a key with 'true' as the value
        reference.Child(itemID).SetValueAsync(true).ContinueWith(task => {
            if (task.IsCompleted)
            {
                Debug.Log($"shop item {itemID} marked as purchase");
            }
            else if (task.IsFaulted)
            {
                Debug.LogError($"Failed to save purchase item {itemID}: {task.Exception}");
            }
        });
    }
    public void LoadDataFromFirebase()
    {
        LoadAchievements();
        LoadShopData();
        if (isSignUp)
        {
            LoadHistory();
            LoadTemplateData();
            LoadExercises();
            LoadMeasurementData();
            LoadMeasurementHistory();
            LoadNotesHistory();
            
            LoadPersonalBest();
            LoadCoins();
            LoadUserStreak();
            LoadCharacterLevel();
            LoadCurrentWeekStartDateFromFirebase();
            //LoadBadgeName();
            LoadFriendData();
            SetClotheOnFirebase("no clothes");
            SetWeeklyGoalStatusOnFirebase(false);
            LoadGymVisitsFromFirebase();
        }
        else
            LoadCompleteData();

        
        gameTheme = LoadTheme();
    }
    public void LoadCompleteData()
    {
        FirebaseManager.Instance.GetDataFromFirebase("/users/" + FirebaseManager.Instance.user.UserId, data =>
        {
            if (data != null)
            {
                print("retriving data");
                // workoutHistory
                string workoutHistory = data.Child("workoutHistory").GetRawJsonValue();
                historyData = (HistoryModel)LoadData(workoutHistory, typeof(HistoryModel));
                // workoutTempletes
                string workoutTempletes = data.Child("workoutTempletes").GetRawJsonValue();
                templateData = (TemplateData)LoadData(workoutTempletes, typeof(TemplateData));
                // exercise
                string exercise = data.Child("exercises").GetRawJsonValue();
                exerciseData = (ExerciseData)LoadData(exercise, typeof(ExerciseData));
                SyncExercise();
                // measurements
                string measurements = data.Child("measurements").GetRawJsonValue();
                measurementData = (MeasurementModel)LoadData(measurements, typeof(MeasurementModel));
                // measurementHistory
                string _measurementHistory = data.Child("measurementHistory").GetRawJsonValue();
                measurementHistory = (MeasurementHistory)LoadData(_measurementHistory, typeof(MeasurementHistory));
                // exerciseNotes
                string exerciseNotes = data.Child("exerciseNotes").GetRawJsonValue();
                notesHistory = (ExerciseNotesHistory)LoadData(exerciseNotes, typeof(ExerciseNotesHistory));
                // achievements
                if (data.HasChild("achievements"))
                {
                    print("has achievement");
                    CheckCompletedAchievements(data.Child("achievements"), achievementData);
                }
                //shop

                if (data.HasChild("shop"))
                {
                    CheckPurchaseItems(data.Child("shop"), shopData);
                }
                //profile image link
                if (data.HasChild("profileImageUrl"))
                {
                    string profileImageUrl = data.Child("profileImageUrl").Value.ToString();
                    userSessionManager.Instance.profileImageUrl = profileImageUrl;
                    print(profileImageUrl);
                    StartCoroutine(LoadImageFromUrl(profileImageUrl, (loadedSprite) =>
                    {
                        // This callback will receive the newly loaded sprite
                        userSessionManager.Instance.profileSprite = loadedSprite;  // Assuming profileImage is your UI Image component
                    }));
                    //StartCoroutine(LoadImageFromUrl(profileImageUrl,userSessionManager.Instance.profileSprite));
                }
                // personalBest
                string personalBest = data.Child("personalBest").GetRawJsonValue();
                personalBestData = (PersonalBestData)LoadData(personalBest, typeof(PersonalBestData));
                //coins
                string coin = data.Child("coins").Value.ToString();
                userSessionManager.Instance.currentCoins = int.Parse(coin);
                //streak
                string streak = data.Child("streak").Value.ToString();
                userSessionManager.Instance.userStreak = int.Parse(streak);
                // CharacterLevel
                string level = data.Child("CharacterLevel").Value.ToString();
                userSessionManager.Instance.characterLevel = int.Parse(level);
                // CurrentWeekStartDate
                string startDateString = data.Child("CurrentWeekStartDate").Value.ToString();
                StreakAndCharacterManager.Instance.startOfCurrentWeek = DateTime.Parse(startDateString);
                // BadgeName
                string badgeName = data.Child("BadgeName").Value.ToString();
                userSessionManager.Instance.badgeName = badgeName;
                //Clothe
                if (data.HasChild("clothes"))
                {
                    userSessionManager.Instance.clotheName = data.Child("clothes").Value.ToString();
                }
                else
                {
                    SetClotheOnFirebase("no clothes");
                }
                // added friend
                if (data.HasChild("addedFriends"))
                {
                    string addedFriends = data.Child("addedFriends").Value.ToString();
                    userSessionManager.Instance.addedFriends = int.Parse(addedFriends);
                }
                // removed friend
                if (data.HasChild("removedFriends"))
                {
                    string addedFriends = data.Child("removedFriends").Value.ToString();
                    userSessionManager.Instance.removedFriends = int.Parse(addedFriends);
                }
                // weekly goal status
                if (data.HasChild("WeeklyGoalStatus"))
                {
                    string value = data.Child("WeeklyGoalStatus").Value.ToString();
                    bool staus = bool.Parse(value);
                    StreakAndCharacterManager.Instance.weeklyGoalStatus = staus;
                }
                // friends
                DataSnapshot friendData = data.Child("friend");
                foreach (var child in friendData.Children)
                {
                    friendsData.Add(child.Key, child.Value.ToString());
                    StartCoroutine(FetchFriendDetails(child.Value.ToString(), child.Key));
                }
                // GymVisits
                if (data.HasChild("GymVisits"))
                {
                    DataSnapshot visits = data.Child("GymVisits");
                    foreach (var child in visits.Children)
                    {
                        StreakAndCharacterManager.Instance.gymVisits.Add(child.Value.ToString());
                    }
                }
                // WeeklyGoalSetDate
                if (data.HasChild("WeeklyGoalSetDate"))
                {
                    string dateString = data.Child("WeeklyGoalSetDate").Value.ToString();
                    userSessionManager.Instance.weeklyGoalSetDate = DateTime.ParseExact(dateString, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);
                }
            }
        });
    }

    public void SyncExercise()
    {
        try{
            TextAsset exerciseJsonFile = Resources.Load<TextAsset>("data/exercise");
            string exerciseJson = exerciseJsonFile.text;
            ExerciseData localExercises = JsonUtility.FromJson<ExerciseData>(exerciseJson);

            foreach (var exercise in localExercises.exercises)
            {
                if (!this.exerciseData.exercises.Any(e => e.exerciseName == exercise.exerciseName))
                {
                    this.exerciseData.exercises.Add(exercise);
                }
            }
        }catch (Exception ex){
            print("asd");
        }
    }
    private IEnumerator FetchFriendDetails(string friendId, string friendName)
    {
        FriendData friendDetails = new FriendData();

        // Start Firebase request to get friend details
        var dataTask = FirebaseDatabase.DefaultInstance.RootReference.Child("users").Child(friendId).GetValueAsync();

        // Yield until the task completes
        yield return new WaitUntil(() => dataTask.IsCompleted);

        // Once the task completes, process the result
        if (dataTask.IsCompleted && dataTask.Result != null)
        {
            DataSnapshot snapshot = dataTask.Result;

            friendDetails.userName = friendName;

            string level = snapshot.Child("CharacterLevel").Value.ToString();
            friendDetails.level = int.Parse(level);

            friendDetails.badgeName = snapshot.Child("BadgeName").Value.ToString();

            if (snapshot.HasChild("clothes"))
            {
                friendDetails.clothe = snapshot.Child("clothes").Value.ToString();
            }
            else { friendDetails.clothe = "no clothes"; }

            string streak = snapshot.Child("streak").Value.ToString();
            friendDetails.streak = int.Parse(streak);

            string goal = snapshot.Child("weeklyGoal").Value.ToString();
            friendDetails.goal = int.Parse(goal);

            friendDetails.joiningDate = snapshot.Child("joiningDate").Value.ToString();

            TextAsset achievementJsonFile = Resources.Load<TextAsset>("data/achievement");
            string achievementJson = achievementJsonFile.text;
            friendDetails.achievementData = JsonUtility.FromJson<AchievementData>(achievementJson);
            ApiDataHandler.Instance.CheckCompletedAchievements(snapshot.Child("achievements"), friendDetails.achievementData);

            string personalBest = snapshot.Child("personalBest").GetRawJsonValue();
            friendDetails.personalBestData = (PersonalBestData)LoadData(personalBest, typeof(PersonalBestData));

            if (snapshot.HasChild("profileImageUrl"))
            {
                friendDetails.profileImageUrl= snapshot.Child("profileImageUrl").Value.ToString();
                StartCoroutine(LoadImageFromUrl(friendDetails.profileImageUrl, (loadedSprite) => {
                    // This callback will receive the newly loaded sprite
                    friendDetails.profileImage = loadedSprite;
                }));
            }

            friendDataModel.friendData.Add(friendDetails);
        }
        else
        {
            Debug.LogWarning("Failed to fetch data for friend: " + friendName);
        }
    }
    public FriendModel getAllFriendDetails()
    {
        if (this.friendDataModel == null)
            this.friendDataModel = new FriendModel();
        return this.friendDataModel;
    }
    public ExerciseData getExerciseData()
    {
        return this.exerciseData;
    }
    public AchievementData getAchievementData()
    {
        return this.achievementData;
    }
    public PersonalBestData getPersonalBestData()
    {
        return this.personalBestData;
    }
    public HistoryModel getHistoryData()
    {
        if (this.historyData == null)
            return this.historyData = new HistoryModel();
        return this.historyData;
    }
    public TemplateData getTemplateData()
    {
        if(this.templateData == null)
            this.templateData = new TemplateData();

        return this.templateData;
    }
   public ShopModel getShopData()
    {
        return shopData;
    }
    
   
    public void RemovePersonalBestData(PersonalBestDataItem item)
    {
        personalBestData.exercises.Remove(item);
    }
    public void SetPersonalBestData(PersonalBestDataItem item)
    {
        personalBestData.exercises.Add(item);
    }
    public void SetJoiningDate(DateTime date)
    {
        string dateInString = date.ToString("MMM / yyyy");
        FirebaseManager.Instance.databaseReference.Child("users").Child(FirebaseManager.Instance.user.UserId).Child("joiningDate")
            .SetValueAsync(dateInString).ContinueWith(task =>
            {
                if (task.IsCompleted)
                {
                    userSessionManager.Instance.joiningDate = dateInString;
                    Debug.Log("joining date seted");
                }
                else
                {
                    Debug.LogError("Failed to save weekly goal: " + task.Exception);
                }
            });
    }
    public void SetWeeklyGoal(int goal)
    {
        FirebaseManager.Instance.databaseReference.Child("users").Child(FirebaseManager.Instance.user.UserId).Child("weeklyGoal")
            .SetValueAsync(goal).ContinueWith(task =>
            {
                if (task.IsCompleted)
                {
                    userSessionManager.Instance.weeklyGoal = goal;
                    Debug.Log("weekly goal seted");
                }
                else
                {
                    Debug.LogError("Failed to save weekly goal: " + task.Exception);
                }
            });
    }



    //----------------------------------------------------------------------------------------------------------------------------------------------------------

    public void LoadCoins()
    {
        FirebaseManager.Instance.CheckIfLocationExists("/users/" + FirebaseManager.Instance.user.UserId + "/coins", result =>
        {
            //print(result);
            if (result)
            {
                //print("if");
                FirebaseManager.Instance.GetDataFromFirebase("/users/" + FirebaseManager.Instance.user.UserId + "/coins", data =>
                {
                    if (data.Exists)  // Ensure that data exists
                    {
                        string coin = data.Value.ToString();  // Directly get the value as string
                        userSessionManager.Instance.currentCoins = int.Parse(coin);
                        Debug.Log("Coins retrieved: " + coin);
                    }
                });
            }
            else
            {
                SetCoinsToFirebase(0);
            }
        });
    }
    public void SetCoinsToFirebase(int coin, Action<bool> onSave=null)
    {
        FirebaseManager.Instance.databaseReference.Child("users").Child(FirebaseManager.Instance.user.UserId).Child("coins")
                .SetValueAsync(coin).ContinueWith(task =>
                {
                    if (task.IsCompleted)
                    {
                        //userSessionManager.Instance.currentCoins = coin;
                        onSave?.Invoke(true);
                        Debug.Log("coin seted on firebase: " + coin);
                    }
                    else
                    {
                        onSave?.Invoke(false);
                        Debug.LogError("Failed to save coin: " + task.Exception);
                    }
                });
    }
    public void LoadUserStreak()
    {
        FirebaseManager.Instance.CheckIfLocationExists("/users/" + FirebaseManager.Instance.user.UserId + "/streak", result =>
        {
            print(result);
            if (result)
            {
                print("if");
                FirebaseManager.Instance.GetDataFromFirebase("/users/" + FirebaseManager.Instance.user.UserId + "/streak", data =>
                {
                    if (data.Exists)  // Ensure that data exists
                    {
                        string streak = data.Value.ToString();  // Directly get the value as string
                        userSessionManager.Instance.userStreak = int.Parse(streak); ;
                        print("Streak retrieved: " + streak);
                    }
                });
            }
            else
            {
                SetUserStreakToFirebase(0);
            }
        });
    }
    public void SetUserStreakToFirebase(int streak)
    {
        FirebaseManager.Instance.databaseReference.Child("users").Child(FirebaseManager.Instance.user.UserId).Child("streak")
                .SetValueAsync(streak).ContinueWith(task =>
                {
                    if (task.IsCompleted)
                    {
                        userSessionManager.Instance.userStreak = streak;
                        Debug.Log("streak seted: " + streak);
                    }
                    else
                    {
                        Debug.LogError("Failed to save coin: " + task.Exception);
                    }
                });
    }
    public void LoadCharacterLevel()
    {
        print("load character");
        FirebaseManager.Instance.CheckIfLocationExists("/users/" + FirebaseManager.Instance.user.UserId + "/CharacterLevel", result =>
        {
            //print(result);
            if (result)
            {
                //print("if");
                FirebaseManager.Instance.GetDataFromFirebase("/users/" + FirebaseManager.Instance.user.UserId + "/CharacterLevel", data =>
                {
                    if (data.Exists)  // Ensure that data exists
                    {
                        string level = data.Value.ToString();  // Directly get the value as string
                        userSessionManager.Instance.characterLevel = int.Parse(level); ;
                        print("level retrieved: " + level);
                    }
                });
            }
            else
            {
                SetCharacterLevelToFirebase(0);
            }
        });
    }
    public void SetCharacterLevelToFirebase(int level)
    {
        print("---------------------------------------");
        FirebaseManager.Instance.databaseReference.Child("users").Child(FirebaseManager.Instance.user.UserId).Child("CharacterLevel")
                .SetValueAsync(level).ContinueWith(task =>
                {
                    if (task.IsCompleted)
                    {
                        userSessionManager.Instance.characterLevel = level;
                        Debug.Log("level seted: " + level);
                    }
                    else
                    {
                        Debug.LogError("Failed to save level: " + task.Exception);
                    }
                });
    }
    public void LoadBadgeName()
    {
        print("load badge");
        FirebaseManager.Instance.CheckIfLocationExists("/users/" + FirebaseManager.Instance.user.UserId + "/BadgeName", result =>
        {
            print(result);
            if (result)
            {
                print("if");
                FirebaseManager.Instance.GetDataFromFirebase("/users/" + FirebaseManager.Instance.user.UserId + "/BadgeName", data =>
                {
                    if (data.Exists)  // Ensure that data exists
                    {
                        string badgeName = data.Value.ToString();  // Directly get the value as string
                        userSessionManager.Instance.badgeName = badgeName;
                        print("level retrieved: " + badgeName);
                    }
                });
            }
            else
            {
                SetBadgeNameToFirebase("TheGorillaBadge");
            }
        });
    }
    public void SetBadgeNameToFirebase(string name )
    {
        FirebaseManager.Instance.databaseReference.Child("users").Child(FirebaseManager.Instance.user.UserId).Child("BadgeName")
                .SetValueAsync(name).ContinueWith(task =>
                {
                    if (task.IsCompleted)
                    {
                        userSessionManager.Instance.badgeName = name;
                        Debug.Log("badge name seted: " + name);
                    }
                    else
                    {
                        Debug.LogError("Failed to save badge name: " + task.Exception);
                    }
                });
    }


    //public void SetCharacterLevel(int level)
    //{
    //    PreferenceManager.Instance.SetInt("CharacterLevel", level);
    //}
    //public int GetCharacterLevel()
    //{
    //    return PreferenceManager.Instance.GetInt("CharacterLevel", 0);
    //}


    public void SetWeightUnit(int unit)
    {
        PreferenceManager.Instance.SetInt("WeightUnit", unit);
    }
    public int GetWeightUnit()
    {
        return PreferenceManager.Instance.GetInt("WeightUnit", 1);
    }

    public void SetCreatedWorkoutTempleteCount(int count)
    {
        PreferenceManager.Instance.SetInt("CreatedWorkoutTempleteCount", count);
    }
    public int GetCreatedWorkoutTempleteCount()
    {
        return PreferenceManager.Instance.GetInt("CreatedWorkoutTempleteCount", 0);
    }
    public void SetRemoveFriendCount(int count)
    {
        PreferenceManager.Instance.SetInt("RemoveFriendCount", count);
    }
    public int GetRemoveFriendCount()
    {
        return PreferenceManager.Instance.GetInt("RemoveFriendCount", 0);
    }
    
    public void SetAddFriendCount(int count)
    {
        PreferenceManager.Instance.SetInt("AddFriendCount", count);
    }
    public int GetAddFriendCount()
    {
        return PreferenceManager.Instance.GetInt("AddFriendCount", 0);
    }
    public Dictionary<string,string> GetFriendsData()
    {
        return friendsData;
    }



    public void AddItemToHistoryData(HistoryTempleteModel item)
    {
        historyData.exerciseTempleteModel.Add(item);
    }
    public void AddItemToTemplateData(DefaultTempleteModel item)
    {
        templateData.exerciseTemplete.Add(item);
    }


    //-------------------------------------------------------------------------------------------------------------------------------------------------------------


    //public void SetCurrentWeekStartDate(DateTime startDate)
    //{
    //    string startDateString = startDate.ToString("yyyy-MM-dd");
    //    PlayerPrefs.SetString("CurrentWeekStartDate", startDateString);
    //    PlayerPrefs.Save();
    //}
    public void LoadCurrentWeekStartDateFromFirebase()
    {
        FirebaseManager.Instance.CheckIfLocationExists("/users/" + FirebaseManager.Instance.user.UserId + "/CurrentWeekStartDate", result =>
        {
            print(result);
            if (result)
            {
                print("if");
                FirebaseManager.Instance.GetDataFromFirebase("/users/" + FirebaseManager.Instance.user.UserId + "/CurrentWeekStartDate", data =>
                {
                    if (data.Exists)  // Ensure that data exists
                    {
                        string startDateString = data.Value.ToString();  // Directly get the value as string
                        StreakAndCharacterManager.Instance.startOfCurrentWeek = DateTime.Parse(startDateString);
                        print("level retrieved: " + startDateString);
                    }
                });
            }
        });

    }
    public void SetCurrentWeekStartDate(DateTime startDate)
    {
        string startDateString = startDate.ToString("yyyy-MM-dd");
        string userId = FirebaseManager.Instance.user.UserId;

        FirebaseManager.Instance.databaseReference.Child("users").Child(userId).Child("CurrentWeekStartDate")
            .SetValueAsync(startDateString).ContinueWith(task =>
            {
                if (task.IsCompleted)
                {
                    StreakAndCharacterManager.Instance.startOfCurrentWeek = startDate;
                    Debug.Log("CurrentWeekStartDate saved to Firebase: " + startDateString);
                }
                else
                {
                    Debug.LogError("Failed to save CurrentWeekStartDate: " + task.Exception);
                }
            });
    }
    public void SetWeeklyGoalStatusOnFirebase(bool status)
    {
        string userId = FirebaseManager.Instance.user.UserId;
        FirebaseManager.Instance.databaseReference.Child("users").Child(userId).Child("WeeklyGoalStatus")
            .SetValueAsync(status).ContinueWith(task =>
            {
                if (task.IsCompleted)
                {
                    StreakAndCharacterManager.Instance.weeklyGoalStatus = status;
                    Debug.Log("WeeklyGoalStatus saved to Firebase: " + status);
                }
                else
                {
                    Debug.LogError("Failed to save WeeklyGoalStatus: " + task.Exception);
                }
            });
    }
    public void WeeklyGoalSetDate(DateTime date)
    {
        string dateString = date.ToString("yyyy-MM-dd");
        string userId = FirebaseManager.Instance.user.UserId;
        FirebaseManager.Instance.databaseReference.Child("users").Child(userId).Child("WeeklyGoalSetDate")
            .SetValueAsync(dateString).ContinueWith(task =>
            {
                if (task.IsCompleted)
                {
                    userSessionManager.Instance.weeklyGoalSetDate = date;
                    Debug.Log("WeeklyGoalStatus saved to Firebase: " + date);
                }
                else
                {
                    Debug.LogError("Failed to save WeeklyGoalStatus: " + task.Exception);
                }
            });
    }
    public DateTime GetStartOfCurrentWeek()
    {
        return DateTime.Now;
        //DateTime today = DateTime.Now;
        //int daysSinceMonday = (int)today.DayOfWeek - (int)DayOfWeek.Monday;
        //if (daysSinceMonday < 0) daysSinceMonday += 7;
        //return today.AddDays(-daysSinceMonday);
    }

    public void LoadGymVisitsFromFirebase()
    {
        string userId = FirebaseManager.Instance.user.UserId;
        FirebaseManager.Instance.databaseReference.Child("users").Child(userId).Child("GymVisits")
            .GetValueAsync().ContinueWith(task =>
            {
                if (task.IsCompleted && task.Result.Exists)
                {
                    List<string> visits = new List<string>();
                    foreach (var visit in task.Result.Children)
                    {
                        visits.Add(visit.Value.ToString());
                    }
                    StreakAndCharacterManager.Instance.gymVisits = visits;
                    Debug.Log("Gym visits loaded from Firebase.");
                    //onLoaded(visits);
                }
                else
                {
                    Debug.LogError("Failed to load gym visits: " + task.Exception);
                    //onLoaded(new List<string>());
                }
            });
    }
    public void SetGymVisitsToFirebase(List<string> visits)
    {
        string userId = FirebaseManager.Instance.user.UserId;
        FirebaseManager.Instance.databaseReference.Child("users").Child(userId).Child("GymVisits")
            .SetValueAsync(visits).ContinueWith(task =>
            {
                if (task.IsCompleted)
                {
                    Debug.Log("Gym visits saved to Firebase.");
                }
                else
                {
                    Debug.LogError("Failed to save gym visits: " + task.Exception);
                }
            });
    }

    public void SetClotheOnFirebase(string name)
    {
        string userId = FirebaseManager.Instance.user.UserId;
        FirebaseManager.Instance.databaseReference.Child("users").Child(userId).Child("clothes")
            .SetValueAsync(name).ContinueWith(task =>
            {
                if (task.IsCompleted)
                {
                    userSessionManager.Instance.clotheName = name;
                }
                else
                {
                    Debug.LogError("Failed to save clothe: " + task.Exception);
                }
            });
    }
    public void SetCloths(string name)
    {
        print("set cloths");
        PreferenceManager.Instance.SetString("clothes", name);
    }
    public string GetClothes()
    {
        return PreferenceManager.Instance.GetString("clothes", "no clothes");
    }
    //------------------------------------------------------------------------------------------------------------------------------------------------------------

    public IEnumerator LoadImageFromUrl(string imageUrl, Action<Sprite> onImageLoaded)
    {
        print("loading start");
        UnityWebRequest www = UnityWebRequestTexture.GetTexture(imageUrl);
        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.Success)
        {
            Texture2D texture = ((DownloadHandlerTexture)www.downloadHandler).texture;
            Sprite newSprite = Sprite.Create(texture, new Rect(0.0f, 0.0f, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            print("image loaded");
            onImageLoaded?.Invoke(newSprite);
        }
        else
        {
            print("not load image");
            Debug.LogError("Failed to load image from URL");
        }
        print("complete loading");
    }

    public IEnumerator CheckFriendSpriteDownloaded(FriendData data, Image image, GameObject loading)
    {
        while (data.profileImage == null)
        {
            loading.SetActive(true);
            yield return new WaitForSeconds(3);
        }
        loading.SetActive(false);
        image.sprite = data.profileImage;
        RectTransform mask = image.transform.parent.GetComponent<RectTransform>();
        userSessionManager.Instance.FitImage(image, mask);
    }

    public int GetBuyedCloths()
    {
        return shopData.items.Count(item => item.buyed);
    }

    public int GetCompletedAchievements(AchievementData data)
    {
        int completedCount = 0;
        foreach (AchievementTemplate achievement in data.achievements)
        {
            bool isAchievementCompleted = achievement.achievementData.All(item => item.isCompleted);
            if (isAchievementCompleted)
            {
                completedCount++;
            }
        }

        return completedCount;
    }
    public int GetTotalAchievements()
    {
        return achievementData.achievements.Count;
    }
    public (int, int) GetRankedCompletedAchievements()
    {
        int totalCount = 0;
        int completedCount = 0;
        foreach (AchievementTemplate achievement in achievementData.achievements)
        {
            if (achievement.achievementData.Count > 1)
            {
                bool isAchievementCompleted = achievement.achievementData.All(item => item.isCompleted);
                if (isAchievementCompleted)
                {
                    completedCount++;
                }
                totalCount++;
            }
        }

        return (completedCount, totalCount);
    }
    public (int, int) GetMilestoneCompletedAchievements()
    {
        int totalCount = 0;
        int completedCount = 0;
        foreach (AchievementTemplate achievement in achievementData.achievements)
        {
            if (achievement.achievementData.Count == 1)
            {
                bool isAchievementCompleted = achievement.achievementData.All(item => item.isCompleted);
                if (isAchievementCompleted)
                {
                    completedCount++;
                }
                totalCount++;
            }
        }

        return (completedCount, totalCount);
    }
    public int GetTotalTrophys()
    {
        int count = 0;
        foreach (AchievementTemplate achievement in achievementData.achievements)
        {
            count = count + achievement.achievementData.Count;
        }

        return count;
    }
    public int GetCompletedTrophys()
    {
        int count = 0;
        foreach (AchievementTemplate achievement in achievementData.achievements)
        {
            foreach (AchievementTemplateDataItem item in achievement.achievementData)
            {
                if (item.isCompleted)
                {
                    count++;
                }
            }
        }

        return count;
    }
    public (int, int) GetRankedCompletedTrophys()
    {
        int total = 0;
        int count = 0;
        foreach (AchievementTemplate achievement in achievementData.achievements)
        {
            if (achievement.achievementData.Count > 1)
            {
                foreach (AchievementTemplateDataItem item in achievement.achievementData)
                {
                    if (item.isCompleted)
                    {
                        count++;
                    }
                    total++;
                }
            }
        }

        return (count, total);
    }
    public (int, int) GetMilestoneCompletedTrophys()
    {
        int total = 0;
        int count = 0;
        foreach (AchievementTemplate achievement in achievementData.achievements)
        {
            if (achievement.achievementData.Count == 1)
            {
                foreach (AchievementTemplateDataItem item in achievement.achievementData)
                {
                    if (item.isCompleted)
                    {
                        count++;
                    }
                    total++;
                }
            }
        }

        return (count, total);
    }


   

    
}
