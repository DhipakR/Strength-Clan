using DG.Tweening;
using Firebase.Auth;
using Firebase.Database;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using static AuthController;
public enum AccountCreationStep
{
    None,
    Username,
    WeeklyGoal,
    Weight,
    JoiningDate,
    Badge
}
public class StateManager : GenericSingletonClass<StateManager>
{
    public List<GameObject> inactivePages = new List<GameObject>();
    public GameObject footer;
    public bool isProcessing = false;

    public void OpenStaticScreen(string folderPath, GameObject currentPage, string newPage, Dictionary<string, object> data, bool keepState = false, Action<object> callback = null, bool isfooter = false, int i = 0)
    {
        if (isProcessing)
        {
            return;
        }

        isProcessing = true;

        if (!keepState)
        {
            onRemoveBackHistory();
        }
        var prefabPath = "Prefabs/"+ folderPath + "/" + newPage;
        var prefabResource = Resources.Load<GameObject>(prefabPath);
        var prefab = Instantiate(prefabResource);
        var container = GameObject.FindGameObjectWithTag(newPage);
        Debug.LogWarning($"(FolderPath({folderPath})) CurrentPage({currentPage}) {container.transform.parent.childCount - (2 + i)}");


        container.transform.SetSiblingIndex(container.transform.parent.childCount - (2+i));
        prefab.transform.SetParent(container.transform, false);
        var mController = prefab.GetComponent<PageController>();
        mController.onInit(data, callback);


        if (currentPage != null)
        {
            Action callbackSuccess = () =>
            {
                if (keepState)
                {
                    currentPage.SetActive(false);
                    inactivePages.Add(currentPage);
                }
                else
                {
                    Destroy(currentPage);
                }
                isProcessing = false;
            };
            if(folderPath != "userName")
            {
                GlobalAnimator.Instance.ApplyParallax(currentPage, prefab, callbackSuccess, keepState);
            }
        }
        else
        {
            isProcessing = false;
        }
        if (isfooter)
            callback?.Invoke(null);
    }

    public void openSidebar(string folderPath, GameObject currentPage, string newPage)
    {
        var prefabPath = "Prefabs/" + folderPath + "/" + newPage;
        var prefabResource = Resources.Load<GameObject>(prefabPath);
        var prefab = Instantiate(prefabResource);
        var container = GameObject.FindGameObjectWithTag(newPage);

        prefab.transform.SetParent(container.transform, false);

        GlobalAnimator.Instance.openSidebar(prefab);

    }
    public void OpenFooter(string folderPath , string newPage,bool create)
    {
        if (create)
        {
            var prefabPath = "Prefabs/" + folderPath + "/" + newPage;
            var prefabResource = Resources.Load<GameObject>(prefabPath);
            var prefab = Instantiate(prefabResource);
            var container = GameObject.FindGameObjectWithTag(newPage);

            prefab.transform.SetParent(container.transform, false);
            footer = prefab.gameObject;
        }
        else
        {
            footer.SetActive(true);
            footer.transform.parent.SetAsLastSibling();
        }

        //GlobalAnimator.Instance.openSidebar(prefab);
    }
    public void DeleteFooter()
    {
        Destroy(footer.gameObject); footer = null;
    }
    public void SetSpecificFooterButton(FooterButtons button)
    {
        footer.GetComponent<FooterController>().BottomButtonSelectionSeter(button);
    }
    public void CloseFooter()
    {
        footer.SetActive(false);
    }
    public void HandleSidebarBackAction(GameObject currentActivePage)
    {
        GlobalAnimator.Instance.closeSidebar(currentActivePage);
    }

