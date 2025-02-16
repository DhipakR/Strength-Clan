using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InfoBoxPopup : MonoBehaviour, IPrefabInitializer
{
    public TMP_Text titleText;
    public TMP_Text bodyText;
    public Button okayButton;

    public void InitPrefab(Action<List<object>> onFinish, List<object> data)
    {
        string title = (string)data[0];
        string body = (string)data[1];

        titleText.text = title;
        bodyText.text = body;

        okayButton.onClick.AddListener(() =>
        {
            onFinish?.Invoke(data);
            PopupController.Instance.ClosePopup("InfoBoxPopup");
        });
    }
}