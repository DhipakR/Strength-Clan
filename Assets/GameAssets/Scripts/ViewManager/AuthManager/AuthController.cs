using UnityEngine;
using TMPro;
using System;
using System.Collections.Generic;
using UnityEngine.UI;
//using Assets.SimpleGoogleSignIn.Scripts;
using System.Collections;
using Firebase;
using Firebase.Auth;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;

public class AuthController : MonoBehaviour, PageController
{

    public Button backButton;

    [Header("Utilities")]
    public TMP_Text aError;
    public TMP_Text aHeading;
    public TMP_Text aPageToggleText1;
    public TMP_Text aPageToggleText2;

    [Header("Auth Fields")]
    public TMP_InputField aUsername;
    public TMP_InputField aPassword;
    public TMP_InputField aReEnterPassword;
    public TextMeshProUGUI aTriggerButton;
    public GameObject aForgetPassword;
    public RectTransform aLineDevider;
    public RectTransform togglePage;
    public RectTransform contnueButton;
    public RectTransform agoogle;
    public RectTransform aApple;

    private string mAuthType;

    //public GoogleAuth GoogleAuth;
    public Text Log;
    public Text Output;
    public bool isRegistering;

    public void onInit(Dictionary<string, object> pData, Action<object> callback)
    {
        this.mAuthType = (string)pData.GetValueOrDefault(AuthKey.sAuthType, AuthConstant.sAuthTypeSignup);
        print("BRUH " + mAuthType);
        if (this.mAuthType == AuthConstant.sAuthTypeLogin)
        {
            aHeading.text = "login";
            aTriggerButton.text = "Continue";
            aPageToggleText1.text = "New user?";
            aPageToggleText2.text = "Create an account.";
            aForgetPassword.SetActive(true);
            aReEnterPassword.gameObject.SetActive(false);
            ChangeYPosition(aLineDevider, -95f);
            ChangeYPosition(togglePage, -515f);
            ChangeYPosition(contnueButton, -598f);
            ChangeYPosition(agoogle, -86.6f);
            ChangeYPosition(aApple, -168.3f);
            ChangeYPosition(aError.transform.parent.GetComponent<RectTransform>(), -55);
            backButton.gameObject.SetActive(false);
        }
        else if (this.mAuthType == AuthConstant.sAuthTypeSignup)
        {
            aHeading.text = "create account";
            aTriggerButton.text = "next";
            aPageToggleText1.text = "Already have an acount?";
            aPageToggleText2.text = "Log In";
            aForgetPassword.SetActive(false);
            aReEnterPassword.gameObject.SetActive(true);
            ChangeYPosition(aError.transform.parent.GetComponent<RectTransform>(), -90);
            ChangeYPosition(aLineDevider, -130f);
            togglePage.gameObject.SetActive(false);
            ChangeYPosition(contnueButton, -603f);
            ChangeYPosition(agoogle, -138);
            ChangeYPosition(aApple, -220);
            aPageToggleText1.gameObject.SetActive(false);
            aPageToggleText2.gameObject.SetActive(false);
            backButton.gameObject.SetActive(true);

        }
        StartCoroutine(prelaodAssets());
        userSessionManager.Instance.mSidebar = false;
        aUsername.text = "";
        // GoogleAuth = new GoogleAuth();
        //GoogleAuth.TryResume(OnSignIn, OnGetAccessToken);

        onVerifyFirstLogin();
    }
    public void Start()
    {
    }
    public void Backer()
    {
        StateManager.Instance.Backer(gameObject);
    }

    IEnumerator prelaodAssets()
    {
        UnityEngine.Object[] prefabs = Resources.LoadAll("Prefabs", typeof(GameObject));
        List<GameObject> instantiatedObjects = new List<GameObject>();

        foreach (UnityEngine.Object prefab in prefabs)
        {
            if (!prefab.name.Contains("auth") && prefab.name.Contains("screen"))
            {
                prefab.name = "cached";
                GameObject instantiatedPrefab = Instantiate(prefab, Vector3.zero, Quaternion.identity, transform.parent.parent.parent) as GameObject;
                instantiatedObjects.Add(instantiatedPrefab);
                instantiatedPrefab.transform.SetAsFirstSibling();
            }
            yield return null;
        }

        foreach (GameObject obj in instantiatedObjects)
        {
            Destroy(obj);
        }

    }