    public void HandleBackAction(GameObject currentActivePage)
    {
        float moveTargetX = currentActivePage.transform.position.x + Screen.width;  

        currentActivePage.transform.DOMoveX(moveTargetX, 0.3f).SetEase(Ease.InOutQuad).OnComplete(() =>
        {
            Destroy(currentActivePage);
        });

        if (inactivePages.Count > 0)
        {
            GameObject lastPage = inactivePages[inactivePages.Count - 1];
            CanvasGroup canvasGroup = lastPage.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = lastPage.AddComponent<CanvasGroup>();
            }
            canvasGroup.alpha = 1;  
            lastPage.SetActive(true);
            lastPage.GetComponent<CanvasGroup>().interactable = true;

            lastPage.transform.position = new Vector3(lastPage.transform.position.x - 500, lastPage.transform.position.y, lastPage.transform.position.z);
            lastPage.transform.DOMoveX(lastPage.transform.position.x + 500, 0f).SetEase(Ease.InOutQuad);

            GameObject overlayBlocker = lastPage.transform.Find("overlayBlocker(Clone)").gameObject;
            if (overlayBlocker != null)
            {
                CanvasGroup overlayCanvasGroup = overlayBlocker.GetComponent<CanvasGroup>();
                if (overlayCanvasGroup == null)
                {
                    overlayCanvasGroup = overlayBlocker.AddComponent<CanvasGroup>();
                }
                overlayCanvasGroup.DOFade(0, 0.5f).SetEase(Ease.InOutQuad).OnComplete(() =>
                {
                    Destroy(overlayBlocker);
                });
            }
            userSessionManager.Instance.currentScreen = lastPage;
            inactivePages.RemoveAt(inactivePages.Count - 1);

        }
    }
    public bool CheckTutorial(string mode, string description, int tutorialId)
    {
        return true;
        // Check if the tutorial is already finished
        if (PlayerPrefs.GetInt("TutorialFinished", 0) == 1 || PlayerPrefs.GetInt($"TutorialFinishedPart{tutorialId}", 0) == 1)
        {
            return true;
        }
        else
        {
            // Prepare the initial data for the popup
            List<object> initialData = new List<object> { mode, description, null, tutorialId };

            // Define the callback for when the popup is closed
            Action<List<object>> onFinish = (data) => { };

            if (!GameObject.FindGameObjectWithTag("FinishWorkoutPopup"))
            {
                PopupController.Instance.OpenPopup("workoutLog", "ContinueWorkoutPopup", onFinish, initialData);
            }
            return false;
        }
    }
    public void onRemoveBackHistory()
    {
        foreach (GameObject page in inactivePages)
        {
            Destroy(page);
        }
        inactivePages.Clear();
    }

    public int getInactivePagesCount()
    {
        return inactivePages.Count;
    }
    public bool checkPageByTag(string tag)
    {
        foreach(GameObject page in inactivePages)
        {
            if(page.tag == tag)
            {
                return true;
            }
        }
        return false;
    }
    public void ShiftStep(AccountCreationStep step)
    {
        currentStep = step;
    }


    public AccountCreationStep currentStep = AccountCreationStep.None;

    public void Backer(GameObject go = null)
    {
        Debug.LogWarning("Button clicked");

        switch (currentStep)
        {
            case AccountCreationStep.Username:
                DeleteAccount();
                FirebaseSignOut();
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
                break;

            case AccountCreationStep.WeeklyGoal:
                currentStep = AccountCreationStep.Username;
                OpenStaticScreen("userName", go, "userNameScreen", null);
                break;

            case AccountCreationStep.Weight:
                currentStep = AccountCreationStep.WeeklyGoal;
                OpenStaticScreen("profile", go, "weeklyGoalScreen", null);
                break;

            case AccountCreationStep.JoiningDate:
                currentStep = AccountCreationStep.Weight;
                OpenStaticScreen("weight", gameObject, "weightScreen", null);
                break;

            case AccountCreationStep.Badge:
                currentStep = AccountCreationStep.JoiningDate;
                OpenStaticScreen("date", gameObject, "DateScreen", null);
                break;

            default:
                // HandleBackAction(go); 
                Dictionary<string, object> mData5 = new Dictionary<string, object>
                {
                    { AuthKey.sAuthType, AuthConstant.sAuthTypeLogin }
                };
                OpenStaticScreen("auth", go, "authScreen", mData5);
                break;
        }

    }
    private void FirebaseSignOut()
    {
        FirebaseAuth auth = FirebaseAuth.DefaultInstance;
        if (auth.CurrentUser != null)
        {
            auth.SignOut();
            Debug.Log("User signed out successfully.");
        }
        else
        {
            Debug.Log("No user is currently signed in.");
        }
    }

    private async void DeleteAccount()
    {
        Debug.LogWarning("FF");

        if (FirebaseManager.Instance.user != null)
        {
            try
            {
                // Show loader while deleting the account
                GlobalAnimator.Instance.FadeInLoader();

                // Get the current username
                string username = userSessionManager.Instance.mProfileUsername;

                // Delete the username from the "usernames" node in the Realtime Database
                if (!string.IsNullOrEmpty(username))
                {
                    var usernameRef = FirebaseDatabase.DefaultInstance.GetReference("usernames");
                    await usernameRef.Child(username).RemoveValueAsync();
                    Debug.Log("Username deleted from the 'usernames' node.");
                }

                await FirebaseManager.Instance.user.DeleteAsync();
                Debug.LogWarning("Account deleted successfully.");
            }
            catch (Exception ex)
            {
                Debug.LogWarning("Failed to delete account: " + ex.Message);
                GlobalAnimator.Instance.FadeOutLoader();
            }
            finally
            {
                // Hide the loader regardless of success or failure
                GlobalAnimator.Instance.FadeOutLoader();
            }
        }
        else
        {
            Debug.LogWarning("No user is currently signed in.");
        }
    }
}
