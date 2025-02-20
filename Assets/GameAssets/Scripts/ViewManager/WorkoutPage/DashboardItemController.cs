using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

public class DashboardItemController : MonoBehaviour, ItemController
{
    public TMP_Text workoutNameText;
    //public TMP_InputField editWorkoutName;
    public Transform exerciseParent;
    public Button playButton;
    public Button editButton;
    public Image spriteImage;
    public Sprite darkTheme, lightTheme;
    public DefaultTempleteModel defaultTempleteModel;
    public ScrollRect parentScroll;
    private Action<object> callback;
    List<TextMeshProUGUI> exerciseText=new List<TextMeshProUGUI>();
    private GameObject parent;
    public void onInit(Dictionary<string, object> data, Action<object> callback = null)
    {
        this.callback = callback;
        defaultTempleteModel = (DefaultTempleteModel)data["data"];
        parent = (GameObject)data["parent"];
        parentScroll = (ScrollRect)data["scroll"];
        workoutNameText.text = userSessionManager.Instance.FormatStringAbc(defaultTempleteModel.templeteName);
        //editWorkoutName.textComponent.text = defaultTempleteModel.templeteNotes;
        //editWorkoutName.onEndEdit.AddListener(OnNameChanged);
        if (playButton != null) 
            playButton.onClick.AddListener(PlayButton);
        //editButton.onClick.AddListener(EditWorkoutName);
        editButton.onClick.AddListener(AudioController.Instance.OnButtonClick);
        editButton.onClick.AddListener(EditWorkout);

        foreach (var exercise in defaultTempleteModel.exerciseTemplete)
        {
            ExerciseTypeModel exerciseData = exercise;
            // Create the main parent GameObject
            GameObject mainParentObject = new GameObject($"ExerciseParent_{exerciseData.name}");
            mainParentObject.transform.SetParent(exerciseParent, false);
            Image imageComponent = mainParentObject.AddComponent<Image>();
            mainParentObject.GetComponent<RectTransform>().sizeDelta = new Vector2(25, 25);
            imageComponent.color = Color.clear;

            // Create the first GameObject (Image with mask)
            GameObject firstImageObject = new GameObject($"Image_{exerciseData.name}");
            firstImageObject.transform.SetParent(mainParentObject.transform, false); // Set it as a child of the main parent
            firstImageObject.transform.localScale = new Vector3(0.8f, 0.8f, 0.8f);

            // Add Image component to the first GameObject
            Image firstImage = firstImageObject.AddComponent<Image>();
            firstImage.type = Image.Type.Sliced; // Set image type to sliced
            firstImage.preserveAspect = true; // Preserve aspect ratio if needed

            // Load sprite from Resources folder and assign to the first image
            Sprite firstSprite = Resources.Load<Sprite>("UIAssets/Shared/Images/Rounded Corners/circle"); // Adjust the path
            firstImage.sprite = firstSprite;
            firstImage.color = new Color32(236,229, 210, 255);

            // Set width, height, and pixel per unit
            RectTransform firstRectTransform = firstImageObject.GetComponent<RectTransform>();
            firstRectTransform.sizeDelta = new Vector2(40, 40); // Set width and height as 25
            firstImage.pixelsPerUnitMultiplier = 4.8f; // Set pixel per unit to 1.75

            // Add Mask component to the first image
            firstImageObject.AddComponent<Mask>();

            // Create the second GameObject (another image)
            GameObject secondImageObject = new GameObject($"Image_{exerciseData.name}_Second");
            secondImageObject.transform.SetParent(firstImageObject.transform, false); // Parent it to the first image

            // Add Image component to the second GameObject
            Image secondImage = secondImageObject.AddComponent<Image>();
            secondImage.preserveAspect = true; // Preserve aspect ratio if needed

            // Load sprite from Resources and assign to the second image
            Sprite sp = null;
            if (exerciseData.name == "Pec deck")
                sp = Resources.Load<Sprite>("UIAssets/ExcerciseIcons/Chest fly (machine)-1");
            else
                sp = Resources.Load<Sprite>("UIAssets/ExcerciseIcons/" + exerciseData.name + "-1");
            if (sp != null)
                secondImage.sprite = sp;

            // Set width and height for the second image
            RectTransform secondRectTransform = secondImageObject.GetComponent<RectTransform>();
            secondRectTransform.sizeDelta = new Vector2(43, 43); // Set width and height as 28

            // Create Overlay
            GameObject overlay = new GameObject($"Image_{exerciseData.name}_Overlay");
            overlay.transform.SetParent(mainParentObject.transform, false); // Parent it to the first image

            // Add Image component to the second GameObject
            Image overlayImage = overlay.AddComponent<Image>();
            overlayImage.preserveAspect = true; // Preserve aspect ratio if needed

            // Load sprite from Resources and assign to the second image
            overlayImage.sprite = Resources.Load<Sprite>("UIAssets/Shared/Images/Rounded Corners/profile circle/profile overlay"); ;
            overlayImage.color= new Color32(249, 249, 241, 255);

            // Set width and height for the second image
            RectTransform overlayRectTransform = overlay.GetComponent<RectTransform>();
            overlayRectTransform.sizeDelta = new Vector2(50, 50); // Set width and height as 28

            // Create the TextMeshPro GameObject
            GameObject textLabelObject = new GameObject($"Exercise_{exerciseData.name}");
            textLabelObject.transform.SetParent(mainParentObject.transform, false); // Parent it to the main parent

            // Add TextMeshProUGUI component to the text label object
            TextMeshProUGUI textMeshPro = textLabelObject.AddComponent<TextMeshProUGUI>();
            //textMeshPro.maskable = false;

            // Set the size of the text box and text properties
            RectTransform textRectTransform = textLabelObject.GetComponent<RectTransform>();
            textRectTransform.sizeDelta = new Vector2(240, 20); // Set width and height
            textMeshPro.text = exerciseData.name + " x " + exerciseData.exerciseModel.Count.ToString(); // Set text content
            textMeshPro.fontSize = 16;
            textMeshPro.fontStyle = FontStyles.Normal;
            textMeshPro.alignment = TextAlignmentOptions.Left;

            // Set the pivot and position of the TextMeshPro element
            textRectTransform.pivot = new Vector2(0, 0.5f); // Left-middle pivot
            textRectTransform.anchoredPosition = new Vector2(30, 0); // Set position with X offset of 30

            // Add the TextMeshProUGUI element to the list
            exerciseText.Add(textMeshPro);

            // Optionally set the text color or any other settings
            SetColor(textMeshPro);

            // Optional margin (uncomment if needed)
            // textMeshPro.margin = new Vector4(20, 0, 0, 0); // Optional margin
        }
        
        transform.SetSiblingIndex(transform.parent.childCount - 1);
        if (exerciseParent.childCount < 7)
        {
            GetComponentInChildren<ScrollRect>().enabled = false;
        }
    }
    private void OnEnable()
    {
        foreach(TextMeshProUGUI text in exerciseText)
        {
            SetColor(text);
        }
    }
    public void EditWorkout()
    {
        Dictionary<string, object> mData = new Dictionary<string, object>
        {
                { "templeteModel", defaultTempleteModel},
                { "editWorkout", true }
            };
        StateManager.Instance.OpenStaticScreen("createWorkout", parent, "createNewWorkoutScreen", mData, true, null, true);
        StateManager.Instance.CloseFooter();
    }
    public void PlayButton()
    {
        callback?.Invoke(defaultTempleteModel);
        AudioController.Instance.OnButtonClick();
        StateManager.Instance.CloseFooter();
    }
    //public void EditWorkoutName()
    //{
    //    workoutNameText.gameObject.SetActive(false);
    //    editWorkoutName.gameObject.SetActive(true);
    //    editWorkoutName.ActivateInputField();
    //    editWorkoutName.text = workoutNameText.text;
    //    editButton.gameObject.SetActive(false);
    //}
    //public void OnNameChanged(string name)
    //{
    //    if (string.IsNullOrWhiteSpace(name))
    //    {
    //        workoutNameText.gameObject.SetActive(true);
    //        editWorkoutName.gameObject.SetActive(false);
    //        editButton.gameObject.SetActive(true);
    //    }
    //    else
    //    {
    //        defaultTempleteModel.templeteName = name.ToUpper();
    //        workoutNameText.text = name.ToUpper();
    //        //float textWidth = workoutNameText.preferredWidth;
    //        //workoutNameText.transform.GetComponent<RectTransform>().sizeDelta = new Vector2(textWidth, workoutNameText.transform.GetComponent<RectTransform>().sizeDelta.y);
    //        workoutNameText.gameObject.SetActive(true);
    //        editWorkoutName.gameObject.SetActive(false);
    //        editButton.gameObject.SetActive(true);
    //        ApiDataHandler.Instance.SaveTemplateData();
    //    }
    //}
    public void SetColor(TextMeshProUGUI text)
    {
        switch (ApiDataHandler.Instance.gameTheme)
        {
            case Theme.Light:
                text.color = new Color(150, 0, 0, 255);
                text.font = userSessionManager.Instance.lightSecondaryFont;
                break;
            case Theme.Dark:
                text.color = new Color(255, 255, 255, 255);
                text.font = userSessionManager.Instance.darkSecondaryFont;
                break;
        }
    }
}
