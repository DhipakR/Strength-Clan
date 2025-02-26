using System;
using System.Collections;
using Firebase;
using Firebase.Auth;
using Firebase.Storage;
using Firebase.Database;
using Firebase.Extensions;
using UnityEngine;
using System.Threading.Tasks;

public class FirebaseManager : GenericSingletonClass<FirebaseManager>
{
    public FirebaseAuth auth;
    public FirebaseUser user;
    public DependencyStatus dependencyStatus = DependencyStatus.UnavilableMissing;
    public DatabaseReference databaseReference { get; private set; }
    public StorageReference storageReference { get; private set; }
    public bool firebaseInitialized;

    private void Start()
    {

    }

    public void Load(Action onFirebaseInitialized)
    {
        print("Load");
        // Initialize Firebase
        Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            if (task.Result == Firebase.DependencyStatus.Available)
            {
                // Firebase is ready
                dependencyStatus = DependencyStatus.Available;
                auth = FirebaseAuth.DefaultInstance;
                databaseReference = FirebaseDatabase.DefaultInstance.RootReference;
                storageReference = FirebaseStorage.DefaultInstance.RootReference;
                firebaseInitialized = true;
                user = auth.CurrentUser;
                if (user != null)
                {
                    // User is logged in, proceed to the main screen or home
                    Debug.Log("User is logged in: " + user.UserId);
                }
                onFirebaseInitialized?.Invoke();
            }
            else
            {
                Debug.LogError($"Could not resolve all Firebase dependencies: {task.Result}");
            }
            if (auth == null)
                print("null");
        });
    }

    public void OnSaveUser(string email, string password)
    {
        PlayerPrefs.SetString("email", email);
        PlayerPrefs.SetString("password", password);
        PlayerPrefs.Save();
    }

    public void OnTryLogin(string email, string password, Action onSuccess, Action<FirebaseException> onFailure)
    {
        print("out");
        if (!firebaseInitialized)
        {
            print("Firebase not initialized. Loading...");
            Load(() => OnTryLogin(email, password, onSuccess, onFailure));
            return;
        }
        Credential credential =
         EmailAuthProvider.GetCredential(email, password);
        auth.SignInAndRetrieveDataWithCredentialAsync(credential).ContinueWithOnMainThread(task =>
        {

            if (task.IsCanceled || task.IsFaulted)
            {
                onFailure?.Invoke(task.Exception.GetBaseException() as FirebaseException);
            }
            else
            {
                user = task.Result.User;
                //OnSaveUser(email, password);
                string username = HelperMethods.Instance.ExtractUsernameFromEmail(email);
                string userId = user.UserId;
                print(username + "  " + userId);
                onSuccess?.Invoke();
            }
        });
    }

    public void OnTryRegisterNewAccount(string email, string password, Action pCallbackSuccess, Action<AggregateException> onFailure)
    {
        print("try to create");
        auth.CreateUserWithEmailAndPasswordAsync(email, password).ContinueWithOnMainThread(task =>
        {
            print("checking");
            if (task.IsCanceled)
            {
                onFailure(task.Exception);
                return;
            }
            if (task.IsFaulted)
            {
                // Handle specific error codes here
                FirebaseException firebaseException = task.Exception?.InnerException as FirebaseException;
                if (firebaseException != null)
                {
                    AuthError errorCode = (AuthError)firebaseException.ErrorCode;
                    if (errorCode == AuthError.EmailAlreadyInUse)
                    {
                        // add code for sign in
                        //Debug.Log("This email is already in use.");
                        OnTryLogin(email, password, pCallbackSuccess, null);
                    }
                    else
                    {
                        //Debug.Log("Error checking email: " + firebaseException.Message);
                    }
                }
                else
                {
                    // General error handling if we don't get a FirebaseException
                    //Debug.Log("An unknown error occurred: " + task.Exception?.Message);
                }
                return;
            }


            if (task.IsCompleted)
            {
                print("create");
                user = task.Result.User;
                //OnSaveUser(email, password);
                pCallbackSuccess.Invoke();
            }
        });
    }

    public void OnTryPasswordReset(string email, Action onSuccess, Action<FirebaseException> onFailure)
    {
        auth.SendPasswordResetEmailAsync(email).ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted|| task.IsCanceled)
            {
                onFailure?.Invoke(task.Exception.GetBaseException() as FirebaseException);
                return;
            }
            if (task.IsCompleted)
            {
                onSuccess?.Invoke();
            }
        });
    }
    public void OnLogout()
    {
        auth.SignOut();
        user = null;
        PlayerPrefs.DeleteAll();
    }

    public void OnLogoutForced()
    {
        //auth.SignOut();
        //PlayerPrefs.DeleteKey("email");
        //PlayerPrefs.DeleteKey("password");
    }

    public void InitiatePasswordRecovery(string email, Action onSuccess, Action<AggregateException> onFailure)
    {
        //auth.SendPasswordResetEmailAsync(email).ContinueWith(task =>
        //{
        //    if (task.IsCanceled || task.IsFaulted)
        //    {
        //        onFailure(task.Exception);
        //    }
        //    else
        //    {
        //        Debug.Log("Password reset email sent successfully.");
        //        onSuccess();
        //    }
        //});
    }

    public void InitiateAccountDeletion(Action onSuccess, Action<AggregateException> onFailure)
    {
        //if (user != null)
        //{
        //    user.DeleteAsync().ContinueWith(task =>
        //    {
        //        if (task.IsCanceled || task.IsFaulted)
        //        {
        //            onFailure(task.Exception);
        //        }
        //        else
        //        {
        //            Debug.Log("Account deleted successfully.");
        //            onSuccess();
        //        }
        //    });
        //}
        //else
        //{
        //    Debug.LogError("No user is signed in.");
        //    onFailure(new AggregateException(new Exception("No user signed in.")));
        //}
    }
    public void CheckIfLocationExists(string path, System.Action<bool> callback)
    {
        databaseReference.Child(path).GetValueAsync().ContinueWithOnMainThread(task =>
        {
            bool exists = task.IsCompleted && task.Result.Exists;
            callback(exists);  // Call the callback with true or false
            return exists;
        });
    }
    //public void SaveDataToFirebase(string path, object value)
    //{
    //    print("saving...");
    //    databaseReference.Child(path).SetValueAsync(value).ContinueWithOnMainThread(task =>
    //    {
    //        if (task.IsCompleted)
    //        {
    //            Debug.Log("Data saved successfully at: " + path);
    //        }
    //        else
    //        {
    //            Debug.LogError("Failed to save data at: " + path);
    //        }
    //    });
    //}
    public async void GetDataFromFirebase(string path, System.Action<DataSnapshot> callback)
    {
        try
        {
            DataSnapshot snapshot = await databaseReference.Child(path).GetValueAsync();

            if (snapshot.Exists)
            {
                // Update Unity-related elements here
                callback(snapshot);
            }
            else
            {
                print("No data exist at path: " + path);
            }
        }
        catch (System.Exception ex)
        {
            print("Error: " + ex.Message);
        }
    }

    public Task<(string,string)> FetchFriendDetails(string friendId)
    {
        var taskCompletionSource = new TaskCompletionSource<(string, string)>();
        databaseReference.Child("users").Child(friendId).GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;

                 string level = snapshot.Child("CharacterLevel").Value.ToString();
                 string badge = snapshot.Child("BadgeName").Value.ToString();

                // Do something with the friend data (level and badge)
                Debug.Log("Friend ID: " + friendId + " Level: " + level + " Badge: " + badge);
                taskCompletionSource.SetResult((level, badge));

            }
            else
            {
                taskCompletionSource.SetResult(("", "")); // Return empty values if something goes wrong
            }
        });
        return taskCompletionSource.Task;
    }
   

    public void OnServerInitialized()
    {
        // Firebase initialization logic (optional)
    }
}