    public void onVerifyFirstLogin()
    {
        string mUsername = PreferenceManager.Instance.GetString("login_username");
        print(mUsername);
        if (mUsername.Length > 2)
        {
            if (mUsername.Contains("@"))
            {
                mUsername = HelperMethods.Instance.ExtractUsernameFromEmail(mUsername);
            }
            userSessionManager.Instance.OnInitialize(mUsername, mUsername);
            onSignIn();
        }
        else if (mUsername == "")
        {
            PreferenceManager.Instance.SetBool("FirstTimePlanInitialized_" /*+ userSessionManager.Instance.mProfileUsername*/, true);
        }
        if (FirebaseManager.Instance.user != null)
        {
            onSignIn();
            userSessionManager.Instance.mProfileID = FirebaseManager.Instance.user.UserId;
        }
    }

    //IEnumerator CallSavedlogins()
    //{


    //    if (GoogleAuth.SavedAuth != null)
    //    {
    //        GlobalAnimator.Instance.FadeInLoader();
    //        yield return new WaitForSeconds(2);
    //        GoogleAuth.SignIn(OnSignIn, caching: true);
    //        userSessionManager.Instance.OnInitialize(mAuthType, "");
    //        onSignIn();
    //    }


    //}
    void GmailSignIn()
    {

        //if (GoogleAuth.SavedAuth != null)
        //{
        //    GoogleAuth.SignIn(OnSignIn, caching: true);
        //    userSessionManager.Instance.OnInitialize(mAuthType, "");
        //    onSignIn();
        //}

        //else
        {
            GlobalAnimator.Instance.FadeInLoader();
            // GoogleAuth.SignIn(OnSignIn, caching: true);
        }


    }

    public void SignOut()
    {
        // GoogleAuth.SignOut(revokeAccessToken: true);
    }

    public void GetAccessToken()
    {
        // GoogleAuth.GetAccessToken(OnGetAccessToken);
    }

    //private void OnSignIn(bool success, string error, Assets.SimpleGoogleSignIn.Scripts.UserInfo userInfo)
    //{
    //    if (success)
    //    {
    //        GlobalAnimator.Instance.FadeOutLoader();
    //        Action mCallbackSuccess = () =>
    //        {
    //            GlobalAnimator.Instance.FadeOutLoader();
    //            userSessionManager.Instance.mProfileID = FirebaseManager.Instance.user.UserId;
    //            onSignIn();
    //        };
    //        FirebaseManager.Instance.OnTryRegisterNewAccount(userInfo.email, "z4zazgS4LaejfKcs", mCallbackSuccess, null);
    //        //mAuthType = success ? $"{userInfo.name}" : error;
    //        //userSessionManager.Instance.OnInitialize(mAuthType, "");
    //        //onSignIn();
    //    }

    //}

    //private void OnGetAccessToken(bool success, string error, Assets.SimpleGoogleSignIn.Scripts.TokenResponse tokenResponse)
    //{
    //    if (!success) return;

    //    var jwt = new Assets.SimpleGoogleSignIn.Scripts.JWT(tokenResponse.IdToken);

    //    Debug.Log($"JSON Web Token (JWT) Payload: {jwt.Payload}");

    //    jwt.ValidateSignature(GoogleAuth.ClientId, OnValidateSignature);
    //}

    private void OnValidateSignature(bool success, string error)
    {
    }

    public void Navigate(string url)
    {
        Application.OpenURL(url);
    }


    public void onSignIn()
    {
        print("sign in");
        gameObject.transform.parent.SetSiblingIndex(1);
        StateManager.Instance.isProcessing = false;
        bool mFirsTimePlanInitialized = PreferenceManager.Instance.GetBool("FirstTimePlanInitialized_" /*+ userSessionManager.Instance.mProfileUsername*/, false);
        GlobalAnimator.Instance.FadeOutLoader();
        CheckUserNameSet();


    }

