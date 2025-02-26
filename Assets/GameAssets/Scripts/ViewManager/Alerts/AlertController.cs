using System;
using TMPro;
using UnityEngine;

public class AlertController : MonoBehaviour
{
    public TMP_Text aHeader { get; set; }
    public TMP_Text aTrigger { get; set; }
    public TMP_Text aSecondaryTrigger { get; set; }
    public TMP_Text aMessage { get; set; }
    public Action mCallbackSuccess { get; set; }


    public void InitController(string pMessage, Action pCallbackSuccess = null, string pHeader = "Success", string pTrigger = "Proceed", string pSecondaryTrigger = "Dismiss")
    {
        if (aHeader == null || aTrigger == null || aSecondaryTrigger == null || aMessage == null)
        {
            Debug.LogWarning("One or more TMP_Text components are missing in AlertController.");
            return;
        }

        aHeader.text = pHeader;
        aTrigger.text = pTrigger;
        aSecondaryTrigger.text = pSecondaryTrigger;
        aMessage.text = pMessage;
        mCallbackSuccess = pCallbackSuccess;
    }

    public void OnTriggerPrimary()
    {
        if(mCallbackSuccess != null)
        {
            mCallbackSuccess.Invoke();
        }
        GlobalAnimator.Instance.AnimateAlpha(gameObject, false);
    }

    public void OnTriggerSecondary()
    {
        GlobalAnimator.Instance.AnimateAlpha(gameObject, false);
    }

    public void OnClose()
    {
        GlobalAnimator.Instance.AnimateAlpha(gameObject, false);
    }

    void Start()
    {
        
    }

    void Update()
    {
        
    }
}
