using Firebase.Database;
using Firebase.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Windows;

public class UserNameController : MonoBehaviour, PageController
{
    public TMP_InputField userNameInput;
    public TextMeshProUGUI messageText;
    public Button nextButton;
    public Button backButton;

    public void onInit(Dictionary<string, object> data, Action<object> callback)
    {
        nextButton.onClick.AddListener(Next);
        StateManager.Instance.ShiftStep(AccountCreationStep.Username);

        backButton.onClick.AddListener(() => StateManager.Instance.Backer());

        /*
        if (!string.IsNullOrEmpty(userSessionManager.Instance.mProfileUsername))
        {
            userNameInput.text = userSessionManager.Instance.mProfileUsername;
        }*/
    }
    public void Next()
    {
        string stringWithoutSpaces = userNameInput.text.Replace(" ", "");
        int lengthWithoutSpaces = stringWithoutSpaces.Length;
        if (lengthWithoutSpaces < 4)
        {
            GlobalAnimator.Instance.ShowTextMessage(messageText, "Username: minimum 4 characters", 2);
        }
        else if (lengthWithoutSpaces > 17)
        {
            GlobalAnimator.Instance.ShowTextMessage(messageText, "Username: maximum 17 characters", 2);
        }
        else
            CheckAndStoreUsername(stringWithoutSpaces, userSessionManager.Instance.mProfileID);
    }
    public void CheckAndStoreUsername(string username,string userID)
    {
        StartCoroutine(CheckUsernameExists(username,userID));
    }

    private IEnumerator CheckUsernameExists(string username, string userID)
    {
        yield return null;
        var usernameRef = FirebaseDatabase.DefaultInstance.GetReference("usernames");

        // Check if the username exists in the database
        var checkUserTask = usernameRef.Child(username).GetValueAsync();

        yield return new WaitUntil(() => checkUserTask.IsCompleted);

        if (checkUserTask.Exception != null)
        {
            Debug.LogError("Error checking username: " + checkUserTask.Exception);
            yield break;
        }

        // If the username doesn't exist, it's unique
        if (!checkUserTask.Result.Exists)
        {
            // Store the username in the database
            StoreUsername(username, userID);
        }
        else
        {
            // Check if the username belongs to the current user
            if (checkUserTask.Result.Value.ToString() == userID)
            {
                // Allow the user to keep the same username
                StoreUsername(username, userID);
            }
            else
            {
                GlobalAnimator.Instance.ShowTextMessage(messageText, "Username is already taken", 2);
                Debug.Log("Username is already taken.");
            }
        }
    }

    // Function to store the username in the Firebase Database
    private void StoreUsername(string username, string userID)
    {
        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(userID))
        {
            Debug.LogError("Username or userID is empty.");
            return;
        }

        // Sanitize userID and username to ensure no invalid characters
        username = username.Replace(".", "_").Replace("#", "_");

        Debug.Log("Saving username under path: users/" + userID + "/username");
        Debug.Log("Saving username under path: usernames/" + username);

        // Store username under the user UID
        FirebaseManager.Instance.databaseReference.Child("users")
            .Child(userID)
            .Child("username").SetValueAsync(username)
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsCompleted)
                {
                    Debug.Log("Username saved successfully under user UID.");
                }
                else
                {
                    GlobalAnimator.Instance.ShowTextMessage(messageText, "Failed to store username", 2);
                    Debug.LogError("Error saving username under UID: " + task.Exception);
                }
            });

        // Store username under the "usernames" node to prevent duplicates
        var usernameRef = FirebaseDatabase.DefaultInstance.GetReference("usernames");
        usernameRef.Child(username).SetValueAsync(userID).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                StateManager.Instance.ShiftStep(AccountCreationStep.WeeklyGoal);

                StateManager.Instance.OpenStaticScreen("profile", gameObject, "weeklyGoalScreen", null);
                PreferenceManager.Instance.SetBool("FirstTimePlanInitialized_", false);
                Debug.Log("Username stored successfully!");
            }
            else
            {
                GlobalAnimator.Instance.ShowTextMessage(messageText, "Failed to store username", 2);
                Debug.LogError("Failed to store username: " + task.Exception);
            }
        });
        userSessionManager.Instance.mProfileUsername= username;
    }
}