    public void CheckUserNameSet()
    {
        print("check userName");
        FirebaseManager.Instance.CheckIfLocationExists("/users/" + FirebaseManager.Instance.user.UserId + "/username", result =>
        {
            //print(result);
            if (result)
            {
                //print("if");
                FirebaseManager.Instance.GetDataFromFirebase("/users/" + FirebaseManager.Instance.user.UserId + "/username", data =>
                {
                    if (data.Exists)  // Ensure that data exists
                    {
                        string username = data.Value.ToString();  // Directly get the value as string
                        userSessionManager.Instance.mProfileUsername = username;
                        Debug.Log("Username retrieved: " + username);
                        CheckWeeklyGoalSet();
                    }
                });
            }
            else
            {
                GlobalAnimator.Instance.FadeOutLoader();
                StateManager.Instance.OpenStaticScreen("userName", gameObject, "userNameScreen", null);
            }
        });
    }
    public void CheckWeeklyGoalSet()
    {
        print("check weeklygoal");
        FirebaseManager.Instance.CheckIfLocationExists("/users/" + FirebaseManager.Instance.user.UserId + "/weeklyGoal", result =>
        {
            if (result)
            {
                FirebaseManager.Instance.GetDataFromFirebase("/users/" + FirebaseManager.Instance.user.UserId + "/weeklyGoal", data =>
                {
                    if (data.Exists)  // Ensure that data exists
                    {
                        string stringGoal = data.Value.ToString();  // Directly get the value as string
                        int goal = int.Parse(stringGoal);  // Directly get the value as string
                        userSessionManager.Instance.weeklyGoal = goal;
                        //ApiDataHandler.Instance.userName = username;  // Set the username in your handler
                        Debug.Log("weekly goal: " + goal);
                        CheckWeightSet();
                    }
                    else print("data is null");
                });
            }
            else
            {
                GlobalAnimator.Instance.FadeOutLoader();
                StateManager.Instance.ShiftStep(AccountCreationStep.WeeklyGoal);
                StateManager.Instance.OpenStaticScreen("profile", gameObject, "weeklyGoalScreen", null);
            }
        });

    }
    public void CheckWeightSet()
    {
        print("check weight");
        FirebaseManager.Instance.CheckIfLocationExists("/users/" + FirebaseManager.Instance.user.UserId + "/measurements", result =>
        {
            if (result)
            {
                FirebaseManager.Instance.GetDataFromFirebase("/users/" + FirebaseManager.Instance.user.UserId + "/measurements/weight", data =>
                {
                    if (data.Exists)  // Ensure that data exists
                    {
                        string stringWeight = data.Value.ToString();
                        float weight = float.Parse(stringWeight);
                        Debug.Log("setted weight: " + weight);
                        CheckJoiningDateSet();
                    }
                });
            }
            else
            {
                GlobalAnimator.Instance.FadeOutLoader();
                StateManager.Instance.ShiftStep(AccountCreationStep.Weight);
                StateManager.Instance.OpenStaticScreen("weight", gameObject, "weightScreen", null);
            }
        });
    }
    public void CheckJoiningDateSet()
    {
        print("check joining date");
        FirebaseManager.Instance.CheckIfLocationExists("/users/" + FirebaseManager.Instance.user.UserId + "/joiningDate", result =>
        {
            if (result)
            {
                FirebaseManager.Instance.GetDataFromFirebase("/users/" + FirebaseManager.Instance.user.UserId + "/joiningDate", data =>
                {
                    if (data.Exists)  // Ensure that data exists
                    {
                        string date = data.Value.ToString();
                        userSessionManager.Instance.joiningDate = date;
                        Debug.Log("joining date: " + date);
                        CheckBadgeNameSet();
                    }
                });
            }
            else
            {
                GlobalAnimator.Instance.FadeOutLoader();
                StateManager.Instance.ShiftStep(AccountCreationStep.JoiningDate);
                StateManager.Instance.OpenStaticScreen("date", gameObject, "DateScreen", null);
            }
        });

    }
    public void CheckBadgeNameSet()
    {
        print("check badge name");
        FirebaseManager.Instance.CheckIfLocationExists("/users/" + FirebaseManager.Instance.user.UserId + "/BadgeName", result =>
        {
            if (result)
            {
                FirebaseManager.Instance.GetDataFromFirebase("/users/" + FirebaseManager.Instance.user.UserId + "/BadgeName", data =>
                {
                    if (data.Exists)  // Ensure that data exists
                    {
                        string date = data.Value.ToString();
                        userSessionManager.Instance.badgeName = date;
                        Debug.Log("badge name: " + date);
                        GlobalAnimator.Instance.FadeOutLoader();
                        StateManager.Instance.OpenStaticScreen("loading", gameObject, "loadingScreen", null);
                    }
                });
            }
            else
            {
                GlobalAnimator.Instance.FadeOutLoader();
                StateManager.Instance.OpenStaticScreen("profile", gameObject, "ChangeBadgeScreen", null);
            }
        });
        StateManager.Instance.ShiftStep(AccountCreationStep.Badge);
    }
    void ChangeYPosition(RectTransform rectTransform, float yPos)
    {
        Vector2 newPosition = rectTransform.anchoredPosition;
        newPosition.y = yPos;
        rectTransform.anchoredPosition = newPosition;
    }

