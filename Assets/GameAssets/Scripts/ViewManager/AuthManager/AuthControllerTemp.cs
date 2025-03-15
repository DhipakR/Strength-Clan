//using UnityEngine;
//using System.Collections;
//using Assets.SimpleGoogleSignIn.Scripts;

//public class GoogleSignInController : GenericSingletonClass<GoogleSignInController>
//{
//    public GoogleAuth GoogleAuth;

//    private string mAuthType;

//    public void Start()
//    {
//        GoogleAuth = new GoogleAuth();
//        GoogleAuth.TryResume(OnSignIn, OnGetAccessToken);
//    }

//    public void GmailSignIn()
//    {
//        if (GoogleAuth.SavedAuth != null)
//        {
//            GoogleAuth.SignIn(OnSignIn, caching: true);
//            Debug.Log("Saved Gmail Logged In: " + GoogleAuth.SavedAuth);
//            onSignIn();
//        }
//        else
//        {
//            GoogleAuth.SignIn(OnSignIn, caching: true);
//            Debug.Log("Fresh Gmail Sign-In initiated.");
//        }
//    }

//    public void SignOut()
//    {
//        GoogleAuth.SignOut(revokeAccessToken: true);
//        Debug.Log("Signed out of Google.");
//    }

//    public void GetAccessToken()
//    {
//        GoogleAuth.GetAccessToken(OnGetAccessToken);
//    }

//    private void OnSignIn(bool success, string error, UserInfo userInfo)
//    {
//        if (success)
//        {
//            mAuthType = userInfo.name;
//            Debug.Log("Google Sign-In Successful: " + mAuthType);
//        }
//        else
//        {
//            Debug.LogError("Google Sign-In Failed: " + error);
//        }
//    }

//    private void OnGetAccessToken(bool success, string error, TokenResponse tokenResponse)
//    {
//        if (success)
//        {
//            var jwt = new JWT(tokenResponse.IdToken);
//            Debug.Log("JWT Payload: " + jwt.Payload);
//            jwt.ValidateSignature(GoogleAuth.ClientId, OnValidateSignature);
//        }
//        else
//        {
//            Debug.LogError("Failed to get access token: " + error);
//        }
//    }

//    private void OnValidateSignature(bool success, string error)
//    {
//        if (success)
//        {
//            Debug.Log("JWT Signature Validated Successfully.");
//        }
//        else
//        {
//            Debug.LogError("JWT Signature Validation Failed: " + error);
//        }
//    }

//    private void onSignIn()
//    {
//        Debug.Log("Executing post sign-in actions.");
//        // Add further actions after successful sign-in, if needed.
//    }
//}
