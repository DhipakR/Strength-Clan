using Firebase.Database;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class Badge
{
    public Sprite icon;
    public string title;
    public string description;
}

public class BadgeController : MonoBehaviour,PageController
{
    [SerializeField] private List<Badge> badges;
    [SerializeField] private Image badgeIcon;
    [SerializeField] private TMP_Text badgeTitle;
    [SerializeField] private TMP_Text badgeDescription;
    [SerializeField] private Button leftButton;
    [SerializeField] private Button rightButton;
    [SerializeField] private Button continuButton;
    [SerializeField] private Button backButton;
    private int currentIndex = 0;
    private Vector2 startTouchPosition;
    private Vector2 endTouchPosition;
    bool firstTime;
    public void onInit(Dictionary<string, object> data, Action<object> callback)
    {
        StateManager.Instance.ShiftStep(AccountCreationStep.Badge);

        try
        {
            firstTime = (bool)data["data"];
        }
        catch { }
        continuButton.onClick.AddListener(AudioController.Instance.OnButtonClick);
        continuButton.onClick.AddListener(Continu);
        if (firstTime)
        {
            backButton.onClick.AddListener(() => StateManager.Instance.Backer(gameObject));
        }
        else
        {
            backButton.onClick.AddListener(() => StateManager.Instance.HandleBackAction(gameObject));
        }        // leftButton.onClick.AddListener(() => ChangeBadge(-1));
        // rightButton.onClick.AddListener(() => ChangeBadge(1));

        UpdateBadgeDisplay();
    }
    private void UpdateBadgeDisplay()
    {
        badgeIcon.sprite = badges[currentIndex].icon;
        badgeTitle.text = badges[currentIndex].title;
        badgeDescription.text = badges[currentIndex].description;
    }
    private void ChangeBadge(int direction)
    {
        currentIndex += direction;
        if (currentIndex < 0)
            currentIndex = badges.Count - 1;
        else if (currentIndex >= badges.Count)
            currentIndex = 0;

        UpdateBadgeDisplay();
    }
    private void Update()
    {
        DetectSwipe();
    }
    private void DetectSwipe()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began)
                startTouchPosition = touch.position;
            else if (touch.phase == TouchPhase.Ended)
            {
                endTouchPosition = touch.position;
                float swipeDistance = endTouchPosition.x - startTouchPosition.x;

                if (Mathf.Abs(swipeDistance) > Screen.width * 0.1f) // Sensitivity
                {
                    if (swipeDistance > 0)
                        ChangeBadge(-1); // Swipe Right
                    else
                        ChangeBadge(1);  // Swipe Left
                }
            }
        }
    }
    public void Continu()
    {
        StartCoroutine(SetBadgeName(badges[currentIndex].title.Replace(" ", "")));
    }
    public void Back()
    {
        StateManager.Instance.HandleBackAction(gameObject);
        StateManager.Instance.OpenFooter(null,null,false);
    }

    public IEnumerator SetBadgeName(string name)
    {
        ApiDataHandler.Instance.isSignUp = true;
        GlobalAnimator.Instance.FadeInLoader();
        string path = $"users/{FirebaseManager.Instance.user.UserId}/BadgeName/";
        var dataTask = FirebaseDatabase.DefaultInstance.RootReference.Child(path).SetValueAsync(name);

        yield return new WaitUntil(() => dataTask.IsCompleted);

        if (dataTask.Exception != null)
            Debug.LogError("Error while saving badge: " + dataTask.Exception);
        else
            userSessionManager.Instance.badgeName = name;

        GlobalAnimator.Instance.FadeOutLoader();
        StateManager.Instance.OpenStaticScreen("loading", gameObject, "loadingScreen", null);
    }

    /*
    public IEnumerator SetBadgeName(string name)
    {
        ApiDataHandler.Instance.isSignUp = true;
        GlobalAnimator.Instance.FadeInLoader();
        // Build the reference path for the 'friends' node
        string path = $"users/{FirebaseManager.Instance.user.UserId}/BadgeName/";

        // Start deleting the friend from Firebase
        //var deleteTask = FirebaseDatabase.DefaultInstance.RootReference.Child(path).RemoveValueAsync();
        var dataTask = FirebaseDatabase.DefaultInstance.RootReference.Child(path).SetValueAsync(name);

        // Wait until the task completes
        yield return new WaitUntil(() => dataTask.IsCompleted);

        // Check for errors
        if (dataTask.Exception != null)
        {
            Debug.LogError("Error while saving badge: " + dataTask.Exception);
        }
        else
        {
            userSessionManager.Instance.badgeName = name;
        }
        GlobalAnimator.Instance.FadeOutLoader();
        StateManager.Instance.OpenStaticScreen("loading", gameObject, "loadingScreen", null);

        if (firstTime)
        {
                StateManager.Instance.OpenStaticScreen("loading", gameObject, "loadingScreen", null);
        }
        else
        {
            userSessionManager.Instance.badgeChange = true;
            userSessionManager.Instance.CheckAchievementStatus();
            Back();
        }
        
       }
    */
}