    public void OnPrivacyPolicy()
    {
        Application.OpenURL("");
    }

    public void OnOpenWebsite()
    {
        Application.OpenURL("");
    }

    public async void OnTrigger()
    {
        AudioController.Instance.OnButtonClick();
        if (this.mAuthType == AuthConstant.sAuthTypeLogin)
        {
            Action/*<string, string>*/ mCallbackSuccess = (/*string pResult1, string pResult2*/) =>
            {
                GlobalAnimator.Instance.FadeOutLoader();
                //userSessionManager.Instance.OnInitialize(pResult1, pResult2);
                onSignIn();
            };
            Action<FirebaseException> callbackFailure = (pError) =>
            {
                print(pError);
                GlobalAnimator.Instance.FadeOutLoader();
                GlobalAnimator.Instance.FadeIn(aError.gameObject);
                //var errorMessage = pError.InnerException != null
                //    ? pError.InnerException.Message
                //    : pError.Message;
                //aError.text = ErrorManager.Instance.getTranslateError(errorMessage);

                AuthError errorCode = (AuthError)pError.ErrorCode;
                string message = "Login Failed!";
                switch (errorCode)
                {
                    case AuthError.MissingEmail:
                        message = "Missing Email";
                        break;
                    case AuthError.MissingPassword:
                        message = "Missing Password";
                        break;
                    case AuthError.WrongPassword:
                        message = "Wrong Password";
                        break;
                    case AuthError.InvalidEmail:
                        message = "Invalid Email";
                        break;
                    case AuthError.UserNotFound:
                        message = "Account does not exist";
                        break;
                }
                aError.text = message;
            };

            GlobalAnimator.Instance.FadeInLoader();
            print("login");
            FirebaseManager.Instance.OnTryLogin(this.aUsername.text, this.aPassword.text, mCallbackSuccess, callbackFailure);
        }
        else if (this.mAuthType == AuthConstant.sAuthTypeSignup)
        {
            string checkThis = aUsername.text.ToLower();
            if (string.IsNullOrEmpty(checkThis))
            {
                aError.text = "Email is required";
                aError.gameObject.SetActive(true);
                return;
            }
            else if (!checkThis.Contains("@gmail.com"))
            {
                aError.text = "Email invalid domain";
                aError.gameObject.SetActive(true);
                return;
            }
            else if (checkThis.Length <= 10)
            {
                aError.text = "Email invalid length";
                aError.gameObject.SetActive(true);
                return;
            }
            if (aPassword.text.Length < 6)
            {
                aError.text = "Password must be 6 characters or longer";
                aError.gameObject.SetActive(true);
                return;
            }
            else if (aPassword.text != aReEnterPassword.text)
            {
                aError.text = "Password doesn't match";
                aError.gameObject.SetActive(true);
                return;
            }
            else
            {
                aError.text = "";
                aError.gameObject.SetActive(false);
            }

            bool emailExists = await CheckEmailAlreadyInUse(aUsername.text, aPassword.text);
            if (emailExists)
            {
                aError.text = "Email already in use";
                aError.gameObject.SetActive(true);
                return;
            }
            else
            {
                aError.text = "";
                aError.gameObject.SetActive(false);
            }

            Action callbackSuccess = () =>
            {
                GlobalAnimator.Instance.FadeOutLoader();

                //PreferenceManager.Instance.SetBool("FirstTimePlanInitialized_" + userSessionManager.Instance.mProfileUsername, true);
                GameObject alertPrefab = Resources.Load<GameObject>("Prefabs/alerts/alertSuccess");
                GameObject alertsContainer = GameObject.FindGameObjectWithTag("alerts");
                GameObject instantiatedAlert = Instantiate(alertPrefab, alertsContainer.transform);
                AlertController alertController = instantiatedAlert.GetComponent<AlertController>();
                alertController.InitController("Account Created Successfully", pTrigger: "Continue Login");
                GlobalAnimator.Instance.AnimateAlpha(instantiatedAlert, true);

                Dictionary<string, object> mData = new Dictionary<string, object>
                {
                    { AuthKey.sAuthType, AuthConstant.sAuthTypeLogin }
                };
                StateManager.Instance.OpenStaticScreen("auth", gameObject, "authScreen", mData);
                ApiDataHandler.Instance.SetJoiningDate(DateTime.Now);
            };

            Action<AggregateException> callbackFailure = (pError) =>
            {
                //GlobalAnimator.Instance.FadeOutLoader();
                //GlobalAnimator.Instance.FadeIn(aError.gameObject);
                //aError.text = ErrorManager.Instance.getTranslateError(pError.Error.ToString());
                print(2);
                GlobalAnimator.Instance.FadeOutLoader();
                GlobalAnimator.Instance.FadeIn(aError.gameObject);
                var errorMessage = pError.InnerException != null
                    ? pError.InnerException.Message
                    : pError.Message;
                aError.text = ErrorManager.Instance.getTranslateError(errorMessage);
            };

            GlobalAnimator.Instance.FadeInLoader();
            FirebaseManager.Instance.OnTryRegisterNewAccount(this.aUsername.text, this.aPassword.text, callbackSuccess, callbackFailure);
        }

    }
    public async Task<bool> CheckEmailAlreadyInUse(string email, string password)
    {
        try
        {
            FirebaseAuth auth = FirebaseAuth.DefaultInstance;
            await auth.CreateUserWithEmailAndPasswordAsync(email, password);
            return false;
        }
        catch (FirebaseException ex)
        {
            if ((AuthError)ex.ErrorCode == AuthError.EmailAlreadyInUse)
            {
                return true;
            }
            else
            {
                Debug.LogError("Fail Firebase: " + ex.Message);
                return false;
            }
        }
    }

    public void OnForgotPassword()
    {
        if (aUsername.text == "")
        {
            aError.text = "Invalid or emtpy email";
            GlobalAnimator.Instance.FadeIn(aError.gameObject);
            return;
        }
        Action callbackSuccess = () =>
        {
            Action callbackSuccess = () =>
            {
                Application.OpenURL("mailto:");
            };

            GlobalAnimator.Instance.FadeOutLoader();
            GameObject alertPrefab = Resources.Load<GameObject>("Prefabs/alerts/alertSuccess");
            GameObject alertsContainer = GameObject.FindGameObjectWithTag("alerts");
            alertsContainer.transform.SetAsLastSibling();
            GameObject instantiatedAlert = Instantiate(alertPrefab, alertsContainer.transform);
            AlertController alertController = instantiatedAlert.GetComponent<AlertController>();
            alertController.InitController("Reset password instructions have been sent to your email address", pCallbackSuccess: callbackSuccess, pTrigger: "Open Mail");
            GlobalAnimator.Instance.AnimateAlpha(instantiatedAlert, true);
        };
        Action<FirebaseException> callbackFailure = (pError) =>
        {
            GlobalAnimator.Instance.FadeOutLoader();
            GlobalAnimator.Instance.FadeIn(aError.gameObject);
            string errorMessage = pError?.InnerException != null
                ? pError.InnerException.Message
                : pError?.Message ?? "An unknown error occurred.";
            aError.text = ErrorManager.Instance.getTranslateError(errorMessage);
            GameObject alertPrefab = Resources.Load<GameObject>("Prefabs/alerts/alertFailure");
            GameObject alertsContainer = GameObject.FindGameObjectWithTag("alerts");
            alertsContainer.transform.SetAsLastSibling();
            GameObject instantiatedAlert = Instantiate(alertPrefab, alertsContainer.transform);
            AlertController alertController = instantiatedAlert.GetComponent<AlertController>();
            alertController.InitController("Email address was not found in our database", pTrigger: "Continue", pHeader: "Request Error");
            GlobalAnimator.Instance.AnimateAlpha(instantiatedAlert, true);
        };
        GlobalAnimator.Instance.FadeInLoader();
        FirebaseManager.Instance.OnTryPasswordReset(aUsername.text, callbackSuccess, callbackFailure);

    }


    public void OnSignGmail()
    {
        GmailSignIn();

    }


    public void OnResetErrors()
    {
        GlobalAnimator.Instance.FadeOut(aUsername.gameObject);
        GlobalAnimator.Instance.FadeOut(aPassword.gameObject);
    }

    public void OnToogleAuth()
    {
        if (this.mAuthType == AuthConstant.sAuthTypeLogin)
        {
            Dictionary<string, object> mData = new Dictionary<string, object>
            {
                { AuthKey.sAuthType, AuthConstant.sAuthTypeSignup}
            };
            StateManager.Instance.OpenStaticScreen("auth", gameObject, "authScreen", mData);
        }
        else if (this.mAuthType == AuthConstant.sAuthTypeSignup)
        {
            OnResetErrors();
            Dictionary<string, object> mData = new Dictionary<string, object>
            {
                { AuthKey.sAuthType, AuthConstant.sAuthTypeLogin}
            };
            StateManager.Instance.OpenStaticScreen("auth", gameObject, "authScreen", mData);
        }
    }

}